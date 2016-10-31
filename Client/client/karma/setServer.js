
if (process.argv.length > 2) {
    writepathFile(process.argv[2]);

} else {
    var os = require('os');
    var dns = require('dns');

    var h = os.hostname();
    console.log('UQDN: ' + h);

    dns.lookup(h, { hints: dns.ADDRCONFIG }, function(err, ip) {
        console.log('IP: ' + ip);
        dns.lookupService(ip, 0, function (err, hostname, service) {
            if (err) {
                console.log(err);
                return;
            }
            console.log('FQDN: ' + hostname);
            console.log('Service: ' + service);
            writepathFile(hostname);
        });
    });
}

function writepathFile(hostname) {
    console.log('set spapi server to ', hostname);

    var fs = require('fs');
    var text = "// auto generated file\nwindow.spapiBasePath='https://" + hostname + "';";
    fs.writeFile('testSupport/spapiBasePath.js', text, function (err) {
        if (err) {
            return console.log(err);
        }
        console.log('wrote the file');
    });
}

