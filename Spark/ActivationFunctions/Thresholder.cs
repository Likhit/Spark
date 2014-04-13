using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Spark.ActivationFunctions
{
	public class Thresholder : IActivationFunction
	{
		private double threshold;

		private Func<double, double> thresholdFunc;

		public DenseMatrix Apply(DenseMatrix inputs)
		{
			return DenseMatrix.Create(inputs.RowCount, inputs.ColumnCount, (r, c) => thresholdFunc(inputs[r, c]));
		}

		public Thresholder(double threshold, bool bipolar = false)
		{
			this.threshold = threshold;
			if (bipolar)
			{
				thresholdFunc = BipolarThresholder;
			}
			else
			{
				thresholdFunc = SimpleThresholer;
			}
		}

		private double BipolarThresholder(double x)
		{
			if (x < threshold)
			{
				return -1;
			}
			else if (x == threshold)
			{
				return 0;
			}
			else
			{
				return 1;
			}
		}

		private double SimpleThresholer(double x)
		{
			if (x < threshold)
			{
				return 0;
			}
			else
			{
				return 1;
			}
		}

		public DenseMatrix Differentiate(DenseMatrix x, DenseMatrix inputs)
		{
			throw new NotImplementedException();
		}
	}
}
