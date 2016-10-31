// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('editForm|spec:|resourceScopeService', function() {
    "use strict";

    var _spResourceScope, sender1, receiver11, receiver12, sender2, receiver2, relSender1, relSender2, relReceiver1, relReceiver2, lastMessage;
    

    beforeEach(module('mod.app.resourceScopeService'));

    beforeEach(inject(function(spResourceScope) {
        expect(spResourceScope).toBeTruthy();
        _spResourceScope = spResourceScope;

        sender1 = spEntity.fromJSON(
            {
                id: 'sender1',
                typeId: '9991'
            });

        sender2 = spEntity.fromJSON(
            {
                id: 'sender2',
                typeId: '9991'
            });

        receiver11 = spEntity.fromJSON(
            {
                id: 'receiver11',
                typeId: '9991',
                'console:receiveContextFrom': sender1
            });

        receiver12 = spEntity.fromJSON(
            {
                id: 'receiver12',
                typeId: '9991',
                'console:receiveContextFrom': sender1
            });

        receiver2 = spEntity.fromJSON(
            {
                id: 'receiver2',
                typeId: '9991',
                'console:receiveContextFrom': sender2
            });

        relSender1 = spEntity.fromJSON(
            {
                id: 'relSender1',
                typeId: '9991'
            });

        relSender2 = spEntity.fromJSON(
            {
                id: 'relSender2',
                typeId: '9991'
            });


        relReceiver1 = spEntity.fromJSON(
            {
                id: 'relReceiver1',
                typeId: '9991',
                'console:receiveContextFrom': relSender1
            });

        var rel1 = spEntity.fromJSON({
            id:
                'rel1',
            typeId:
                '9992'
        });


        relReceiver2 = spEntity.fromJSON(
        {
            id: 'relReceiver2',
            typeId: '9991',
            'console:receiveContextFrom': relSender2
        });


        receiver11.received = 0;
        receiver12.received = 0;
        receiver2.received = 0;
        relReceiver1.received = 0;
        relReceiver2.received = 0;

        spResourceScope.onScopeUpdate(spResourceScope.getChannelIdFromReceiver(receiver11), function (id) { receiver11.received++; lastMessage = id; });
        spResourceScope.onScopeUpdate(spResourceScope.getChannelIdFromReceiver(receiver12), function (id) { receiver12.received++; lastMessage = id; });
        spResourceScope.onScopeUpdate(spResourceScope.getChannelIdFromReceiver(receiver2), function (id) { receiver2.received++; lastMessage = id; });

        spResourceScope.onScopeUpdate(spResourceScope.getChannelIdFromReceiver(relReceiver1), function (id) { relReceiver1.received++; lastMessage = id; });
        spResourceScope.onScopeUpdate(spResourceScope.getChannelIdFromReceiver(relReceiver2), function (id) { relReceiver2.received++; lastMessage = id; });
    }));

    describe('Test that messages are broadcasting correctly', function () {

        beforeEach(function () {
            var channelId = _spResourceScope.getChannelIdFromSender(sender1);
            _spResourceScope.sendScopeUpdate(channelId, 111);
            _spResourceScope.sendScopeUpdate(channelId, 222);
        });
        
        it('that the receivers from sender1 got the message', function () {
            expect(receiver11.received).toEqual(2);
            expect(receiver12.received).toEqual(2);
        });
        
        it('that the receivers from sender2 did not get any messages', function () {
            expect(receiver2.received).toEqual(0);
        });

        it('that the receivers received the correct id', function () {
            expect(lastMessage).toEqual(222);
        });
    });
    
    
    describe('Test that relationship messages are broadcasting correctly', function () {

        beforeEach(function () {
            var channelId = _spResourceScope.getChannelIdFromSender(relSender1);
            _spResourceScope.sendScopeUpdate(channelId, 333);
            _spResourceScope.sendScopeUpdate(channelId, 444);
        });

        it('that the receivers from relSender1 got the message', function () {
            expect(relReceiver1.received).toEqual(2);
            expect(relReceiver2.received).toEqual(0);
        });
        

        it('that the receivers received the correct id', function () {
            expect(lastMessage).toEqual(444);
        });

    });

});

