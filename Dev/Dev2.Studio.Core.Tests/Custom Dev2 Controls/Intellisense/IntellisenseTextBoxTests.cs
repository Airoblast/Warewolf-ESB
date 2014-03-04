﻿using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using Dev2.Core.Tests.Utils;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Interfaces;
using Dev2.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Windows;
using Clipboard = System.Windows.Clipboard;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Dev2.Core.Tests.Custom_Dev2_Controls.Intellisense
// ReSharper restore CheckNamespace
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class IntellisenseTextBoxTests
    {

        [TestInitialize]
        public void MyTestInitialize()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            SynchronizationContext.SetSynchronizationContext(null);
        }

        #region Test Initialization

        //BUG 9639
        [TestMethod]
        // This test is here for when the designers load. The check is to prevent them from hammering the providers on load ;)
        public void IntellisenseBoxDoesntQueryProvidersWhenTextLengthIsZero()
        {

            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
            intellisenseProvider.Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>())).Verifiable();

            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.IntellisenseProvider = intellisenseProvider.Object;
            textBox.Text = "";

            // Ensure the get results method is never called, mimics a initalize event from the design surface ;)
            intellisenseProvider.Verify(s => s.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()), Times.Exactly(0));
        }


        //BUG 8761
        [TestMethod]
        public void IntellisenseBoxDoesntCrashWhenGettingResultsGivenAProviderThatThrowsAnException()
        {
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
            intellisenseProvider.Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                                .Throws(new Exception());

            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.IntellisenseProvider = intellisenseProvider.Object;
            textBox.Text = "[[City([[Scalar]]).Na";

            // When exceptions are thrown, no results are to be displayed
            Assert.AreEqual(0, textBox.Items.Count);
            //The desired result is that an exception isn't thrown


            // GetIntellisenseResults -> OnIntellisenseProviderChanged
        }

        //BUG 8761
        [TestMethod]
        public void IntellisenseBoxDoesntCrashWhenInsertingResultsGivenAProviderThatThrowsAnException()
        {
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
            intellisenseProvider.Setup(
                a => a.PerformResultInsertion(It.IsAny<string>(), It.IsAny<IntellisenseProviderContext>()))
                                .Throws(new Exception());
            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(true);

            IntellisenseProviderResult intellisenseProviderResult =
                new IntellisenseProviderResult(intellisenseProvider.Object, "City", "cake");

            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.InsertItem(intellisenseProviderResult, true);

            // When exepctions are thrown, no results are to be displayed
            Assert.AreEqual(0, textBox.Items.Count, "Expected [ 0 ] But got [ " + textBox.Items.Count + " ]");
            //The desired result is that an exception isn't thrown
        }

        [TestMethod]
        public void TextContaningTabIsPasedIntoAnIntellisenseTextBoxExpectedTabInsertedEventIsRaised()
        {
            bool eventRaised = false;
            IntellisenseTextBox sender = null;
            EventManager.RegisterClassHandler(typeof(IntellisenseTextBox), IntellisenseTextBox.TabInsertedEvent,
                                              new RoutedEventHandler((s, e) =>
                                                  {
                                                      eventRaised = true;
                                                      sender = s as IntellisenseTextBox;
                                                  }));

            Clipboard.SetText("Cake\t");

            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();

            textBox.Paste();

            Assert.IsTrue(eventRaised,
                          "The 'IntellisenseTextBox.TabInsertedEvent' wasn't raised when text containing a tab was pasted into the IntellisenseTextBox.");
            Assert.AreEqual(textBox, sender,
                            "The IntellisenseTextBox in which the text containg a tab was pasted was different from the one which raised teh event.");

        }

        [TestMethod]
        public void TextContaningNoTabIsPasedIntoAnIntellisenseTextBoxExpectedTabInsertedEventNotRaised()
        {
            var preserveClipboard = Clipboard.GetText();
            try
            {
                bool eventRaised = false;
                EventManager.RegisterClassHandler(typeof(IntellisenseTextBox), IntellisenseTextBox.TabInsertedEvent,
                                                  new RoutedEventHandler((s, e) =>
                                                      {
                                                          eventRaised = true;
                                                      }));

                Clipboard.SetText("Cake");

                IntellisenseTextBox textBox = new IntellisenseTextBox();
                textBox.CreateVisualTree();
                textBox.Paste();

                Assert.IsFalse(eventRaised,
                               "The 'IntellisenseTextBox.TabInsertedEvent' was raised when text that didn't contain a tab was pasted into the IntellisenseTextBox.");
            }
            finally
            {
                Clipboard.SetText(preserveClipboard);
            }

        }

        #endregion Test Initialization

        [TestMethod]
        public void InsertItemExpectedTextboxTextChangedAndErrorStatusUpdated()
        {
            const string ExpectedText = "[[City()";
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(true);
            intellisenseProvider.Setup(
                a => a.PerformResultInsertion(It.IsAny<string>(), It.IsAny<IntellisenseProviderContext>())).Returns(ExpectedText);

            IntellisenseProviderResult intellisenseProviderResult =
                new IntellisenseProviderResult(intellisenseProvider.Object, ExpectedText, "cake");

            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.InsertItem(intellisenseProviderResult, true);

            Thread.Sleep(250);
            Thread.Sleep(100);

            Assert.AreEqual(ExpectedText, textBox.Text, "Expected [ " + ExpectedText + " ] But got [ " + textBox.Text + " ]");
            Assert.AreEqual(true, textBox.HasError, "Expected [ True ] But got [ " + textBox.HasError + " ]");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IntellisenseTextBoxTests_SetText")]
        public void IntellisenseTextBoxTests_SetText_FilterTypeIsRecordsetFieldsButTextIsScalar_ToolTipHasErrorMessage()
        {
            // ReSharper disable UnusedVariable
            var thread = new Thread(() =>
            // ReSharper restore UnusedVariable
            {
                var textBox = new IntellisenseTextBox();
                textBox.FilterType = enIntellisensePartType.RecordsetFields;
                textBox.Text = "[[var]]";
                Assert.IsTrue(textBox.HasError);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IntellisenseTextBoxTests_SetText")]
        public void IntellisenseTextBoxTests_SetText_FilterTypeIsRecordsetFieldsAndTextIsRecordset_ToolTipHasNoErrorMessage()
        {

            // ReSharper disable UnusedVariable
            var thread = new Thread(() =>
            // ReSharper restore UnusedVariable
            {
                var textBox = new IntellisenseTextBox();
                textBox.FilterType = enIntellisensePartType.RecordsetFields;
                textBox.Text = "[[var()]]";
                Assert.IsFalse(textBox.HasError);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IntellisenseTextBoxTests_SetText")]
        public void IntellisenseTextBoxTests_SetText_FilterTypeIsScalarsOnlyAndTextIsScalar_ToolTipHasNoErrorMessage()
        {
            // ReSharper disable UnusedVariable
            var thread = new Thread(() =>
            // ReSharper restore UnusedVariable
            {
                var textBox = new IntellisenseTextBox();
                textBox.FilterType = enIntellisensePartType.ScalarsOnly;
                textBox.Text = "[[var]]";
                Assert.IsFalse(textBox.HasError);
            });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IntellisenseTextBoxTests_SetText")]
        public void IntellisenseTextBoxTests_SetText_FilterTypeIsScalarsOnlyButTextIsRecordset_ToolTipHaErrorMessage()
        {
            // ReSharper disable UnusedVariable
            var thread = new Thread(() =>
            // ReSharper restore UnusedVariable
            {
                var textBox = new IntellisenseTextBox();
                textBox.FilterType = enIntellisensePartType.ScalarsOnly;
                textBox.Text = "[[var()]]";
                Thread.Sleep(250);
                Assert.IsTrue(textBox.HasError);
            });

        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IntellisenseTextBox_TextChanged")]
        public void IntellisenseTextBox_TextChanged_LessThen200msBetweenTextChanged_TextChangedFiredOnce()
        {
            //------------Setup for test--------------------------            
            var mockIntellisenseTextBox = new MockIntellisenseTextbox();

            var chars = new[] { 'a', 'b', 'c', 'd' };
            mockIntellisenseTextBox.InitTestClass();

            //------------Execute Test---------------------------

            foreach(var c in chars)
            {
                mockIntellisenseTextBox.Text = c.ToString(CultureInfo.InvariantCulture);
                Thread.Sleep(50);
            }

            Thread.Sleep(250);

            //------------Assert Results-------------------------
            Thread.Sleep(100);
            Assert.AreEqual(1, mockIntellisenseTextBox.TextChangedCounter);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IntellisenseTextBox_TextChanged")]
        public void IntellisenseTextBox_TextChanged_GreaterThen200msBetweenTextChanged_TextChangedFiredFourTimes()
        {
            //------------Setup for test--------------------------            
            var mockIntellisenseTextBox = new MockIntellisenseTextbox();

            var chars = new[] { 'a', 'b', 'c', 'd' };
            mockIntellisenseTextBox.InitTestClass();

            //------------Execute Test---------------------------

            foreach(var c in chars)
            {
                mockIntellisenseTextBox.Text = c.ToString(CultureInfo.InvariantCulture);
                Thread.Sleep(250);
            }

            //------------Assert Results-------------------------
            Thread.Sleep(100);
            Assert.AreEqual(4, mockIntellisenseTextBox.TextChangedCounter);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IntellisenseTextBox_InsertItem")]
        public void IntellisenseTextBox_InsertItem_InsertDateTimeParts_InsertsCorrectly()
        {
            //------------Setup for test--------------------------            
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();

            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(false);

            IntellisenseProviderResult intellisenseProviderResult =
                new IntellisenseProviderResult(intellisenseProvider.Object, "yyyy", "yyyy");
            //------------Execute Test---------------------------
            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.IsOpen = true;
            textBox.Text = "ddyy";
            textBox.CaretIndex = 4;
            textBox.InsertItem(intellisenseProviderResult, false);
            //------------Assert Results-------------------------
            Assert.AreEqual("ddyyyy", textBox.Text);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IntellisenseTextBox_InsertItem")]
        public void IntellisenseTextBox_InsertItem_InsertDateTimePartsWithSpace_InsertsCorrectly()
        {
            //------------Setup for test--------------------------            
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();

            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(false);

            IntellisenseProviderResult intellisenseProviderResult =
                new IntellisenseProviderResult(intellisenseProvider.Object, "yyyy", "yyyy");
            //------------Execute Test---------------------------
            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.IsOpen = true;
            textBox.Text = "dd yy";
            textBox.CaretIndex = 5;
            textBox.InsertItem(intellisenseProviderResult, false);
            //------------Assert Results-------------------------
            Assert.AreEqual("dd yyyy", textBox.Text);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IntellisenseTextBox_InsertItem")]
        public void IntellisenseTextBox_InsertItem_InsertDateTimePartsWithDifferentCase_InsertsCorrectly()
        {
            //------------Setup for test--------------------------            
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();

            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(false);

            IntellisenseProviderResult intellisenseProviderResult =
                new IntellisenseProviderResult(intellisenseProvider.Object, "yyyy", "yyyy");
            //------------Execute Test---------------------------
            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.IsOpen = true;
            textBox.Text = "dd YY";
            textBox.CaretIndex = 5;
            textBox.InsertItem(intellisenseProviderResult, false);
            //------------Assert Results-------------------------
            Assert.AreEqual("dd yyyy", textBox.Text);
        }
    }
}
