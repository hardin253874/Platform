# ReadiTest - Test Automation for ReadiNow

## Refactor Notes

*   move all the readinow drivers over to the rn namespace, so they are such as
rn.app and rn.chart-builder rather than rt.po.app and rt.po.chart-builder. While tests
are being switched over we could have the rt.po.app namespace exist and simply refer all rn.app.

## Things to fix in refactor

*   provide a single experience for writing and running a test
*   generate events for before and after suites and tests, e.g. can be used by TeamCity
*   limit info in UI to avoid poor performance
*   streamline some process like operations like moving tests between suites
*   allow organising entities in folders or some kind of grouping

## Todo list






## General Notes


### RT UI Ideas

How about the following for the runner UI

#### a "plan" view to set up the test run

-   for now just take a suite or a test
-   show the entity structure, that is the contained suites, fixtures and tests. Don't show steps.
-   user chooses what to run, either a suite or a test
-   reset button to create the test run => switch to test view

#### test view

-   show the list of tests and the steps for the selected test. these steps
    include steps for active fixtures for the test
-   auto refresh checkbox defaults to on and means we watch for updates to
    the test run and sync the view for status of executed tests and steps
    and we set the selected test to the currently running or last run test
-   functions available are run to end, run to error, run next, reset last,
    run to step, run step, pause
-   can also edit, insert and delete steps for the test (only the test for
    now, not the fixture). These changes may be saved to the test.
-   view a step shows the step view

#### step view

-   shows the summary of the current test run and position of this step within
    the test run and the current test
-   functions available are view prev, view next, run to end, run to error,
    run next, reset last, pause
-   can also edit
-   have auto refresh view checkbox, default to on, meaning if the test run
    advances to another step then this view is switched to that step




# Let's lay this all out again.

Let's work from the top down on what I need in RT.
And to allocate to namespaces as I sort this.

## RT cli

Most commands to the CLI run and then complete, except for the "repl" and "server"
commands which are discussed in their own sections.

The following are the commands other than repl and server.

* list entities given a search string
     - search on name or doc

* run one or more suites or tests
     - with optional concurrency options (only making sense if a suite)
     - suites and tests chosen with an id list
     - write to std out messages as each suite and test starts and finishes
     - write a junit xml file as well as any artifacts such as screenshot files
     - if it detects we are running in team city then it exits with code 0 even if tests fail,
          and it outputs teamcity format messages

## RT repl

The rt repl provides a set of convenient functions and state for use in
a REPL like environment. Like the standard Clojure REPL uses \*1 for the
result of the previous statement, and \*e for the last exception,
the rt repl has a set of vars to track certain results. Some of these are
visible to the user, some are used implicitly in the rt repl functions.

Running tests and test steps is based on sessions. The RT repl has default
session that will be used, however additional sessions can be used if desired.

* list entities given a search string and optional type
* init the default session with a test and context
* list the involved entities (tests, fixtures or scripts)
     - suites are also listed but less interesting as they don't have steps themselves
* list test steps summary including status
* run a step, either specific, next or the last
* run all steps
* run until error
* run an expression
* add a new step to the test or any other involved entities
* remove a step to the test or any other involved entities
* modify a step to the test or any other involved entities
* save any or all of the involved entities, if any changes have occurred
* create a new session
     - returns an id that can then be used in most functions

## RT server and the RT UI

The RT server is a web server to both host the RT UI which is a HTML SPA,
and provide the web api that the UI uses.

The web api includes the following.

* get entity listing for a given entity type
     - only include properties useful in a report or chooser
* get entity for id
* put entity
* init session with a test and set of suites or fixtures
* get the session
     - optionally including steps
     - optionally including only recent events, given a time
* run a step, either specific, next or the last
     - option to step over or into scripts
* run to end
* run to error
* stop running (if a run to end or run to error is happening)
* run expression
* get artifact links
* get driver function list

The RT UI allows listing and editing of all RT entity types: tests,
fixtures, scripts and suites. However its most important function is
the interactive development and debugging of tests. In the future it
will also allow development of app driver functions.

During the development of a test, the RT user can run steps one at a
time or until some condition (next step, fail, error, time, step count,
specific step...). They can modify, add or remove steps to the test or
any involved script, or fixture, and then choose to commit or discard those changes.



# Modelling thoughts

*   split out out the fixture "handlers" as a reporter, allowing a fixture to reference
    a reporter, but also allow a reporter to be selected when running a suite (or test)
    ... maybe ... but not sure about the term "reporter" and a handler for doing such as
    saving and restoring the taxi \*driver\* doesn't really fit

