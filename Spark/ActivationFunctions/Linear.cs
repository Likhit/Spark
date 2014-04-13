using MathNet.Numerics.LinearAlgebra.Double;

namespace Spark.ActivationFunctions
{
	public class Linear : IActivationFunction
	{
		public DenseMatrix Apply(DenseMatrix inputs)
		{
			return inputs;
		}

		public DenseMatrix Differentiate(DenseMatrix x, DenseMatrix inputs)
		{
			return new DenseMatrix(x.RowCount, x.ColumnCount, 1);
		}
	}
}
