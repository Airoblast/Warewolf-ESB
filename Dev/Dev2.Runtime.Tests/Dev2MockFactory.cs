﻿using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Network;
using Dev2.Network.Execution;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Network;

namespace Dev2.DynamicServices.Test
{
    public static class Dev2MockFactory
    {
        public static Mock<IDataListServer> SetupDataListServer(bool writeResult = true, bool persistChildChainResult = true, IBinaryDataList readResult = null, bool readCausesException = false, bool writeCausesException = false, bool persistChildChainCausesException = false, bool deleteCausesException = false)
        {
            Mock<IDataListServer> mockDataListServer = new Mock<IDataListServer>();
            ErrorResultTO errors;

            if (readCausesException)
            {
                mockDataListServer.Setup(e => e.ReadDatalist(It.IsAny<Guid>(), out errors)).Throws(new Exception());
            }
            else
            {
                mockDataListServer.Setup(e => e.ReadDatalist(It.IsAny<Guid>(), out errors)).Verifiable();
                mockDataListServer.Setup(e => e.ReadDatalist(It.IsAny<Guid>(), out errors)).Returns(readResult);
            }

            if (writeCausesException)
            {
                mockDataListServer.Setup(e => e.WriteDataList(It.IsAny<Guid>(), It.IsAny<IBinaryDataList>(), out errors)).Throws(new Exception());
            }
            else
            {
                mockDataListServer.Setup(e => e.WriteDataList(It.IsAny<Guid>(), It.IsAny<IBinaryDataList>(), out errors)).Verifiable();
                mockDataListServer.Setup(e => e.WriteDataList(It.IsAny<Guid>(), It.IsAny<IBinaryDataList>(), out errors)).Returns(writeResult);
            }

            if (persistChildChainCausesException)
            {
                mockDataListServer.Setup(e => e.PersistChildChain(It.IsAny<Guid>())).Throws(new Exception());
            }
            else
            {
                mockDataListServer.Setup(e => e.PersistChildChain(It.IsAny<Guid>())).Verifiable();
                mockDataListServer.Setup(e => e.PersistChildChain(It.IsAny<Guid>())).Returns(persistChildChainResult); 
            }


            if (deleteCausesException)
            {
                mockDataListServer.Setup(e => e.DeleteDataList(It.IsAny<Guid>(), It.IsAny<bool>())).Throws(new Exception());
            }
            else
            {
                mockDataListServer.Setup(e => e.DeleteDataList(It.IsAny<Guid>(), It.IsAny<bool>())).Verifiable();
            }

            return mockDataListServer;
        }

        public static Mock<IExecutionStatusCallbackDispatcher> SetupExecutionStatusCallbackDispatcher(bool addResult = true, bool removeResult = true)
        {
            Mock<IExecutionStatusCallbackDispatcher> mockExecutionStatusCallbackDispatcher = new Mock<IExecutionStatusCallbackDispatcher>();
            mockExecutionStatusCallbackDispatcher.Setup(e => e.Add(It.IsAny<Guid>(), It.IsAny<Action<ExecutionStatusCallbackMessage>>())).Verifiable();
            mockExecutionStatusCallbackDispatcher.Setup(e => e.Add(It.IsAny<Guid>(), It.IsAny<Action<ExecutionStatusCallbackMessage>>())).Returns(addResult);

            mockExecutionStatusCallbackDispatcher.Setup(e => e.Remove(It.IsAny<Guid>())).Verifiable();
            mockExecutionStatusCallbackDispatcher.Setup(e => e.Remove(It.IsAny<Guid>())).Returns(removeResult);

            mockExecutionStatusCallbackDispatcher.Setup(e => e.RemoveRange(It.IsAny<IList<Guid>>())).Verifiable();

            mockExecutionStatusCallbackDispatcher.Setup(e => e.Post(It.IsAny<ExecutionStatusCallbackMessage>())).Verifiable();
            mockExecutionStatusCallbackDispatcher.Setup(e => e.Send(It.IsAny<ExecutionStatusCallbackMessage>())).Verifiable();

            return mockExecutionStatusCallbackDispatcher;
        }

        public static Mock<INetworkMessageBroker> SetupNetworkMessageBroker(bool sendThrowsException = false)
        {
            Mock<INetworkMessageBroker> mockNetworkMessageBroker = new Mock<INetworkMessageBroker>();

            if (sendThrowsException)
            {
                mockNetworkMessageBroker.Setup(e => e.Send<ExecutionStatusCallbackMessage>(It.IsAny<ExecutionStatusCallbackMessage>(), It.IsAny<INetworkOperator>())).Throws(new Exception());
            }
            else
            {
                mockNetworkMessageBroker.Setup(e => e.Send<ExecutionStatusCallbackMessage>(It.IsAny<ExecutionStatusCallbackMessage>(), It.IsAny<INetworkOperator>())).Verifiable();
            }

            return mockNetworkMessageBroker;
        }

        public static Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> SetupServerNetworkMessageAggregator()
        {
            Mock<IServerNetworkMessageAggregator<StudioNetworkSession>> mockStudioNetworkMessageAggregator = new Mock<IServerNetworkMessageAggregator<StudioNetworkSession>>();

            return mockStudioNetworkMessageAggregator;
        }

        public static Mock<IServerNetworkChannelContext<StudioNetworkSession>> SetupServerNetworkChannelContext()
        {
            Mock<IServerNetworkChannelContext<StudioNetworkSession>> mockSetupServerNetworkChannelContext = new Mock<IServerNetworkChannelContext<StudioNetworkSession>>();
            mockSetupServerNetworkChannelContext.Setup(s => s.NetworkContext).Returns(new StudioNetworkSession());

            return mockSetupServerNetworkChannelContext;
        }

        public static Mock<IDataReader> SetupDataReader(List<object[]> results)
        {
            int readCount = 0;
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(() => readCount < results.Count).Callback(() =>
            {
                readCount++;
            });
            reader.Setup(r => r.GetValues(It.IsAny<object[]>())).Returns(results[readCount].Length);

            return reader;
        }

        public static Mock<IDbConnection> SetupDbConnection()
        {
            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.Close()).Verifiable();
            connection.Setup(c => c.Dispose()).Verifiable();

            return connection;
        }

        public static Mock<IDbCommand> SetupDbCommand(Mock<IDataReader> dataReader, Mock<IDbConnection> connection)
        {
            var command = new Mock<IDbCommand>();
            command.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(dataReader.Object);
            command.Setup(c => c.Connection).Returns(connection.Object);

            return command;
        }

        public static List<object[]> FakeDataBrokerTestsResults(int rows, int columns)
        {
            List<object[]> results = new List<object[]>();

            for (int j = 0; j < rows; j++)
            {
                object[] row = new object[columns];
                for (int i = 0; i < columns; i++)
                {
                    row[i] = i;
                }
                results.Add(row);
            }

            return results;
        }
    }
}
