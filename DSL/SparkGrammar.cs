using Irony.Parsing;

namespace DSL
{
	[Language("Spark", "1.0", "A DSL for creating artificial neural networks")]
    class SparkGrammar : Grammar
    {
		public SparkGrammar()
			: base(true)
		{
			var program = new NonTerminal("Program");
			var statement = new NonTerminal("Statement");
			
			var assignStmt = new NonTerminal("AssignStatement");
			var commandStmt = new NonTerminal("CommandStatement");
			
			var assignRHS = new NonTerminal("AssignRHS");
			var createLayer = new NonTerminal("CreateLayer");
			var createNetwork = new NonTerminal("CreateNetwork");
			var createTrainer = new NonTerminal("CreateTrainer");
			var runNetwork = new NonTerminal("RunNetwork");
			var loadFile = new NonTerminal("LoadFile");
			var convertData = new NonTerminal("ConvertData");
			var findError = new NonTerminal("FindError");

			var basicNetwork = new NonTerminal("BasicNetwork");
			var advancedNetwork = new NonTerminal("AdvancedNetwork");

			var matrixLoadType = new NonTerminal("MatrixLoadType");

			var convertToVector = new NonTerminal("ConvertToVector");
			var convertToIndices = new NonTerminal("ConvertToIndices");

			var supportedFileTypes = new NonTerminal("SupportedFileTypes");

			var printStatement = new NonTerminal("PrintStatement");
			var trainStatement = new NonTerminal("TrainStatement");
			var saveStatement = new NonTerminal("SaveStatement");

			var kvPair = new NonTerminal("KeyValuePair");
			var identifier = TerminalFactory.CreateCSharpIdentifier("Identifier");
			var number = TerminalFactory.CreateCSharpNumber("Number");
			var str = TerminalFactory.CreateCSharpString("string");


			program.Rule = statement + ";" | statement + ";" + program;
			
			statement.Rule = assignStmt | commandStmt;

			assignStmt.Rule = identifier + "=" + assignRHS;

			assignRHS.Rule = createLayer
				| createNetwork
				| createTrainer
				| runNetwork
				| loadFile
				| convertData
				| findError;

			createLayer.Rule = ToTerm("layer") + "with" + kvPair;

			createNetwork.Rule = basicNetwork
				| advancedNetwork;

			basicNetwork.Rule = identifier + "->" + identifier;

			advancedNetwork.Rule = createNetwork + "->" + identifier;

			runNetwork.Rule = ToTerm("run") + identifier + "on" + identifier;

			loadFile.Rule = ToTerm("load") + str + "as" + matrixLoadType
				| ToTerm("load") + supportedFileTypes + str + "as" + matrixLoadType;

			matrixLoadType.Rule = ToTerm("row") + "major"
				| ToTerm("row") + "major" + "with" + "headers"
				| ToTerm("column") + "major";

			createTrainer.Rule = ToTerm("trainer") + "of" + "type" + identifier + "with" + kvPair;

			kvPair.Rule = identifier + "=" + (number | identifier)
				| identifier + "=" + (number | identifier) + "," + kvPair;

			convertData.Rule = convertToVector
				| convertToIndices;

			convertToVector.Rule = ToTerm("convert") + identifier + "to" + "class" + "vector"
				| ToTerm("convert") + identifier + "to" + "class" + "vector" + "of" + "size" + "=" + number;

			convertToIndices.Rule = ToTerm("convert") + identifier + "to" + "class" + "indices";

			findError.Rule = ToTerm("count") + "mismatches" + "between" + identifier + "and" + identifier
				| ToTerm("get") + "mismatches" + "between" + identifier + "and" + identifier;

			commandStmt.Rule = printStatement
				| trainStatement
				| saveStatement;

			printStatement.Rule = ToTerm("print") + identifier;

			trainStatement.Rule = ToTerm("train") + identifier + "with" + identifier + "on"
				+ "inputs" + identifier + "and" + "targets" + identifier;

			saveStatement.Rule = ToTerm("save") + identifier + "to" + str
				| ToTerm("save") + identifier + "as" + supportedFileTypes + "to" + str;

			supportedFileTypes.Rule = ToTerm("csv") | "tsv" | "ssv";

			MarkPunctuation("layer", "run", "train", "trainer",
				"on", "and", "inputs", "targets", "major",
				"load", "save", "print", "type", "indices",
				"convert", "class", "vector", "of",
				"mismatches", "between", "size", "as",
				"to", "with", "of", "as", ";", ",",
				"=", "->");

			this.Root = program;

		}
    }
}
