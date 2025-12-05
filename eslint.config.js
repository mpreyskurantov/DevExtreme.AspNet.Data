const js = require("@eslint/js");

module.exports = [
  {
    ignores: [
      "docs/**",
      "js-test/check-ts-compilation.*",
      "js-test/test-bundle.js",
      "net/**",
      "node_modules/**"
    ]
  },
  js.configs.recommended,
  {
    rules: {
      "eqeqeq": ["error", "allow-null"],
      "no-undef": "error",
      "no-unused-expressions": "error",
      "no-unused-vars": "error"
    },
    languageOptions: {
      ecmaVersion: 2015,
      sourceType: "script",
      globals: {
        define: "readonly",
        module: "readonly",
        require: "readonly"
      }
    }
  },
  {
    files: ["build/**/*.js", "js-test/webpack.config.js"],
    languageOptions: {
      ecmaVersion: 2015,
      sourceType: "script",
      globals: {
        console: "readonly",
        process: "readonly",
        require: "readonly",
        __dirname: "readonly",
        module: "readonly"
      }
    }
  },
  {
    files: ["js-test/**/*.js"],
    ignores: ["js-test/webpack.config.js"],
    languageOptions: {
      ecmaVersion: 2015,
      sourceType: "script",
      globals: {
        define: "readonly",
        module: "readonly",
        require: "readonly",
        window: "readonly",
        document: "readonly",
        Promise: "readonly"
      }
    }
  }
];

