# Moving JS tests off Karma

Versions and counts as of November 2026.

## How Karma is used today

`js-test/test.js` holds 56 QUnit tests. Karma runs them in headless Chrome in four modes:

| npm script | What the page loads | What it checks |
|---|---|---|
| `karma` | `test-deps.js` as `<script>` tags: jQuery, `devextreme-dist` `dx.all.js`, xhr-mock, `js/dx.aspnet.data.js`, then `test.js` | Browser-global branch; produces coverage (`js-test/coverage/lcov.info` → Codecov) |
| `karma-bundled` | webpack bundle of `test.js` + `js/dx.aspnet.data.js` | AMD branch, `require("jquery")` |
| `karma-bundled-nojquery` | same bundle with `js-nojquery/index.js` | no-jQuery package |
| `karma-bundled-nojquery-cjs` | same, with `define` disabled | CommonJS branch |

| Package | Role | After migration |
|---|---|---|
| `karma` | Serves files, starts the browser, collects results | removed |
| `karma-chrome-launcher` | Starts `ChromeHeadless` | removed |
| `karma-qunit` | QUnit adapter | removed |
| `karma-coverage` | Instruments `js/*.js` with istanbul, writes lcov | removed |
| `qunit` | Test framework; also the manual UI in `test.html` | kept |
| `xhr-mock` | Mocks `XMLHttpRequest` | kept |
| `webpack`, `webpack-cli` | Build `test-bundle.js` for the bundled modes | kept |

Related files: `js-test/karma.conf.js`, `test-deps.js`, `test.html`, `webpack.config.js`, and the `test-js` job in `.github/workflows/ci.yml`.

What Karma costs:
- Karma is deprecated; there have been no releases since 6.4.4 (July 2024).
- The Karma packages pull in 180 of the 414 locked packages.
- They cause all 6 remaining `npm audit` findings (minimatch 3.x). Without them, `npm audit` reports 0.

## Options

| Option | Test code changes | Added packages | Coverage | Notes |
|---|---|---|---|---|
| **A. Playwright opens the existing QUnit pages** | none | 3 (`@playwright/test`) + 14 (`v8-to-istanbul`, `istanbul-lib-*`) | V8 coverage → lcov | Covers all 4 modes |
| B. Vitest 5 browser mode | Rewrite all 56 tests | ~90 | built in | Vite is ESM-only, so the AMD and script-tag modes need a separate setup |
| C. Web Test Runner 1.0 + `web-test-runner-qunit` | none | not measured | built in | The adapter's last release was February 2024; untested with WTR 1.0 |
| D. `node-qunit-puppeteer` | none | puppeteer (downloads its own Chrome) | none | No coverage |

QTap, the QUnit team's Karma replacement, is only a 0.0.1 placeholder on npm.

**Recommendation: A.** The tests stay QUnit and stay unchanged. Only the runner changes, and only a few packages are added.

## Plan (option A)

1. **Pages.** Keep `test.html` for the script-tag mode. Add `test-bundled.html`, which loads `qunit.js` and `test-bundle.js`. Both pages save QUnit results with `QUnit.on("testEnd")` and `QUnit.on("runEnd")`.
2. **Runner.** Add `js-test/playwright.config.js` and `js-test/qunit.spec.js`, with one Playwright test per mode:
   - open the page, using `file://` or a tiny static server
   - wait for `runEnd`
   - fail, listing the names of the failed QUnit tests

   Use the installed Chrome (`channel: "chrome"`). GitHub's Ubuntu runners include it, so CI downloads no browser.
3. **Coverage.** In the script-tag mode, use `page.coverage.startJSCoverage()` and keep only `js/dx.aspnet.data.js`. Convert with `v8-to-istanbul` and write `js-test/coverage/lcov.info`, so the Codecov step doesn't change.
4. **Scripts and CI.** Replace the four `karma*` scripts. The webpack builds stay as they are, followed by `playwright test`. Update the `test-js` steps.
5. **Remove** `karma`, `karma-chrome-launcher`, `karma-qunit`, `karma-coverage` and `karma.conf.js`.
6. **Verify:**
   - 56/56 pass in all four modes
   - line coverage is close to today's ~95% (V8 and istanbul count slightly differently)
   - `npm audit` reports 0
   - a deliberately broken test fails CI

Points to check early: the page works over `file://` (requests are mocked, so it should), and the V8 → lcov conversion.

## Optional cleanups

- `test.js` still has checks that are dead with a single DevExtreme version: `useLegacyStoreResult()` for DevExtreme < 18.2, and the IE `document.documentMode` checks.
- `xhr-mock` is unmaintained (2.5.1); `mock-xmlhttprequest` is a maintained replacement. This is unrelated to Karma and can wait.
