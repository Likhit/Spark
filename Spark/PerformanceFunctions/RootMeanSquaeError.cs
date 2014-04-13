using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;


namespace Spark.PerformanceFunctions
{
	class RootMeanSquaeError : IPerformanceFunction
	{
		public DenseVector Apply(List<DenseMatrix> outputs, List<DenseMatrix> targets)
		{
			var mse = new MeanSquareError();
			var err = mse.Apply(outputs, targets);
			err.MapInplace(x => Math.Sqrt(x));
			return err;
		}

		public List<DenseMatrix> Differentiate(DenseVector x, List<DenseMatrix> outputs, List<DenseMatrix> targets)
		{
			var errors = new List<DenseMatrix>(outputs.Count);
			for (int i = 0; i < outputs.Count; i++)
			{
				var numerator = targets[i] - outputs[i];
				var lift = DenseMatrix.OfRowVectors(Enumerable.Repeat(x, numerator.RowCount).ToArray());
				errors.Add((DenseMatrix)numerator.PointwiseDivide(lift));
			}
			return errors;
		}
	}
}
