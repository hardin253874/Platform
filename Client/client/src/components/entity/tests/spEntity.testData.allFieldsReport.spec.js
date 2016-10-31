// Copyright 2011-2016 Global Software Innovation Pty Ltd
var entityTestData;
(function (entityTestData) {
    // https://syd1dev19.entdata.local/spapi/data/v2/entity/test/allFieldsReport?request=name,queryXml
    entityTestData.allFieldsReport =
    {
        "ids": [
          16906
        ],
        "entities": {
            "16906": {
                "7820": "AA_All Fields",
                "7712": "<Query xmlns:xsd=\"http:\/\/www.w3.org\/2001\/XMLSchema\" xmlns:xsi=\"http:\/\/www.w3.org\/2001\/XMLSchema-instance\" xmlns=\"http:\/\/enterprisedata.com.au\/readinow\/v2\/query\/2.0\">\r\n  <RootEntity xsi:type=\"ResourceEntity\" id=\"e0d5f8c6-5d40-4783-921f-7cab4dabbaf0\">\r\n    <RelatedEntities>\r\n      <Entity xsi:type=\"RelatedResource\" id=\"3cc52ba5-4c78-4103-ad73-a00c6a66ed83\">\r\n        <EntityTypeId entityRef=\"true\">core:photoFileType<\/EntityTypeId>\r\n        <RelationshipTypeId entityRef=\"true\">16680<\/RelationshipTypeId>\r\n        <RelationshipDirection>Forward<\/RelationshipDirection>\r\n      <\/Entity>\r\n    <\/RelatedEntities>\r\n    <EntityTypeId entityRef=\"true\">16994<\/EntityTypeId>\r\n  <\/RootEntity>\r\n  <Columns>\r\n    <Column id=\"82925996-1373-48a4-85f7-3c3793a7b4ba\">\r\n      <Expression xsi:type=\"IdExpression\">\r\n        <NodeId>e0d5f8c6-5d40-4783-921f-7cab4dabbaf0<\/NodeId>\r\n      <\/Expression>\r\n      <ColumnName>_Id<\/ColumnName>\r\n      <IsHidden>true<\/IsHidden>\r\n    <\/Column>\r\n    <Column id=\"94af7712-9a52-4802-9a27-164537770fd0\">\r\n      <Expression xsi:type=\"ResourceDataColumn\">\r\n        <NodeId>e0d5f8c6-5d40-4783-921f-7cab4dabbaf0<\/NodeId>\r\n        <FieldId entityRef=\"true\">core:name<\/FieldId>\r\n      <\/Expression>\r\n      <ColumnName>Name<\/ColumnName>\r\n      <DisplayName>TS-All Fields<\/DisplayName>\r\n    <\/Column>\r\n    <Column id=\"4cb5f15a-d3e3-4260-903a-670a4a6cd14b\">\r\n      <Expression xsi:type=\"ResourceExpression\">\r\n        <NodeId>3cc52ba5-4c78-4103-ad73-a00c6a66ed83<\/NodeId>\r\n        <FieldId entityRef=\"true\">7820<\/FieldId>\r\n        <CastType xsi:type=\"InlineRelationshipType\" \/>\r\n      <\/Expression>\r\n      <ColumnName>Name<\/ColumnName>\r\n      <DisplayName>New Image Field<\/DisplayName>\r\n    <\/Column>\r\n  <\/Columns>\r\n  <Conditions>\r\n    <Condition>\r\n      <Expression xsi:type=\"ResourceDataColumn\" id=\"ce11ea33-c30e-4a53-8e15-a1c59d4a4dcc\">\r\n        <NodeId>e0d5f8c6-5d40-4783-921f-7cab4dabbaf0<\/NodeId>\r\n        <FieldId entityRef=\"true\">core:name<\/FieldId>\r\n      <\/Expression>\r\n      <Operator>Unspecified<\/Operator>\r\n      <Arguments>\r\n        <TypedValue type=\"String\"><\/TypedValue>\r\n      <\/Arguments>\r\n    <\/Condition>\r\n  <\/Conditions>\r\n  <Ordering>\r\n    <OrderBy>\r\n      <Expression xsi:type=\"ColumnReference\">\r\n        <ColumnId>94af7712-9a52-4802-9a27-164537770fd0<\/ColumnId>\r\n      <\/Expression>\r\n      <Direction>Ascending<\/Direction>\r\n    <\/OrderBy>\r\n  <\/Ordering>\r\n<\/Query>",
                "8750": "00000000-0000-0000-0000-000000000000"
            }
        },
        "members": {
            "7820": {
                "alias": "core:name",
                "dt": "String"
            },
            "7712": {
                "alias": "core:queryXml",
                "dt": "Xml"
            },
            "8750": {
                "alias": "core:defaultDataViewId",
                "dt": "Guid"
            }
        }
    };

})(entityTestData || (entityTestData = {}));
