﻿using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.AppResources.WindowManagers;
using Dev2.Studio.Core.Diagnostics;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.StartupResources;
using Dev2.Studio.ViewModels;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Dev2
{
    public class Bootstrapper : Bootstrapper<IMainViewModel>
    {

        protected override void PrepareApplication()
        {
            base.PrepareApplication();
            PreloadReferences();
            CheckPath();
            FileHelper.MigrateTempData(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            var assemblies = base.SelectAssemblies().ToList();
            assemblies.AddRange(new[]
                {
                    Assembly.GetAssembly(typeof (Bootstrapper)),
                    Assembly.GetAssembly(typeof (DebugWriter))
                });
            return assemblies.Distinct();
        }

        #region Fields

        private CompositionContainer _container;

        #endregion

        #region Overrides

        protected override void Configure()
        {
            IEnumerable<AssemblyCatalog> assemblyCatalog = AssemblySource.Instance.Select(x => new AssemblyCatalog(x));
            // ReSharper disable once RedundantEnumerableCastCall
            IEnumerable<ComposablePartCatalog> ofType = assemblyCatalog.OfType<ComposablePartCatalog>();
            _container = new CompositionContainer(new AggregateCatalog(ofType));

            var batch = new CompositionBatch();

            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IDockAwareWindowManager>(new XamDockManagerDockAwareWindowManager());
            batch.AddExportedValue(WorkspaceItemRepository.Instance);
            batch.AddExportedValue(_container);

            _container.Compose(batch);
            ImportService.Initialize(_container);

            ClassRoutedEventHandlers.RegisterEvents();
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = _container.GetExportedValues<object>(contract).ToList();

            if(exports.Any())
            {
                return exports.First();
            }

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override void BuildUp(object instance)
        {
            _container.SatisfyImportsOnce(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            //Dev2MessageBoxViewModel.Show("cake you broke something with something ntuff cake you broke something with something ntuff", "heading cake", MessageBoxButton.OK, MessageBoxImage.Error);

            // ReSharper disable once ConvertToConstant.Local
            bool start = true;
#if !DEBUG
            start = CheckWindowsService();
#endif

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if(start)
            {
                base.OnStartup(sender, e);
            }
            else
            // ReSharper disable HeuristicUnreachableCode
            {
                Application.Shutdown();
            }
            // ReSharper restore HeuristicUnreachableCode
        }

        protected override void StartRuntime()
        {
            Dev2SplashScreen.Show();
            base.StartRuntime();
        }

        #endregion Public Methods

        #region Private Methods


        private void PreloadReferences()
        {
            var currentAsm = typeof(App).Assembly;
            var inspected = new HashSet<string> { currentAsm.GetName().ToString() };
            LoadReferences(currentAsm, inspected);
        }

        private void LoadReferences(Assembly asm, HashSet<string> inspected)
        {
            var allReferences = asm.GetReferencedAssemblies();

            foreach(AssemblyName toLoad in allReferences)
            {
                if(inspected.Add(toLoad.ToString()))
                {
                    try
                    {
                        Assembly loaded = AppDomain.CurrentDomain.Load(toLoad);
                        LoadReferences(loaded, inspected);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                        // Pissing me off ;) - Some strange dependency :: 'Microsoft.Scripting.Metadata'
                    }
                }
            }
        }

        private void CheckPath()
        {
            var sysUri = new Uri(AppDomain.CurrentDomain.BaseDirectory);

            if(IsLocal(sysUri)) return;

            var popup = new PopupController
                {
                    Header = "Load Error",
                    Description = String.Format(@"The Design Studio could not be launched from a network location.
                                                    {0}Please install the application on your local machine",
                                                Environment.NewLine),
                    Buttons = MessageBoxButton.OK
                };

            popup.Show();

            Application.Current.Shutdown();
        }

        private bool IsLocal(Uri sysUri)
        {
            if(IsUnc(sysUri))
            {
                return false;
            }

            if(!IsUnc(sysUri))
            {
                var currentLocation = new DriveInfo(sysUri.AbsolutePath);
                DriveInfo[] drives = DriveInfo.GetDrives();
                IEnumerable<DriveInfo> info = drives.Where(c => c.DriveType == DriveType.Network);
                if(info.Any(c => c.RootDirectory.Name == currentLocation.RootDirectory.Name))
                {
                    return false;
                }
            }
            else
            {
                return true;
            }

            return true;
        }

        private static bool IsUnc(Uri sysUri)
        {
            return sysUri.IsUnc;
        }

        #endregion Private Methods
    }
}