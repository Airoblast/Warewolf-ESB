﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Data.Enums;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.DynamicServices.Objects.Base;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.XML;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ResourceCatalogTests
    {
        // Change this if you change the number of resources saved by SaveResources()
        const int SaveResourceCount = 6;
        static object syncRoot = new object();

        static string _testDir;

        [ClassInitialize]
        public static void MyClassInit(TestContext context)
        {
            _testDir = context.DeploymentDirectory;
        }

        #region Instance

        [TestMethod]
        public void InstanceExpectedIsSingleton()
        {
            var instance1 = ResourceCatalog.Instance;
            var instance2 = ResourceCatalog.Instance;
            Assert.IsNotNull(instance1);
            Assert.IsNotNull(instance2);
            Assert.AreSame(instance1, instance2);
        }

        #endregion

        #region This

        [TestMethod]
        public void ThisWithNewWorkspaceIDExpectedAddsCatalogForWorkspace()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            var expectedWorkspaceCount = catalog.WorkspaceCount + 1;

            var actualCount = catalog.GetResourceCount(workspaceID);
            Assert.AreEqual(SaveResourceCount, actualCount);
            Assert.AreEqual(expectedWorkspaceCount, catalog.WorkspaceCount);
        }

        [TestMethod]
        public void ThisWithExistingWorkspaceIDExpectedReturnsExistingCatalogForWorkspace()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            var expectedWorkspaceCount = catalog.WorkspaceCount + 1;

            catalog.LoadWorkspace(workspaceID); // Loads from disk
            var actualCount = catalog.GetResourceCount(workspaceID);
            Assert.AreEqual(SaveResourceCount, actualCount);
            Assert.AreEqual(expectedWorkspaceCount, catalog.WorkspaceCount);
        }

        #endregion

        #region LoadWorkspace

        [TestMethod]
        public void LoadWorkspaceWithNewWorkspaceIDExpectedAddsCatalogForWorkspace()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            var expectedWorkspaceCount = catalog.WorkspaceCount + 1;
            catalog.LoadWorkspace(workspaceID); // Loads from disk

            var actualCount = catalog.GetResourceCount(workspaceID);
            Assert.AreEqual(SaveResourceCount, actualCount);
            Assert.AreEqual(expectedWorkspaceCount, catalog.WorkspaceCount);
        }

        [TestMethod]
        public void LoadWorkspaceWithExistingWorkspaceIDExpectedUpdatesCatalogForWorkspace()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);
            var resource = resources[0];

            var catalog = new ResourceCatalog();
            var expectedWorkspaceCount = catalog.WorkspaceCount + 1;

            catalog.LoadWorkspace(workspaceID); // Loads from disk

            catalog.DeleteResource(workspaceID, resource.ResourceName, ResourceTypeConverter.ToTypeString(resource.ResourceType), resource.AuthorRoles);
            var actualCount = catalog.GetResourceCount(workspaceID);
            Assert.AreEqual(SaveResourceCount - 1, actualCount);
            Assert.AreEqual(expectedWorkspaceCount, catalog.WorkspaceCount);

            catalog.LoadWorkspace(workspaceID); // Loads from disk
            actualCount = catalog.GetResourceCount(workspaceID);
            Assert.AreEqual(SaveResourceCount - 1, actualCount);
            Assert.AreEqual(expectedWorkspaceCount, catalog.WorkspaceCount);
        }

        #endregion

        #region LoadWorkspaceAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoadWorkspaceAsyncWithNullWorkspaceArgumentExpectedThrowsArgumentNullException()
        {
            var rc = new ResourceCatalog();

            rc.LoadWorkspaceViaBuilder(null, new string[0]);
        }

        [TestMethod]
        public void LoadWorkspaceAsyncWithEmptyFoldersArgumentExpectedReturnsEmptyCatalog()
        {
            var rc = new ResourceCatalog();

            Assert.AreEqual(0, rc.LoadWorkspaceViaBuilder("xx", new string[0]).Count);
        }


        [TestMethod]
        public void LoadWorkspaceAsyncWithExistingSourcesPathAndNonExistingServicesPathExpectedReturnsCatalogForSources()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            var resources = SaveResources(sourcesPath, null, false, false, new[] { "CatalogSourceAnytingToXmlPlugin", "CatalogSourceCitiesDatabase", "CatalogSourceTestServer" }, new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            Assert.AreEqual(3, result.Count);

            foreach(var resource in result)
            {
                var expected = resources.First(r => r.ResourceName == resource.ResourceName);
                Assert.AreEqual(expected.FilePath, resource.FilePath);
            }
        }


        [TestMethod]
        public void LoadWorkspaceAsyncWithValidWorkspaceIDExpectedReturnsCatalogForWorkspace()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            var workspacePath = SaveResources(workspaceID, out resources);

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            Assert.AreEqual(SaveResourceCount, result.Count);

            foreach(var resource in result)
            {
                var expected = resources.First(r => r.ResourceName == resource.ResourceName);
                Assert.AreEqual(expected.FilePath, resource.FilePath);
            }
        }


        [TestMethod]
        public void LoadWorkspaceAsyncWithWithOneSignedAndOneUnsignedServiceExpectedLoadsSignedService()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resources = SaveResources(path, null, false, false, new[] { "Calculate_RecordSet_Subtract", "TestDecisionUnsigned" }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            Assert.AreEqual(1, result.Count);

            foreach(var resource in result)
            {
                var expected = resources.First(r => r.ResourceName == resource.ResourceName);
                Assert.AreEqual(expected.FilePath, resource.FilePath);
            }
        }


        [TestMethod]
        public void LoadWorkspaceAsyncWithSourceWithoutIDExpectedInjectsID()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var path = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(path);
            var resources = SaveResources(path, null, false, false, new[] { "CitiesDatabase" }, new[] { Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            Assert.AreEqual(1, result.Count);

            foreach(var resource in result)
            {
                var expected = resources.First(r => r.ResourceName == resource.ResourceName);
                Assert.AreNotEqual(expected.ResourceID, resource.ResourceID);
            }
        }


        [TestMethod]
        public void LoadWorkspaceAsyncWithUpgradableXmlExpectedUpgradesXmlWithoutLocking()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            SaveResources(path, null, false, false, new[] { "CatalogServiceAllTools" }, new[] { Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            Assert.AreEqual(1, result.Count);

            var actual = result[0];
            Assert.IsTrue(actual.IsUpgraded);

            // TestCleanup will fail if file is locked! 
        }

        #endregion

        #region ParallelExecution

        [TestMethod]
        public void LoadInParallelExpectedBehavesConcurrently()
        {
            const int NumWorkspaces = 5;
            const int NumThreadsPerWorkspace = 5;
            const int ExpectedWorkspaceCount = NumWorkspaces + 1; // add 1 for server workspace that is auto-loaded

            var catalog = new ResourceCatalog();

            var threadArray = new Thread[NumWorkspaces * NumThreadsPerWorkspace];

            #region Create threads

            for(var i = 0; i < NumWorkspaces; i++)
            {
                var workspaceID = Guid.NewGuid();

                List<IResource> resources;
                SaveResources(workspaceID, out resources);

                for(var j = 0; j < NumThreadsPerWorkspace; j++)
                {
                    var t = (i * NumThreadsPerWorkspace) + j;
                    if(j == 0)
                    {
                        threadArray[t] = new Thread(() =>
                        {
                            catalog.LoadWorkspace(workspaceID); // Always loads from disk
                            var actualCount = catalog.GetResourceCount(workspaceID); // Loads from disk if not in memory
                            Assert.AreEqual(SaveResourceCount, actualCount);
                        });
                    }
                    else
                    {
                        threadArray[t] = new Thread(() =>
                        {
                            var actualCount = catalog.GetResourceCount(workspaceID); // Loads from disk if not in memory
                            Assert.AreEqual(SaveResourceCount, actualCount);
                        });
                    }
                }
            }

            #endregion


            //Start the threads.
            Parallel.For(0, threadArray.Length, i => threadArray[i].Start());

            //Wait until all the threads spawn out and finish.
            foreach(var t in threadArray)
            {
                t.Join();
            }

            Assert.AreEqual(ExpectedWorkspaceCount, catalog.WorkspaceCount);
        }

        #endregion

        #region SaveResource

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveResourceWithNullResourceArgumentExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, (IResource)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveResourceWithNullResourceXmlArgumentExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            Resource model = null;
            catalog.SaveResource(workspaceID, model);
        }

        [TestMethod]
        public void SaveResourceWithUnsignedServiceExpectedSignsFile()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");

            var xml = XmlResource.Fetch("TestDecisionUnsigned");
            var resource = new Workflow(xml);
            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, resource);

            var signedXml = File.ReadAllText(Path.Combine(path, resource.ResourceName + ".xml"));

            var isValid = HostSecurityProvider.Instance.VerifyXml(new StringBuilder(signedXml));

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void SaveResourceWithSourceWithoutIDExpectedSourceSavedWithID()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var path = Path.Combine(workspacePath, "Sources");

            var xml = XmlResource.Fetch("CitiesDatabase");
            var resource = new DbSource(xml);
            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, resource);

            xml = XElement.Load(Path.Combine(path, resource.ResourceName + ".xml"));
            var attr = xml.Attributes("ID").ToList();

            Assert.AreEqual(1, attr.Count);
        }

        [TestMethod]
        public void SaveResourceWithExistingResourceExpectedResourceOverwritten()
        {
            var workspaceID = Guid.NewGuid();

            var resource1 = new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "TestSource", DatabaseName = "TestOldDb", Server = "TestOldServer", ServerType = enSourceType.SqlDatabase, Version = new Version(1, 0) };

            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, resource1);

            var expected = new DbSource { ResourceID = resource1.ResourceID, ResourceName = resource1.ResourceName, DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase, Version = new Version(1, 1) };
            catalog.SaveResource(workspaceID, expected);

            var contents = catalog.GetResourceContents(workspaceID, expected.ResourceID, expected.Version);
            var actual = new DbSource(XElement.Parse(contents.ToString()));

            Assert.AreEqual(expected.ResourceName, actual.ResourceName);
            Assert.AreEqual(expected.DatabaseName, actual.DatabaseName);
            Assert.AreEqual(expected.Server, actual.Server);
            Assert.AreEqual(expected.ServerType, actual.ServerType);
        }

        [TestMethod]
        public void SaveResourceWithExistingResourceAndReadonlyExpectedResourceOverwritten()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var resource1 = new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "TestSource", DatabaseName = "TestOldDb", Server = "TestOldServer", ServerType = enSourceType.SqlDatabase, Version = new Version(1, 0) };

            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, resource1);

            var path = Path.Combine(workspacePath, "Sources", resource1.ResourceName + ".xml");
            var attributes = File.GetAttributes(path);
            if((attributes & FileAttributes.ReadOnly) != FileAttributes.ReadOnly)
            {
                File.SetAttributes(path, attributes ^ FileAttributes.ReadOnly);
            }

            var expected = new DbSource { ResourceID = resource1.ResourceID, ResourceName = resource1.ResourceName, DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase, Version = new Version(1, 1) };
            catalog.SaveResource(workspaceID, expected);

            var contents = catalog.GetResourceContents(workspaceID, expected.ResourceID, expected.Version);
            var actual = new DbSource(XElement.Parse(contents.ToString()));

            Assert.AreEqual(expected.ResourceName, actual.ResourceName);
            Assert.AreEqual(expected.DatabaseName, actual.DatabaseName);
            Assert.AreEqual(expected.Server, actual.Server);
            Assert.AreEqual(expected.ServerType, actual.ServerType);
        }

        [TestMethod]
        public void SaveResourceWithNewResourceExpectedResourceWritten()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();

            var expected = new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "TestSource", DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase, Version = new Version(1, 1) };
            catalog.SaveResource(workspaceID, expected);

            var contents = catalog.GetResourceContents(workspaceID, expected.ResourceID, expected.Version);
            var actual = new DbSource(XElement.Parse(contents.ToString()));

            Assert.AreEqual(expected.ResourceName, actual.ResourceName);
            Assert.AreEqual(expected.DatabaseName, actual.DatabaseName);
            Assert.AreEqual(expected.Server, actual.Server);
            Assert.AreEqual(expected.ServerType, actual.ServerType);
        }

        [TestMethod]
        [ExpectedException(typeof(DirectoryNotFoundException))]
        public void SaveResourceWithSlashesInResourceNameExpectedThrowsDirectoryNotFoundException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();

            var expected = new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "Test\\Source", DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase, Version = new Version(1, 1) };
            catalog.SaveResource(workspaceID, expected);
        }

        [TestMethod]
        public void SaveResourceWithNewResourceXmlExpectedResourceWritten()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();

            var expected = new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "TestSource", DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase, Version = new Version(1, 1) };
            catalog.SaveResource(workspaceID, new StringBuilder(expected.ToXml().ToString()));

            var contents = catalog.GetResourceContents(workspaceID, expected.ResourceID, expected.Version);
            var actual = new DbSource(XElement.Parse(contents.ToString()));

            Assert.AreEqual(expected.ResourceName, actual.ResourceName);
            Assert.AreEqual(expected.DatabaseName, actual.DatabaseName);
            Assert.AreEqual(expected.Server, actual.Server);
            Assert.AreEqual(expected.ServerType, actual.ServerType);
        }

        #endregion

        #region GetResource

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetResourceWithNullResourceNameExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            catalog.GetResource(workspaceID, null);
        }

        [TestMethod]
        public void GetResourceWithResourceNameExpectedReturnsResource()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            foreach(var expected in resources)
            {
                var actual = catalog.GetResource(workspaceID, expected.ResourceName);
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.ResourceID, actual.ResourceID);
                Assert.AreEqual(expected.ResourceName, actual.ResourceName);
                Assert.AreEqual(expected.ResourcePath, actual.ResourcePath);
                Assert.AreEqual(expected.ResourcePath, actual.ResourcePath);
            }
        }

        [TestMethod]
        public void GetResource_UnitTest_WhereTypeIsProvided_ExpectTypedResourceWorkflow()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            //------------Assert Precondition-----------------
            //------------Execute Test---------------------------
            var workflow = ResourceCatalog.Instance.GetResource<Workflow>(workspaceID, resourceName);
            //------------Assert Results-------------------------
            Assert.IsNotNull(workflow);
            Assert.IsInstanceOfType(workflow, typeof(Workflow));
        }

        [TestMethod]
        public void GetResource_UnitTest_WhereTypeIsProvided_ExpectTypedResourceWebService()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "WebService";
            SaveResources(path, null, false, true, new[] { "WebService", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();
            path = Path.Combine(workspacePath, "Sources");

            var xml = XmlResource.Fetch("WeatherWebSource");
            var resource = new WebSource(xml);
            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, resource);

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");

            //------------Assert Precondition-----------------
            Assert.AreEqual(1, result.Count);
            //------------Execute Test---------------------------
            var webService = ResourceCatalog.Instance.GetResource<WebService>(workspaceID, result[0].ResourceID);
            //------------Assert Results-------------------------
            Assert.IsNotNull(webService);
            Assert.IsInstanceOfType(webService, typeof(WebService));
        }

        [TestMethod]
        public void GetResource_UnitTest_WhereTypeIsProvided_ExpectTypedResourceWebSource()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "WebService";
            SaveResources(path, null, false, true, new[] { "WebService", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();
            path = Path.Combine(workspacePath, "Sources");

            var xml = XmlResource.Fetch("WeatherWebSource");
            var resource = new WebSource(xml);
            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, resource);
            //------------Assert Precondition-----------------
            //------------Execute Test---------------------------
            var webSource = catalog.GetResource<WebSource>(workspaceID, resource.ResourceID);
            //------------Assert Results-------------------------
            Assert.IsNotNull(webSource);
            Assert.IsInstanceOfType(webSource, typeof(WebSource));
        }

        #endregion

        #region GetResourceContents

        [TestMethod]
        public void GetResourceContentsWithNullResourceExpectedReturnsEmptyString()
        {
            var catalog = new ResourceCatalog();
            var result = catalog.GetResourceContents(null);
            Assert.AreEqual(string.Empty, result.ToString());
        }

        [TestMethod]
        public void GetResourceContentsWithNullResourceFilePathExpectedReturnsEmptyString()
        {
            var catalog = new ResourceCatalog();
            var result = catalog.GetResourceContents(new Resource());
            Assert.AreEqual(string.Empty, result.ToString());
        }

        [TestMethod]
        public void GetResourceContentsWithExistingResourceExpectedReturnsResourceContents()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            foreach(var expected in resources)
            {
                var actual = catalog.GetResourceContents(expected);
                Assert.IsNotNull(actual);
            }
        }

        [TestMethod]
        public void GetResourceContentsWithNonExistentResourceExpectedReturnsEmptyString()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            var actual = catalog.GetResourceContents(new Resource { ResourceID = Guid.NewGuid(), FilePath = Path.GetRandomFileName() });
            Assert.AreEqual(string.Empty, actual.ToString());
        }

        [TestMethod]
        public void GetResourceContentsWithExistingResourceIDExpectedReturnsResourceContents()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            foreach(var expected in resources)
            {
                var xml = catalog.GetResourceContents(workspaceID, expected.ResourceID, expected.Version);

                var actual = new Resource(XElement.Parse(xml.ToString()));
                Assert.AreEqual(expected.ResourceID, actual.ResourceID);
                Assert.AreEqual(expected.ResourceName, actual.ResourceName);
            }
        }

        [TestMethod]
        public void GetResourceContentsWithNonExistentResourceIDExpectedReturnsEmptyString()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            var actual = catalog.GetResourceContents(workspaceID, Guid.NewGuid());
            Assert.AreEqual(string.Empty, actual.ToString());
        }

        #endregion

        #region SyncTo

        [TestMethod]
        public void SyncToWithDeleteIsFalseAndFileDeletedFromSourceExpectedFileNotDeletedInDestination()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out sourceResources);

            List<IResource> targetResources;
            var targetWorkspaceID = Guid.NewGuid();
            var targetWorkspacePath = SaveResources(targetWorkspaceID, out targetResources);

            var sourceResource = sourceResources[0];
            var targetResource = targetResources.First(r => r.ResourceID == sourceResource.ResourceID);
            var sourceFile = new FileInfo(sourceResource.FilePath);
            var targetFile = new FileInfo(targetResource.FilePath);

            sourceFile.Delete();
            new ResourceCatalog().SyncTo(sourceWorkspacePath, targetWorkspacePath, true, false);
            targetFile.Refresh();
            Assert.IsTrue(targetFile.Exists);
        }

        [TestMethod]
        public void SyncToWithOverwriteIsTrueExpectedFileInDestinationOverwritten()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out sourceResources);

            List<IResource> targetResources;
            var targetWorkspaceID = Guid.NewGuid();
            var targetWorkspacePath = SaveResources(targetWorkspaceID, out targetResources);

            var sourceResource = sourceResources[0];
            var targetResource = targetResources.First(r => r.ResourceID == sourceResource.ResourceID);
            var sourceFile = new FileInfo(sourceResource.FilePath);
            var targetFile = new FileInfo(targetResource.FilePath);

            var fs = sourceFile.Open(FileMode.Append, FileAccess.Write, FileShare.None);
            fs.Write(new byte[]
            {
                200
            }, 0, 1);
            fs.Close();

            new ResourceCatalog().SyncTo(sourceWorkspacePath, targetWorkspacePath, true, false);

            sourceFile.Refresh();
            targetFile.Refresh();

            var expected = sourceFile.Length;
            var actual = targetFile.Length;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SyncToWithOverwriteIsFalseExpectedFileInDestinationUnchanged()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out sourceResources);

            List<IResource> targetResources;
            var targetWorkspaceID = Guid.NewGuid();
            var targetWorkspacePath = SaveResources(targetWorkspaceID, out targetResources);

            var sourceResource = sourceResources[0];
            var targetResource = targetResources.First(r => r.ResourceID == sourceResource.ResourceID);
            var sourceFile = new FileInfo(sourceResource.FilePath);
            var targetFile = new FileInfo(targetResource.FilePath);

            var fs = sourceFile.Open(FileMode.Append, FileAccess.Write, FileShare.None);
            fs.Write(new byte[]
            {
                200
            }, 0, 1);
            fs.Close();

            targetFile.Refresh();
            var expected = targetFile.Length;

            new ResourceCatalog().SyncTo(sourceWorkspacePath, targetWorkspacePath, false, false);

            targetFile.Refresh();

            var actual = targetFile.Length;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SyncToWithFilesToIgnoreSpecifiedExpectedIgnoredFilesAreNotCopied()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out sourceResources);

            List<IResource> targetResources;
            var targetWorkspaceID = Guid.NewGuid();
            var targetWorkspacePath = SaveResources(targetWorkspaceID, out targetResources);

            var sourceResource = sourceResources[0];
            var targetResource = targetResources.First(r => r.ResourceID == sourceResource.ResourceID);
            var targetFile = new FileInfo(targetResource.FilePath);

            targetFile.Delete();
            new ResourceCatalog().SyncTo(sourceWorkspacePath, targetWorkspacePath, false, false, new List<string> { targetFile.Name });
            targetFile.Refresh();
            Assert.IsFalse(targetFile.Exists);
        }

        [TestMethod]
        public void SyncToWithFilesToIgnoreSpecifiedExpectedIgnoredFilesAreNotDeleted()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out sourceResources);

            List<IResource> targetResources;
            var targetWorkspaceID = Guid.NewGuid();
            var targetWorkspacePath = SaveResources(targetWorkspaceID, out targetResources);

            var sourceResource = sourceResources[0];
            var targetResource = targetResources.First(r => r.ResourceID == sourceResource.ResourceID);
            var sourceFile = new FileInfo(sourceResource.FilePath);
            var targetFile = new FileInfo(targetResource.FilePath);

            sourceFile.Delete();
            new ResourceCatalog().SyncTo(sourceWorkspacePath, targetWorkspacePath, false, false, new List<string> { targetFile.Name });
            targetFile.Refresh();
            Assert.IsTrue(targetFile.Exists);
        }

        [TestMethod]
        public void SyncToWithNonExistingDestinationDirectoryExpectedDestinationDirectoryCreated()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out sourceResources);

            var targetWorkspaceID = Guid.NewGuid();
            var targetWorkspacePath = EnvironmentVariables.GetWorkspacePath(targetWorkspaceID);

            var targetDir = new DirectoryInfo(targetWorkspacePath);

            new ResourceCatalog().SyncTo(sourceWorkspacePath, targetWorkspacePath, false, false);
            targetDir.Refresh();
            Assert.IsTrue(targetDir.Exists);

        }

        #endregion

        #region ToPayload

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ResourceCatalog_ToPayload")]
        public void ResourceCatalog_ToPayload_GetServiceNormalPayload_ConnectionStringAsAttributeOfRootTag()
        {
            //------------Setup for test--------------------------

            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            var catalog = new ResourceCatalog();
            List<IResource> saveResources = SaveResources(sourcesPath, null, false, false, new[] { "ServerConnection1", "ServerConnection2" }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            //------------Execute Test---------------------------

            var payload = catalog.ToPayload(saveResources[0]);

            //------------Assert Results-------------------------

            Assert.IsTrue(payload.ToString().StartsWith("<Source"));
            XElement payloadElement = XElement.Parse(payload.ToString());
            string connectionStringattribute = payloadElement.AttributeSafe("ConnectionString");
            Assert.IsFalse(string.IsNullOrEmpty(connectionStringattribute));
        }

        #endregion

        #region GetPayload

        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void GetPayloadWithNullResourceNameAndTypeExpectedThrowsInvalidDataContractException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            catalog.GetPayload(workspaceID, null, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetPayloadWithNullTypeExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            catalog.GetPayload(workspaceID, null, null);
        }

        [TestMethod]
        public void GetPayloadWithGuidsExpectedReturnsPayloadForGuids()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var expectedResources = resources.Where(r => r.ResourceType == ResourceType.WorkflowService).ToList();
            var guids = expectedResources.Select(r => r.ResourceID).ToList();

            var catalog = new ResourceCatalog();
            var payloadXml = catalog.GetPayload(workspaceID, string.Join(",", guids), ResourceTypeConverter.TypeWorkflowService);

            VerifyPayload(expectedResources, "<x>" + payloadXml + "</x>");
        }

        [TestMethod]
        public void GetPayloadWithInvalidGuidsExpectedReturnsEmptyPayload()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var guids = new[] { Guid.NewGuid(), Guid.NewGuid() };

            var catalog = new ResourceCatalog();
            var payloadXml = catalog.GetPayload(workspaceID, string.Join(",", guids), ResourceTypeConverter.TypeWorkflowService);

            VerifyPayload(new List<IResource>(), "<x>" + payloadXml + "</x>");
        }

        [TestMethod]
        public void GetPayloadWithExistingResourceNameExpectedReturnsPayloadForResources()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            foreach(var expected in resources)
            {
                var payloadXml = catalog.GetPayload(workspaceID, expected.ResourceName, ResourceTypeConverter.ToTypeString(expected.ResourceType), null);
                VerifyPayload(new List<IResource> { expected }, "<x>" + payloadXml + "</x>");
            }
        }

        [TestMethod]
        public void GetPayloadWithExistingResourceNameAndContainsFalseExpectedReturnsPayloadForResources()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            foreach(var expected in resources)
            {
                var payloadXml = catalog.GetPayload(workspaceID, expected.ResourceName, ResourceTypeConverter.ToTypeString(expected.ResourceType), null, false);
                VerifyPayload(new List<IResource> { expected }, "<x>" + payloadXml + "</x>");
            }
        }

        [TestMethod]
        public void GetPayloadWithNonExistentResourceNameExpectedReturnsEmptyString()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            var payloadXml = catalog.GetPayload(workspaceID, "xxx", ResourceTypeConverter.TypeService, null);
            VerifyPayload(new List<IResource>(), "<x>" + payloadXml + "</x>");
        }

        [TestMethod]
        public void GetPayloadWithValidSourceTypeExpectedReturnsResources()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var expected = resources.First(r => r.ResourceType == ResourceType.PluginSource);

            var catalog = new ResourceCatalog();
            var payloadXml = catalog.GetPayload(workspaceID, enSourceType.Plugin);

            VerifyPayload(new List<IResource> { expected }, "<x>" + payloadXml + "</x>");
        }

        [TestMethod]
        public void GetPayloadWithInvalidSourceTypeExpectedReturnsNothing()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            var payloadXml = catalog.GetPayload(workspaceID, enSourceType.Unknown);

            VerifyPayload(new List<IResource>(), "<x>" + payloadXml + "</x>");
        }
        #endregion

        #region CopyResource

        [TestMethod]
        public void CopyResourceWithNullResourceExpectedDoesNotCopyResourceToTarget()
        {
            var targetWorkspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            var result = catalog.CopyResource(null, targetWorkspaceID);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CopyResourceWithExistingResourceNameExpectedCopiesResourceToTarget()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            SaveResources(sourceWorkspaceID, out sourceResources);

            List<IResource> targetResources;
            var targetWorkspaceID = Guid.NewGuid();
            SaveResources(targetWorkspaceID, out targetResources);

            var sourceResource = sourceResources[0];
            var targetResource = targetResources.First(r => r.ResourceID == sourceResource.ResourceID);
            var targetFile = new FileInfo(targetResource.FilePath);

            targetFile.Delete();
            var result = new ResourceCatalog().CopyResource(sourceResource.ResourceID, sourceWorkspaceID, targetWorkspaceID);
            Assert.IsTrue(result);
            targetFile.Refresh();
            Assert.IsTrue(targetFile.Exists);
        }

        [TestMethod]
        public void CopyResourceWithNonExistingResourceNameExpectedDoesNotCopyResourceToTarget()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            SaveResources(sourceWorkspaceID, out sourceResources);

            List<IResource> targetResources;
            var targetWorkspaceID = Guid.NewGuid();
            SaveResources(targetWorkspaceID, out targetResources);

            var result = new ResourceCatalog().CopyResource(Guid.Empty, sourceWorkspaceID, targetWorkspaceID);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CopyResourceWithNonExistingResourceFilePathExpectedDoesNotCopyResourceToTarget()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            SaveResources(sourceWorkspaceID, out sourceResources);

            List<IResource> targetResources;
            var targetWorkspaceID = Guid.NewGuid();
            SaveResources(targetWorkspaceID, out targetResources);

            var sourceResource = sourceResources[0];
            var targetResource = targetResources.First(r => r.ResourceID == sourceResource.ResourceID);
            var sourceFile = new FileInfo(sourceResource.FilePath);
            var targetFile = new FileInfo(targetResource.FilePath);

            sourceFile.Delete();
            targetFile.Delete();

            var result = new ResourceCatalog().CopyResource(sourceResource.ResourceID, sourceWorkspaceID, targetWorkspaceID);
            Assert.IsFalse(result);
            targetFile.Refresh();
            Assert.IsFalse(targetFile.Exists);
        }

        #endregion

        #region RollbackResource

        [TestMethod]
        public void RollbackResourceWithUnsignedVersionExpectedDoesNotRollback()
        {
            const string ResourceName = "TestDecisionUnsigned";
            var toVersion = new Version(9999, 0);

            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var path = Path.Combine(workspacePath, "Services");
            var versionControlPath = Path.Combine(path, "VersionControl");
            Directory.CreateDirectory(versionControlPath);

            // Save unsigned version 
            var xml = XmlResource.Fetch(ResourceName);
            xml.Save(Path.Combine(versionControlPath, ResourceName + ".V" + toVersion.Major + ".xml"));

            // Sign and save
            var resource = new Workflow(xml);
            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, resource);

            // Perform test
            var rolledBack = catalog.RollbackResource(workspaceID, resource.ResourceID, resource.Version, toVersion);

            Assert.IsFalse(rolledBack);
        }

        [TestMethod]
        public void RollbackResourceWithSignedVersionExpectedDoesRollback()
        {
            const string ResourceName = "Calculate_RecordSet_Subtract";
            var toVersion = new Version(9999, 0);

            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var path = Path.Combine(workspacePath, "Services");
            var versionControlPath = Path.Combine(path, "VersionControl");
            Directory.CreateDirectory(versionControlPath);

            // Save unsigned version 
            var xml = XmlResource.Fetch(ResourceName);
            xml.Save(Path.Combine(versionControlPath, ResourceName + ".V" + toVersion.Major + ".xml"));

            // Sign and save
            var resource = new Workflow(xml);
            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, resource);

            // Perform test
            var rolledBack = catalog.RollbackResource(workspaceID, resource.ResourceID, resource.Version, toVersion);

            Assert.IsTrue(rolledBack);
        }

        #endregion

        #region DeleteResource

        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void DeleteResourceWithNullResourceNameExpectedThrowsInvalidDataContractException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            catalog.DeleteResource(workspaceID, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void DeleteResourceWithNullTypeExpectedThrowsInvalidDataContractException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            catalog.DeleteResource(workspaceID, "xxx", null);
        }

        [TestMethod]
        public void DeleteResourceWithWildcardResourceNameExpectedReturnsNoWildcardsAllowed()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            var result = catalog.DeleteResource(workspaceID, "*", ResourceTypeConverter.TypeWorkflowService);
            Assert.AreEqual(ExecStatus.NoWildcardsAllowed, result.Status);
        }

        [TestMethod]
        public void DeleteResourceWithNonExistingResourceNameExpectedReturnsNoMatch()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            var result = catalog.DeleteResource(workspaceID, "xxx", ResourceTypeConverter.TypeWorkflowService);
            Assert.AreEqual(ExecStatus.NoMatch, result.Status);
        }

        [TestMethod]
        public void DeleteResourceWithManyExistingResourceNamesExpectedReturnsDuplicateMatch()
        {
            const string ResourceName = "Test";

            var workspaceID = Guid.NewGuid();

            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, new Resource { ResourceID = Guid.NewGuid(), ResourceName = ResourceName, ResourceType = ResourceType.WorkflowService });
            catalog.SaveResource(workspaceID, new Resource { ResourceID = Guid.NewGuid(), ResourceName = ResourceName, ResourceType = ResourceType.WorkflowService });

            var result = catalog.DeleteResource(workspaceID, ResourceName, ResourceTypeConverter.TypeWorkflowService);
            Assert.AreEqual(ExecStatus.DuplicateMatch, result.Status);
        }


        [TestMethod]
        public void DeleteResourceWithOneExistingResourceNameAndValidUserRolesExpectedReturnsSuccessAndDeletesFileFromDisk()
        {
            const string ResourceName = "Test";
            const string UserRoles = "Admins, Power Users";

            var workspaceID = Guid.NewGuid();

            var catalog = new ResourceCatalog();
            var resource = new Resource { ResourceID = Guid.NewGuid(), ResourceName = ResourceName, ResourceType = ResourceType.WorkflowService, AuthorRoles = "Admins, Domain Admins" };
            catalog.SaveResource(workspaceID, resource, UserRoles);

            var file = new FileInfo(resource.FilePath);
            Assert.IsTrue(file.Exists);

            var result = catalog.DeleteResource(workspaceID, ResourceName, ResourceTypeConverter.TypeWorkflowService, UserRoles);
            Assert.AreEqual(ExecStatus.Success, result.Status);

            var actual = catalog.GetResource(workspaceID, resource.ResourceID);
            Assert.IsNull(actual);

            file.Refresh();
            Assert.IsFalse(file.Exists);
        }

        [TestMethod]
        [TestCategory("ResourceCatelog_Delete")]
        [Description("Unassigned resources can be deleted")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ResourceCatelog_UnitTest_DeleteUnassignedResource_ResourceDeletedFromCatalog()
        // ReSharper restore InconsistentNaming
        {
            const string ResourceName = "Test";
            const string UserRoles = "Admins, Power Users";

            var workspaceID = Guid.NewGuid();

            var catalog = new ResourceCatalog();
            var resource = new Resource { ResourceID = Guid.NewGuid(), ResourceName = ResourceName, ResourceType = ResourceType.WorkflowService, AuthorRoles = "Admins, Domain Admins", ResourcePath = string.Empty };
            catalog.SaveResource(workspaceID, resource, UserRoles);

            var file = new FileInfo(resource.FilePath);
            Assert.IsTrue(file.Exists);

            var result = catalog.DeleteResource(workspaceID, ResourceName, ResourceTypeConverter.TypeWorkflowService, UserRoles);
            Assert.AreEqual(ExecStatus.Success, result.Status);

            var actual = catalog.GetResource(workspaceID, resource.ResourceID);
            Assert.IsNull(actual);

            file.Refresh();
            Assert.IsFalse(file.Exists);
        }

        #endregion

        #region GetDynamicObjects

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDynamicObjectsWithNullResourceNameExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            catalog.GetDynamicObjects<DynamicServiceObjectBase>(workspaceID, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDynamicObjectsWithNullResourceNameAndContainsTrueExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            catalog.GetDynamicObjects<DynamicServiceObjectBase>(workspaceID, null, true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDynamicObjectsWithNullResourceExpectedThrowsArgumentNullException()
        {
            var catalog = new ResourceCatalog();
            catalog.GetDynamicObjects((IResource)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDynamicObjectsWithNullResourcesExpectedThrowsArgumentNullException()
        {
            var catalog = new ResourceCatalog();
            catalog.GetDynamicObjects((IEnumerable<IResource>)null);
        }

        [TestMethod]
        public void GetDynamicObjectsWithResourceNameExpectedReturnsObjectGraph()
        {
            var workspaceID = Guid.NewGuid();
            List<IResource> resources;
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();

            var expected = resources.First(r => r.ResourceType == ResourceType.WorkflowService);

            var graph = catalog.GetDynamicObjects<DynamicService>(workspaceID, expected.ResourceName, true);

            VerifyObjectGraph(new List<IResource> { expected }, graph);
        }

        [TestMethod]
        public void GetDynamicObjectsWithResourceExpectedReturnsObjectGraph()
        {
            var workspaceID = Guid.NewGuid();
            List<IResource> resources;
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();

            var expected = resources.First(r => r.ResourceType == ResourceType.WorkflowService);

            var graph = catalog.GetDynamicObjects(expected);

            VerifyObjectGraph(new List<IResource> { expected }, graph);
        }

        [TestMethod]
        public void GetDynamicObjectsWithResourcesExpectedReturnsObjectGraphs()
        {
            var workspaceID = Guid.NewGuid();
            List<IResource> resources;
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();

            var expecteds = resources.Where(r => r.ResourceType == ResourceType.WorkflowService).ToList();

            var graph = catalog.GetDynamicObjects(expecteds);

            VerifyObjectGraph(expecteds, graph);
        }

        [TestMethod]
        public void GetDynamicObjectsWithWorkspaceIDExpectedReturnsObjectGraphsForWorkspace()
        {
            var workspaceID = Guid.NewGuid();
            List<IResource> resources;
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();

            var graph = catalog.GetDynamicObjects(workspaceID);

            VerifyObjectGraph(resources, graph);
        }

        #endregion

        #region RemoveWorkspace

        [TestMethod]
        public void RemoveWorkspaceWithInvalidIDExpectedDoesNothing()
        {
            const int WorkspaceCount = 5;
            const int ExpectedWorkspaceCount = WorkspaceCount + 1; // add 1 for server workspace that is auto-loaded

            var catalog = new ResourceCatalog();

            var workspaces = new List<Guid>();
            for(var i = 0; i < WorkspaceCount; i++)
            {
                var id = Guid.NewGuid();
                catalog.LoadWorkspace(id);
                workspaces.Add(id);
            }

            Assert.AreEqual(ExpectedWorkspaceCount, catalog.WorkspaceCount);
            catalog.RemoveWorkspace(Guid.NewGuid());
            Assert.AreEqual(ExpectedWorkspaceCount, catalog.WorkspaceCount);
        }

        [TestMethod]
        public void RemoveWorkspaceWithValidIDExpectedRemovesWorkspace()
        {
            const int WorkspaceCount = 5;
            const int ExpectedWorkspaceCount = WorkspaceCount + 1; // add 1 for server workspace that is auto-loaded

            var catalog = new ResourceCatalog();

            var workspaces = new List<Guid>();
            for(var i = 0; i < WorkspaceCount; i++)
            {
                var id = Guid.NewGuid();
                catalog.LoadWorkspace(id);
                workspaces.Add(id);
            }
            Assert.AreEqual(ExpectedWorkspaceCount, catalog.WorkspaceCount);
            catalog.RemoveWorkspace(workspaces[3]);
            Assert.AreEqual(ExpectedWorkspaceCount - 1, catalog.WorkspaceCount);
        }

        #endregion

        #region GetModels

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumDev2Server_ExpectConnectionObjects()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "ServerConnection1", "ServerConnection2" }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            Assert.AreEqual(2, result.Count);

            //------------Execute Test---------------------------

            var models = rc.GetModels(workspaceID, enSourceType.Dev2Server);
            //------------Assert Results-------------------------

            foreach(var model in models)
            {
                Assert.AreEqual(typeof(Connection), model.GetType());
            }

            var payload = JsonConvert.SerializeObject(models);

            Assert.IsNotNull(payload);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumEmailSource_ExpectEmailSourceObjects()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "EmailSource" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            Assert.AreEqual(1, result.Count);

            //------------Execute Test---------------------------
            var models = rc.GetModels(workspaceID, enSourceType.EmailSource);

            //------------Assert Results-------------------------
            foreach(var model in models)
            {
                Assert.AreEqual(typeof(EmailSource), model.GetType());
            }

            var payload = JsonConvert.SerializeObject(models);

            Assert.IsNotNull(payload);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumSqlDatabase_ExpectDbSourceObjects()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "EmailSource" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            Assert.AreEqual(1, result.Count);

            //------------Execute Test---------------------------
            var models = rc.GetModels(workspaceID, enSourceType.SqlDatabase);

            //------------Assert Results-------------------------
            foreach(var model in models)
            {
                Assert.AreEqual(typeof(DbSource), model.GetType());
            }

            var payload = JsonConvert.SerializeObject(models);

            Assert.IsNotNull(payload);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumPlugin_ExpectPluginSourceObjects()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "PluginSource" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            Assert.AreEqual(1, result.Count);

            //------------Execute Test---------------------------
            var models = rc.GetModels(workspaceID, enSourceType.Plugin);

            //------------Assert Results-------------------------
            foreach(var model in models)
            {
                Assert.AreEqual(typeof(PluginSource), model.GetType());
            }

            var payload = JsonConvert.SerializeObject(models);

            Assert.IsNotNull(payload);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumWebSource_ExpectWebSourceObjects()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "WebSourceWithoutInputs" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            Assert.AreEqual(1, result.Count);

            //------------Execute Test---------------------------
            var models = rc.GetModels(workspaceID, enSourceType.WebSource);

            //------------Assert Results-------------------------
            foreach(var model in models)
            {
                Assert.AreEqual(typeof(WebSource), model.GetType());
            }

            var payload = JsonConvert.SerializeObject(models);

            Assert.IsNotNull(payload);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumWebService_ExpectNull()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "EmailSource" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog();
            rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            var models = rc.GetModels(workspaceID, enSourceType.WebService);

            Assert.IsNull(models);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumDynamicService_ExpectNull()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "EmailSource" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog();
            rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            var models = rc.GetModels(workspaceID, enSourceType.DynamicService);

            Assert.IsNull(models);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumMySqlDatabase_ExpectNull()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "EmailSource" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog();
            rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            var models = rc.GetModels(workspaceID, enSourceType.MySqlDatabase);

            Assert.IsNull(models);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumManagementDynamicService_ExpectNull()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "EmailSource" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog();
            rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            var models = rc.GetModels(workspaceID, enSourceType.ManagementDynamicService);

            Assert.IsNull(models);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumUnknown_ExpectNullModels()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "EmailSource" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog();
            rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            var models = rc.GetModels(workspaceID, enSourceType.Unknown);

            Assert.IsNull(models);
        }


        #endregion

        //
        // Static helpers
        //

        #region UpdateResourceCatalog

        [TestMethod]
        [Description("Updates the Name of a resource and updates where used")]
        [Owner("Ashley Lewis")]
        public void ResourceCatalog_UnitTest_UpdateResourceNameValidArguments_ExpectFileContentsAndNameUpdatedBothResources()
        {
            //------------Setup for test-------------------------
            var newName = "RenamedResource";
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceID = "ec636256-5f11-40ab-a044-10e731d87555";
            var depResourceID = "1736ca6e-b870-467f-8d25-262972d8c3e8";
            SaveResources(path, null, true, false, new[] { "Bug6619Dep", "Bug6619" }, new[] { Guid.Parse(resourceID), Guid.Parse(depResourceID) }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceID.ToString() == resourceID);
            IResource depResource = result.FirstOrDefault(resource => resource.ResourceID.ToString() == depResourceID);
            //------------Assert Precondition--------------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.RenameResource(workspaceID, oldResource.ResourceID, newName);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual("Renamed Resource 'ec636256-5f11-40ab-a044-10e731d87555' to '" + newName + "'", resourceCatalogResult.Message);

            //assert resource renamed
            string resourceContents = rc.GetResourceContents(workspaceID, oldResource.ResourceID).ToString();
            XElement xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            XAttribute nameAttrib = xElement.Attribute("Name");
            Assert.IsNotNull(nameAttrib);
            Assert.AreEqual(newName, nameAttrib.Value);
            Assert.IsTrue(File.Exists(path + "\\" + newName + ".xml"));

            //assert rename where used
            result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            depResource = result.FirstOrDefault(resource => resource.ResourceID.ToString() == depResourceID);
            resourceContents = rc.GetResourceContents(workspaceID, depResource.ResourceID).ToString();
            xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            var actionElem = xElement.Element("Action");
            Assert.IsNotNull(actionElem);
            var xamlElem = actionElem.Element("XamlDefinition");
            Assert.IsNotNull(xamlElem);
            Assert.IsTrue(xamlElem.ToString().Contains("DisplayName=\"" + newName + "\""), "Resource not renamed where used.");
            Assert.IsFalse(xamlElem.ToString().Contains("DisplayName=\"Bug6619Dep\""), "Resource not renamed where used.");
            Assert.AreEqual(depResource.Dependencies[0].ResourceName, newName, "Resource not renamed where used");
        }

        [TestMethod]
        [Description("Requires Valid arguments")]
        [Owner("Ashley Lewis")]
        public void ResourceCatalog_UnitTest_UpdateResourceNameWithNullOldName_ExpectRename()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceID = "ec636256-5f11-40ab-a044-10e731d87555";
            SaveResources(path, null, false, false, new[] { "Bug6619Dep" }, new[] { Guid.Parse(resourceID) }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceID.ToString() == resourceID);
            //------------Assert Precondition--------------------
            Assert.AreEqual(1, result.Count);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.RenameResource(workspaceID, Guid.Parse(resourceID), "TestName");
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual("Renamed Resource '" + resourceID + "' to 'TestName'", resourceCatalogResult.Message);
            string resourceContents = rc.GetResourceContents(workspaceID, oldResource.ResourceID).ToString();
            XElement xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            var element = xElement.Attribute("Name");
            Assert.IsNotNull(element);
            Assert.AreEqual("TestName", element.Value);
        }

        [TestMethod]
        [Description("Needs valid arguments")]
        [Owner("Ashley Lewis")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourceCatalog_UnitTest_UpdateResourceWithNullNewName_ExpectArgumentNullException()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceID = "ec636256-5f11-40ab-a044-10e731d87555";
            SaveResources(path, null, false, false, new[] { "Bug6619Dep" }, new[] { Guid.Parse(resourceID) }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceID.ToString() == resourceID);
            //------------Assert Precondition--------------------
            Assert.AreEqual(1, result.Count);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.RenameResource(workspaceID, oldResource.ResourceID, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual("<CompilerMessage>Updated Resource '50fef451-b41e-4bdf-92a1-4a41e254cde2' renamed to ''</CompilerMessage>", resourceCatalogResult.Message);
            string resourceContents = rc.GetResourceContents(workspaceID, oldResource.ResourceID).ToString();
            XElement xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            XElement element = xElement.Element("Name");
            Assert.IsNotNull(element);
            Assert.AreEqual("Bug6619Dep", element.Value);
        }

        [TestMethod]
        [Description("Needs valid arguments")]
        [Owner("Ashley Lewis")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourceCatalog_UnitTest_UpdateResourceNameWithEmptyNewName_ExpectArgumentNullException()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceID = "ec636256-5f11-40ab-a044-10e731d87555";
            SaveResources(path, null, false, false, new[] { "Bug6619Dep" }, new[] { Guid.Parse(resourceID) }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceID.ToString() == resourceID);
            //------------Assert Precondition--------------------
            Assert.AreEqual(1, result.Count);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.RenameResource(workspaceID, oldResource.ResourceID, "");
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual("<CompilerMessage>Updated Resource '50fef451-b41e-4bdf-92a1-4a41e254cde2' renamed to ''</CompilerMessage>", resourceCatalogResult.Message);
            string resourceContents = rc.GetResourceContents(workspaceID, oldResource.ResourceID).ToString();
            XElement xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            XElement element = xElement.Element("Name");
            Assert.IsNotNull(element);
            Assert.AreEqual("Bug6619Dep", element.Value);
        }

        [TestMethod]
        [Description("Updates the Category of the resource")]
        [Owner("Huggs")]
        public void ResourceCatalog_UnitTest_UpdateResourceCategoryValidArguments_ExpectFileContentsUpdated()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.RenameCategory(workspaceID, oldResource.ResourcePath, "TestCategory", ResourceType.WorkflowService.ToString());
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual("<CompilerMessage>Updated Category from 'Bugs' to 'TestCategory'</CompilerMessage>", resourceCatalogResult.Message);
            string resourceContents = rc.GetResourceContents(workspaceID, oldResource.ResourceID).ToString();
            XElement xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            XElement element = xElement.Element("Category");
            Assert.IsNotNull(element);
            Assert.AreEqual("TestCategory", element.Value);
        }

        [TestMethod]
        [Description("Updates the Category of the resource")]
        [Owner("Huggs")]
        public void ResourceCatalog_UnitTest_UpdateResourceCategoryValidArgumentsDifferentCasing_ExpectFileContentsUpdated()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.RenameCategory(workspaceID, oldResource.ResourcePath.ToLower(), "TestCategory", ResourceType.WorkflowService.ToString());
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual("<CompilerMessage>Updated Category from 'bugs' to 'TestCategory'</CompilerMessage>", resourceCatalogResult.Message);
            string resourceContents = rc.GetResourceContents(workspaceID, oldResource.ResourceID).ToString();
            XElement xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            XElement element = xElement.Element("Category");
            Assert.IsNotNull(element);
            Assert.AreEqual("TestCategory", element.Value);
        }

        [TestMethod]
        [Description("Requires Valid arguments")]
        [Owner("Huggs")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourceCatalog_UnitTest_UpdateResourceCategoryWithNullOldCategory_ExpectArgumentNullException()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.RenameCategory(workspaceID, null, "TestCategory", ResourceType.WorkflowService.ToString());
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual("<CompilerMessage>Updated Category from 'Bugs' to 'TestCategory'</CompilerMessage>", resourceCatalogResult.Message);
            string resourceContents = rc.GetResourceContents(workspaceID, oldResource.ResourceID).ToString();
            XElement xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            XElement element = xElement.Element("Category");
            Assert.IsNotNull(element);
            Assert.AreEqual("TestCategory", element.Value);
        }

        [TestMethod]
        [Description("Needs valid arguments")]
        [Owner("Huggs")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourceCatalog_UnitTest_UpdateResourceCategoryWithNullNewCategory_ExpectArgumentNullException()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.RenameCategory(workspaceID, oldResource.ResourcePath, null, ResourceType.WorkflowService.ToString());
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual("<CompilerMessage>Updated Category from 'Bugs' to 'TestCategory'</CompilerMessage>", resourceCatalogResult.Message);
            string resourceContents = rc.GetResourceContents(workspaceID, oldResource.ResourceID).ToString();
            XElement xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            XElement element = xElement.Element("Category");
            Assert.IsNotNull(element);
            Assert.AreEqual("TestCategory", element.Value);
        }

        [TestMethod]
        [Description("Needs valid arguments")]
        [Owner("Huggs")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourceCatalog_UnitTest_UpdateResourceCategoryWithEmptyNewCategory_ExpectArgumentNullException()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.RenameCategory(workspaceID, oldResource.ResourcePath, "", ResourceType.WorkflowService.ToString());
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual("<CompilerMessage>Updated Category from 'Bugs' to 'TestCategory'</CompilerMessage>", resourceCatalogResult.Message);
            string resourceContents = rc.GetResourceContents(workspaceID, oldResource.ResourceID).ToString();
            XElement xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            XElement element = xElement.Element("Category");
            Assert.IsNotNull(element);
            Assert.AreEqual("TestCategory", element.Value);
        }

        #endregion

        #region SaveResources

        public static void SaveResources(Guid sourceWorkspaceID, Guid copyToWorkspaceID, string versionNo, bool injectID, bool signXml, string[] sources, string[] services, out List<IResource> resources, Guid[] sourceIDs, Guid[] serviceIDs)
        {
            lock(syncRoot)
            {
                var sourceWorkspacePath = SaveResources(sourceWorkspaceID, versionNo, injectID, signXml, sources,
                                                        services, out resources, sourceIDs, serviceIDs);
                var targetWorkspacePath = EnvironmentVariables.GetWorkspacePath(copyToWorkspaceID);
                DirectoryHelper.Copy(sourceWorkspacePath, targetWorkspacePath, true);
            }
        }

        public static string SaveResources(Guid workspaceID, string versionNo, bool injectID, bool signXml, string[] sources, string[] services, out List<IResource> resources, Guid[] sourceIDs, Guid[] serviceIDs)
        {
            lock(syncRoot)
            {
                var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
                var sourcesPath = Path.Combine(workspacePath, "Sources");
                var servicesPath = Path.Combine(workspacePath, "Services");

                Directory.CreateDirectory(Path.Combine(sourcesPath, "VersionControl"));
                Directory.CreateDirectory(Path.Combine(servicesPath, "VersionControl"));

                resources = new List<IResource>();
                if(sources != null && sources.Length != 0)
                {
                    resources.AddRange(SaveResources(sourcesPath, versionNo, injectID, signXml, sources, sourceIDs));
                }
                if(services != null && services.Length != 0)
                {
                    resources.AddRange(SaveResources(servicesPath, versionNo, injectID, signXml, services, serviceIDs));
                }

                return workspacePath;
            }
        }

        static string SaveResources(Guid workspaceID, out List<IResource> resources)
        {
            return SaveResources(workspaceID, null, false, false,
                new[] { "CatalogSourceAnytingToXmlPlugin", "CatalogSourceCitiesDatabase", "CatalogSourceTestServer" },
                new[] { "CatalogServiceAllTools", "CatalogServiceCitiesDatabase", "CatalogServiceCalculateRecordSetSubtract" },
                out resources,
                new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() },
                new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() });
        }

        static IEnumerable<IResource> SaveResources(string resourcesPath, string versionNo, bool injectID, bool signXml, string[] resourceNames, Guid[] resourceIDs)
        {
            lock(syncRoot)
            {
                var result = new List<IResource>();
                int count = 0;
                foreach(var resourceName in resourceNames)
                {
                    var xml = XmlResource.Fetch(resourceName);
                    if(injectID)
                    {
                        var idAttr = xml.Attribute("ID");
                        if(idAttr == null)
                        {
                            xml.Add(new XAttribute("ID", resourceIDs[count]));
                        }
                    }

                    var contents = xml.ToString(SaveOptions.DisableFormatting);

                    if(signXml)
                    {
                        contents = HostSecurityProvider.Instance.SignXml(new StringBuilder(contents)).ToString();
                    }
                    var res = new Resource(xml)
                        {
                            FilePath = Path.Combine(resourcesPath, resourceName + ".xml")
                        };

                    // Just in case sign the xml

                    File.WriteAllText(res.FilePath, contents, Encoding.UTF8);

                    if(!string.IsNullOrEmpty(versionNo))
                    {
                        File.WriteAllText(
                            Path.Combine(resourcesPath,
                                         string.Format("VersionControl\\{0}.V{1}.xml", resourceName, versionNo)),
                            contents, Encoding.UTF8);
                    }
                    result.Add(res);
                    count++;
                }
                return result;
            }
        }

        #endregion

        #region GetDependants

        [TestMethod]
        public void GetDependantsWhereResourceIsDependedOnExpectNonEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependants(workspaceID, resourceName);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dependants.Count);
            Assert.AreEqual("Bug6619", dependants[0]);
        }

        [TestMethod]
        public void UpdateResourceWhereResourceIsDependedOnExpectNonEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            var resource = result.FirstOrDefault(r => r.ResourceName == "Bug6619Dep");
            var depresource = result.FirstOrDefault(r => r.ResourceName == "Bug6619");
            var beforeService = rc.GetDynamicObjects<DynamicService>(workspaceID, resource.ResourceName).FirstOrDefault();
            ServiceAction beforeAction = beforeService.Actions.FirstOrDefault();
            var xElement = rc.GetResourceContents(resource).ToXElement();
            var element = xElement.Element("DataList");
            element.Add(new XElement("a"));
            element.Add(new XElement("b"));
            var s = xElement.ToString();

            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            rc.CompileTheResourceAfterSave(workspaceID, resource, new StringBuilder(s), beforeAction);
            //------------Assert Results-------------------------

            xElement = rc.GetResourceContents(depresource).ToXElement();
            var errorElement = xElement.Element("ErrorMessages").Element("ErrorMessage");
            var isValid = xElement.AttributeSafe("IsValid");
            var messageType = Enum.Parse(typeof(CompileMessageType), errorElement.AttributeSafe("MessageType"), true);
            Assert.AreEqual("false", isValid);
            Assert.AreEqual(CompileMessageType.MappingChange, messageType);

            var messages = CompileMessageRepo.Instance.FetchMessages(workspaceID, depresource.ResourceID, new List<string>());
            var message = messages.MessageList[0];
            Assert.AreEqual(workspaceID, message.WorkspaceID);
            Assert.AreEqual(depresource.ResourceID, message.ServiceID);
            Assert.AreEqual(depresource.ResourceName, message.ServiceName);
            Assert.AreNotEqual(depresource.ResourceID, message.UniqueID);
            Assert.AreNotEqual(resource.ResourceID, message.UniqueID);
        }

        [TestMethod]
        public void UpdateResourceWhereResourceIsDependedOnExpectNonEmptyListForResource()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            var resource = result.FirstOrDefault(r => r.ResourceName == "Bug6619Dep");
            var depresource = result.FirstOrDefault(r => r.ResourceName == "Bug6619");
            var beforeService = rc.GetDynamicObjects<DynamicService>(workspaceID, resource.ResourceName).FirstOrDefault();
            ServiceAction beforeAction = beforeService.Actions.FirstOrDefault();

            var xElement = rc.GetResourceContents(resource).ToXElement();

            var element = xElement.Element("DataList");
            element.Add(new XElement("a"));
            element.Add(new XElement("b"));
            var s = xElement.ToString();

            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            rc.CompileTheResourceAfterSave(workspaceID, resource, new StringBuilder(s), beforeAction);
            //------------Assert Results-------------------------

            xElement = rc.GetResourceContents(depresource).ToXElement();
            var errorElement = xElement.Element("ErrorMessages").Element("ErrorMessage");
            var isValid = xElement.AttributeSafe("IsValid");
            var messageType = Enum.Parse(typeof(CompileMessageType), errorElement.AttributeSafe("MessageType"), true);
            Assert.AreEqual("false", isValid);
            Assert.AreEqual(CompileMessageType.MappingChange, messageType);

            var messages = CompileMessageRepo.Instance.FetchMessages(workspaceID, depresource.ResourceID, new List<string>());
            var message = messages.MessageList[0];
            Assert.AreEqual(workspaceID, message.WorkspaceID);
            Assert.AreEqual(depresource.ResourceID, message.ServiceID);
            Assert.AreEqual(depresource.ResourceName, message.ServiceName);
            Assert.AreNotEqual(depresource.ResourceID, message.UniqueID);
            Assert.AreNotEqual(resource.ResourceID, message.UniqueID);
        }

        [TestMethod]
        public void GetDependantsWhereResourceIsDependedOnExpectNonEmptyListForWorkerService()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "WebService";
            SaveResources(path, null, false, true, new[] { "WebService", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();
            path = Path.Combine(workspacePath, "Sources");

            var xml = XmlResource.Fetch("WeatherWebSource");
            var resource = new WebSource(xml);
            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, resource);

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");

            //------------Assert Precondition-----------------
            Assert.AreEqual(1, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependants(workspaceID, "WeatherFranceParis");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dependants.Count);
        }


        [TestMethod]
        public void GetDependantsWhereNoResourcesExpectEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder("xx", new string[0]);
            var resourceName = "resource";
            //------------Assert Precondition-----------------
            Assert.AreEqual(0, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependants(workspaceID, resourceName);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, dependants.Count);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDependantsWhereResourceNameEmptyStringExpectException()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");

            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependants(workspaceID, "");
            //------------Assert Results-------------------------
            //Exception thrown see attribute
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDependantsWhereResourceNameNullStringExpectException()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");


            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependants(workspaceID, "");
            //------------Assert Results-------------------------
            //Exception thrown see attribute
        }


        [TestMethod]
        public void GetDependantsWhereResourceHasNoDependedOnExpectNonEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { resourceName }, new[] { Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            //------------Assert Precondition-----------------
            Assert.AreEqual(1, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependants(workspaceID, resourceName);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, dependants.Count);
        }

        #endregion

        #region GetDependantsAsResourceForTrees

        [TestMethod]
        public void GetDependantsAsResourceForTreesWhereResourceIsDependedOnExpectNonEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependentsAsResourceForTrees(workspaceID, resourceName);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dependants.Count);
            Assert.AreEqual("Bug6619", dependants[0].ResourceName);
        }

        [TestMethod]
        public void GetDependantsAsResourceForTreesWhereResourceIsDependedOnExpectNonEmptyListForWorkerService()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "WebService";
            SaveResources(path, null, false, true, new[] { "WebService", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();
            path = Path.Combine(workspacePath, "Sources");

            var xml = XmlResource.Fetch("WeatherWebSource");
            var resource = new WebSource(xml);
            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, resource);

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");

            //------------Assert Precondition-----------------
            Assert.AreEqual(1, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependentsAsResourceForTrees(workspaceID, "WeatherFranceParis");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dependants.Count);
        }


        [TestMethod]
        public void GetDependantsAsResourceForTreesWhereNoResourcesExpectEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder("xx", new string[0]);
            var resourceName = "resource";
            //------------Assert Precondition-----------------
            Assert.AreEqual(0, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependentsAsResourceForTrees(workspaceID, resourceName);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, dependants.Count);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDependantsAsResourceForTreesWhereResourceNameEmptyStringExpectException()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");

            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependentsAsResourceForTrees(workspaceID, "");
            //------------Assert Results-------------------------
            //Exception thrown see attribute
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDependantsAsResourceForTreesWhereResourceNameNullStringExpectException()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");


            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependentsAsResourceForTrees(workspaceID, "");
            //------------Assert Results-------------------------
            //Exception thrown see attribute
        }


        [TestMethod]
        public void GetDependantsAsResourceForTreesWhereResourceHasNoDependedOnExpectNonEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { resourceName }, new[] { Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            //------------Assert Precondition-----------------
            Assert.AreEqual(1, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependentsAsResourceForTrees(workspaceID, resourceName);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, dependants.Count);
        }
        #endregion

        #region VerifyPayload

        static void VerifyPayload(ICollection<IResource> expectedResources, string payloadXml)
        {
            var actualResources = XElement.Parse(payloadXml).Elements().Select(x => new Resource(x)).ToList();

            Assert.AreEqual(expectedResources.Count, actualResources.Count);

            foreach(var expected in expectedResources)
            {
                var actual = actualResources.FirstOrDefault(r => r.ResourceID == expected.ResourceID && r.ResourceName == expected.ResourceName);
                Assert.IsNotNull(actual);
            }
        }

        #endregion

        #region VerifyObjectGraph

        static void VerifyObjectGraph<TGraph>(ICollection<IResource> expectedResources, ICollection<TGraph> actualGraphs)
            where TGraph : DynamicServiceObjectBase
        {
            Assert.AreEqual(expectedResources.Count, actualGraphs.Count);

            foreach(var expected in expectedResources)
            {
                var actualGraph = actualGraphs.FirstOrDefault(g => g.Name == expected.ResourceName);
                Assert.IsNotNull(actualGraph);

                var actual = new Resource(actualGraph.ResourceDefinition.ToXElement());
                Assert.AreEqual(expected.ResourceID, actual.ResourceID);
                Assert.AreEqual(expected.ResourceType, actual.ResourceType);
                Assert.AreEqual(expected.ResourcePath, actual.ResourcePath);
            }
        }

        #endregion

        #region Rename Resource

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("RenameResourceService_Execute")]
        public void RenameResourceService_Execute_DashesInNewName_ResourceFileNameChanged()
        {
            var workspace = Guid.NewGuid();
            var resourceID = Guid.NewGuid();
            const string newResourceName = "New-Name-With-Dashes";
            const string oldResourceName = "Old Resource Name";

            var getCatalog = ResourceCatalog.Instance;
            var resource = new Resource
            {
                ResourceName = oldResourceName,
                ResourceID = resourceID,
            };
            getCatalog.SaveResource(workspace, resource);
            var renameResourceService = new RenameResource();
            var mockedWorkspace = new Mock<IWorkspace>();
            mockedWorkspace.Setup(ws => ws.ID).Returns(workspace);
            Directory.CreateDirectory(string.Concat(_testDir, "\\Workspaces\\"));
            Directory.CreateDirectory(string.Concat(_testDir, "\\Workspaces\\", workspace));
            Directory.CreateDirectory(string.Concat(_testDir, "\\Workspaces\\", workspace, "\\Services\\"));

            //------------Execute Test---------------------------
            var result = renameResourceService.Execute(new Dictionary<string, StringBuilder> { { "ResourceID", new StringBuilder(resourceID.ToString()) }, { "NewName", new StringBuilder(newResourceName) } }, mockedWorkspace.Object);

            var obj = ConvertToMsg(result.ToString());
            var renamedResource = getCatalog.GetResource(workspace, resourceID);
            Assert.IsNotNull(renamedResource);
            // Assert Resource FileName Changed
            Assert.IsTrue(obj.Message.Contains("Renamed Resource"));
            Assert.IsTrue(File.Exists(renamedResource.FilePath), "Resource does not exist");
            StringAssert.Contains(renamedResource.FilePath, newResourceName + ".xml");
        }

        #endregion

        #region GetResourceList

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetResourceList")]
        public void ResourceCatalog_GetResourceList_WhenUsingNameAndResourcesPresent_ExpectResourceList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            //------------Execute Test---------------------------
            var workflow = ResourceCatalog.Instance.GetResourceList(workspaceID, resourceName, "*", "*");
            //------------Assert Results-------------------------
            Assert.IsNotNull(workflow);
            Assert.AreEqual(1, workflow.Count);
            Assert.AreEqual(resourceName, workflow[0].ResourceName);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetResourceList")]
        public void ResourceCatalog_GetResourceList_WhenUsingNameAndResourcesPresentAndTypeNull_ExpectResourceList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            //------------Execute Test---------------------------
            var workflow = ResourceCatalog.Instance.GetResourceList(workspaceID, resourceName, null, "*");
            //------------Assert Results-------------------------
            Assert.IsNotNull(workflow);
            Assert.AreEqual(1, workflow.Count);
            Assert.AreEqual(resourceName, workflow[0].ResourceName);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetResourceList")]
        public void ResourceCatalog_GetResourceList_WhenUsingNameAndResourcesNotPresent_ExpectEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            //------------Execute Test---------------------------
            var workflow = ResourceCatalog.Instance.GetResourceList(workspaceID, "Bob", "*", "*");
            //------------Assert Results-------------------------
            Assert.AreEqual(0, workflow.Count);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetResourceList")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void ResourceCatalog_GetResourceList_WhenNameAndResourceNameAndTypeNull_ExpectException()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog();
            rc.LoadWorkspaceViaBuilder(workspacePath, "Services");
            //------------Execute Test---------------------------
            ResourceCatalog.Instance.GetResourceList(workspaceID, null, null, "*");

        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetResourceList")]
        public void ResourceCatalog_GetResourceList_WhenUsingIdAndResourcesPresent_ExpectResourceList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var resources = SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { id1, id2 }).ToList();

            var rc = new ResourceCatalog();
            rc.LoadWorkspaceViaBuilder(workspacePath, "Workflows");

            var searchString = string.Empty;
            foreach(var res in resources)
            {
                searchString += res.ResourceID + ",";
            }

            //------------Execute Test---------------------------
            var workflow = ResourceCatalog.Instance.GetResourceList(workspaceID, searchString, "*");
            //------------Assert Results-------------------------
            Assert.IsNotNull(workflow);
            Assert.AreEqual(2, workflow.Count);

            // the ordering swaps around - hence the contains assert ;)

            StringAssert.Contains(workflow[0].ResourceName, "Bug6619");
            StringAssert.Contains(workflow[1].ResourceName, "Bug6619");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetResourceList")]
        public void ResourceCatalog_GetResourceList_WhenUsingIdAndResourcesNotPresent_ExpectEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var resources = SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { id1, id2 }).ToList();

            var rc = new ResourceCatalog();
            rc.LoadWorkspaceViaBuilder(workspacePath, "Workflows");

            var searchString = string.Empty;
            foreach(var res in resources)
            {
                searchString += Guid.NewGuid() + ",";
            }

            //------------Execute Test---------------------------
            var workflow = ResourceCatalog.Instance.GetResourceList(workspaceID, searchString, "*");
            //------------Assert Results-------------------------
            Assert.AreEqual(0, workflow.Count);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetResourceList")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourceCatalog_GetResourceList_WhenUsingIdAndTypeNull_ExpectException()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var resources = SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { id1, id2 }).ToList();

            var rc = new ResourceCatalog();
            rc.LoadWorkspaceViaBuilder(workspacePath, "Workflows");

            var searchString = string.Empty;
            foreach(var res in resources)
            {
                searchString += Guid.NewGuid() + ",";
            }

            //------------Execute Test---------------------------
            var workflow = ResourceCatalog.Instance.GetResourceList(workspaceID, searchString, null);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetResourceList")]
        public void ResourceCatalog_GetResourceList_WhenUsingIdAndGuidCsvNull_ExpectEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var resources = SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { id1, id2 }).ToList();

            var rc = new ResourceCatalog();
            rc.LoadWorkspaceViaBuilder(workspacePath, "Workflows");

            //------------Execute Test---------------------------
            var workflow = ResourceCatalog.Instance.GetResourceList(workspaceID, null, "*");
            //------------Assert Results-------------------------
            Assert.AreEqual(0, workflow.Count);

        }

        #endregion


        private ExecuteMessage ConvertToMsg(string payload)
        {
            return JsonConvert.DeserializeObject<ExecuteMessage>(payload);
        }
    }
}
