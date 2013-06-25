﻿/// <reference path="../../wwwroot/Scripts/Decisions/DecisionModel.js" />
/// <reference path="../../wwwroot/Scripts/_references.js" />


/* 
    Travis.Frisinger - 06.02.2013
    Test Decision Model Functionality
    REF : http://www.testdrivenjs.com/getting-started/introduction-to-tests-in-qunit/
    REF : http://tjvantoll.com/2012/08/22/logging-test-failures-in-a-phantomjs-qunit-runner/
*/


module("Core DecisionModel Test");


test("ConstructorWithNoParamsExpectedDecisionStackInitialized", function () {

    var model = new DecisionViewModel();
    equal(model.data.TheStack().length, 0, "Did Decision Stack Initialize");
});

//TODO
//test("ConstructorWithNoParamsExpectedCorrectNumberofDecisionFunctions", function () {

//    var model = new DecisionViewModel();
//    equal(model.data.decisionFunctions().length, 26, "Did Correct Number of Decision Functions Initialize");
//});

test("AddRowExpectedDecisionStackLengthIncreased", function () {

    var model = new DecisionViewModel();
    var expected = model.data.TheStack.length + 1;
    model.addRow();
    equal(model.data.TheStack().length, expected, "Did Decision Stack Length Increase");
});

test("ToggleModeExpectedModeChanged", function () {

    var model = new DecisionViewModel();
    ok(model.toggleMode(), "Can Toggle Mode");
});

test("AddDecisionExpectedStackLengthIncreased", function () {

    var model = new DecisionViewModel();
    var expected = model.data.TheStack.length + 1;
    model.AddDecision({ Col1: '', Col2: '', Col3: '', PopulatedColumnCnt: 1, EvaluationFn: 'Choose...' });
    equal(model.data.TheStack().length, expected, "Did Stack Length Increase");
});