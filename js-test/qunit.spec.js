/* global QUnit:false, window:false */
"use strict";

const fs = require("fs");
const path = require("path");
const { test, expect } = require("@playwright/test");
const v8toIstanbul = require("v8-to-istanbul");
const libCoverage = require("istanbul-lib-coverage");
const libReport = require("istanbul-lib-report");
const reports = require("istanbul-reports");
const TEST_DEPS = require("./test-deps.js");

// Requests to ORIGIN are answered by page.route from the repository root, no web server needed
const ORIGIN = "http://localhost";
const ROOT_DIR = path.join(__dirname, "..");
const PAGE_URL = ORIGIN + "/js-test/index.html";
const COVERED_SCRIPT = "js/dx.aspnet.data.js";

const MODES = [
    { name: "scripts", scripts: TEST_DEPS.concat("test.js"), coverage: true },
    { name: "bundled", scripts: [ "test-bundle.js" ] },
    { name: "bundled nojquery", scripts: [ "test-bundle-nojquery.js" ] },
    { name: "bundled nojquery cjs", scripts: [ "test-bundle-nojquery-cjs.js" ] }
];

MODES.forEach(mode => {
    test(mode.name, async({ page }) => {
        const pageErrors = [];
        page.on("pageerror", e => pageErrors.push(e.message));

        await page.route(ORIGIN + "/**", route => serve(route, mode.scripts));
        await page.addInitScript(() => {
            window.qunit_config_autostart = false;
        });

        if(mode.coverage)
            await page.coverage.startJSCoverage();

        await page.goto(PAGE_URL);
        const result = await page.evaluate(runQUnit);

        if(mode.coverage)
            await writeCoverage(await page.coverage.stopJSCoverage());

        console.log(`${mode.name}: ${result.testCounts.passed} of ${result.testCounts.total} passed`);
        expect(pageErrors).toEqual([]);
        expect(result.failures).toEqual([]);
        expect(result.status).toBe("passed");
    });
});

function serve(route, scripts) {
    const url = new URL(route.request().url());

    if(url.href === PAGE_URL) {
        return route.fulfill({
            contentType: "text/html",
            body: [ "<!DOCTYPE html>" ]
                .concat([ "../node_modules/qunit/qunit/qunit.js" ].concat(scripts).map(src => `<script src="${src}"></script>`))
                .join("\n")
        });
    }

    const filePath = path.join(ROOT_DIR, url.pathname);
    if(!fs.existsSync(filePath))
        return route.fulfill({ status: 404 });

    return route.fulfill({ path: filePath });
}

// Runs in the browser
function runQUnit() {
    return new Promise(resolve => {
        const failures = [];

        QUnit.on("testEnd", testEnd => {
            if(testEnd.status !== "failed")
                return;

            const errors = testEnd.errors.map(e => `${e.message || "failed"}`
                + ` (expected: ${QUnit.dump.parse(e.expected)}, actual: ${QUnit.dump.parse(e.actual)})`);

            failures.push(`${testEnd.fullName.join(" > ")}: ${errors.join("; ")}`);
        });

        QUnit.on("error", error => {
            failures.push(`global failure: ${error && error.message || error}`);
        });

        QUnit.on("runEnd", runEnd => {
            resolve({ status: runEnd.status, testCounts: runEnd.testCounts, failures: failures });
        });

        QUnit.start();
    });
}

async function writeCoverage(entries) {
    const entry = entries.find(i => i.url === ORIGIN + "/" + COVERED_SCRIPT);
    const converter = v8toIstanbul(path.join(ROOT_DIR, COVERED_SCRIPT), 0, { source: entry.source });

    await converter.load();
    converter.applyCoverage(entry.functions);

    const context = libReport.createContext({
        dir: path.join(__dirname, "coverage"),
        coverageMap: libCoverage.createCoverageMap(converter.toIstanbul())
    });

    reports.create("lcovonly", { file: "lcov.info", projectRoot: ROOT_DIR }).execute(context);
    reports.create("text-summary").execute(context);
}
