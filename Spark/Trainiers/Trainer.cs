using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using Spark.Core;
using Spark.PerformanceFunctions;
using System;
using System.Threading.Tasks;

namespace Spark.Trainiers
{
	public enum TrainingModes
	{
		OnLine,
		OffLine
	}

	public abstract class Trainer
	{
		private double learnRate;

		public double LearnRate
		{
			get
			{
				return learnRate;
			}
		}


		private double minError;

		public double MinError
		{
			get
			{
				return minError;
			}
		}


		private double momentum;

		public double Momentum
		{
			get
			{
				return momentum;
			}
		}


		private int maxEpochs;

		public int MaxEpochs
		{
			get
			{
				return maxEpochs;
			}
		}


		private int show;

		public int Show
		{
			get
			{
				return show;
			}
		}

		private TrainingModes tMode;

		public TrainingModes TMode
		{
			get
			{
				return tMode;
			}
		}

		private IPerformanceFunction pFunc = Core.Utils.PerformanceFunctions.MeanSquareError;

		public IPerformanceFunction PFunc
		{
			get
			{
				return pFunc;
			}
		}

		public Trainer(double learnRate = 0.05, double minError = 0.01,
			double momentum = 0.01, int maxEpochs = 100,
			int show = 10, IPerformanceFunction pFunc = null,
			TrainingModes tMode = TrainingModes.OffLine)
		{
			this.learnRate = learnRate;
			this.minError = minError;
			this.momentum = momentum;
			this.maxEpochs = maxEpochs;
			this.show = show;
			this.tMode = tMode;
			if (pFunc != null)
			{
				this.pFunc = pFunc;
			}
		}

		public virtual IEnumerable<object> Train(Network net, List<DenseMatrix> inputs, List<DenseMatrix> targets)
		{
			if (this.tMode == TrainingModes.OnLine)
			{
				return TrainOnLine(net, inputs, targets);
			}
			else
			{
				return TrainOffLine(net, inputs, targets);
			}
		}

		protected abstract IEnumerable<object> TrainOnLine(Network net, List<DenseMatrix> inputs, List<DenseMatrix> targets);

		protected abstract IEnumerable<object> TrainOffLine(Network net, List<DenseMatrix> inputs, List<DenseMatrix> targets);
	}
}
