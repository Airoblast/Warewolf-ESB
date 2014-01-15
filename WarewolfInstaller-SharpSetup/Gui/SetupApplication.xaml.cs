using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SharpSetup.Base;

namespace Gui
{
    /// <summary>
    /// Interaction logic for SetupApplication.xaml
    /// </summary>
    public partial class SetupApplication
    {
        private void Application_Startup(object sender, StartupEventArgs e)
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

        void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Begin dragging the window 
            MainWindow.DragMove();
        }

        /// <summary>
        /// Handles the SilentInstall event of the SetupHelper control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void SetupHelper_SilentInstall(object sender, EventArgs e)
        {
            // TODO : Something logical ;)
            Shutdown();
        }


        /// <summary>
        /// Handles the Install event of the SetupHelper control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void SetupHelper_Install(object sender, EventArgs e)
        {
            MainWindow = new SetupWizard();
            MainWindow.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            MainWindow.WindowStyle = WindowStyle.None;
            MainWindow.AllowsTransparency = true;
            MainWindow.Background = new SolidColorBrush(Colors.Transparent);
            MainWindow.Show();

            // Hi-jack the exit event to start the studio ;)
            AppDomain.CurrentDomain.ProcessExit += (o, args) =>
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
                    }
                    catch(Exception e2)
                    {
                        MessageBox.Show("An error occurred while removing Warewolf." + Environment.NewLine + e2.Message);
                    }
                }
            };
        }



    }
}
