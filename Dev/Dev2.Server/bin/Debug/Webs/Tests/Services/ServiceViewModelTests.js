﻿/// <reference path="../../wwwroot/Scripts/_references.js" />
/// <reference path="~/wwwroot/Scripts/Services/DbServiceViewModel.js" />
/// <reference path="~/wwwroot/Scripts/Dialogs/SaveViewModel.js" />
/// <reference path="~/wwwroot/Scripts/Sources/DbSourceViewModel.js" />

//module("ServiceViewModelTests", {
//    setup: function() {
//        $.mockjaxSettings = {
//            contentType: "text/json",
//            dataType: "json"
//        };
//    },
//    teardown: function () {
//        $.mockjaxClear();
//    }
//});

module("Service Model Constructor");

test("ConstructorWithInvalidResourcedIDExpectedIsEditingIsFalse", function () {

    var model = new DbServiceViewModel("dbServiceSaveDialogContainer", "xxxx");
    ok(!model.isEditing, "Is IsEditing False");
});

test("ConstructorWithValidResourcedIDExpectedIsEditingIsTrue", function () {

    var model = new DbServiceViewModel("dbServiceSaveDialogContainer", "{97A9EFED-4127-4421-BCE8-1AC90CAFB7D4}");
    ok(model.isEditing, "Is IsEditing True");
});

test("ConstructorWithInvalidResourcedIDExpectedTitleContainsNew", function () {

    var model = new DbServiceViewModel("dbServiceSaveDialogContainer", "xxxx");
    ok(model.title().indexOf("New") != -1, "Does Title Contain New");
});

test("ConstructorExpectedModelResourceTypeIsDbService", function () {

    var model = new DbServiceViewModel();
    equal(model.data.resourceType(), "DbService", "Is Resource Type DbService");
});

module("Service Model Form Validation");

test("FormValidationWithIsNotEdittingAndMethodNameChangedAndHasTestResultsAndOneRecordSetExpectedFormIsValid", function () {

    var model = new DbServiceViewModel();
    model.isEditing = false;
    model.methodNameChanged(true);
    model.hasTestResults(true);
    model.data.recordset.Records(["testvar"]);
    ok(model.isFormValid(), "Is Form Valid" );
});

test("FormValidationWithIsEdittingAndMethodNameChangedAndHasTestResultsAndOneRecordSetExpectedFormIsValid", function () {

    var model = new DbServiceViewModel();
    model.isEditing = true;
    model.methodNameChanged(true);
    model.hasTestResults(true);
    model.data.recordset.Records(["testvar"]);
    ok(model.isFormValid(), "Is Form Valid");
});

test("FormValidationWithIsEdittingAndMethodNameNotChangedAndOneRecordSetExpectedFormIsValid", function () {

    var model = new DbServiceViewModel();
    model.isEditing = true;
    model.hasTestResults(true);
    model.data.recordset.Records(["testvar"]);
    ok(model.isFormValid(), "Is Form Valid");
});

test("IsFormValidWithoutTestResultsExpectedFormIsNotValid", function () {

    var model = new DbServiceViewModel();
    model.isEditing = false;
    model.hasTestResults(false);
    model.data.recordset.Records(["testvar"]);
    ok(!model.isFormValid(), "Is Form Not Valid");
});

test("MethodNameSubscriberWithChangingMethodNameExpectedSubscriberChangesMethodName", function () {

    var model = new DbServiceViewModel();
    model.data.method.Name("change method name");
    ok(model.methodNameChanged(), "Did Subscriber Change Method Name");
});

test("IsFormValidWithIsEdittingAndMethodNameChangedAndHasNoTestResultsExpectedFormIsNotValid", function () {

    var model = new DbServiceViewModel();
    model.isEditing = true;
    model.methodNameChanged(true);
    model.data.recordset.Records(["testvar"]);
    ok(!model.isFormValid(), "Is Form Not Valid");
});

test("MethodSearchResultsWithoutTermExpectedMethodsReturned", function() {

    var model = new DbServiceViewModel();
    model.sourceMethods(["test method", "another test method"]);
    ok(model.sourceMethodSearchResults()[1], "Did Methods Return");
});

test("SourceNameSubscriberWithChangingSourceNameExpectedSubscriberResetsMethodName", function () {
    
    var model = new DbServiceViewModel();
    model.data.method.Name("new method name");
    model.data.source("change source name");
    equal(model.data.method.Name(), "", "Did Methods Return");
});

module("Database Service Model to Save Model Binding");

test("SaveDialogConstructorExpectedResourceTypeIsDbService", function () {

    var model = new DbServiceViewModel();
    equal(model.saveViewModel.data.resourceType(), 'DbService', "Is Resource Type Db Service");
});

test("ChangeServiceModelResourceIDExpectedSaveModelResourceIDChanged", function () {

    var model = new DbServiceViewModel();
    var resourceID = $.Guid.New();
    model.data.resourceID(resourceID);
    equal(model.saveViewModel.data.resourceID(), resourceID, "Did SaveModel Resource ID Change");
});

test("ChangeServiceModelResourceNameExpectedSaveModelResourceNameChanged", function () {

    var model = new DbServiceViewModel();
    var resourceName = "new database service";
    model.data.resourceName(resourceName);
    equal(model.saveViewModel.data.resourceName(), resourceName, "Did SaveModel Resource Name Change");
});

