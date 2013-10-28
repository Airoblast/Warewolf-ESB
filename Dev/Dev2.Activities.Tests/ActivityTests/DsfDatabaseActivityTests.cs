﻿using System.Diagnostics.CodeAnalysis;
using Dev2.Activities;
using Dev2.DataList.Contract;
using Dev2.Services.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Moq;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass][ExcludeFromCodeCoverage]
    public class DsfDatabaseActivityTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Database Service Execution

        [TestMethod]
        [TestCategory("DsfDatabaseActivity_BeforeExecutionStart")]
        [Description("DsfDatabaseActivity BeforeExecutionStart constructs a valid database service execution.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void DsfDatabaseActivity_UnitTest_BeforeExecutionStart_CreatesServiceExecutionAndInvokesServiceExecutionBeforeExecution()
        // ReSharper restore InconsistentNaming
        {
            //init
            var databaseActivity = new MockDsfDatabaseActivity();
            var dataObj = new Mock<IDSFDataObject>();

            //exe
            databaseActivity.MockBeforeExecutionStart(dataObj.Object);

            //assert
            Assert.IsNotNull(databaseActivity.ServiceExecution, "DsfDatabaseActivity did not construct a correct DatabaseServiceExecution.");
            Assert.AreSame(dataObj.Object, databaseActivity.ServiceExecution.DataObj, "Data Object not assigned to DatabaseServiceExecution.");
        }

        [TestMethod]
        [TestCategory("DsfDatabaseActivity_ExecutionImpl")]
        [Description("DsfDatabaseActivity ExecutionImpl invokes the database service.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void DsfDatabaseActivity_UnitTest_ExecutionImpl_InvokesDatabaseServiceExecution()
        // ReSharper restore InconsistentNaming
        {
            var errors = new ErrorResultTO();

            var dataObj = new Mock<IDSFDataObject>();

            var dbServiceExecution = new Mock<IServiceExecution>();
            dbServiceExecution.Setup(s => s.DataObj).Returns(dataObj.Object);
            dbServiceExecution.Setup(s => s.Execute(out errors)).Verifiable();

            var databaseActivity = new MockDsfDatabaseActivity(dbServiceExecution.Object);

            // ShapeForSubRequest
            var mockEsb = new Mock<IEsbChannel>();
            //mockEsb.Setup(s => s.ShapeForSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<string>(), It.IsAny<string>(), out errors)).Returns(new List<KeyValuePair<enDev2ArgumentType, List<IDev2Definition>>>());
            databaseActivity.MockExecutionImpl(mockEsb.Object, dataObj.Object, string.Empty, string.Empty, out errors);

            //assert
            Assert.IsFalse(errors.HasErrors(), "Errors where thrown while executing a database servic");
            dbServiceExecution.Verify(s => s.Execute(out errors));
        }

        [TestMethod]
        [TestCategory("DsfDatabaseActivity_AfterExecutionCompleted")]
        [Description("DsfDatabaseActivity AfterExecutionCompleted invokes DatabaseServiceExecution AfterExecution.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void DsfDatabaseActivity_UnitTest_AfterExecutionCompleted_InvokesServiceExecutionAfterExecution()
        // ReSharper restore InconsistentNaming
        {
            var dataObj = new Mock<IDSFDataObject>();

            var dbServiceExecution = new Mock<IServiceExecution>();
            dbServiceExecution.Setup(s => s.DataObj).Returns(dataObj.Object);
            dbServiceExecution.Setup(s => s.AfterExecution(It.IsAny<ErrorResultTO>())).Verifiable();

            var databaseActivity = new MockDsfDatabaseActivity(dbServiceExecution.Object);

            databaseActivity.MockAfterExecutionCompleted();

            //assert
            dbServiceExecution.Verify(s => s.AfterExecution(It.IsAny<ErrorResultTO>()));
        }

        #endregion
    }
}
