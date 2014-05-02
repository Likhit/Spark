using System;
using System.Collections.Generic;
using System.Linq;
using Irony.Parsing;
using Spark.Core;
using Spark.Core.Utils;
using Spark.ActivationFunctions;
using Spark.InputFunctions;
using Spark.WeightFunctions;
using Spark.WeightInitializers;
using Spark.PerformanceFunctions;
using Spark.Trainiers;
using MathNet.Numerics.LinearAlgebra.Double;
using System.IO;

namespace DSL
{
	public class Interpretter
	{
		private Parser parser;

		private Action<object> printer;

		public Action<object> Printer
		{
			get
			{
				return printer;
			}
		}

		private Dictionary<string, Tuple<Type, object>> state;

		public Dictionary<string, Tuple<Type, object>> State
		{
			get
			{
				return state;
			}
		}

		public Interpretter(Action<object> printer, bool useNative = false)
		{
			parser = new Parser(new SparkGrammar());
			state = new Dictionary<string, Tuple<Type, object>>();
			this.printer = printer;
			if (useNative)
			{
				Helpers.InitializeNative();
			}
		}

		public void Evaluate(string sourceCode)
		{
			Printer(new { msg = "Start" });
			try
			{
				var root = parser.Parse(sourceCode).Root;
				Dispatch(root);
				Printer(new { msg = "End" });
			}
			catch (NullReferenceException)
			{
				Printer(new { msg = "Error", data = "Syntax error in code." });
			}
			catch (Exception ex)
			{
				Printer(new { msg = "Error", data = ex.Message });
			}
		}

		private void Dispatch(ParseTreeNode node)
		{
			switch (node.ToString())
			{
				case "Program":
					EvaluateChildren(node);
					break;
				case "Statement":
					EvaluateChildren(node);
					break;
				case "AssignStatement":
					EvaluateAssignStatement(node);
					break;
				case "CommandStatement":
					EvaluateCommandStatement(node);
					break;
			}
		}

		private void EvaluateChildren(ParseTreeNode node)
		{
			foreach (var child in node.ChildNodes)
			{
				Dispatch(child);
			}
		}

		private void EvaluateCommandStatement(ParseTreeNode node)
		{
			var child = node.ChildNodes[0];
			switch (child.ToString())
			{
				case "TrainStatement":
					EvaluateTrainStatement(child);
					break;
				case "SaveStatement":
					EvaluateSaveStatement(child);
					Printer(new { msg = "Saved" });
					break;
				case "PrintStatement":
					EvaluatePrintStatement(child);
					break;
			}
		}

		private void EvaluatePrintStatement(ParseTreeNode node)
		{
			Tuple<Type, object> objInfo;
			var key = node.ChildNodes[0].Token.Text;
			if (State.ContainsKey(key))
			{
				objInfo = State[key];
			}
			else
			{
				throw new Exception(string.Format("Variable {0} not defined!", key));
			}
			var objType = objInfo.Item1;
			if (objType == typeof(DenseMatrix))
			{
				var matrix = ((DenseMatrix)objInfo.Item2).Transpose();
				var headers = GetMatrixHeaders(key);
				Printer(new
				{
					msg = "Matrix",
					data = new
					{
						matrix = matrix.ToArray(),
						headers = headers
					}
				});
			}
			else if (objType == typeof(Network))
			{
				Printer(new { msg = "Network", data = objInfo.Item2 });
			}
			else if (objType == typeof(Layer))
			{
				Printer(new { msg = "Layer", data = objInfo.Item2 });
			}
			else if (objType == typeof(Trainer))
			{
				Printer(new { msg = "Trainer", data = objInfo.Item2 });
			}
			else if (objType == typeof(int))
			{
				Printer(new { msg = "Mismatches count", data = objInfo.Item2 });
			}
			else if (objType == typeof(IEnumerable<Tuple<int, DenseVector, DenseVector>>))
			{
				var mismatches = (IEnumerable<Tuple<int, DenseVector, DenseVector>>)objInfo.Item2;
				var headers = GetMatrixHeaders(key);
				Printer(new
				{
					msg = "Mismatches",
					data = new
					{
						mismatches = mismatches.ToList(),
						headers = headers
					}
				});
			}
			else
			{
				Printer(new { msg = "News", data = objInfo.Item2 });
			}
		}

		private void EvaluateSaveStatement(ParseTreeNode child)
		{
			var toSave = (DenseMatrix)GetFromState(child.ChildNodes[0].Token.Text, typeof(DenseMatrix));
			var fileType = EvaluateSupportedFileType(child.ChildNodes[1]);
			int savePathIndex = 2;
			string delimiter;
			switch (fileType)
			{
				case "csv":
					delimiter = @",";
					break;
				case "tsv":
					delimiter = @"\t";
					break;
				case "ssv":
					delimiter = @";";
					break;
				default:
					delimiter = @"\s";
					savePathIndex = 1;
					break;
			}
			var savePath = (string)child.ChildNodes[savePathIndex].Token.Value;
			Helpers.Save(toSave, savePath, delimiter);
		}

