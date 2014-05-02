var request = require("request");
var express = require("express");
var app = express();

var server = app.listen(8085, function() {
    console.log("Listening on port %d", server.address().port);
});

var io = require("socket.io").listen(server);

var selfSocket = require("socket.io-client").connect("http://localhost:8085");

app.use(express.json({limit: '5mb'}));
app.use(express.urlencoded({limit: '5mb'}));

app.use("/js", express.static(__dirname + "/js"));
app.use("/css", express.static(__dirname + "/css"));

app.set("views", __dirname + "/Views");
app.engine("jade", require("jade").__express);

app.get("/", function(req, res) {
	res.render("index.jade");
});

app.post("/eval", function(req, res) {
	var code = req.body.code;
	request.post("http://localhost:8080/eval", {
		form: {
			code: code
		}
	}, function (err, res) {
		if (err || res.statusCode != 200) {
			console.log("Experienced an error!");
		}
	});
	res.send(200);
});

app.post("/output", function(req, res) {
	var output = req.body.output;
	selfSocket.emit("output", output);
	res.send(200);
});

io.sockets.on("connection", function(socket) {
	socket.emit("news", "Connected");
	console.log("Connected to client.");

	socket.on("output", function(output) {
		var out = JSON.parse(output);
		console.log(out.msg);
		socket.broadcast.emit(out.msg.toLowerCase(), out.data);
	});
});

selfSocket.on("news", function(data) {
	console.log(data);
});