using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Spark.InputFunctions
{
	public class Sum : IInputFunction
	{
		public DenseMatrix Apply(IEnumerable<DenseMatrix> inputs)
		{
			return inputs.Aggregate((a, x) => a + x);
		}

		public DenseMatrix Differentiate(DenseMatrix x, DenseMatrix input)
		{
			return new DenseMatrix(x.RowCount, x.ColumnCount, 1);
		}
	}
}
