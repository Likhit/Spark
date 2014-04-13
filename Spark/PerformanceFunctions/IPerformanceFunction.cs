using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Spark.PerformanceFunctions
{
	public interface IPerformanceFunction
	{
		DenseVector Apply(List<DenseMatrix> outputs, List<DenseMatrix> targets);

		List<DenseMatrix> Differentiate(DenseVector x, List<DenseMatrix> outputs, List<DenseMatrix> targets);
	}
}
