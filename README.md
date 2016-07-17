#Spark

Spark is a language for creating, running and training Neural Networks. Spark provides syntax which makes it easy to create neural networks with arbritary number of layers, neurons and connections.

Special constructs exist to load data to run, and train the networks on; and also to count the errors/mismatches of the network. Currently three training algorithms have been implemented, namely:
1. Perceptron learning
2. Backpropagation training
3. MTiling algorithm

Spark comes with an interactive shell similar to that of iPython that allows the user to visualize the created networks; it also allows the user to view the change in the error while training is taking place.

##Build & Run Spark

*Requirements: Visual Studios with .NET Framework 4.5, [Node.js](http://www.nodejs.org), [NPM](http://npmjs.org)*

1. Download the repository from https://github.com/Likhit/Spark.
2. Build Spark.sln with Visual Studios (the project was created in VS 2012).
3. Inside the folder `GUI`, open the command prompt and run `npm install`. This will install all required dependencies.
4. Inside the folder `GUI`, run `node main.js`. This will start the shell on `localhost:8085`.
5. Open in a browser (preferably Chrome) http://localhost:8085.

##Documentation

The full documentation of the language can be found at the [documentation page](Documentation.pdf)
