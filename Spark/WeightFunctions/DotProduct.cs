using MathNet.Numerics.LinearAlgebra.Double;

namespace Spark.WeightFunctions
{
	public class DotProduct : IWeightFunction
	{
		public DenseMatrix Apply(DenseMatrix weights, DenseMatrix inputs)
		{
			return weights * inputs;
		}

		public DenseMatrix Differentiate(DenseMatrix x, DenseMatrix weights, DenseMatrix inputs)
		{
			return (DenseMatrix)weights.Transpose();
		}
	}
}
