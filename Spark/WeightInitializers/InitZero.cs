using MathNet.Numerics.LinearAlgebra.Double;

namespace Spark.WeightInitializers
{
	public class InitZero : IWeightInitializer
	{
		public DenseMatrix Initialize(int rows, int cols)
		{
			return new DenseMatrix(rows, cols);
		}

		public DenseVector Initialize(int rows)
		{
			throw new DenseVector(rows);
		}
	}
}
