"use strict";

const DefinePlugin = require("webpack").DefinePlugin;

function createConfig(filename, aspNetDataScript, cjs) {
    const definitions = {
        ASPNET_DATA_SCRIPT: JSON.stringify(aspNetDataScript)
    };

    if(cjs) {
        definitions["define"] = false;
    }

    return {
        mode: "development",
        entry: "./test.js",
        output: {
            path: __dirname,
            filename: filename
        },
        plugins: [
            new DefinePlugin(definitions)
        ],
        devtool: false
    }
}

module.exports = [
    createConfig("test-bundle.js", "../js/dx.aspnet.data.js"),
    createConfig("test-bundle-nojquery.js", "../js-nojquery/index.js"),
    createConfig("test-bundle-nojquery-cjs.js", "../js-nojquery/index.js", true)
];
