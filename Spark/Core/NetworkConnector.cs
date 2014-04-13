using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spark.Core
{
	public partial class Network
	{
		public Network ConnectTo(List<Layer> outLayers)
		{
			this.AddLayers(outLayers);
			foreach (var fromLayer in this.outputLayers)
			{
				foreach (var toLayer in outLayers)
				{
					this.AddEdge(fromLayer, toLayer);
				}
			}
			this.outputLayers = outLayers;
			return this;
		}

		public Network ConnectTo(Layer outLayer)
		{
			var wrap = new List<Layer>(1) { outLayer };
			return this.ConnectTo(wrap);
		}

		public Network ConnectTo(Network net)
		{
			this.AddLayers(net.Layers.Select(kvPair => kvPair.Value));
			this.ConnectTo(net.InputLayers);
			this.AddEdges(net.Edges);
			this.outputLayers = net.outputLayers;
			return this;
		}

		public Network AppendTo(Network net)
		{
			return net.ConnectTo(this);
		}

		private void AddLayers(IEnumerable<Layer> layers)
		{
			foreach (var layer in layers)
			{
				if (this.layers.ContainsKey(layer.Id))
				{
					if (this.layers[layer.Id] != layer)
					{
						throw new Exception("Two different layers with same id in same network not allowed.");
					}
				}
				else
				{
					this.layers.Add(layer.Id, layer);
				}
			}
		}

		private void AddEdge(Layer from, Layer to, DenseMatrix matrix = null)
		{
			if (!this.edges.ContainsKey(from.Id))
			{
				this.edges.Add(from.Id, new Dictionary<string, DenseMatrix>());
			}

			if (matrix == null)
			{
				matrix = to.WInit.Initialize(to.Length, from.Length);
			}

			this.edges[from.Id][to.Id] = matrix;
		}

		private void AddEdges(Dictionary<string, Dictionary<string, DenseMatrix>> newEdges)
		{
			foreach (var from in newEdges.Keys)
			{
				foreach (var to in newEdges[from].Keys)
				{
					this.AddEdge(this.layers[from], this.layers[to], newEdges[from][to]);
				}
			}
		}
	}
}
