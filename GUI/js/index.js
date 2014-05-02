$(function() {
	var socket = io.connect("http://localhost:8085");

	$("#docs").click(function (e) {
		e.preventDefault();
		console.log("clicked");
		window.open("https://github.com/Likhit/Spark", "_blank");
	});

	var editMode = true;

	var iShell = $(".language-spark");

	var defaultInputNodeContent = "Enter your code here...";

	function createInputNode(innerContent) {
		innerContent = innerContent || defaultInputNodeContent;
		return $('<blockquote><pre class="language-spark"><code>' + innerContent + '</code></pre></blockquote>');
	}

	function insertInputNodeAfter(inputNode, innerContent) {
		var nexts = inputNode.nextAll();
		if (nexts.length < 2) {
			inputNode.parent().append(createInputNode(innerContent));
		}
	}

	var defaultOutputNodeContent = "<p class='notification default'>Code succesfully evaluated.</p>"

	function createOutputNode(innerContent) {
		return $('<blockquote class="blockquote-reverse"><pre class="language-spark"><code>' + innerContent + '</code></pre></blockquote>');
	}

	function insertOutputNodeAfter(inputNode, innerContent, appendFunc) {
		var next = inputNode.next();
		if (next.hasClass("blockquote-reverse")) {
			if (innerContent) {
				var tag = next.find("code");
				appendFunc ? appendFunc(tag, innerContent) : tag.html("<p class='notification'>" +innerContent + "</p>")
			}
		}
		else
		{
			inputNode.parent().append(createOutputNode("<p>" +innerContent + "</p>"));
		}
	}

	function simpleAppendFunc(tag, content) {
		tag.append(content);
	}

	var lastFiredInputNode = null;

	iShell.on("keyup", "code", function(e) {
		var codeTag = e.target.tagName === "CODE" ? $(e.target) : $(e.target).parent("code");
		if (e.shiftKey === true && e.keyCode === 13) {
			$.ajax({
				type: "POST",
				url: "/eval",
				data: {
					"code": codeTag.text()
				}
			});
			codeTag.trigger("blur");
			lastFiredInputNode = codeTag.parents("blockquote");
		}
		else if (e.keyCode === 186 || e.keyCode === 59) {
			Prism.highlightElement(codeTag[0]);
		}
	})
	.on("click", "code", function(e) {
		var codeTag = e.target.tagName === "CODE" ? $(e.target) : $(e.target).parent("code");
		if (editMode && !codeTag.parents("blockquote").hasClass("blockquote-reverse"))
		{
			codeTag[0].contentEditable = true;
			if (!codeTag.data("cleared")) {
				codeTag[0].textContent = "";
				codeTag.data("cleared", true);
			}
		}
	})
	.on("blur", "code", function(e) {
		var codeTag = e.target.tagName === "CODE" ? $(e.target) : $(e.target).parent("code");
		codeTag[0].contentEditable = false;
		if (codeTag[0].textContent === "") {
			codeTag[0].textContent = defaultInputNodeContent;
			codeTag.data("cleared", false);
		}
	});

	Prism.languages.spark = Prism.languages.extend('csharp', {
		keyword: /\b(layer|run|train|trainer|on|and|inputs|targets|load|save|print|type|indices|convert|class|vector|of|mismatches|between|size|to|with|of|as)\b/g,
		punctuation: /[;,]/g,
		operator: /=|-&gt/g,
		number: /\b-?(0x[\dA-Fa-f]+|\d*\.?\d+([Ee]-?\d+)?)\b/g
	});

	socket.on("news", function(data) {
		console.log(data);
	});

	socket.on("start", function(data) {
		editMode = false;
		insertOutputNodeAfter(lastFiredInputNode, defaultOutputNodeContent);
	});

	socket.on("error", function(data) {
		insertOutputNodeAfter(lastFiredInputNode, data);
		editMode = true;
		insertInputNodeAfter(lastFiredInputNode);
	});

	socket.on("mismatches count", function(data) {
		insertOutputNodeAfter(lastFiredInputNode, '<p class="notification">Number of mismatches: ' + data + '</p>', simpleAppendFunc);
	});

	var matrixCount = 0;

	socket.on("matrix", function(data) {
		var newId = "matrix" + matrixCount++;
		insertOutputNodeAfter(lastFiredInputNode, newId, function(tag, innerContent) {
			tag.attr("id", innerContent);
		});
		displayMatrix(newId, data);
	});

	socket.on("mismatches", function(data) {
		var newId = "matrix" + matrixCount++;
		insertOutputNodeAfter(lastFiredInputNode, newId, function(tag, innerContent) {
			tag.attr("id", innerContent);
		});
		displayErrorMatrix(newId, data);
	});

	socket.on("saved", function(data) {
		insertOutputNodeAfter(lastFiredInputNode, '<p class="notification">File succesfull saved.</p>', simpleAppendFunc);
	});

	var networkCount = 0;

	socket.on("network", function(data) {
		var newId = "net" + networkCount++;
		insertOutputNodeAfter(lastFiredInputNode, newId, function(tag, innerContent) {
			tag.attr("id", innerContent);
		});
		var newData = transformNetworkData(data);
		makeNetwork(newId, newData);
	});

	socket.on("layer", function(data) {
		var text = "<div><p><span style='color:skyblue'>Layer Id:</span> " + data.Id +
		"</p><p><span style='color:skyblue'>Length:</span> " + data.Length +
		"</p><p><span style='color:skyblue'>Activation Function:</span> " + data.AFunc +
		"</p><p><span style='color:skyblue'>Weight Function:</span> " + data.WFunc +
		"</p><p><span style='color:skyblue'>Input Function:</span> " + data.IFunc +
		"</p><p><span style='color:skyblue'>Weight Initializer:</span> "
		+ data.WInit + "</p></div>";
		insertOutputNodeAfter(lastFiredInputNode, text, simpleAppendFunc);
	});

	var chartCount = 0;

	socket.on("starting training", function(data) {
		var newId = "errChart" + chartCount++;
		insertOutputNodeAfter(lastFiredInputNode, newId, function(tag, innerContent) {
			tag.attr("id", innerContent);
		});
		makeDynamicErrorChart(newId, data);
	});

	socket.on("training", function(data) {
		var chart = $("#errChart" + (chartCount - 1));
		var dataStore = chart.data("dataStore");
		var totalDataStore = chart.data("totalDataStore");
		if (dataStore._array.length >= 20) {
			dataStore._array.shift();
		}
		dataStore.insert({
			epoch: data.epoch,
			error: data.error,
			minError: chart.data("minError")
		});

		var dataSource = chart.data("dataSource");
		dataSource.load();

		var maxEpoch = chart.data("maxEpochs");
		if (data.epoch == maxEpoch - 1) {
			chart.removeData("dataStore")
				.removeData("dataSource")
				.removeData("minError")
				.removeData("maxEpochs")
				.removeData("firstError");
		}

		if (data.hiddenLayerCount) {
			var content = '<div>Adding new hidden layer.</div>';
			insertOutputNodeAfter(lastFiredInputNode, content, simpleAppendFunc);
		}
	});

	socket.on("end", function(data) {
		editMode = true;
		insertInputNodeAfter(lastFiredInputNode);
	});

	function displayErrorMatrix(id, data) {
		var thColSpan = 1;
		try
		{
			thColSpan = data.mismatches[0].Item1.length;
		}
		catch (Error) {}

		var table = d3.select("#" + id).append("table")
			.attr("class", "table table-condensed table-responsive");
		var thead = table.append("thead").append("tr");
		var tbody = table.append("tbody");

		var th = thead.selectAll("th")
			.data(data.headers)
			.enter().append("th")
				.attr("colspan", thColSpan)
				.text(function(d) { return d; });

		var tr = tbody.selectAll("tr")
			.data(data.mismatches)
			.enter().append("tr");

		var td = tr.selectAll("td")
			.data(function(d) { return [d.Item1].concat(d.Item2, d.Item3); })
			.enter().append("td")
				.text(function(d) {return d;});
	}

	function displayMatrix(id, data) {
		var table = d3.select("#" + id).append("table")
			.attr("class", "table table-condensed table-responsive");
		var thead = table.append("thead").append("tr");
		var tbody = table.append("tbody");

		var th = thead.selectAll("th")
			.data(["#"].concat(data.headers))
			.enter().append("th")
				.text(function(d) { return d; });

		var tr = tbody.selectAll("tr")
			.data(data.matrix.map(function(x, i) { x.unshift(i); return x; }))
			.enter().append("tr");

		var td = tr.selectAll("td")
			.data(function(d) {return d; })
			.enter().append("td")
				.text(function(d) {return d;});
	}

	function makeDynamicErrorChart(id, data) {
		var dataStore = new DevExpress.data.ArrayStore({
			key: 'epoch',
			value: []
		});

		dataStore.insert({
			epoch: 0,
			error: data.firstError,
			minError: data.minError
		});

		var dataSource = new DevExpress.data.DataSource(dataStore);

		var node = $("#" + id)
			.data("dataStore", dataStore)
			.data("dataSource", dataSource)
			.data("minError", data.minError)
			.data("maxEpochs", data.maxEpochs)
			.data("firstError", data.firstError)
			.append("<div>").find("div");
		node.dxChart(makeErrorChartConfig(dataSource));
	}

	function makeErrorChartConfig(dataSource) {
		return {
			animation: false,
			dataSource: dataSource,
			series: [{
				valueField: 'error', name: "Error", color: "skyblue"
			}, {
				valueField: 'minError', name: "Minimum Error", color: "red"
			}],
			commonSeriesSettings: {
				argumentField: "epoch"
			},
			argumentAxis: {
				argumentType: 'numeric',
				type: 'discrete',
				title: {
					text: "Epoch"
				},
				grid: {
					visible: true
				}
			},
			valueAxis: {
				title: {
					text: "Error"
				}
			},
			title: {
				text: "Training Error",
				font: { color: "white" }
			},
			commonPaneSettings: {
				border: {
					visible: true
				}
			},
			tooltip: {
				enabled: true
			},
			commonAxisSettings: {
				title: {
					font: { color: "#d3d3d3" }
				},
				label: {
					font: { color: "#d3d3d3" }
				}
			},
			legend: {
				font: { color: "d3d3d3" }
			}
		}
	}

	function transformNetworkData(data) {
		var newData = {
			nodes: [],
			links: []
		};

		var neuronIndex = {};

		var groupIndex = 0
		var index = 0;
		for (var layer in data.Layers) {
			for (var j = 0; j < data.Layers[layer].Length; j++) {
				var name = data.Layers[layer].Id + " " + j;
				newData.nodes.push({
					name: name,
					group: groupIndex,
					neuronNumber: index,
					layer: layer
				});
				neuronIndex[name] = index++;
			}
			groupIndex++;
		}

		for (var layer in data.Layers) {
			for (var i = 0; i < data.Layers[layer].Length; i++) {
				var sName = data.Layers[layer].Id + " " + i;
				for (var j = i; j < data.Layers[layer].Length; j++) {
					var tName = data.Layers[layer].Id + " " + j;
					newData.links.push({
						source: neuronIndex[sName],
						target: neuronIndex[tName],
						weight: NaN
					});
				}
			}
		}

		var connectionList = [];
		for (var from in data.Edges) {
			for (var to in data.Edges[from]) {
				var matrix = data.Edges[from][to];
				connectionList.push({
					source: from,
					target: to,
					weights: matrix
				});
			}
		}

		for (var connectionIndex = 0; connectionIndex < connectionList.length; connectionIndex++) {
			var connection = connectionList[connectionIndex];
			for (var i = 0; i < data.Layers[connection.source].Length; i++) {
				var sName = data.Layers[connection.source].Id + " " + i;
				for (var j = 0; j < data.Layers[connection.target].Length; j++) {
					var tName = data.Layers[connection.target].Id + " " + j;
					newData.links.push({
						source: neuronIndex[sName],
						target: neuronIndex[tName],
						weight: connection.weights[j][i]
					});
				}
			}
		}

		return newData;
	}

	function makeNetwork(id, graph) {
		var width = 960, height = 400;

		var color = d3.scale.category20();

		var force = d3.layout.force()
			.charge(-700)
			.linkDistance(200)
			.size([width, height]);

		var tip = d3.tip()
			.attr('class', 'd3-tip')
			.offset([-10, 0])
			.html(function(d) {
				if (d.name) {
					return '<div><strong>Layer:</strong> <span style="color:skyblue">' + d.layer + '</span></div>'
						+ '<div><strong>Neuron:</strong> <span style="color:skyblue">#' + d.neuronNumber + '</span></div>';
				}
				else if (d.weight) {
					return '<div><strong>From:</strong> <span style="color:skyblue">#' + d.source.index + '</span></div>'
						+ '<div><strong>To:</strong> <span style="color:skyblue">#' + d.target.index + '</span></div>'
						+ '<div><strong>Weight:</strong> <span style="color:skyblue">' + d.weight + '</span></div>';
				}
			});

		var svg = d3.select("#" + id).append("svg")
			.attr("width", width)
			.attr("height", height);

		svg.call(tip);

		var grad = svg.append("defs").append("linearGradient")
			.attr("id", "grad")

		grad.append("stop").attr("stop-color", "rgb(255, 255, 0)").attr("offset", "0%");
		grad.append("stop").attr("stop-color", "red").attr("offset", "100%");

		force.nodes(graph.nodes)
			.links(graph.links)
			.start();

		var link = svg.selectAll(".link")
			.data(graph.links)
			.enter().append("line")
				.attr("class", "link")
				.style("stroke", "url(#grad)")
				.style("stroke-width", function(d) { return isNaN(d.weight) ? 0 : 2})
				.on("mouseover", tip.show)
				.on("mouseout", tip.hide);


		var nodeEnter = svg.selectAll(".node")
			.data(graph.nodes)
			.enter().append("g")
				.on("mouseover", tip.show)
				.on("mouseout", tip.hide);

		var node = nodeEnter.append("circle")
				.attr("class", "node")
				.attr("r", 20)
				.style("fill", function(d) { return color(d.group); })
				.call(force.drag)

		node.append("title")
			.text(function(d) { return d.name; });

		nodeEnter.append("text")
            .attr("x", 0)
            .attr("dy", ".35em")
            .attr('class', 'nodeText')
            .attr("text-anchor", "middle")
            .text(function(d) {
                return d.name;
            });

		force.on("tick", function() {
			link.attr("x1", function(d) { return d.source.x; })
				.attr("y1", function(d) { return d.source.y; })
				.attr("x2", function(d) { return d.target.x; })
				.attr("y2", function(d) { return d.target.y; });

			nodeEnter.attr("transform", function(d) { return "translate(" + d.x + ", " + d.y  + ")" });
		});
	}
});
