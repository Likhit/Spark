using MathNet.Numerics.LinearAlgebra.Double;
using Spark.Core;
using Spark.PerformanceFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Trainiers
{
	public class BackPropagationTrainer : Trainer
	{
		public BackPropagationTrainer(double learnRate = 0.05, double minError = 0.01,
			double momentum = 0.01, int maxEpochs = 100, int show = 10,
			IPerformanceFunction pFunc = null, TrainingModes tMode = TrainingModes.OffLine)
			: base(learnRate, minError, momentum, maxEpochs, show, pFunc, tMode)
		{ }

		protected override IEnumerable<object> TrainOnLine(Network net, List<DenseMatrix> inputs, List<DenseMatrix> targets)
		{
			var sInputs = inputs.Select(x => (DenseMatrix)x.Clone()).ToList();
			var sTargets = targets.Select(x => (DenseMatrix)x.Clone()).ToList();
			for (int epoch = 0; epoch < MaxEpochs + 1; epoch++)
			{
				var error = this.PFunc.Apply(net.Run(inputs), targets);
				var totalError = error.Sum() / error.Count;

				var errorObj = new
				{
					epoch = epoch,
					error = totalError
				};

				if (totalError <= MinError)
				{
					yield return errorObj;
					break;
				}

				if (epoch % Show == 0 || epoch == MaxEpochs)
				{
					yield return errorObj;
				}

				if (epoch < MaxEpochs)
				{
					Core.Utils.Helpers.Shuffle(sInputs, sTargets);
					for (int i = 0; i < sInputs[0].ColumnCount; i++)
					{
						var inp = sInputs.Select(x => DenseMatrix.OfColumnVectors(x.Column(i))).ToList();
						var targ = sTargets.Select(x => DenseMatrix.OfColumnVectors(x.Column(i))).ToList();
						var err = DenseVector.Create(1, _ => error[i]);
						var feedForwardResult = net.Run(inp, storeDerivatives: true);
						var errorDiffs = this.PFunc.Differentiate(err, feedForwardResult, targ);
						var gradients = FindGradients(net, errorDiffs);
						UpdateWeights(net, gradients);
					}
				}
			}
		}

		protected override IEnumerable<object> TrainOffLine(Network net, List<DenseMatrix> inputs, List<DenseMatrix> targets)
		{
			var sInputs = inputs.Select(x => (DenseMatrix)x.Clone()).ToList();
			var sTargets = targets.Select(x => (DenseMatrix)x.Clone()).ToList();
			for (int epoch = 0; epoch < MaxEpochs + 1; epoch++)
			{
				var error = this.PFunc.Apply(net.Run(inputs), targets);
				var totalError = error.Sum() / error.Count;

				var errorObj = new
				{
					epoch = epoch,
					error = totalError
				};

				if (totalError <= MinError)
				{
					yield return errorObj;
					break;
				}

				if (epoch % Show == 0 || epoch == MaxEpochs - 1)
				{
					yield return errorObj;
				}

				if (epoch < MaxEpochs)
				{
					Core.Utils.Helpers.Shuffle(sInputs, sTargets);
					var feedForwardResult = net.Run(sInputs, storeDerivatives: true);
					var errorDiffs = this.PFunc.Differentiate(error, feedForwardResult, sTargets);
					var gradients = FindGradients(net, errorDiffs);
					UpdateWeights(net, gradients);
				}
			}
		}

		private Dictionary<string, DenseMatrix> FindGradients(Network net, List<DenseMatrix> errors)
		{
			var gradients = new Dictionary<string, DenseMatrix>();
			for (int i = 0; i < net.OutputLayers.Count; i++)
			{
				var layer = net.OutputLayers[i];
				gradients[layer.Id] = (DenseMatrix)layer.ActivationFuncDerivative.PointwiseMultiply(errors[i]);
			}

			var currentLayers = new HashSet<Layer>(net.OutputLayers);
			while (currentLayers.Count > 0)
			{
				var prevLayers = new HashSet<Layer>(currentLayers.SelectMany(layer => net.PreviousLayers(layer)));
				foreach (var layer in prevLayers)
				{
					var successors = net.Edges[layer.Id].Keys.Select(sId => net.Layers[sId]);
					var partialGrads = successors.Select(l =>
						{
							var weightFuncDerivative = l.WeightFuncDerivatives[layer.Id];
							var inputFuncDerivative = l.InputFuncDerivative[layer.Id];
							var backpropogatedGrad = gradients[l.Id];
							return weightFuncDerivative * backpropogatedGrad.PointwiseMultiply(inputFuncDerivative);
						});
					gradients[layer.Id] = (DenseMatrix)partialGrads.Aggregate((a, x) => a + x)
						.PointwiseMultiply(layer.ActivationFuncDerivative);
				}
				currentLayers = prevLayers;
			}
			return gradients;
		}

		private void UpdateWeights(Network net, Dictionary<string, DenseMatrix> gradients)
		{
			var currentLayers = new HashSet<Layer>(net.InputLayers);
			while (currentLayers.Count > 0)
			{
				var nextLayers = new HashSet<Layer>();
				foreach (var cLayer in currentLayers)
				{
					var deltaH = LearnRate * gradients[cLayer.Id];
					if (cLayer.Biased)
					{
						cLayer.Biases = DenseVector.OfVector(deltaH.ColumnEnumerator().Select(tuple => tuple.Item2)
							.Aggregate((x, a) => a + x)) + cLayer.Biases;
					}
					if (net.Edges.ContainsKey(cLayer.Id))
					{
						var nxtLys = net.Edges[cLayer.Id];
						foreach (var key in nxtLys.Keys.ToList())
						{
							var weights = nxtLys[key];
							var delta = LearnRate * gradients[key] * cLayer.Output.Transpose();
							net.Edges[cLayer.Id][key] = weights + (DenseMatrix)delta;
							nextLayers.Add(net.Layers[key]);
						}
					}
				}
				currentLayers = nextLayers;
			}
		}
	}
}
