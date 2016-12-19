module.exports = function (grunt) {
    'use strict';

    // measures the time each task takes
    require('time-grunt')(grunt);

    // load all grunt plug-ins in package.json
    require('jit-grunt')(grunt, {
        html2js: '../utils/grunt-tasks/grunt-html2js.js'
    });

    var browserRule = 'last 2 versions';

    /**
     * This is the configuration object Grunt uses to give each plugin its instructions.
     */
    var config = {
        /**
         * We read in our `package.json` file so we can access the package name and
         * version. It's already there, so we don't repeat ourselves here.
         */
        pkg: grunt.file.readJSON("package.json"),

        /**
         * The banner is the comment that is placed at the top of our compiled
         * source files. It is first processed as a Grunt template, where the `<%=`
         * pairs are evaluated based on this very configuration object.
         */
        meta: {
            banner: '/**\n' +
            ' * <%= pkg.title || pkg.name %> - v<%= pkg.version %> - <%= grunt.template.today("yyyy-mm-dd") %>\n' +
            ' * <%= pkg.homepage %>\n' +
            ' *\n' +
            ' * Copyright 2011-<%= grunt.template.today("yyyy") %> <%= pkg.author %>\n' +
            ' */\n'
            //' * Licensed <%= pkg.licenses.type %> <<%= pkg.licenses.url %>>\n' +
        },

        generatedHtmlFileHeader: 'WARNING - this is a generated file. Do not edit.',
        buildVer: '0.0.0',
        buildName: 'dev',

        /**
         * Output folders
         */
        distdir: 'dist',
        testsdir: 'dist/test',
        builddir: 'build',

        /**
         * Substitutions for use in globs
         */
        app2dirs: '+(editForm|workflow|page)',

        /**
         * An explicit list of files to be included in our build. We typically don't wildcard these
         * as often there are multiple files in the folders to choose among.
         * Order may matter.
         */
        libs: {
            js: [
                'lib/lodash/lodash.js',
                'lib/jquery/jquery.js',
                'lib/jquery-ui/jquery-ui.js',
                'lib/jquery-ellipsis/jquery.dotdotdot.js',
//                'lib/bootstrap/js/bootstrap.js',
                'lib/q/q.js',
                'lib/angular/angular.js',
                'lib/angular/angular-route.js',
                'lib/angular/angular-animate.js',
                'lib/angular/angular-cookies.js',
                'lib/angular-ui/angular-ui.js',
                'lib/angular-ui-router/angular-ui-router.js',
                'lib/angular-ui-bootstrap/ui-bootstrap-tpls.js',
                'lib/raphael/raphael.js',
                'lib/d3/d3.v3.js',
                'lib/d3-tip/index.js',
                'lib/globalize/globalize.js',
                'lib/jquery-jshashtable/jshashtable-3.0.js',
                'lib/jquery-numberformatter/jquery.numberformatter-1.2.4.js', // requires jshashtable
                'lib/angular-timer/angular-timer.js',
                'lib/angular-once/once.js',
                'lib/jquery-zeroclipboard/ZeroClipboard.js',
                'lib/hammer/hammer.js',
                'lib/angular-hammer/angular.hammer.js',
                'lib/angular-cache/angular-cache.js',
                'lib/angular-load/angular-load.js',
                'lib/ngletteravatar/ngletteravatar.js',
                'lib/showdown/showdown.js',
                'lib/jsTimezoneDetect/jstz.js',
                'lib/angular-grid/ng-grid.debug.js', // DO NOT SWAP TO MINIFIED WITHOUT CONSULTING CON. THE DEBUG AND THE MINIFIED MAY BE OUT OF SYNC
                'lib/codemirror/codemirror.js',
                'lib/codemirror/show-hint.js',
                'lib/jquery-fileupload/jquery.ui.widget.js',
                'lib/jquery-fileupload/jquery.iframe-transport.js',
                'lib/jquery-fileupload/jquery.fileupload.js',
                'lib/fastclick/fastclick.js',
                'lib/html2canvas/html2canvas.js',
                'lib/html2canvas/html2canvas.svg.js',
                'lib/angular-http-auth/http-auth-interceptor.js'
            ]
        },

        /**
         * Plain old Javascript source files that need to load first
         */
        poj: {
            js: []
        },

        /**
         * Configure the 'clean' task
         */
        clean: ['<%= distdir %>', '<%= builddir %>', '<%= testsdir %>', 'tests', 'deploy/*.zip'],

        /**
         * less handles our LESS compilation and uglification automatically. Only
         * our main app less file is included in compilation; all other files must be
         * imported from this file.
         */
        less: {
            /* we are not currently splitting out the builders less files as it may be unreliable */
            build: {
                files: {'<%= distdir %>/assets/<%= pkg.name %>.css': 'src/less/main.less'},
                options: {}
            }

        },

        sass: {
            options: {sourceMap: true},
            dist: {
                files: {'<%= distdir %>/assets/app.css': 'src/styles/app.scss'}
            }
        },

        stylelint: {
            options: {
                configFile: '.stylelintrc',
                format: 'less'
            },
            src: 'src/**/*.{css,less}'
        },

        /**
         * clean up css wrt to prefixes
         */
        postcss: {
            options: {
                map: false,
                processors: [
                    require('autoprefixer')({
                        browsers: [browserRule],
                        add: true,
                        remove: true
                    })
                ]
            },
            dist: {
                src: '<%= distdir %>/assets/<%= pkg.name %>.css'
            }
        },

        /**
         * Static type checking
         */
        // flow: {
        //     once: {
        //         src: 'src/**/*.js',
        //         options: {
        //             server: false
        //         }
        //     },
        //     watch: {
        //         src: 'src/**/*.js',
        //         options: {
        //             server: true
        //         }
        //     }
        // },

        /**
         * HTML2JS takes all of your template files and places them into JavaScript
         * files as strings that are added to AngularJS's template cache.
         * This means that the templates too become part of the initial payload as one JavaScript file.
         */
        html2js: {
            // must call component, not components for backward compat.... maybe change later
            component: {
                src: ['src/components/**/*.tpl.html'],
                base: 'src/components',
                dest: '<%=builddir%>/components/tmpl'
            },
            app: {
                src: ['src/app/**/*.tpl.html'],
                base: 'src/app',
                dest: '<%=builddir%>/app/tmpl'
            },
            builders: {
                src: ['src/builders/**/*.tpl.html'],
                base: 'src/builders',
                dest: '<%=builddir%>/builders/tmpl'
            }
        },

        /**
         * Transpile es6/es2015
         */
        babel: {
            options: {
                sourceMap: false, // off as not working well when we concat
                presets: ['es2015'],
                plugins: ['transform-flow-strip-types'],
                compact: false // we have some > 100K files we want formatted
            },

            //Experimenting with concat THEN babel to help with source map handling
            // components_src: {
            //     // this one is all three for normal builds to save on the per-babel spin up time
            //     files: [
            //         {
            //             src: ['<%= distdir %>/js/<%= pkg.name %>_components.es6'],
            //             dest: '<%= distdir %>/js/<%= pkg.name %>_components.js'
            //         }
            //     ],
            //     options: {
            //         sourceMap: true
            //     }
            // },

            src: {
                // this one is all src for normal builds to save on the per-babel spin up time
                files: [
                    {
                        expand: true,
                        cwd: 'src',
                        src: ['**/*.+(es6|js)', '!**/*.+(spec|intg).+(es6|js)'],
                        dest: 'build',
                        ext: '.js',
                        extDot: 'last'
                    }
                ]
            },
            components: {
                files: [
                    {
                        expand: true,
                        cwd: 'src',
                        src: ['components/**/*.+(es6|js)', '!**/*.+(spec|intg).+(es6|js)'],
                        dest: 'build',
                        ext: '.js',
                        extDot: 'last'
                    }
                ]
            },
            app: {
                files: [
                    {
                        expand: true,
                        cwd: 'src',
                        src: ['app/**/*.+(es6|js)', '!**/*.+(spec|intg).+(es6|js)'],
                        dest: 'build',
                        ext: '.js',
                        extDot: 'last'
                    }
                ]
            },
            builders: {
                files: [
                    {
                        expand: true,
                        cwd: 'src',
                        src: ['builders/**/*.+(es6|js)', '!**/*.+(spec|intg).+(es6|js)'],
                        dest: 'build',
                        ext: '.js',
                        extDot: 'last'
                    }
                ]
            },
            tests: {
                files: [
                    {
                        expand: true,
                        cwd: 'src',
                        src: ['**/*.+(spec|intg).+(es6|js)'],
                        dest: 'build/es6tests',
                        ext: '.js',
                        extDot: 'last'
                    }
                ]
            }
            // ,
            // testPages: {
            //     files: [
            //         {
            //             expand: true,
            //             cwd: '',
            //             src: ['testPages/**/*.js', '!testPages/**/*-converted.js'],
            //             dest: '',
            //             ext: '-converted.js',
            //             extDot: 'last'
            //         }
            //     ]
            // }
        },

        /**
         * `grunt copy` just copies files from A to B. We use it here to copy our
         * project assets (images, fonts, etc.) into our distribution directory.
         */
        sync: {
            src: {
                files: [
                    {
                        src: ['**'],
                        dest: '<%=distdir%>/assets/',
                        cwd: 'src/assets',
                        expand: true
                    },
                    {
                        src: ['Web.config'],
                        dest: '<%=distdir%>',
                        cwd: '',
                        expand: true
                    },
                    {
                        src: ['**'],
                        dest: '<%=distdir%>/customerrors/',
                        cwd: 'src/customerrors',
                        expand: true
                    }
                ]
            },
            vendor: {
                files: [
                    {
                        src: [
                            'lib/angular/*.css',
                            'lib/boilerplate/*.css',
                            'lib/bootstrap/img/*.*',
                            'lib/codemirror/*.css',
                            'lib/angular-grid/*.css',
                            'lib/jquery-ui/css/*.css',
                            'lib/jquery-ui/css/images/*.*',
                            'lib/jquery-zeroclipboard/ZeroClipboard.swf'
                        ],
                        dest: '<%=distdir%>/assets/',
                        cwd: '',
                        expand: true
                    },
                    {
                        src: [
                            'angular/i18n/angular-locale_*.js',
                            'globalize/cultures/globalize.culture.*.js'
                        ],
                        dest: '<%=distdir%>/lib/',
                        cwd: 'lib',
                        expand: true
                    },
                    {
                        src: [
                            '*.*'
                        ],
                        dest: '<%=distdir%>/assets/fonts/',
                        cwd: 'lib/bootstrap/fonts',
                        expand: true
                    }
                ]
            },
            tests: {
                files: [
                    {
                        src: ['**/*.+(spec|intg).js'],
                        dest: '<%= testsdir %>/src',
                        cwd: 'build/es6tests',
                        expand: true
                    },
                    {
                        src: ['**/*.*', '!./**/*.tmpl'],
                        dest: '<%= testsdir %>',
                        cwd: 'karma',
                        expand: true
                    },
                    {
                        src: [
                            'lib/angular/angular.js',
                            'lib/angular/angular-mocks.js',
                            'lib/jasmine/*.*'
                        ],
                        dest: '<%= testsdir %>/',
                        cwd: '',
                        expand: true
                    }
                ]
            }
        },

        /**
         * `grunt concat` concatenates multiple source files into a single file.
         */
        concat: {
            options: {
                sourceMap: true
            },
            //Experimenting with concat THEN babel to help with source map handling
            // all: {
            //     src: [
            //         'src/components/**/*.module.+(js|es6)',
            //         'src/components/**/*.+(js|es6)',
            //         '!src/**/*.+(spec|intg).+(js|es6)'
            //     ],
            //     dest: '<%= distdir %>/js/<%= pkg.name %>_components.es6'
            // },
            components: {
                options: {
                    banner: '<%= meta.banner %>'
                },
                src: ['build/components/**/*.module.js', 'build/components/**/*.js', '!build/**/*.+(spec|intg).js'],
                dest: '<%= distdir %>/js/<%= pkg.name %>_components.js'
            },
            app: {
                options: {
                    banner: '<%= meta.banner %>'
                },
                src: ['build/app/**/*.module.js', 'build/app/**/*.js', '!build/app/**/<%=app2dirs%>/**', '!build/**/*.+(spec|intg).js'],
                dest: '<%= distdir %>/js/<%= pkg.name %>_app.js'
            },
            app2: {
                options: {
                    banner: '<%= meta.banner %>'
                },
                src: ['build/app/**/<%=app2dirs%>/**/*.module.js', 'build/app/**/<%=app2dirs%>/**/*.js', '!build/**/*.+(spec|intg).js'],
                dest: '<%= distdir %>/js/<%= pkg.name %>_app2.js'
            },
            builders: {
                options: {
                    banner: '<%= meta.banner %>'
                },
                src: ['build/builders/**/*.module.js', 'build/builders/**/*.js', '!build/**/*.+(spec|intg).js'],
                dest: '<%= distdir %>/js/<%= pkg.name %>_builders.js'
            },
            libs: {
                options: {
                    sourceMap: false
                },
                src: ['<%= libs.js %>'],
                dest: '<%= distdir %>/js/libs.js'
            }
        },

        /**
         * Use ng-annotate to annotate the sources before minifying
         */
        ngAnnotate: {
            options: {
                singleQuotes: true
            },
            components: {
                src: ['<%= distdir %>/js/<%= pkg.name %>_components.js'],
                dest: '<%= distdir %>/js/<%= pkg.name %>_components.annotated.js'
            },
            app: {
                src: ['<%= distdir %>/js/<%= pkg.name %>_app.js'],
                dest: '<%= distdir %>/js/<%= pkg.name %>_app.annotated.js'
            },
            app2: {
                src: ['<%= distdir %>/js/<%= pkg.name %>_app2.js'],
                dest: '<%= distdir %>/js/<%= pkg.name %>_app2.annotated.js'
            },
            builders: {
                src: ['<%= distdir %>/js/<%= pkg.name %>_builders.js'],
                dest: '<%= distdir %>/js/<%= pkg.name %>_builders.annotated.js'
            }
        },

        /**
         * Minify the sources!
         */
        uglify: {
            options: {
                banner: '<%= meta.banner %>',
                sourceMap: true
            },
            components: {
                files: {
                    '<%= distdir %>/js/<%= pkg.name %>_components.min.js': ['<%= distdir %>/js/<%= pkg.name %>_components.annotated.js']
                }
            },
            app: {
                files: {
                    '<%= distdir %>/js/<%= pkg.name %>_app.min.js': ['<%= distdir %>/js/<%= pkg.name %>_app.annotated.js']
                }
            },
            app2: {
                files: {
                    '<%= distdir %>/js/<%= pkg.name %>_app2.min.js': ['<%= distdir %>/js/<%= pkg.name %>_app2.annotated.js']
                }
            },
            builders: {
                files: {
                    '<%= distdir %>/js/<%= pkg.name %>_builders.min.js': ['<%= distdir %>/js/<%= pkg.name %>_builders.annotated.js']
                }
            },
            libs: {
                files: {
                    '<%= distdir %>/js/libs.min.js': ['<%= distdir %>/js/libs.js']
                }
            }
        },

        /**
         * Zip up our packages
         */
        compress: {
            dist: {
                options: {
                    archive: './deploy/<%= pkg.name %>.zip'
                },
                files: [
                    {
                        src: [
                            '*.html', 'assets/**', 'customerrors/**', 'js/*.min.js',
                            'lib/globalize/cultures/**', 'lib/angular/i18n/**', 'Web.config'
                        ],
                        cwd: '<%= distdir %>',
                        expand: true
                    }

                ]
            },
            sourceMap: {
                options: {
                    archive: './deploy/<%= pkg.name %>-dbg.zip'
                },
                files: [
                    {
                        src: ['js/*.*'],
                        cwd: '<%= distdir %>',
                        expand: true
                    }

                ]
            },
            tests: {
                options: {
                    archive: './deploy/<%= pkg.name %>-tests.zip'
                },
                files: [
                    {
                        src: ['**'],
                        cwd: '<%= testsdir %>',
                        expand: true
                    }

                ]
            }

        },

        eslint: {
            options: {
                format: 'visualstudio',
            },
            components: [
                'src/components/**/*.module.+(js|es6)', 'src/components/**/*.+(js|es6)',
                '!src/**/*.+(spec|intg).+(js|es6)', '!src/components/placeholders/**/*'
            ],
            app: [
                'src/app/**/*.module.+(js|es6)', 'src/app/**/*.+(js|es6)',
                '!src/app/**/<%=app2dirs%>/**', '!src/**/*.+(spec|intg).+(js|es6)'
            ],
            app2: [
                'src/app/**/<%=app2dirs%>/**/*.module.+(js|es6)', 'src/app/**/<%=app2dirs%>/**/*.+(js|es6)',
                '!src/**/*.+(spec|intg).+(js|es6)'
            ],
            builders: [
                'src/builders/**/*.module.+(js|es6)', 'src/builders/**/*.+(js|es6)',
                '!src/**/*.+(spec|intg).+(js|es6)'
            ],
            test: ['src/**/*.+(spec|intg).+(js|es6)']
        },

        /**
         * A task for generating doc
         */
        shell: {
            doc: {
                command: 'node node_modules/jsdoc/jsdoc src/components src/app src/builders -r -d doc -t lib/sp-jsdoc'
            }
        },

        /**
         * The Karma configurations.
         * These should follow a sync:tests task to ensure the test folder is up to date.
         */
        karma: {
            config: {
                singleRun: true
            },
            watch: {
                // unit testing during development
                configFile: '<%= testsdir %>/karma-unit.conf.js',
                singleRun: false,
                background: true,
                reporters: ['progress', 'dots']
                //runnerPort: 9101
            },
            unit: {
                // unit testing as part of build process
                configFile: '<%= testsdir %>/karma-unit.conf.js',
                reporters: ['teamcity', 'progress', 'junit', 'dots']
            },
            coverage: {
                configFile: '<%= testsdir %>/karma-coverage.conf.js',
                reporters: ['progress', 'junit', 'dots', 'coverage']
            },
            intg: {
                configFile: '<%= testsdir %>/karma-intg.conf.js'
            }
        },

        /**
         * Watch for file changes and do stuff.
         * Assume we are watching only for *debug* builds
         */
        watch: {
            /**
             * By default, we want the Live Reload to work for all tasks; this is
             * overridden in some tasks where browser resources are unaffected.
             * It runs by default on port 35729.
             */
            options: {
                livereload: true,
                interval: 5007
            },
            // flow: {
            //     files: ['src/**/*.js'],
            //     tasks: ['flow:watch'] // Get the status from the server
            // },
            // components: {
            //     files: ['<%=watchSrcDir%>/components/**/*.js', '!src/**/*.+(spec|intg).js'],
            //     tasks: [
            //         'notifyGrowl:ReadiNow Client:Building components script file(s)...',
            //         'dev:components', 'ready',
            //         'notifyGrowl:ReadiNow Client:Script file(s) updated!',
            //         'newer:eslint:components'
            //     ]
            // },
            components_es6: {
                files: ['src/components/**/*.+(es6|js)', '!src/**/*.+(spec|intg).+(es6|js)'],
                tasks: [
                    'notifyGrowl:ReadiNow Client:Building components es6 file(s)...',
                    'newer:babel:components', 'dev:components', 'ready',
                    'notifyGrowl:ReadiNow Client:Script file(s) updated!'
                ]
            },
            components_templates: {
                files: ['src/components/**/*.tpl.html'],
                tasks: [
                    'notifyGrowl:ReadiNow Client:Building components template file(s)...',
                    'html2js:component', 'dev:components', 'ready',
                    'notifyGrowl:ReadiNow Client:Script file(s) updated!'
                ]
            },
            // app: {
            //     files: ['src/app/**/*.js', '!src/app/**/<%=app2dirs%>/**', '!src/**/*.+(spec|intg).js'],
            //     tasks: [
            //         'notifyGrowl:ReadiNow Client:Building app script file(s)...',
            //         'dev:app', 'index:dev', 'ready',
            //         'notifyGrowl:ReadiNow Client:Script file(s) updated!',
            //         'newer:eslint:app'
            //     ]
            // },
            app_es6: {
                files: ['src/app/**/*.+(es6|js)', '!src/app/**/<%=app2dirs%>/**', '!src/**/*.+(spec|intg).+(es6|js)'],
                tasks: [
                    'notifyGrowl:ReadiNow Client:Building app es6 file(s)...',
                    'newer:babel:app', 'dev:app', 'ready',
                    'notifyGrowl:ReadiNow Client:Script file(s) updated!'
                ]
            },
            app_templates: {
                files: ['src/app/**/*.tpl.html', '!src/app/**/<%=app2dirs%>/**'],
                tasks: [
                    'notifyGrowl:ReadiNow Client:Building app template file(s)...',
                    'html2js:app', 'dev:app', 'ready',
                    'notifyGrowl:ReadiNow Client:Script file(s) updated!'
                ]
            },
            // app2: {
            //     files: ['src/app/**/<%=app2dirs%>/**/*.js', '!src/**/*.+(spec|intg).js'],
            //     tasks: [
            //         'notifyGrowl:ReadiNow Client:Building app script file(s)...',
            //         'dev:app2', 'index:dev', 'ready',
            //         'notifyGrowl:ReadiNow Client:Script file(s) updated!',
            //         'newer:eslint:app2'
            //     ]
            // },
            app2_es6: {
                files: ['src/app/**/<%=app2dirs%>/**/*.+(es6|js)', '!src/**/*.+(spec|intg).+(es6|js)'],
                tasks: [
                    'notifyGrowl:ReadiNow Client:Building app es6 file(s)...',
                    'newer:babel:app', 'dev:app2', 'ready',
                    'notifyGrowl:ReadiNow Client:Script file(s) updated!'
                ]
            },
            app2_templates: {
                files: ['src/app/**/<%=app2dirs%>/**/*.tpl.html'],
                tasks: [
                    'notifyGrowl:ReadiNow Client:Building app template file(s)...',
                    'html2js:app', 'dev:app2', 'ready',
                    'notifyGrowl:ReadiNow Client:Script file(s) updated!'
                ]
            },
            // builders: {
            //     files: ['src/builders/**/*.js', '!src/**/*.+(spec|intg).js'],
            //     tasks: [
            //         'notifyGrowl:ReadiNow Client:Building builder script file(s)...',
            //         'dev:builders', 'ready',
            //         'notifyGrowl:ReadiNow Client:Script file(s) updated!',
            //         'newer:eslint:builders'
            //     ]
            // },
            builders_es6: {
                files: ['src/builders/**/*.+(es6|js)', '!src/**/*.+(spec|intg).+(es6|js)'],
                tasks: [
                    'notifyGrowl:ReadiNow Client:Building builder es6 file(s)...',
                    'newer:babel:builders', 'dev:builders', 'ready',
                    'notifyGrowl:ReadiNow Client:Script file(s) updated!'
                ]
            },
            builders_templates: {
                files: ['src/builders/**/*.tpl.html'],
                tasks: [
                    'notifyGrowl:ReadiNow Client:Building builder template file(s)...',
                    'html2js:builders', 'dev:builders', 'ready',
                    'notifyGrowl:ReadiNow Client:Script file(s) updated!'
                ]
            },
            es6tests: {
                files: ['src/**/*.+(spec|intg).+(es6|js)'],
                tasks: [
                    'notifyGrowl:ReadiNow Client:Building es6 test file(s)...',
                    'newer:babel:tests', 'sync:tests', 'testConfig:dev',
                    'karma:watch:run',
                    'notifyGrowl:ReadiNow Client:Build done!'
                ]
            },
            assets: {
                files: ['src/assets/**/*'],
                tasks: [
                    'notifyGrowl:ReadiNow Client:Copying assets...',
                    'sync:assets', 'ready',
                    'notifyGrowl:ReadiNow Client:Asset(s) updated!'
                ]
            },
            html: {
                files: ['src/index*.tpl.html'],
                tasks: [
                    'notifyGrowl:ReadiNow Client:Building Html file(s)...',
                    'index:dev', 'ready',
                    'notifyGrowl:ReadiNow Client:Html file(s) updated!'
                ]
            },
            less: {
                files: ['src/**/*.less'],
                tasks: [
                    'notifyGrowl:ReadiNow Client:Building less file(s)...',
                    'less', 'postcss', 'ready',
                    'notifyGrowl:ReadiNow Client:Less file(s) updated!'
                ]
            },
            sass: {
                files: ['src/**/*.scss'],
                tasks: [
                    'notifyGrowl:ReadiNow Client:Building scss file(s)...',
                    'sass', 'postcss', 'ready',
                    'notifyGrowl:ReadiNow Client:Scss file(s) updated!'
                ]
            },
            // unittest: {
            //     files: ['src/**/*.spec.js'],
            //     tasks: [
            //         'notifyGrowl:ReadiNow Client:Preparing unit test(s)...',
            //         'prepTests', 'karma:watch:run', 'ready',
            //         'notifyGrowl:ReadiNow Client:Unit test(s) prepared!',
            //         'newer:eslint:tests'
            //     ],
            //     options: {
            //         livereload: false
            //     }
            // },
            // intgtest: {
            //     files: ['src/**/*.intg.js'],
            //     tasks: [
            //         'notifyGrowl:ReadiNow Client:Preparing integration test(s)...',
            //         'prepTests', 'karma:watch:run', 'ready',
            //         'notifyGrowl:ReadiNow Client:Integration test(s) prepared!',
            //         'newer:eslint:tests'
            //     ],
            //     options: {
            //         livereload: false
            //     }
            // },
            // testPages: {
            //     files: ['testPages/**/*.js', '!testPages/**/*-converted.js', 'testPages/**/*.html'],
            //     tasks: [
            //         'notifyGrowl:ReadiNow Client:Building testPages es6 file(s)...',
            //         'newer:babel:testPages',
            //         'notifyGrowl:ReadiNow Client:Script file(s) updated!'
            //     ]
            // }

        }
    };

    grunt.initConfig(config);

    //Experimenting with concat THEN babel to help with source map handling
    // grunt.task.registerTask("configureBabel", "configures babel options", function() {
    //     var path = 'dist/js/SoftwarePlatform.client_components.es6.map';
    //     config.babel.components_src.options.inputSourceMap = grunt.file.readJSON(path);
    // });

    grunt.config.set('buildVer', process.env.CCNetLabel || grunt.config('buildVer'));
    grunt.config.set('buildName', process.env.CCNetProject || grunt.config('buildName'));
    grunt.config.set('buildLabel', grunt.config('buildVer') + '-' + grunt.config('buildName'));

    /**
     * Tasks
     */

    grunt.registerTask('jshint', ['eslint']);

    grunt.registerTask('default', ['clean', 'eslint', 'dev']);

    grunt.registerTask('help', printHelp);

    grunt.registerTask('dev', 'dev build - incremental (no clean), no linting or tests', [
        'html2js', 'newer:babel:src', 'concat', 'less', 'postcss',
        'sync:src', 'sync:vendor', 'index:dev', 'prepTests'
    ]);

    grunt.registerTask('prod', 'prod build - incremental (no clean), no linting or tests', [
        'dev', 'ngAnnotate', 'uglify', 'index:prod'
    ]);

    grunt.registerTask('prepTests', ['newer:babel:tests', 'sync:tests', 'testConfig:dev']);
    grunt.registerTask('unitwatch', 'Prepare unit tests to run when a watch fires.\n' +
        'You need to specify a watch after using this, see examples.\n' +
        'Expects a dev or prod build to exist.\n' +
        'May specify a search term to limit which tests are run (but the watch fires on all).\n' +
        'For example grunt unitwatch:editForm watch' +
        'Or grunt unitwatch:board watch:es6tests\n',
        function (searchTerm) {
            if (searchTerm) grunt.config.set('COVER', searchTerm);
            grunt.task.run(['prepTests', 'karma:watch']);
        });
    grunt.registerTask('unit', function (searchTerm) {
        if (searchTerm) grunt.config.set('COVER', searchTerm);
        grunt.task.run(['prepTests', 'karma:unit']);
    });
    grunt.registerTask('intg', ['prepTests', 'karma:intg']);

    grunt.registerTask('coverage', 'Run unit tests with code coverage.\n' +
        'Expects a dev or prod build to exist.\n' +
        'May specify a search term to limit which tests are run.\n' +
        'For example grunt coverage:editForm',
        function (searchTerm) {
            grunt.file.copy('karma/tree-summarizer-patched.js', 'node_modules/istanbul/lib/util/tree-summarizer.js', {process: grunt.template.process});
            grunt.file.copy('karma/tree-summarizer-patched.js', 'node_modules/karma-coverage/node_modules/istanbul/lib/util/tree-summarizer.js', {process: grunt.template.process});
            if (searchTerm) grunt.config.set('COVER', searchTerm);
            grunt.task.run(['babel:tests', 'sync:tests', 'testConfig:coverage', 'karma:coverage',
                'notifyGrowl:ReadiNow Client:Coverage report at: client/tests/coverage']);
        });

    grunt.registerTask('doc', ['shell:doc']);

    grunt.registerTask('setServer', 'Set the server path for use by tests. ' +
        'By default it sets the server to the current machine assuming https and on readinow.net, ' +
        'otherwise can be a parameter specifying the full path including protocol. ',
        function (path) {
            setServer(this, path);
        });

    grunt.task.registerTask("altwatch", "watches with alternate tasks (if defined)", function (param) {
        for (var k in config.watch) {
            if (config.watch[k].tasks2) {
                config.watch[k].tasks = config.watch[k].tasks2;
            }
        }
        grunt.task.run([param ? 'watch:' + param : 'watch']);
    });

    grunt.registerTask('package', ['compress']);

    /**
     * a dummy task only here to separate the targets/tasks above with those 'internal' tasks below
     * when someone does a grunt -help
     */
    grunt.registerTask('-------------', '----------------------------------', ['default']);

    /**
     * The index.html template includes the stylesheet and javascript sources
     * based on dynamic names calculated in this Gruntfile. This task compiles it.
     */

    grunt.registerTask('index:dev', 'Produce index.html (and dev.html) for dev', function () {
        grunt.config.set('DEV', true);
        grunt.config.set('buildLabel', grunt.config('buildLabel') + '-DEBUG');
        copyIndexFile('index.html');
        copyIndexFile('dev.html'); // for use even after a prod build of index.html
    });

    grunt.registerTask('index:prod', 'Produce index.html for a prod build', function () {
        grunt.config.set('DEV', false);
        grunt.config.set('buildLabel', grunt.config('buildLabel').replace('-DEBUG', ''));
        copyIndexFile('index.html');
    });

    /**
     * Aliases to help watch config
     */

    grunt.registerTask('dev:components', ['concat:components', 'index:dev', 'testConfig:dev']);
    grunt.registerTask('dev:app', ['concat:app', 'index:dev', 'testConfig:dev']);
    grunt.registerTask('dev:app2', ['concat:app2', 'index:dev', 'testConfig:dev']);
    grunt.registerTask('dev:builders', ['concat:builders', 'index:dev', 'testConfig:dev']);
    grunt.registerTask('sync:assets', ['sync:src']);


    /**
     * Some tasks to assist testing
     */

    grunt.registerTask('testConfig:dev', 'Process karma configs and the jasmineIntg.html for a debug build', function () {
        grunt.config.set('DEV', true);
        copyTestConfigs();
    });

    grunt.registerTask('testConfig:prod', 'Process karma configs and the jasmineIntg.html for a release build', function () {
        grunt.config.set('DEV', false);
        copyTestConfigs();
    });

    grunt.registerTask('testConfig:coverage', 'Process karma configs and the jasmineIntg.html for a debug build', function () {
        copyCoverageConfig();
    });

    grunt.registerTask('notifyGrowl', 'Raise Growl Notification', growl);

    grunt.registerTask('ready', 'Ready to refresh', function () {
        grunt.log.write('READY to reload in browser...'.green);
        grunt.log.write();
    });

    grunt.registerTask('autoprefixer-debug', 'show information about our autoprefixer configuration', function () {
        var autoprefixer = require('autoprefixer');
        var info = autoprefixer({browsers: [browserRule]}).info();
        grunt.log.write(info);

    });

    /**
     * backward compat ...
     */

    grunt.registerTask('jshintwatch', 'jshint then watch', function () {
        // Use the force option to ensure that the file watch starts even if the hint fails
        grunt.option('force', true);
        grunt.task.run(['jshint', 'watch']);
    });

    grunt.registerTask('fastwatch', [
        'dev', 'ready', 'notifyGrowl:ReadiNow Client:File watcher running...', 'jshintwatch'
    ]);

    grunt.registerTask('build-release-notests', ['clean', 'jshint', 'prod']);
    grunt.registerTask('build-debug-notests', ['clean', 'jshint', 'dev']);

    /**
     * Helper functions
     */

    function printHelp() {
        var text = 'grunt -h to see a list of tasks' +
            '\nother tasks include clean, jshint, watch, unit, intg' +
            '\ntodo: write this up better';
        grunt.log.write(text.green);
        grunt.log.write();
    }

    function growl(title, message) {
        var growlNotifyPath = '..\\utils\\growlnotify.exe';

        if (grunt.file.exists(growlNotifyPath)) {
            if (!message) {
                message = '';
            }

            var options = {
                cmd: growlNotifyPath,
                opts: {stdio: 'inherit', detached: true},
                args: ['/t:"' + title + '"', message]
            };
            grunt.util.spawn(options, function () {
                // Need an function here or an error will be thrown.
            });
        }
    }

    function copyIndexFile(indexFileName) {
        var distDir = grunt.config.get('distdir');
        indexFileName = indexFileName || 'index.html';
        grunt.file.copy('src/index.tpl.html', distDir + '/' + indexFileName, {process: grunt.template.process});
    }

    function copyTestConfigs() {
        var testsDir = grunt.config.get('testsdir');
        grunt.file.copy('karma/jasmineIntg.html.tmpl', testsDir + '/jasmineIntg.html', {process: grunt.template.process});
        grunt.file.copy('karma/jasmineUnit.html.tmpl', testsDir + '/jasmineUnit.html', {process: grunt.template.process});
        grunt.file.copy('karma/karma-unit.conf.js.tmpl', testsDir + '/karma-unit.conf.js', {process: grunt.template.process});
        grunt.file.copy('karma/karma-intg.conf.js.tmpl', testsDir + '/karma-intg.conf.js', {process: grunt.template.process});
    }

    function copyCoverageConfig() {
        var testsDir = grunt.config.get('testsdir');
        grunt.file.copy('karma/karma-coverage.conf.js.tmpl', testsDir + '/karma-coverage.conf.js', {process: grunt.template.process});
    }

    function setServer(o, path) {

        if (arguments.length < 2 || !arguments[1]) {
            grunt.log.writeln(o.name + ", no args. defaulting server to Fully Qualified Domain Name of the machine.");
            var os = require("os");
            var cp = require("child_process");

            var asyncTaskCompletion = o.async();
            cp.exec('systeminfo | findstr /B /C:"Domain"', function (err, stdout, stderr) {
                if (err) {
                    grunt.log.writeln(o.name + ", error: " + err);
                    asyncTaskCompletion(false);
                    throw err;
                }

                var domain = stdout.substring(7).trim();
                var hostname = os.hostname();
                var fqdn = hostname + '.' + domain;

                var webApiPath = 'https://' + fqdn + '/';
                grunt.config.set('webApiRoot', webApiPath);
                grunt.log.writeln(o.name + ", server set to: " + webApiPath);
                generateSpApiBasePathConfig();
                asyncTaskCompletion(true);
            });
        } else {
            grunt.config.set('webApiRoot', path);
            grunt.log.writeln(o.name + ", " + path);
            generateSpApiBasePathConfig();
        }
    }

    function generateSpApiBasePathConfig() {
        var basePath = 'karma/testSupport/spapiBasePath.js';
        grunt.file.copy(basePath + '.tmpl', basePath, {process: grunt.template.process});
    }
};

