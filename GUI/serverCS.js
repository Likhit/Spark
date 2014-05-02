var edge = require("edge");
var express = require("express");
var app = express();

var evalDSL = edge.func({
	source: function() {/*
		using DSL;
		using System;
		using System.Threading.Tasks;
		using System.Net;
		using System.Collections.Specialized;
		using Newtonsoft.Json;

		public class Startup
		{
			private Interpretter shell;

			public Startup()
			{
				shell = new Interpretter(x =>
				{
					var json = JsonConvert.SerializeObject(x, Formatting.Indented);
					using (var wb = new WebClient())
					{
						var data = new NameValueCollection();
						data["output"] = json;
						var url = @"http://localhost:8085/output";
						var response = wb.UploadValues(url, "POST", data);
					}
				}, false);
			}

			public async Task<object> Invoke(object input)
			{
				var inp = (string)input;
				shell.Evaluate(inp);
				return null;
			}
		}
	*/},
	references: [
		__dirname + "\\Spark DLLs\\DSL.dll",
		__dirname + "\\Spark DLLs\\Newtonsoft.Json.dll"
	]
});

app.use(express.json());
app.use(express.urlencoded());

app.post("/eval", function(req, res) {
	var code = req.body.code;
	console.log(req.body.code);
	evalDSL(code);
	res.send(200);
});

var server = app.listen(8080, function() {
    console.log("Listening on port %d", server.address().port);
});