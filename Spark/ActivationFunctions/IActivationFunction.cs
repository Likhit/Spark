using MathNet.Numerics.LinearAlgebra.Double;

namespace Spark.ActivationFunctions
{
	public interface IActivationFunction
	{
		DenseMatrix Apply(DenseMatrix inputs);

		DenseMatrix Differentiate(DenseMatrix x, DenseMatrix inputs);
	}
}
