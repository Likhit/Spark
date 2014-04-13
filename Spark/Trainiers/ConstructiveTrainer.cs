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
		public ConstructiveTrainer(double learnRate = 0.05, double minError = 0.01,
			double momentum = 0.01, int maxEpochs = 100, int show = 10,
			IPerformanceFunction pFunc = null, TrainingModes tMode = TrainingModes.OffLine)
			: base(learnRate, minError, momentum, maxEpochs, show, pFunc, tMode)
		{ }

		protected override IEnumerable<object> TrainOnLine(Network net, List<DenseMatrix> inputs, List<DenseMatrix> targets)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<object> TrainOffLine(Network net, List<DenseMatrix> inputs, List<DenseMatrix> targets)
		{
			throw new NotImplementedException();
		}
	}
}
