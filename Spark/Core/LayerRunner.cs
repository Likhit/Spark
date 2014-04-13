using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.Generic;
using System.Linq;

namespace Spark.Core
{
	public partial class Layer
	{
		private Dictionary<string, DenseMatrix> weightFuncOutputs = new Dictionary<string, DenseMatrix>();

		public Dictionary<string, DenseMatrix> WeightFuncOutputs
		{
			get
			{
				return weightFuncOutputs;
			}
		}

		private Dictionary<string, DenseMatrix> weightFuncDerivatives = new Dictionary<string, DenseMatrix>();

		public Dictionary<string, DenseMatrix> WeightFuncDerivatives
		{
			get
			{
				return weightFuncDerivatives;
			}
		}

		public DenseMatrix Feed(string from, DenseMatrix input, DenseMatrix weights, bool storeDerivative = false)
		{
			var output = WFunc.Apply(weights, input);
			weightFuncOutputs.Add(from, output);
			if (storeDerivative)
			{
				var diff = WFunc.Differentiate(output, weights, input);
				weightFuncDerivatives.Add(from, diff);
			}
			return output;
		}

		private DenseMatrix inputFuncOutput;

		public DenseMatrix InputFuncOutput
		{
			get
			{
				return inputFuncOutput;
			}
		}

		private Dictionary<string, DenseMatrix> inputFuncDerivative = new Dictionary<string, DenseMatrix>();

		public Dictionary<string, DenseMatrix> InputFuncDerivative
		{
			get
			{
				return inputFuncDerivative;
			}
		}

		public DenseMatrix Cumulate(bool storeDerivative = false)
		{
			var inps = weightFuncOutputs.Select(kvPair => kvPair.Value);
			inputFuncOutput = IFunc.Apply(inps);
			if (storeDerivative)
			{
				foreach (var kvPair in weightFuncOutputs)
				{
					inputFuncDerivative[kvPair.Key] = IFunc.Differentiate(inputFuncOutput, kvPair.Value);
				}
			}
			return inputFuncOutput;
		}

		private DenseMatrix output;

		public DenseMatrix Output
		{
			get
			{
				return output;
			}
		}

		private DenseMatrix activationFuncDerivative;

		public DenseMatrix ActivationFuncDerivative
		{
			get
			{
				return activationFuncDerivative;
			}
		}

		public DenseMatrix Fire(bool storeDerivative = false)
		{
			output = AFunc.Apply(inputFuncOutput);
			if (storeDerivative)
			{
				activationFuncDerivative = AFunc.Differentiate(output, inputFuncOutput);
			}
			if (this.biased)
			{
				output = DenseMatrix.OfColumnVectors(
					output.ColumnEnumerator().Select(tuple => tuple.Item2 + this.biases).ToArray());
			}
			return output;
		}

		public DenseMatrix FireWith(DenseMatrix input, bool storeDerivative = false)
		{
			inputFuncOutput = input;
			return Fire(storeDerivative);
		}

		public Layer Clean()
		{
			weightFuncOutputs = new Dictionary<string, DenseMatrix>();
			weightFuncDerivatives = new Dictionary<string, DenseMatrix>();
			inputFuncDerivative = new Dictionary<string, DenseMatrix>();
			return this;
		}
	}
}
