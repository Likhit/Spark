using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Spark.ActivationFunctions
{
	public class TanSigmoid : IActivationFunction
	{
		public DenseMatrix Apply(DenseMatrix inputs)
		{
			return DenseMatrix.Create(inputs.RowCount, inputs.ColumnCount, (r, c) => Math.Tanh(inputs[r, c]));
		}

		public DenseMatrix Differentiate(DenseMatrix x, DenseMatrix inputs)
		{
			return DenseMatrix.Create(x.RowCount, x.ColumnCount, (r, c) => 1 - x[r, c] * x[r, c]);
		}
	}
}
