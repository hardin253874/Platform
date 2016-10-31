// Copyright 2011-2016 Global Software Innovation Pty Ltd
/**
 * Tests sit right alongside the file they are testing, which is more intuitive
 * and portable than separating `src` and `test` directories. Additionally, the
 * build process will exclude all `.spec.js` files from the build
 * automatically.
 */
describe('Administration|settings section|spec:', function() {
    beforeEach(module('app.settings'));

    it('should have a dummy test', inject(function() {
        expect(true).toBeTruthy();
    }));
});

