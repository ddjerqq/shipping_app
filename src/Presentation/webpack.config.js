const path = require("path");

/** @type {import('webpack').Configuration} */
module.exports = {
  mode: "production",
  entry: "./wwwroot/scripts/globe.js",
  devtool: "source-map",
  plugins: [],
  output: {
    filename: "bundle.js",
    path: path.resolve(__dirname, "wwwroot/dist"),
  }
};