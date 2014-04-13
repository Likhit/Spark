using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Spark.InputFunctions
{
	public class Product : IInputFunction
	{
		public DenseMatrix Apply(IEnumerable<DenseMatrix> inputs)
		{
			return inputs.Aggregate((a, x) => (DenseMatrix)a.PointwiseMultiply(x));
		}

		public DenseMatrix Differentiate(DenseMatrix x, DenseMatrix input)
		{
			 return (DenseMatrix)x.PointwiseDivide(input);
		}
	}
}
