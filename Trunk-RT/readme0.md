# ReadiTest - Test Automation for ReadiNow

## Introduction to RT

ReadiTest is:

*   about testing the whole system, or end to end, tests, primarily via driving of the 
    ReadiNow Client web app.
*   initially focused on automating regression tests and for gathering performance metrics
*   later will be enhanced for exploratory and simulation

## Getting started

### Setting up your environment

*   Install a JDK. 
    *   We are using the version 7 one from the Java folder on our 
        software share <file://\\spdevnas01.sp.local\development\software\java\>.
    *   Install jdk-7u65-windows-x64.exe or similar
    *   Use default options
*   Install Leiningen
    *   the Clojure project manager and build tool. You can find a Windows installer 
        for it in the Leiningen folder on the software share.
    *   Note on WGET. Recently we have seen some issues with Leiningen failing with messages
        related to not finding its jar file and to use lein self-install to fix it, but 
        then it fails to find wget. Install wget the do the self install again. Easiest way
        to install wget is with Chocolately. From admin prompt do "cinst wget".
    *   \\spdevnas01.sp.local\development\Software\Leiningen\leiningen-installer-1.0.exe    
    *   Use default options (but don't run repl just yet)
*   Checkout RT trunk, or preferably a branch of RT
    *   E.g. https://spsvn01.sp.local/svn/entdata/SoftwarePlatform/RT/Trunk to C:\Development\Trunk-RT
*   Later.. Install IntelliJ IDEA
    *   This is the IDE .. step past for now, we return to this below


### Running a test from command line

To check if it works:

  * In a plain Command or Powershell prompt (non-admin or admin)
  * Go to working directory (C:\Development\Trunk-RT)
  * Run: lein run list
      * lein - the command line clojure runner
      * run  - run its command to run an app
      * list - our app
      * Note: only need lein run during dev in lein. If running from the jar, don't need 'lein run'.
  * will take a while the first time it runs

Trying a test run:
  
  * Chrome can be open or closed .. check your server works
  * lein run test -t perf/perf-test-1
  * Don't touch your keyboard/mouse while it's running
  * Results get stored in .\test-runs    (and there's no cleanup!)


### Set up the IntelliJ IDEA editor

Most Clojure work will be done in IntelliJ IDEA using the Cursive plug-in for Clojure.

*   Install IntelliJ IDEA
    *   General purpose IDE
    *   \\spdevnas01.sp.local\development\Software\Jetbrains\IntelliJIDEA
*   Start IntelliJ  (Windows+Q)
*   Configure Cursive
    *   Configure / Plug Ins
    *   Browse Repositories
    *   Refer to "Installing Cursive" section of https://cursiveclojure.com/userguide and copy the URI for the latest IntelliJ plugin (13.1)
        *   E.g. http://cursiveclojure.com/plugins-13.1.xml
    *   Manage Repositories
    *   Add http://cursiveclojure.com/plugins-13.1.xml
    *   OK
    *   Choose the repository from the dropdown at top of dialog
    *   Select the cursive plugin and click the Install Plugin (green button on right)
    *   Close the IntelliJ program and restart it
*   Open our project
    *   Select Open Project
    *   Select the C:\Development\Trunk-RT\project.clj
    *   When prompted to add to source control, disable it. (In particular .iml in .imda)
*   Configure the project
    *   File / Project Structure
    *   Select 'Project' on the left
    *   Under 'Project SDK' click 'New' and select JDK
    *   Locate your JDK folder .. e.g. C:\Program Files\Java\jdk1.7.0_65 and press OK
    *   A Java version should appear in the combo box next to 'New' (e.g. 1.7)
*   Set up some key bindings (seriously)
    *   File / Settings
    *   Search for Keymap and select it
    *   In the right-hand tree go to Main Menu / Tools / REPL
    *   Bind 'Load file in REPL' to Ctrl+Comma F
    *   Bind 'Switch REPL NS to current file' to Ctrl+Comma N
    *   Bind 'Send form before caret to REPL' to Ctrl+Comma L  (formerly 'Run form before cursor in REPL')
*   Set up REPL (Read Eval Print Loop) Interractive mode
    *   Click the down arrow icon that is to the left of the play button in the top-right corner. :p
    *   Select Edit Configurations
    *   Click [+] .. Closure Repl .. Local
    *   Give it a name - eg REPL
    *   Run with Leiningen should be selected
    *   Press OK (dialog closes)
    *   Press the Play arrow icon (top-right), which should be green now
    *   Wait a bit
*

### Try it out

The interractive REPL window is the bottom-right text area. Paste any tutorial code, etc here.
Try it.. Type  (* 3 5)   and press enter. You should get 15.
  
Run the following from the REPL

*   (in-ns 'rt.repl)    ;; switch to main namespace
*   (reset)
*   (list-tests "perf")
*   (run-tests :perf/perf-test-1)     


## History

A little bit of background...

*   We started with Angular Scenario for our E2E tests, but it is deprecated
*   We then moved the tests over and started building out more using Protractor + WebDriverJS on NodeJS. 
    This was ok, but not great.
    *   async tough
    *   debugging tougher
    *   WebDriverJS is a subset of WebDriver - more than once I saw a comment along the lines of 
        "when feature X in WebDriver is implemented in WebDriverJS".
*   WebDriver on the JVM is the primary and most used offering of WebDriver so to look at using this more directly. 
    We are not a Java shop and no one wants to write Java, so Clojure, a prospering pragmatic LISP language 
    hosted by the JVM was considered.
    *   Way less code than equivalent in Java
    *   The build tool Leiningen hides away lots of Java, classpath, etc hassles that we don't need, esp. not being a Java shop.
    *   Interactive development environment, aka the REPL, ... is awesome!   
*

## ReadiTest Conceptual Model

*   app driver functions
    *   wrap all the target application specifics
        * "page object" style wrappers around GUI components in the web client
        * wrappers to access web services and other environment style concerns, like server logs
*   scripts
    *   Either are Clojure functions or callable like functions
    *   Do this, get that like interface
    *   Used in test fixtures (setup and teardown)
    *   Used in test __steps__ and __checks__
    *   At present defined in code
        *   use app driver functions 
        *   use other scripts (as they are just functions)
        *   use anything available in Clojure and the JVM
    *   Can also be defined in data. At present if a test refers to to a TestScript entity then that
        entities list of actual scripts are swapped in. This allows a common set of scripts to be 
        used in different test cases (and scenarios) and get the benefit of the reporting we have for the
        immediate (ie top level) scripts of a test.
*   tests 
    *   Tests are defined in data, have and id, and include a series of steps to be performed.
    *   We have two types of tests, differentiated only by intent.
        *   test cases - test a specific piece of functionality. Tend to be fine grained. Often the setup
            is greater than the test steps.
        *   test scenarios - automation of a user story that typically uses many features. 
    *   Tests should be independent of other tests and runnable in any sequence or by themselves.
        *   Note - this rule is being violated right now in the qa-daily tests... needs to be fixed
*   suites
    *   A collection of tests. That's all.
    *   At present a suite is statically defined list of tests
    *   In future we'll have suites that are based on queries over tests on attributes such as
        estimated time to run, priority, tags. 
*


## FAQ

### Where is the ReadiTest project?

RT is a regular SVN project (with trunk, branches etc) and is a sibling project to the RN Server and 
Client projects in SVN. The path is <https://spsvn01.sp.local/svn/entdata/SoftwarePlatform/RT/Trunk>.

### How do I set up IntelliJ IDEA + Cursive?

Use the IntelliJ IDEA Community Edition.  Either download it or look in the JetBrains folder on our 
software share: <file://\\spdevnas01.sp.local\development\software\JetBrains>, however double check it
is the latest version. E.g. ideaIC-13.1.3.exe

Instructions for installing Cursive can be found here: <https://cursiveclojure.com/userguide>.

IntelliJ IDEA Community Edition is free.
Cursive is commercial but currently in EAP.

### How do I set up Light Table?

See <http://www.lighttable.com/>. Watch some vids.

### What is Midje?

Midje is a unit test framework and runner for ReadiTest. In short it tests the test code.
Clojure is a dynamically typed language and unit tests that can be automatically run 
during development and during builds is important.

## ReadiTest Project

This section is an overview of the RT project and how it it organised.

The main parts of the project are the project.clj file and the resources and src directories. 
There is also the test directory with RT unit tests. And the CI folder with shell scripts.

The __resources__ directory includes both source files, 3rd party files and files resulting from
a build. The resources directory is automatically included in any built JAR files, and the
files there are conveniently accessible in the running Clojure (JVM) application, and when it matters
will be available on the Java classpath. So it's special.

The __src__ directory has both Clojure (clj) and ClojureScript (cljs) directory trees. The clj tree is
the RT app running on the JVM. The cljs tree is about building a HTML UI for RT. These are together in
the RT project as RT can run in three different ways:

*   as a CLI app that runs tests, amongst other things, and return when done
*   as a REPL that runs an interactive session offering a simple set of commands
    to run tests, as well as expose the full Clojure and JVM environment
*   as a web server that serves up the static content for the ReadiTest UI and
    includes web services to support the ReadiTest UI

### Folder structure
     
Places to look..

  * src\clj - Our source for the ReadiTest framework itself
  * test - Testing of the test ReadiTest framework itself 
  * resources
  * src\clj\rt\repl.clj - our embedded repl .. you're in this ns by default in our embedded REPL

Places to don't bother looking..

  * src\cljs
  * src\clj\rt\server.clj
  
### Output files

The following files are output from a test run:

  * junit-results.xml - Result XML to go to CC.Net cruise control
  * out.txt - Debug output of test runner
  * test-run.edn - Object database of the test run
  * test-run-pprint.edn - Pretty printed version of the above
  * Any screen captures

## Links

*   <https://github.com/bbatsov/clojure-style-guide>
*   <http://tryclj.com>
*   <http://leiningen.org>
*   <http://www.lighttable.com/>
*   <https://cursiveclojure.com/userguide>
*   <http://www.jetbrains.com/idea/download/>

## Alternate Editors

IntelliJ IDEA and Cursive are recommended. Other options include:

    *   IntelliJ IDEA Community Edition plus [Cursive plug-in] (https://cursiveclojure.com/userguide) 
        to handle Clojure and Leiningen.
    *   Light Table for casual dabbling        
        *   Light Table
        *   Sublime plus Clojure related plugins
        *   Plenty of others with EMACS and VIM having great Clojure support and so a good choice if you are 
            already familiar with them
    *   See the sections about these two elsewhere in the documentation.    

## WebDriver tips

*   If you see __WebDriverException unknown error: Runtime.evaluate threw exception: 
    TypeError: Cannot read property 'click' of null__ then you might have the
    chrome developer debug tools window open. If so close it. (not sure what happens with Firefox).

## Clojure tips

*   Restart your REPL if things are going a bit odd. Especially if you have 
    been tweaking and reloading files a lot.

*   After first starting the REPL for RT, run the following to get Midje tests auto running.
    
        (use 'midje.repl)
        (autotest)
    
*   Exception stack traces are suppressed by default. Run (.printStackTrace \*e) to see it for
    most recent exception thrown.
    
*   Beware accidental shadowing of vars ... for example having a local var called __filter__
    that is say the text to use in a search and then wondering why you are getting odd 
    errors trying to filter a sequence... you have hidden the __filter__ function with a 
    string var.

## IntelliJ / Cursive tips

*   If your cursor is at the end of a line, it will run the line
*   If it's in the middle, you'll insert a newline
*   Press Ctrl+Enter to run a form if you're in the middle of it
*   *ns* to show current namespace
*   To change namespace: (in-ns 'rt.repl)   (for example, to change to rt.repl .. which is the src/clj/rt/repl.clj file)
    *   See ns rt.repl at the top of that file
    *   Dashes get translated to underscores in file/folder names 
*   (help) to get help
*   Click on the word help and press Ctrl+B .. this goes to the definition (like F12 in VS)
*   Show stack-trace of last error
    *   (.printStackTrace *e)
    *   *e = last exception
    *   *1 = result of the last form
*   Create yourself a scratch file
    *   Right click on src/clj/rt/scratch
    *   New Clojure Namespace
    *   Give it your name .. eg pete.clj
*   To run the tests for ReadiTest itself:
    *   (use 'midje.repl)
    *   (auto-test)


## Engineering backlog

In addition to the TODO comments in the source and TFS tickets, we have the following:

*   Convert all the date and time handling to use clj-time, converting to java Date only 
    when needed.

