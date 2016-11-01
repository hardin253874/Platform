# ReadiTest

## Notes for reading this document

*   RT = **java -jar rt-standalone.jar**, so when you see a command line such as "RT help" it means 
    to enter "java -jar rt-standalone.jar help"

## Commands

*   RT help

    Shows the help. Use that in conjunction with this for how to run.

*   RT list

    Prints a list of all the tests and suites available.

*   RT repl

    Starts an interactive clojure REPL to allow you to run tests and other.

*   RT server

    Starts RT in server mode. This is used for both authoring driver functions and tests
    via the RT UI and also for coordinating concurrent testing. More on this later.

*   RT test

    Run one or more tests in various ways as defined by additional options.

### RT test run modes

There are three modes of running a test or tests with RT using the "test" command.

*   Test mode - run one or more tests or suites serially as a single test run

*   Concurrent mode - run one or more tests or suites concurrently

*   Parallel mode - run a suite of tests over multiple parallel sessions

### Test mode

This is the original test mode. See the options for all those that apply, but
in the simplest case it is

    RT test --test my-test

or

    RT test --test my-suite


### Concurrent mode

This mode was added to RT to do concurrent and light load testing.

Some notes about it:

*   Turn on using --concurrent --sessions N
*   Each session is a standalone test run using “test” mode
*   Each given test or suite is handed to a session, the list repeated if necessary.

    * RT test --concurrent --tests t1,t2,t3 --sessions 3
        * will run t1, t2, and t3 each on a concurrent session

    * RT test --concurrent --test t1 --sessions 3
        * will run t1 three times using three concurrent sessions

    * RT test --concurrent --test t1,t2 --sessions 10
        * will run t1 on 5 sessions and t2 on 5 sessions

*   the tests sessions are started based on a ramp up time
*   a test session may be repeated using a pace option to manage the max rate
*   each session uses a server (default localhost:3000) to get certain config and report progress and results
*   if no server is specified then the main RT instance will be the server
*   if another server is specified then this RT instance simply runs the said number of sessions
*   each session requests settings from the server, typically the app-url and test.

### Parallel mode

*   this is the new mode to run a suite of tests using concurrent sessions to get
    through the suite as fast as possible
*   turned on if we have both one test (a suite) and sessions > 1
*   the tests of the given suite are distributed across a number of sessions
*   today’s implementation uses standalone test runs to run each batch of tests


## Configuration options

RT may be run in a number of modes. There are various configuration options for these running modes,
and they may be defined in a number of ways. They each have a default value, that may then be
overridden via the config file, that may then be overridden by options provided by an RT server
(if so configured), and finally they may be overridden at the command line.

Command line options are typically expressed via a double dash, such as --tenant. The same option
in the config.edn file is expressed using the clojure keyword form, e.g. :tenant.

The following describes the various configuration options. How to use in the various running modes
is described later.

### Configuration options on the command line

*   --app-url <app url>
    
    Defaults to https://localhost (see footnotes). Only include protocol and server name.
    
    Examples are 
    
    *   --app-url https://spdevfe03.sp.local 

*   --test TEST

*   --sessions N

    The number of sessions to run on this host machine.

*   --server URL

    The RT server to talk to for configuration and to report results.

    Must include protocol and port.
    
    Examples:
    
    *   --server http://spdevrt.sp.local:3000

*   --tenant T

*   --username U

*   --password P

*   --repeat

*   --no-repeat

*   --top N

*   --left N


### Configuration options in the config file

In addition to each of the command line options (where something like --tenant 
is seen as :tenant in the config file) you can define the following: 

*   :shared-db-dirs

Note that command line options override the equivalent config file options.

## How to install RT

*   Copy the jar file to a folder, cd to there and run it... see below.

## How to run RT

### Run a test or a suite locally

     RT test --test t

#### Examples 

     RT test --test basic
     RT test --test rn/suites/smoke-test

#### Comments

Output will be written to a new folder under the test-runs folder.


## Running via the Team City configuration

This is out of date.

http://sptc00.sp.local/viewType.html?buildTypeId=CI_SecondaryTestJobs_RunRt

Choose Run... and adjust the configuration parameters as needed.


## Notes

-   rt-standalone.jar contains all libraries needed to run RT so you only need a JRE
    and ensure java is on the path

-   we don't have a standard cmd file wrapper to run the java -jar and so on - but you can create one

-   regarding --app-url and localhost, there are hardcoded assumptions about our machine names 
    and domains (readinow.net, sp.local)

