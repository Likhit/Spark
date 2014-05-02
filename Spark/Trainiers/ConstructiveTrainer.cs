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
	public class ConstructiveTrainer : Trainer
	{
		private PerceptronTrainer subTrainer;

		private int maxHiddenLayers;

		public ConstructiveTrainer(double learnRate = 0.05, double minError = 0.01,
			double momentum = 0.01, int maxEpochs = 100, int show = 10, int maxHiddenLayers = 2,
			IPerformanceFunction pFunc = null, TrainingModes tMode = TrainingModes.OffLine)
			:base(learnRate, minError, momentum, maxEpochs, show, pFunc, tMode)
		{
			this.maxHiddenLayers = maxHiddenLayers;
			subTrainer = new PerceptronTrainer(learnRate, minError, momentum, maxEpochs, show, pFunc, tMode);
		}

		public override IEnumerable<object> Train(Network net, List<DenseMatrix> inputs, List<DenseMatrix> targets)
		{
			return TrainOnLine(net, inputs, targets);
		}

		protected override IEnumerable<object> TrainOnLine(Network net, List<DenseMatrix> inputs, List<DenseMatrix> targets)
		{
			var errors = subTrainer.Train(net, inputs, targets);
			dynamic lastError = null;
			foreach (dynamic error in errors)
			{
				lastError = error;
				yield return error;
			}
			if (lastError.error > subTrainer.MinError)
			{
				for (int hLayerCount = 0; hLayerCount < maxHiddenLayers; hLayerCount++)
				{
					var outputs = net.Run(inputs[0])[0];
					var newTrainingData = NewTrainingData(outputs, targets[0]);
					if (newTrainingData.Item2.Count == 0)
					{
						break;
					}
					yield return new
					{
						hiddenLayerCount = hLayerCount,
						mismatches = newTrainingData.Item2.Count
					};
					DenseMatrix ancillaryOutput;
					var ancillaryNet = CreateAncillaryNet(net, hLayerCount, newTrainingData.Item1,
						newTrainingData.Item2, targets[0], out ancillaryOutput);
					var auxillaryNet = CreateAuxillaryNet(net, hLayerCount, ancillaryNet,
						ancillaryOutput, outputs, targets[0]);
					net.Absorb(ancillaryNet).Absorb(auxillaryNet, overrideOutputs: true);
				}
			}
		}

		private Network CreateAuxillaryNet(Network net, int hLayerCount, Network ancillaryNet,
			DenseMatrix ancillaryOutputs, DenseMatrix originalOutputs, DenseMatrix targets)
		{
			var auxillaryLayer = Layer.Create("$O" + hLayerCount, net.OutputLayers[0].Length,
				aFunc: net.OutputLayers[0].AFunc, wInit: net.OutputLayers[0].WInit);
			var auxillaryNet = Network
				.WithInputLayers(net.OutputLayers.Concat(ancillaryNet.OutputLayers).ToList())
				.ConnectTo(auxillaryLayer);
			var newInputs = new List<DenseMatrix> () { originalOutputs, ancillaryOutputs };
			Console.WriteLine("Auxilary");
			subTrainer.Train(auxillaryNet, newInputs, Core.Utils.Helpers.Wrap(targets))
				.Select(x => { Console.WriteLine(x); return x; }).ToList();
			return auxillaryNet;
		}

		private Network CreateAncillaryNet(Network net, int hLayerCount, DenseVector selectedClasses,
			List<int> trainingDataIndex, DenseMatrix targets, out DenseMatrix outputs)
		{
			var selectedClassesSet = new HashSet<int>();
			for (int i = 0; i < selectedClasses.Count; i++)
			{
				if (selectedClasses[i] > 0)
				{
					selectedClassesSet.Add(i);
				}
			}
			
			var numOfNeurons = selectedClassesSet.Count;
			var ancillaryLayer = Layer.Create("$H" + hLayerCount, length: numOfNeurons,
				aFunc: net.OutputLayers[0].AFunc, wInit: Core.Utils.WeightInitializers.RandSmall);

			var prevLayers = net.OutputLayers.SelectMany(x => net.PreviousLayers(x)).ToList();
			var allInps = prevLayers.Select(l => l.Output).ToList();
			List<DenseMatrix> trainingInps = new List<DenseMatrix>();
			foreach (var layer in prevLayers)
			{
				trainingInps.Add(
					DenseMatrix.OfColumnVectors(trainingDataIndex.Select(i => layer.Output.Column(i)).ToArray()));
			}
			var trainingTargs = DenseMatrix.OfRowVectors(
				DenseMatrix.OfColumnVectors(trainingDataIndex.Select(i => targets.Column(i)).ToArray())
				.RowEnumerator().Where(tuple => selectedClassesSet.Contains(tuple.Item1)).Select(t => t.Item2)
				.ToArray());

			var ancillaryNet = Network.WithInputLayers(prevLayers).ConnectTo(ancillaryLayer);
			subTrainer.Train(ancillaryNet, trainingInps,
				Core.Utils.Helpers.Wrap(trainingTargs)).Select(x => { Console.WriteLine(x); return x; }).ToList();

			outputs = ancillaryNet.Run(allInps)[0];
			return ancillaryNet;
		}

		private Tuple<DenseVector, List<int>> NewTrainingData(DenseMatrix outputs, DenseMatrix targets)
		{
			var mismatches = GetErrors(outputs, targets).ToList();
			var classMismatchCount = DenseVector.Create(targets.RowCount, x => 0);
			foreach (var tuple in mismatches)
			{
				var classIndex = tuple.Item2.MaximumIndex();
				classMismatchCount[classIndex]++;
			}
			
			var maxMismatchClass = classMismatchCount.MaximumIndex();
			var newTrainingSetIndices = new List<int>();
			var targetClassRepresentedInMismatches = DenseVector.Create(targets.RowCount, x => 0);
			foreach (var tuple in mismatches)
			{
				if (tuple.Item2.MaximumIndex() == maxMismatchClass)
				{
					newTrainingSetIndices.Add(tuple.Item1);
					targetClassRepresentedInMismatches += tuple.Item3;
				}
			}

			return new Tuple<DenseVector, List<int>>(targetClassRepresentedInMismatches, newTrainingSetIndices);
		}

		private IEnumerable<Tuple<int, DenseVector, DenseVector>> GetErrors(DenseMatrix outputs, DenseMatrix targets)
		{
			for (int i = 0; i < outputs.ColumnCount; i++)
			{
				var oCol = outputs.Column(i);
				var tCol = targets.Column(i);
				if (oCol.MaximumIndex() != tCol.MaximumIndex())
				{
					yield return new Tuple<int, DenseVector, DenseVector>(i,
							(DenseVector)oCol, (DenseVector)tCol);
				}
			}
		}

		protected override IEnumerable<object> TrainOffLine(Network net, List<DenseMatrix> inputs, List<DenseMatrix> targets)
		{
			throw new NotImplementedException();
		}
	}
}
