using MathNet.Numerics.LinearAlgebra.Double;

namespace Spark.WeightFunctions
{
	public interface IWeightFunction
	{
		DenseMatrix Apply(DenseMatrix weights, DenseMatrix inputs);

		DenseMatrix Differentiate(DenseMatrix x, DenseMatrix weights, DenseMatrix inputs);
	}
}
