const webpack = require('webpack');
const autoprefixer = require('autoprefixer');
const path = require('path');

module.exports = {
    entry: {
        vendor: ['jquery', 'tether', 'angular', 'angular-ui-router', 'lodash',
            'bootstrap', 'bootstrap-loader'],
        app: ['./src/app.js']
    },
    output: {
        path: 'public',
        publicPath: 'public',
        filename: 'bundle.js'
    },
    module: {
        loaders: [
            {test: /\.scss$/, loaders: ['style', 'css', 'postcss', 'sass']},
            {test: /\.(woff2?|ttf|eot|svg)$/, loader: 'url?limit=10000'},
            {test: /\.html$/, loader: 'html'},
            {test: /bootstrap[\/\\]dist[\/\\]js[\/\\]umd[\/\\]/, loader: 'imports?jQuery=jquery'},
            {
                test: /\.js$/,
                exclude: /(node_modules)|(bower_components)/,
                loaders: [
                    'ng-annotate',
                    'babel?' + JSON.stringify({
                        presets: ['es2015'],
                        cacheDirectory: true
                    })
                ]
            }
        ]
    },
    plugins: [
        new webpack.ProvidePlugin({
            "window.Tether": "tether"
        }),
        new webpack.optimize.CommonsChunkPlugin("vendor", "vendor.bundle.js")
    ],
    postcss: [autoprefixer]
};