		private void EvaluateTrainStatement(ParseTreeNode node)
		{
			var network = (Network)GetFromState(node.ChildNodes[0].Token.Text, typeof(Network));
			var trainer = (Trainer)GetFromState(node.ChildNodes[1].Token.Text, typeof(Trainer));
			var inputs = (DenseMatrix)GetFromState(node.ChildNodes[2].Token.Text, typeof(DenseMatrix));
			var targets = (DenseMatrix)GetFromState(node.ChildNodes[3].Token.Text, typeof(DenseMatrix));
			var meta = trainer.Train(network, Helpers.Wrap(inputs), Helpers.Wrap(targets));

			var firstData = meta.First();
			Printer(new
			{
				msg = "Starting Training",
				data = new
				{
					maxEpochs = trainer.MaxEpochs,
					minError = trainer.MinError,
					firstError = firstData.GetType().GetProperty("error").GetValue(firstData)
				}
			});
			foreach (var obj in meta.Skip(1))
			{
				Printer(new { msg = "Training", data = obj });
			}
		}

		private object GetFromState(string identifier, Type type)
		{
			if (State.ContainsKey(identifier))
			{
				if (State[identifier].Item1 == type)
				{
					return State[identifier].Item2;
				}
				else
				{
					throw new Exception(string.Format("Type mismatch! Expecting a {0}, got a {1}.",
						type, State[identifier].Item1));
				}
			}
			else
			{
				throw new Exception(string.Format("Variable {0} not defined!", identifier));
			}
		}

		private void EvaluateAssignStatement(ParseTreeNode node)
		{
			var identifier = node.ChildNodes[0].Token.Text;
			var value = EvaluateAssignRHS(node.ChildNodes[1], identifier);
			State[identifier] = value;
		}

		private Tuple<Type, object> EvaluateAssignRHS(ParseTreeNode node, string identifier)
		{
			var child = node.ChildNodes[0];
			switch (child.ToString())
			{
				case "CreateLayer":
					return EvaluateCreateLayer(child);
				case "CreateNetwork":
					return EvaluateCreateNetwork(child);
				case "CreateTrainer":
					return EvaluateCreateTrainer(child);
				case "RunNetwork":
					return EvaluateRunNetwork(child, identifier);
				case "LoadFile":
					return EvaluateLoadFile(child, identifier);
				case "ConvertData":
					return EvaluateConvertData(child);
				case "FindError":
					return EvaluateFindError(child, identifier);
			}
			return null;
		}

		private Tuple<Type, object> EvaluateFindError(ParseTreeNode node, string identifier)
		{
			var child = node.ChildNodes[0];
			var id1 = node.ChildNodes[1].Token.Text;
			var id2 = node.ChildNodes[2].Token.Text;
			var arg1 = (DenseMatrix)GetFromState(id1, typeof(DenseMatrix));
			var arg2 = (DenseMatrix)GetFromState(id2, typeof(DenseMatrix));
			if (child.Token.Text == "count")
			{
				var result = Helpers.CountErrors(arg1, arg2);
				return new Tuple<Type, object>(typeof(int), result);
			}
			else if (child.Token.Text == "get")
			{
				var result = Helpers.GetErrors(arg1, arg2);
				var headers = new string[] { "#", id1, id2 };
				SaveMatrixHeaders(identifier, headers);
				return new Tuple<Type, object>(typeof(IEnumerable<Tuple<int, DenseVector, DenseVector>>), result);
			}
			else
			{
				return null;
			}
		}

		private Tuple<Type, object> EvaluateConvertData(ParseTreeNode node)
		{
			var child = node.ChildNodes[0];
			if (child.ToString() == "ConvertToVector")
			{
				var matrix = (DenseMatrix)GetFromState(child.ChildNodes[0].Token.Text, typeof(DenseMatrix));
				var vector = (DenseVector)matrix.Column(0);
				var size = child.ChildNodes.Count > 1 ? (int?)child.ChildNodes[1].Token.Value : null;
				var result = vector.ToClassMatrix(size);
				return new Tuple<Type, object>(typeof(DenseMatrix), result);
			}
			else if (child.ToString() == "ConvertToIndices")
			{
				var matrix = (DenseMatrix)GetFromState(child.ChildNodes[0].Token.Text, typeof(DenseMatrix));
				var result = matrix.ToIndices().ToRowMatrix();
				return new Tuple<Type, object>(typeof(DenseMatrix), result);
			}
			else
			{
				return null;
			}
		}

