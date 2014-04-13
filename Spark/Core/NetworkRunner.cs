using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spark.Core
{
	public partial class Network
	{
		public List<DenseMatrix> Run(List<DenseMatrix> inputs, bool storeDerivatives = false)
		{
			Clean();

			if (this.inputLayers.Count != inputs.Count())
			{
				throw new Exception("Mismatched number of inputs!");
			}

			for (int i = 0; i < this.inputLayers.Count; i++)
			{
				this.inputLayers[i].FireWith(inputs[i], storeDerivatives);
			}

			var currentLayers = new HashSet<Layer>(this.inputLayers);
			while (currentLayers.Count > 0)
			{
				var nextLayers = new HashSet<Layer>();
				foreach (var cLayer in currentLayers)
				{
					if (this.edges.ContainsKey(cLayer.Id))
					{
						var nxtLys = this.edges[cLayer.Id];
						foreach (var kvPair in nxtLys)
						{
							var to = this.layers[kvPair.Key];
							var weightMatrix = kvPair.Value;
							to.Feed(cLayer.Id, cLayer.Output, weightMatrix, storeDerivatives);
							nextLayers.Add(to);
						}
					}
				}

				foreach (var layer in nextLayers)
				{
					layer.Cumulate(storeDerivatives);
					layer.Fire(storeDerivatives);
				}

				currentLayers = nextLayers;
			}

			return this.outputLayers.Select(l => l.Output).ToList();
		}

		public List<DenseMatrix> Run(DenseMatrix input)
		{
			var wrap = new List<DenseMatrix>() { input };
			return Run(wrap);
		}

		private void Clean()
		{
			foreach (var layerId in Layers.Keys)
			{
				Layers[layerId].Clean();
			}
		}
	}
}
