"use strict";

const { defineConfig } = require("@playwright/test");

module.exports = defineConfig({
    testMatch: "qunit.spec.js",
    outputDir: "test-results",
    reporter: "list",
    use: {
        channel: "chrome"
    }
});
