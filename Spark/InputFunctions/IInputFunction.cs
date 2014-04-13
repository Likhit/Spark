using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Spark.InputFunctions
{
	public interface IInputFunction
	{
		DenseMatrix Apply(IEnumerable<DenseMatrix> inputs);

		DenseMatrix Differentiate(DenseMatrix x, DenseMatrix input);
	}
}