		private Tuple<Type, object> EvaluateRunNetwork(ParseTreeNode child, string identifier)
		{
			var network = (Network)GetFromState(child.ChildNodes[0].Token.Text, typeof(Network));
			var inputs = (DenseMatrix)GetFromState(child.ChildNodes[1].Token.Text, typeof(DenseMatrix));
			var result = network.Run(inputs)[0];
			var headers = Enumerable.Range(0, result.RowCount).Select(i => "Column " + i).ToArray();
			SaveMatrixHeaders(identifier, headers);
			return new Tuple<Type, object>(typeof(DenseMatrix), result);
		}

		private Tuple<Type, object> EvaluateLoadFile(ParseTreeNode node, string identifier)
		{
			var fileType = EvaluateSupportedFileType(node.ChildNodes[0]);
			var filePathIndex = 1;
			string delimiter;
			switch (fileType)
			{
				case "csv":
					delimiter = @",";
					break;
				case "tsv":
					delimiter = @"\t";
					break;
				case "ssv":
					delimiter = @";";
					break;
				default:
					delimiter = @"\s";
					filePathIndex = 0;
					break;
			}


			var matrixProps = EvaluateMatrixLoadType(node.ChildNodes[filePathIndex + 1]);
			var filePath = (string)node.ChildNodes[filePathIndex].Token.Value;
			var fileContents = Helpers.Load(filePath, delimiter, matrixProps.Item2);
			var fileHeaders = GetFileHeaders(filePath, delimiter, matrixProps.Item2);
			SaveMatrixHeaders(identifier, fileHeaders);
			return new Tuple<Type, object>(typeof(DenseMatrix), matrixProps.Item1 ? fileContents.Transpose() : fileContents);
		}

		private void SaveMatrixHeaders(string identifier, string[] fileHeaders)
		{
			State["$" + identifier] = new Tuple<Type, object>(typeof(string[]), fileHeaders);
		}

		private string[] GetMatrixHeaders(string identifier)
		{
			var key = "$" + identifier;
			try
			{
				return (string[])GetFromState(key, typeof(string[]));
			}
			catch (Exception)
			{
				return new string[0] {};
			}
		}

		private string[] GetFileHeaders(string filePath, string delimiter, bool hasHeaders)
		{
			using (var reader = new StreamReader(filePath))
			{
				var headerLine = reader.ReadLine();
				var firstRow = headerLine.Split(delimiter.ToArray());
				return hasHeaders ?
					firstRow :
					Enumerable.Range(0, firstRow.Count()).Select(i => string.Format("Column {0}", i)).ToArray();
					
			}
		}

		private Tuple<bool, bool> EvaluateMatrixLoadType(ParseTreeNode node)
		{
			var isRowMajor = (string)node.ChildNodes[0].Token.Value == "row";
			var hasHeaders = isRowMajor && node.ChildNodes.Count > 1;
			return new Tuple<bool, bool>(isRowMajor, hasHeaders);
		}

		private string EvaluateSupportedFileType(ParseTreeNode node)
		{
			return node.ToString() == "SupportedFileTypes" ?
				node.ChildNodes[0].Token.Text : "";			
		}

		private Tuple<Type, object> EvaluateCreateTrainer(ParseTreeNode node)
		{
			var trainerClassName = node.ChildNodes[0];
			var trainerParams = EvaluateKeyValuePair(node.ChildNodes[1]);

			double learnRate = 0.05;
			double minError = 0.01;
			int maxEpochs = 100;
			int maxHiddenLayers = 2;
			int show = 10;
			IPerformanceFunction pFunc = null;
			TrainingModes tMode = TrainingModes.OnLine;

			if (trainerParams.ContainsKey("learnRate"))
			{
				learnRate = double.Parse(trainerParams["learnRate"]);
			}
			if (trainerParams.ContainsKey("minError"))
			{
				minError = double.Parse(trainerParams["minError"]);
			}
			if (trainerParams.ContainsKey("maxEpochs"))
			{
				maxEpochs = int.Parse(trainerParams["maxEpochs"]);
			}
			if (trainerParams.ContainsKey("maxHiddenLayers"))
			{
				maxHiddenLayers = int.Parse(trainerParams["maxHiddenLayers"]);
			}
			if (trainerParams.ContainsKey("show"))
			{
				show = int.Parse(trainerParams["show"]);
			}
			if (trainerParams.ContainsKey("performanceFunction"))
			{
				pFunc = (IPerformanceFunction)(typeof(PerformanceFunctions)
					.GetField(trainerParams["performanceFunction"])
					.GetValue(null));
			}
			if (trainerParams.ContainsKey("mode"))
			{
				tMode = (TrainingModes)Enum.Parse(typeof(TrainingModes), trainerParams["mode"]);
			}

			Trainer trainer = null;
			switch (trainerClassName.Token.Text)
			{
				case "BackPropogationTrainer":
					trainer = new BackPropogationTrainer(learnRate, minError, 0.01, maxEpochs, show, pFunc, tMode);
					break;
				case "PerceptronTrainer":
					trainer = new PerceptronTrainer(learnRate, minError, 0.01, maxEpochs, show, pFunc, tMode);
					break;
				case "ConstructiveTrainer":
					trainer = new ConstructiveTrainer(learnRate, minError, 0.01, maxEpochs, show, maxHiddenLayers,
						pFunc, tMode);
					break;
				default:
					throw new Exception("Trainer of kind " + trainerClassName.Token.Text + " does not exist.");
			}
			return new Tuple<Type, object>(typeof(Trainer), trainer);
		}

