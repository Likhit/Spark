using System;
using Newtonsoft.Json;
using Spark.Core;

namespace Spark.Serializers
{
	class LayerSerializer : JsonConverter
	{
		[JsonObject(MemberSerialization = MemberSerialization.OptOut)]
		private class LayerMetaView
		{
			[JsonProperty]
			public string Id;

			[JsonProperty]
			public int Length;

			[JsonProperty]
			public bool Biased;

			[JsonProperty]
			public string AFunc;

			[JsonProperty]
			public string WFunc;

			[JsonProperty]
			public string IFunc;

			[JsonProperty]
			public string WInit;

			public LayerMetaView(Layer layer)
			{
				Id = layer.Id;
				Length = layer.Length;
				Biased = layer.Biased;
				AFunc = layer.AFunc.GetType().Name;
				WFunc = layer.WFunc.GetType().Name;
				IFunc = layer.IFunc.GetType().Name;
				WInit = layer.WInit.GetType().Name;
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var layer = value as Layer;
			serializer.Serialize(writer, new LayerMetaView(layer));
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(Layer).IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
