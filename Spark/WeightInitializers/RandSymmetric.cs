using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Spark.WeightInitializers
{
	public class RandSymmetric : IWeightInitializer
	{
		private double halfRange;

		public DenseMatrix Initialize(int rows, int cols)
		{
			var rand = new Random();
			return DenseMatrix.Create(rows, cols, (r, c) => 2.0 * rand.NextDouble() * halfRange - halfRange);
		}

		public RandSymmetric(double halfRange)
		{
			this.halfRange = halfRange;
		}

		public DenseVector Initialize(int rows)
		{
			var rand = new Random();
			return DenseVector.Create(rows, r => 2.0 * rand.NextDouble() * halfRange - halfRange);
		}
	}
}