		private Tuple<Type, object> EvaluateCreateNetwork(ParseTreeNode node)
		{
			var child = node.ChildNodes[0];
			switch (child.ToString())
			{
				case "BasicNetwork":
					return EvaluateBasicNetwork(child);
				case "AdvancedNetwork":
					return EvaluateAdvancedNetwork(child);
				default:
					return null;
			}
		}

		private Tuple<Type, object> EvaluateAdvancedNetwork(ParseTreeNode child)
		{
			var innerNet = (Network)EvaluateCreateNetwork(child.ChildNodes[0]).Item2;
			var outLName = child.ChildNodes[1].Token.Text;
			var outL = (Layer)GetFromState(outLName, typeof(Layer));
			return new Tuple<Type, object>(typeof(Network), innerNet.ConnectTo(outL));
		}

		private Tuple<Type, object> EvaluateBasicNetwork(ParseTreeNode child)
		{
			var inpLName = child.ChildNodes[0].Token.Text;
			var outLName = child.ChildNodes[1].Token.Text;
			var inpL = (Layer)GetFromState(inpLName, typeof(Layer));
			var outL = (Layer)GetFromState(outLName, typeof(Layer));
			var net = Network.WithInputLayer(inpL).ConnectTo(outL);
			return new Tuple<Type, object>(typeof(Network), net);
		}

		private Tuple<Type, object> EvaluateCreateLayer(ParseTreeNode node)
		{
			var layerParams = EvaluateKeyValuePair(node.ChildNodes[0]);

			int length = 1;
			bool biased = true;
			IActivationFunction aFunc = null;
			IInputFunction iFunc = null;
			IWeightFunction wFunc = null;
			IWeightInitializer wInit = null;

			if (!layerParams.ContainsKey("id"))
			{
				throw new Exception("ID is necessary to initialize a layer!");
			}
			if (layerParams.ContainsKey("length"))
			{
				length = Int32.Parse(layerParams["length"]);
			}
			if (layerParams.ContainsKey("biased"))
			{
				biased = bool.Parse(layerParams["biased"]);
			}
			if (layerParams.ContainsKey("activationFunction"))
			{
				aFunc = (IActivationFunction)(typeof(ActivationFunctions)
					.GetField(layerParams["activationFunction"])
					.GetValue(null));
			}
			if (layerParams.ContainsKey("weightFunction"))
			{
				wFunc = (IWeightFunction)(typeof(WeightFunctions)
					.GetField(layerParams["weightFunction"])
					.GetValue(null));
			}
			if (layerParams.ContainsKey("inputFunction"))
			{
				iFunc = (IInputFunction)(typeof(InputFunctions)
					.GetField(layerParams["inputFunction"])
					.GetValue(null));
			}
			if (layerParams.ContainsKey("weightInitializer"))
			{
				wInit = (IWeightInitializer)(typeof(WeightInitializers)
					.GetField(layerParams["weightInitializer"])
					.GetValue(null));
			}

			var layer = Layer.Create(layerParams["id"], length: length, aFunc: aFunc, 
				wFunc: wFunc, iFunc: iFunc, wInit: wInit, biased: biased);
			return new Tuple<Type, object>(typeof(Layer), layer);
		}

		private Dictionary<string, string> EvaluateKeyValuePair(ParseTreeNode node)
		{
			var key = node.ChildNodes[0].Token.Text;
			var value = node.ChildNodes[1].ChildNodes[0].Token.Text;
			var result = new Dictionary<string, string>() { { key, value } };
			if (node.ChildNodes.Count > 2)
			{
				var rest = EvaluateKeyValuePair(node.ChildNodes[2]);
				foreach (var iKey in rest.Keys)
				{
					result[iKey] = rest[iKey];
				}
			}
			return result;
		}
	}
}
