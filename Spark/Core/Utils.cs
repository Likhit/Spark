using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using Spark.ActivationFunctions;
using Spark.WeightFunctions;
using Spark.InputFunctions;
using Spark.WeightInitializers;
using Spark.PerformanceFunctions;
using System.Linq;

namespace Spark.Core.Utils
{
	public static class Helpers
	{
		public static void InitializeNative()
		{
			try
			{
				MathNet.Numerics.Control.LinearAlgebraProvider =
					new MathNet.Numerics.Algorithms.LinearAlgebra.Mkl.MklLinearAlgebraProvider();
			}
			catch (Exception)
			{
				Console.WriteLine("Unable to load the native library. Using the normal dlls.");
			}
		}

		public static List<T> Wrap<T>(T x)
		{
			return new List<T>(1) { x };
		}

		public static DenseMatrix Load(string path, string delimiter, bool hasHeaders = false)
		{
			return (DenseMatrix)MathNet.Numerics.Data.Text.DelimitedReader
				.ReadFile<double>(path, delimiter: delimiter, hasHeaders: hasHeaders);
		}

		public static void Save(DenseMatrix matrix, string path, string delimiter)
		{
			MathNet.Numerics.Data.Text.DelimitedWriter.WriteFile(matrix, path, delimiter);
		}

		public static void Shuffle(List<DenseMatrix> inputs, List<DenseMatrix> targets)
		{
			var numOfInps = inputs[0].ColumnCount;
			var sequence = Enumerable.Range(0, numOfInps).ToArray();
			var rnd = new Random();
			for (int i = 0; i < numOfInps; ++i)
			{
				int r = rnd.Next(i, numOfInps);
				int tmp = sequence[r];
				sequence[r] = sequence[i];
				sequence[i] = tmp;
			}
			var permutation = new MathNet.Numerics.Permutation(sequence);
			foreach (var inp in inputs)
			{
				inp.PermuteColumns(permutation);
			}
			foreach (var targ in targets)
			{
				targ.PermuteColumns(permutation);
			}
		}

		public static DenseVector ToIndices(this DenseMatrix x)
		{
			var res = x.ColumnEnumerator().Select(y => (double)y.Item2.MaximumIndex());
			return DenseVector.OfEnumerable(res);
		}

		public static DenseMatrix ToClassMatrix(this DenseVector x, int? length = null)
		{
			int len = length.HasValue ? length.Value : (int)x.Maximum() + 1;
			var res = DenseMatrix.Create(x.Count, len, (_, __) => 0);
			for (int i = 0; i < x.Count; i++)
			{
				res[i, (int)x[i]] = 1.0;
			}
			return res;
		}

		public static int CountErrors(List<DenseMatrix> outputs, List<DenseMatrix> targets)
		{
			int errors = 0;
			for (int i = 0; i < outputs[0].ColumnCount; i++)
			{
				for (int j = 0; j < outputs.Count; j++)
				{
					if (outputs[j].Column(i) != targets[j].Column(i))
					{
						errors++;
					}
				}
			}
			return errors;
		}

		public static int CountErrors(DenseMatrix outputs, DenseMatrix targets)
		{
			int errors = 0;
			for (int i = 0; i < outputs.ColumnCount; i++)
			{
				for (int j = 0; j < outputs.RowCount; j++)
				{
					if (outputs[j, i] != targets[j, i])
					{
						errors++;
						break;
					}
				}
			}
			return errors;
		}

		public static IEnumerable<Tuple<int ,DenseVector, DenseVector>> GetErrors(DenseMatrix outputs, DenseMatrix targets)
		{
			for (int i = 0; i < outputs.ColumnCount; i++)
			{
				for (int j = 0; j < outputs.RowCount; j++)
				{
					if (outputs[j, i] != targets[j, i])
					{
						yield return new Tuple<int, DenseVector, DenseVector>(i,
							(DenseVector)outputs.Column(i), (DenseVector)targets.Column(i));
						break;
					}
				}
			}
		}

		public static int CountErrors(List<DenseVector> outputs, List<DenseVector> targets)
		{
			int errors = 0;
			for (int i = 0; i < outputs[0].Count; i++)
			{
				for (int j = 0; j < outputs.Count; j++)
				{
					if (outputs[j][i] != targets[j][i])
					{
						errors++;
					}
				}
			}
			return errors;
		}
	}

	public static class ActivationFunctions
	{
		public static IActivationFunction Linear = new Linear();

		public static IActivationFunction Sigmoid = new Sigmoid();

		public static IActivationFunction SoftMax = new SoftMax();

		public static IActivationFunction TanSigmoid = new TanSigmoid();

		public static IActivationFunction Inverse = new Inverse();

		public static IActivationFunction HardLim = new Thresholder(0);

		public static IActivationFunction HardLimBipolar = new Thresholder(0, true);
	}

	public static class WeightFunctions
	{
		public static IWeightFunction DotProduct = new DotProduct();
	}

	public static class InputFunctions
	{
		public static IInputFunction Sum = new Sum();

		public static IInputFunction Product = new Product();
	}

	public static class WeightInitializers
	{
		public static IWeightInitializer InitZero = new InitZero();

		public static IWeightInitializer RandSmall = new RandSymmetric(0.1);
	}

	public static class PerformanceFunctions
	{
		public static IPerformanceFunction MeanSquareError = new MeanSquareError();

		public static IPerformanceFunction RootMeanSquareError = new RootMeanSquaeError();
	}
}
