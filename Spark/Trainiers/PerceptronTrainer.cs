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
	public class PerceptronTrainer : Trainer
	{
		public PerceptronTrainer(double learnRate = 0.05, double minError = 0.01,
			double momentum = 0.01, int maxEpochs = 100, int show = 10,
			IPerformanceFunction pFunc = null, TrainingModes tMode = TrainingModes.OffLine)
			: base(learnRate, minError, momentum, maxEpochs, show, pFunc, tMode)
		{}

		protected override IEnumerable<object> TrainOnLine(Network net, List<DenseMatrix> inputs, List<DenseMatrix> targets)
		{
			var sInputs = inputs.Select(x => (DenseMatrix)x.Clone()).ToList();
			var sTargets = targets.Select(x => (DenseMatrix)x.Clone()).ToList();
			for (int epoch = 0; epoch < MaxEpochs + 1; epoch++)
			{
				var error = FindError(net.Run(inputs), targets);
				var stackedMatrix = error.Aggregate((x, a) => (DenseMatrix)x.Stack(a)).Values;
				var totalError = stackedMatrix.Where(x => x != 0).Count();

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
					for (int i = 0; i < sInputs[0].ColumnCount; i++)
					{
						var inp = sInputs.Select(x => DenseMatrix.OfColumnVectors(x.Column(i))).ToList();
						var targ = sTargets.Select(x => DenseMatrix.OfColumnVectors(x.Column(i))).ToList();
						var err = error.Select(x => DenseMatrix.OfColumnVectors(x.Column(i))).ToList();
						UpdateWeights(net, err, inp);
					}
				}
			}
		}

		private void UpdateWeights(Network net, List<DenseMatrix> err, List<DenseMatrix> inputs)
		{
			for (int l = 0; l < net.OutputLayers.Count; l++)
			{
				var oLayer = net.OutputLayers[l];
				var lError = err[l];
				var prevLayers = new HashSet<string>(net.PreviousLayers(oLayer).Select(layer => layer.Id));
				for (int i = 0; i < inputs.Count; i++)
				{
					var iLayer = net.InputLayers[i];
					if (prevLayers.Contains(iLayer.Id))
					{
						var delta = LearnRate * lError * (DenseMatrix)inputs[i].Transpose();
						net.Edges[iLayer.Id][oLayer.Id] = net.Edges[iLayer.Id][oLayer.Id] + delta;
					}
				}
			}
		}

		private List<DenseMatrix> FindError(List<DenseMatrix> outputs, List<DenseMatrix> targets)
		{
			var numOfLayers = outputs.Count;
			var result = new List<DenseMatrix>(numOfLayers);
			for (int l = 0; l < numOfLayers; l++)
			{
				result.Add(DenseMatrix.Create(outputs[l].RowCount, outputs[l].ColumnCount, (r, c) => 0));
				for (int i = 0; i < outputs[l].RowCount; i++)
				{
					for (int j = 0; j < outputs[l].ColumnCount; j++)
					{
						if (outputs[l][i, j] == 1 && targets[l][i, j] == 0)
						{
							result[l][i, j] = -1;
						}
						else if (outputs[l][i, j] == 0 && targets[l][i, j] == 1)
						{
							result[l][i, j] = 1;
						}
					}
				}
			}
			return result;
		}

		protected override IEnumerable<object> TrainOffLine(Network net, List<DenseMatrix> inputs, List<DenseMatrix> targets)
		{
			var sInputs = inputs.Select(x => (DenseMatrix)x.Clone()).ToList();
			var sTargets = targets.Select(x => (DenseMatrix)x.Clone()).ToList();
			for (int epoch = 0; epoch < MaxEpochs + 1; epoch++)
			{
				var outputs = net.Run(inputs);
				var error = FindError(outputs, targets);
				var stackedMatrix = error.Aggregate((x, a) => (DenseMatrix)x.Stack(a)).Values;
				var totalError = stackedMatrix.Where(x => x != 0).Count();

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
					for (int i = 0; i < sInputs[0].ColumnCount; i++)
					{
						UpdateWeights(net, error, inputs);
					}
				}
			}
		}
	}
}
