using MathNet.Numerics.LinearAlgebra.Double;

namespace Spark.WeightInitializers
{
	public interface IWeightInitializer
	{
		DenseMatrix Initialize(int rows, int cols);

		DenseVector Initialize(int rows);
	}
}
