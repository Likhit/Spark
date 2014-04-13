using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Spark.ActivationFunctions
{
	public class SoftMax : IActivationFunction
	{
		public DenseMatrix Apply(DenseMatrix inputs)
		{
			var maximas = inputs.ColumnEnumerator().Select(tuple => tuple.Item2.Maximum());
			var result = DenseMatrix.Create(inputs.RowCount, inputs.ColumnCount, 
				(r, c) => Math.Exp(inputs[r, c] - maximas.ElementAt(c)));
			var scale = result.ColumnEnumerator().Select(tuple => tuple.Item2.Sum());
			result.MapIndexedInplace((r, c, x) => x / scale.ElementAt(c));
			return result;
		}

		public DenseMatrix Differentiate(DenseMatrix x, DenseMatrix inputs)
		{
			return DenseMatrix.Create(x.RowCount, x.ColumnCount, (r, c) => x[r, c] * (1.0 - x[r, c]));
		}
	}
}
