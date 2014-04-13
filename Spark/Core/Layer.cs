using Spark.ActivationFunctions;
using Spark.InputFunctions;
using Spark.WeightFunctions;
using Spark.WeightInitializers;
using Newtonsoft.Json;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Spark.Core
{
	[JsonObject(MemberSerialization.OptIn)]
	[JsonConverter(typeof(Spark.Serializers.LayerSerializer))]
    public partial class Layer
    {
		private readonly string id;
		
		public string Id
		{
			get
			{
				return id;
			}
		}

		
		private readonly int length;

		public int Length
		{
			get
			{
				return length;
			}
		}

		
		private readonly bool biased;

		public bool Biased
		{
			get
			{
				return biased;
			}
		}


		private IActivationFunction aFunc = new Linear();

		public IActivationFunction AFunc
		{
			get
			{
				return aFunc;
			}
		}


		private IWeightFunction wFunc = new DotProduct();

		public IWeightFunction WFunc
		{
			get
			{
				return wFunc;
			}
		}


		private IInputFunction iFunc = new Sum();

		public IInputFunction IFunc
		{
			get
			{
				return iFunc;
			}
		}


		private IWeightInitializer wInit = new InitZero();

		public IWeightInitializer WInit
		{
			get
			{
				return wInit;
			}
		}

		private DenseVector biases;

		public DenseVector Biases
		{
			get
			{
				if (this.biased)
				{
					return biases;
				}
				else
				{
					throw new System.Exception("This network is unbiased!");
				}
			}
		}

		private Layer(string id, int length = 1, bool biased = true, 
			IActivationFunction aFunc = null, IWeightFunction wFunc = null,
			IInputFunction iFunc = null, IWeightInitializer wInit = null)
		{
			this.id = id;
			this.length = length;
			this.biased = biased;
			if (aFunc != null)
			{
				this.aFunc = aFunc;
			}
			if (wFunc != null)
			{
				this.wFunc = wFunc;
			}
			if (iFunc != null)
			{
				this.iFunc = iFunc;
			}
			if (wInit != null)
			{
				this.wInit = wInit;
			}
			if (biased)
			{
				this.biases = this.wInit != null ?
					this.wInit.Initialize(this.length) :
					Utils.WeightInitializers.InitZero.Initialize(this.length);
			}
		}

		public static Layer Create(string id, int length = 1, bool biased = true,
			IActivationFunction aFunc = null, IWeightFunction wFunc = null,
			IInputFunction iFunc = null, IWeightInitializer wInit = null)
		{
			return new Layer(id, length, biased, aFunc, wFunc, iFunc, wInit);
		}
    }
}
