var spawn = require("child_process").spawn;

var serverJS = spawn("node", ["serverJS.js"]);
var serverCS = spawn("node", ["serverCS.js"]);

serverJS.on("exit", function (code) {
	console.log("The web server closed with code: " + code);
});

serverCS.on("exit", function (code) {
	console.log("The c# server closed with code: " + code);
});

console.log("Press Ctrl+C to stop the server. You can access the shell on http://localhost:8085. Works best on chrome.");

process.on("exit", function (code) {
	serverCS.kill(code);
	serverJS.kill(code);
});