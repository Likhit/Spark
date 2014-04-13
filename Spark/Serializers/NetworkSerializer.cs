using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Spark.Core;

namespace Spark.Serializers
{
	class NetworkSerializer: JsonConverter
	{
		[JsonObject(MemberSerialization = MemberSerialization.OptOut)]
		private class NetworkMetaView
		{
			[JsonProperty]
			public Dictionary<string, Layer> Layers;

			[JsonProperty]
			public List<Layer> InputLayers;

			[JsonProperty]
			public List<Layer> OutputLayers;

			[JsonProperty]
			public Dictionary<string, Dictionary<string, double[,]>> Edges;

			/// <summary>
			/// Create a NetworkMetalView from the given neural network.
			/// </summary>
			/// <param name="network">The neural network to convert.</param>
			public NetworkMetaView(Network network)
			{
				Layers = network.Layers;
				InputLayers = network.InputLayers;
				OutputLayers = network.OutputLayers;

				Edges = new Dictionary<string, Dictionary<string, double[,]>>();
				foreach (var from in network.Edges.Keys)
				{
					Edges.Add(from, new Dictionary<string, double[,]>());
					foreach (var to in network.Edges[from].Keys)
					{
						Edges[from][to] = network.Edges[from][to].ToArray();
					}
				}
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var network = value as Network;
			serializer.Serialize(writer, new NetworkMetaView(network));
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(Network).IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
