// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Entity Model|spFileUpload|spec:', function() {
    "use strict";
    var _spUploadManager;

   

    beforeEach(module('sp.common.fileUpload'));

    beforeEach(inject(function(spUploadManager) {

        _spUploadManager = spUploadManager;
    }));


    it("that the upload manager exists", function () {
        
        expect(_spUploadManager).toBeTruthy();

    });
    
    it("that returns null extension for invalid filename", function () {
        expect(_spUploadManager.getFileExtension(null)).toBe(null);
        expect(_spUploadManager.getFileExtension('')).toBe(null);
    });
    
    it("that returns correct extension for valid filename", function () {
        expect(_spUploadManager.getFileExtension('test.png')).toBe('.png');
        expect(_spUploadManager.getFileExtension('test.bak.jpg')).toBe('.jpg');
    });
    
});



