using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using Newtonsoft.Json;

namespace Spark.Core
{
	[JsonObject(MemberSerialization.OptIn)]
	[JsonConverter(typeof(Spark.Serializers.NetworkSerializer))]
	public partial class Network
	{
		private Dictionary<string, Layer> layers;

		public Dictionary<string, Layer> Layers
		{
			get
			{
				return layers;
			}
		}


		private List<Layer> inputLayers;

		public List<Layer> InputLayers
		{
			get
			{
				return inputLayers;
			}
		}


		private List<Layer> outputLayers;

		public List<Layer> OutputLayers
		{
			get
			{
				return outputLayers;
			}
		}


		private Dictionary<string, Dictionary<string, DenseMatrix>> edges;

		public Dictionary<string, Dictionary<string, DenseMatrix>> Edges
		{
			get
			{
				return edges;
			}
		}

		private Network()
		{
			layers = new Dictionary<string, Layer>();
			inputLayers = new List<Layer>();
			outputLayers = new List<Layer>();
			edges = new Dictionary<string, Dictionary<string, DenseMatrix>>();
		}

		public static Network WithInputLayers(List<Layer> inpLayers)
		{
			var result = new Network();
			result.AddLayers(inpLayers);
			result.inputLayers = inpLayers;
			result.outputLayers = inpLayers;
			return result;
		}

		public static Network WithInputLayer(Layer inpLayer)
		{
			var wrap = new List<Layer>(1) { inpLayer };
			return Network.WithInputLayers(wrap);
		}

		public List<Layer> PreviousLayers(Layer layer)
		{
			var result = new List<Layer>();
			foreach (var kvPair in edges)
			{
				if (kvPair.Value.ContainsKey(layer.Id))
				{
					result.Add(layers[kvPair.Key]);
				}
			}
			return result;
		}
	}
}