test("ChangeServiceModelResourcePathExpectedSaveModelResourcePathChanged", function () {

    var model = new DbServiceViewModel();
    var resourcePath = "new resource path";
    model.data.resourcePath(resourcePath);
    equal(model.saveViewModel.data.resourcePath(), resourcePath, "Did SaveModel Resource Path Change");
});

test("ChangeServiceModelSourceExpectedSaveModelSourceChanged", function () {

    var model = new DbServiceViewModel();
    var source = "new source";
    model.data.source(source);
    equal(model.saveViewModel.data.source(), source, "Did SaveModel Source Name Change");
});

test("ChangeServiceModelMethodExpectedSaveModelMethodDataChanged", function () {

    var model = new DbServiceViewModel();
    
    var methodName = "change method name";
    model.data.method.Name(methodName);
    equal(model.saveViewModel.data.method.Name(), methodName, "Did Save Model Method Name Change");
    
    var methodSourceCode = "change method source code";
    model.data.method.SourceCode(methodSourceCode);
    equal(model.saveViewModel.data.method.SourceCode(), methodSourceCode, "Did Save Model Method Source Code Change");
    
    var methodParameters = "change method param";
    model.data.method.Parameters(methodParameters);
    equal(model.saveViewModel.data.method.Parameters(), methodParameters, "Did Save Model Method Parameter Change");
});

test("ChangeServiceModelRecordsetExpectedSaveModelRecordsetDataChanged", function () {

    var model = new DbServiceViewModel();
    
    var recordsetName = "change recordset name";
    model.data.recordset.Name(recordsetName);
    equal(model.saveViewModel.data.recordset.Name(), recordsetName, "Did Save Model Recordset Name Change");

    var recordsetFields = ["change recordset field", "another recordset field"];
    model.data.recordset.Fields(recordsetFields);
    equal(model.saveViewModel.data.recordset.Fields(), recordsetFields, "Did Save Model Recordset Fields Change");

    var recordsetRecords = ["change recordset record", "a second recordset record"];
    model.data.recordset.Records(recordsetRecords);
    deepEqual(model.saveViewModel.data.recordset.Records(), recordsetRecords, "Did Save Model Recordset Records Change");

    model.data.recordset.HasErrors(true);
    equal(model.saveViewModel.data.recordset.HasErrors(), true, "Did Save Model Recordset HasError Change");

    var errorMessage = "change recordset error";
    model.data.recordset.ErrorMessage(errorMessage);
    equal(model.saveViewModel.data.recordset.ErrorMessage(), errorMessage, "Did Save Model Recordset Error Message Change");
});

module("DbSource Model to DbService Model Binding");

test("SaveNewDbSourceExpectedServiceSourceChanged", function() {

    var model = new DbServiceViewModel();
    var sourceModel = new DbSourceViewModel();
    sourceModel.data.resourceName("newdbsource");
    model.getNewSourceName(sourceModel);
    equal(sourceModel.data.resourceName(), model.data.source(), "Service Model Source Name Changed");
});

// TODO: to test the load function, use mockjax...
//test("ConstructorWithValidResourcedIDExpectedTitleContainsEdit", function () {

//    var model = new DbServiceViewModel("dbServiceSaveDialogContainer", "{97A9EFED-4127-4421-BCE8-1AC90CAFB7D4}");
//    ok(model.title().indexOf("Edit") != -1);
//});

//test("ConstructorWithInvalidResourcedIDExpectedDataResourceIDIsEmptyGuid", function () {

//    var model = new DbServiceViewModel("dbServiceSaveDialogContainer", "xxxx");
//    equal(model.data.resourceID(), $.Guid.Empty());
//});

//test("ConstructorWithValidResourcedIDExpectedDataResourceIDIsTheGivenResourceID", function () {

//    var expectedID = "{97A9EFED-4127-4421-BCE8-1AC90CAFB7D4}";
//    var model = new DbServiceViewModel("dbServiceSaveDialogContainer", expectedID);
//    equal(model.data.resourceID(), expectedID);
//});

//asyncTest("LoadExpectedAjaxPostToServicesGetMethod", 1, function () {
//    $.mockjax({
//        url: "Service/Services/Get",
//        type: "POST",
//        response: function () {
//            ok(true);
//            start();
//        }
//    });
    
//    var model = new DbServiceViewModel();
//    model.load();

//});

//asyncTest("Load_Expected_UpdatesData", 1, function () {
//    var expectedRepsonse = {
//        ResourceID: "{8A49D826-0B3E-4DB5-8D01-490A75A1B698}",
//        ResourceType: "SqlDatabase",
//        ResourceName: "My Database",
//        ResourcePath: "My Category"
//    };
    
//    $.mockjax({
//        url: "Service/Services/Get",// + window.location.search,
//        type: "POST",
//        response: function () {
//            this.responseText = expectedRepsonse;
//            start();
//        }
//    });

//    var model = new DbServiceViewModel();
//    model.load(function() {
//        equal(model.data.resourceID(), expectedRepsonse.ResourceID, "model.data.resourceID set");
//        equal(model.data.resourceType(), expectedRepsonse.ResourceType, "model.data.resourceType set");
//        equal(model.data.resourceName(), expectedRepsonse.ResourceName, "model.data.resourceName set");
//        equal(model.data.resourcePath(), expectedRepsonse.ResourcePath, "model.data.resourcePath set");
//    });

//});

