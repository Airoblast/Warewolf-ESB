using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SharpSetup.Base;

namespace Gui
{
    /// <summary>
    /// Interaction logic for SetupApplication.xaml
    /// </summary>
    public partial class SetupApplication
    {
        public static bool IsCancel = false;

// ReSharper disable InconsistentNaming
        private void Application_Startup(object sender, StartupEventArgs e)
// ReSharper restore InconsistentNaming
        {

            SetupHelper.Initialize(e.Args);
            SetupHelper.Install += SetupHelper_Install;
            SetupHelper.StartInstallation();

            //if (slientMode)
            //{
            //    SetupHelper.SilentInstall += new EventHandler<EventArgs>(SetupHelper_SilentInstall);
            //}
            //else
            //{

            //}
        }

// ReSharper disable InconsistentNaming
        void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
// ReSharper restore InconsistentNaming
        {
            // Begin dragging the window 
            MainWindow.DragMove();
        }

        /// <summary>
        /// Handles the SilentInstall event of the SetupHelper control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        // ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedParameter.Local
        void SetupHelper_SilentInstall(object sender, EventArgs e)
// ReSharper restore UnusedParameter.Local
// ReSharper restore InconsistentNaming
        // ReSharper restore UnusedMember.Local
        {
            // TODO : Something logical ;)
            Shutdown();
        }


        /// <summary>
        /// Handles the Install event of the SetupHelper control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
// ReSharper disable InconsistentNaming
        void SetupHelper_Install(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            MainWindow = new SetupWizard();
            MainWindow.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            MainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
            MainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            MainWindow.Icon = new BitmapImage(new Uri("pack://application:,,,/Warewolf.ico",
                                        UriKind.RelativeOrAbsolute));
            MainWindow.Show();

            // Hi-jack the exit event to start the studio ;)
            AppDomain.CurrentDomain.ProcessExit += (o, args) =>
            {

                if(!IsCancel)
                {
                    // do any install, uninstall actions
                    PerformInstallerExitActions();

                    // set the Webs folder ACL
                    //SetWebsACL();

                    // open the readme.txt
                    ViewReadMe();

                    // open release notes 
                    ViewReleaseNotes();
                }
            };
        }

        private void ViewReleaseNotes()
        {
            try
            {
                Process.Start(InstallVariables.ReleaseNotesURL);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch { }
            // ReSharper restore EmptyGeneralCatchClause
        }

        private void ViewReadMe()
        {
            if(!InstallVariables.ViewReadMe)
            {
                return;
            }

            try
            {
                var readmePath = Path.Combine(InstallVariables.InstallRoot, "Samples\\Readme.txt");
                Process.Start(readmePath);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch { }
            // ReSharper restore EmptyGeneralCatchClause
        }

        /// <summary>
        /// Performs the installer exit actions.
        /// </summary>
        private void PerformInstallerExitActions()
        {
            if(InstallVariables.StartStudioOnExit && InstallVariables.IsInstallMode)
            {
                // install with start studio
                if(!string.IsNullOrEmpty(InstallVariables.InstallRoot))
                {
                    var studioPath = InstallVariables.InstallRoot;
                    studioPath = Path.Combine(studioPath, "Studio");
                    studioPath = Path.Combine(studioPath, "Warewolf Studio.exe");

                    try
                    {
                        ProcessHost.Invoke(string.Empty, studioPath, string.Empty, false);
                    }
                    catch(Exception e1)
                    {
                        MessageBox.Show("An error occurred while starting the studio." + Environment.NewLine + e1.Message);
                    }
                }
            }
            else if(!InstallVariables.IsInstallMode && InstallVariables.RemoveAllItems)
            {
                // uninstall with full removal selected ;)
                try
                {
                    if(!string.IsNullOrEmpty(InstallVariables.InstallRoot))
                    {
                        Directory.Delete(InstallVariables.InstallRoot, true);
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while removing Warewolf" + Environment.NewLine + "Cannot locate install directory!");
                    }

                    // remove warewolf directory
                    RemoveWarewolfDirectory();
                }
                catch(Exception e2)
                {
                    if(e2.Message.IndexOf("install.cmd", StringComparison.Ordinal) >= 0)
                    {
                        return;
                    }
                    MessageBox.Show("An error occurred while removing Warewolf." + Environment.NewLine + e2.Message);
                }
            }
        }



        private void RemoveWarewolfDirectory()
        {
            try
            {
                // give the darn files time to remove handles
                Thread.Sleep(1500);

                var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var logDir = Path.Combine(appData, "Warewolf");

                Directory.Delete(logDir, true);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch(Exception) { }
            // ReSharper restore EmptyGeneralCatchClause

        }


        /// <summary>
        /// Sets the webs acl.
        /// </summary>
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
        private void SetWebsACL()
// ReSharper restore InconsistentNaming
// ReSharper restore UnusedMember.Local
        {
            // Set Webs ACL
            if(InstallVariables.IsInstallMode && !string.IsNullOrEmpty(InstallVariables.InstallRoot))
            {
                // build the webs location
                var websPath = Path.Combine(InstallVariables.InstallRoot, "Server");
                websPath = Path.Combine(websPath, "Webs");

                if(Directory.Exists(websPath))
                {
                    try
                    {
                        var acl = File.GetAccessControl(websPath);

                        // give installer full access ;)
                        acl.AddAccessRule(new FileSystemAccessRule("TrustedInstaller", FileSystemRights.FullControl, AccessControlType.Allow));

                        // deny everyone ;)
                        acl.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.CreateFiles, AccessControlType.Deny));
                        acl.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.CreateDirectories, AccessControlType.Deny));

                        // allow local service account
                        acl.AddAccessRule(new FileSystemAccessRule(@"LocalSystem", FileSystemRights.FullControl, AccessControlType.Allow));

                        // set the ACL
                        File.SetAccessControl(websPath, acl);
                    }
                    catch(Exception e)
                    {
                        MessageBox.Show("An error occurred while exiting the installer. " + Environment.NewLine + e.Message);
                    }
                }
            }
        }


    }
}
