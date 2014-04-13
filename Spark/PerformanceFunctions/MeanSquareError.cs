using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Spark.PerformanceFunctions
{
	public class MeanSquareError : IPerformanceFunction
	{
		public DenseVector Apply(List<DenseMatrix> outputs, List<DenseMatrix> targets)
		{
			var errors = new List<DenseVector>(outputs.Count);
			int features = 0;
			for (int i = 0; i < outputs.Count; i++)
			{
				var diff = targets[i] - outputs[i];
				var square = diff.PointwiseMultiply(diff);
				var sum = DenseVector.OfEnumerable(square.ColumnEnumerator().Select(x => x.Item2.Sum()));
				errors.Add(sum);
				features += targets[i].RowCount;
			}
			var mean = errors.Aggregate((a, x) => a + x) * (1.0 / features);
			return mean;
		}

		public List<DenseMatrix> Differentiate(DenseVector x, List<DenseMatrix> outputs, List<DenseMatrix> targets)
		{
			var numOfInps = outputs[0].ColumnCount;
			var coff = 2.0 / numOfInps;
			var errors = new List<DenseMatrix>(outputs.Count);
			for (int i = 0; i < outputs.Count; i++)
			{
				errors.Add((targets[i] - outputs[i]) * coff);
			}
			return errors;
		}
	}
}
