namespace mskold.TfsAdminToolKit
{ 
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Input;
    using Microsoft.TeamFoundation.Common;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Sogeti.VSExtention;

 internal class WaitCursor : IDisposable
{ 
    private Window _control = null;
    private Cursor oldCursor = null;

    public WaitCursor(Window parent)
    { 
        _control = parent;
        oldCursor = parent.Cursor;
        _control.Cursor = Cursors.Wait;
    }

    public void Dispose()
    { 
        _control.Cursor = oldCursor;
    }
}
    
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    /// 
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideToolWindow(typeof(ToolWindow), Transient = true, Style = VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Bottom)] // This attribute registers a tool window exposed by this package.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidTfsAdminToolsPkgstring)]
    public sealed class TfsAdminToolsPackage : Package
    { 
        private TeamExplorerIntergator _teamExplorerIntegrator;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public TfsAdminToolsPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        public TeamExplorerIntergator TeamExplorerIntegrator
        { 
            get
            {
                if (_teamExplorerIntegrator == null)
                {
                    _teamExplorerIntegrator = new TeamExplorerIntergator(
                        this.GetService(typeof(EnvDTE.IVsExtensibility)) as EnvDTE.IVsExtensibility,
                        (IVsTeamExplorer)GetService(typeof(IVsTeamExplorer)));
                }

                return _teamExplorerIntegrator;
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        { 
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            { 
                // Create the command for the menu item.
                CommandID menuCommandNewIterationID = new CommandID(GuidList.guidTfsAdminToolsCmdSet, (int)PkgCmdIDList.cmdidUpdateReports);
                mcs.AddCommand(new MenuCommand(MenuItemCallback, menuCommandNewIterationID));

                CommandID menuCommandChangeIterationID = new CommandID(GuidList.guidTfsAdminToolsCmdSet, (int)PkgCmdIDList.cmdidUpdateWorkItemTypes);
                mcs.AddCommand(new MenuCommand(MenuItemCallback, menuCommandChangeIterationID));

                CommandID menuCommandSettingsID = new CommandID(GuidList.guidTfsAdminToolsCmdSet, (int)PkgCmdIDList.cmdidVCFolderSizes);
                mcs.AddCommand(new MenuCommand(MenuItemCallback, menuCommandSettingsID));

                CommandID menuCommandSearchLargeFiles = new CommandID(GuidList.guidTfsAdminToolsCmdSet, (int)PkgCmdIDList.cmdidSCSearchLargeFiles);
                mcs.AddCommand(new MenuCommand(MenuItemCallback, menuCommandSearchLargeFiles));

                CommandID menuFindInFiles = new CommandID(GuidList.guidTfsAdminToolsCmdSet, (int)PkgCmdIDList.cmdidFindInFiles);
                mcs.AddCommand(new MenuCommand(MenuItemCallback, menuFindInFiles));
                CommandID menuFindInFilesSCEContext = new CommandID(GuidList.guidTfsAdminToolsCmdSet, (int)PkgCmdIDList.cmdidFindInFilesSCEContext);
                mcs.AddCommand(new MenuCommand(MenuItemCallback, menuFindInFilesSCEContext));

                CommandID menuCommandTestAttachements = new CommandID(GuidList.guidTfsAdminToolsCmdSet, (int)PkgCmdIDList.cmdidTestAttachments);
                mcs.AddCommand(new MenuCommand(MenuItemCallback, menuCommandTestAttachements));

                CommandID menuCommandBuildControllers = new CommandID(GuidList.guidTfsAdminToolsCmdSet, (int)PkgCmdIDList.cmdidBuildControllers);
                mcs.AddCommand(new MenuCommand(MenuItemCallback, menuCommandBuildControllers));

                CommandID menuCommandFailedLogins = new CommandID(GuidList.guidTfsAdminToolsCmdSet, (int)PkgCmdIDList.cmdidFailedLogins);
                mcs.AddCommand(new MenuCommand(MenuItemCallback, menuCommandFailedLogins));

                CommandID menuCommandcmdidSubscriptions = new CommandID(GuidList.guidTfsAdminToolsCmdSet, (int)PkgCmdIDList.cmdidSubscriptions);
                mcs.AddCommand(new MenuCommand(MenuItemCallback, menuCommandcmdidSubscriptions));

                CommandID menuCommandAbout = new CommandID(GuidList.guidTfsAdminToolsCmdSet, (int)PkgCmdIDList.cmdidAbout);
                mcs.AddCommand(new MenuCommand(MenuItemCallback, menuCommandAbout));
            }
        }
        #endregion

          /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object userCtrl, string title)
        { 
            try
            { 
                // Get the instance number 0 of this tool window. This window is single instance so this instance is actually the only one.
                // The last flag is set to true so that if the tool window does not exists it will be created.
                ToolWindowPane window = this.FindToolWindow(typeof(ToolWindow), 0, true);
                if ((null == window) || (null == window.Frame))
                { 
                    throw new NotSupportedException();
                }

                var wnd = window as ToolWindow;
                wnd.Content = userCtrl;
                wnd.Caption = title;
                IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
            catch (Exception ex)
            { 
                Trace.WriteLine("***** MATTIAS **** " + ex.Message);

                IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));

                Guid clsid = Guid.Empty;
                int result;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                           0,
                           ref clsid,
                           "IterationManager",
                           string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", ex.Message),
                           string.Empty,
                           0,
                           OLEMSGBUTTON.OLEMSGBUTTON_OK,
                           OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                           OLEMSGICON.OLEMSGICON_INFO,
                           0,        // false
                           out result));
            }
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
       private void MenuItemCallback(object sender, EventArgs e)
        { 
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));

            try
            { 
                uiShell.SetWaitCursor();
                switch ((int)((MenuCommand)sender).CommandID.ID)
                { 
                    case PkgCmdIDList.cmdidUpdateReports:
                        Views.UpdateReports dlg = new Views.UpdateReports(TeamExplorerIntegrator);
                        dlg.ShowDialog();
                        break;
                    case PkgCmdIDList.cmdidVCFolderSizes:
                        Views.FolderSizes dlg1 = new Views.FolderSizes(TeamExplorerIntegrator);
                        dlg1.ShowDialog();
                        break;
                    case PkgCmdIDList.cmdidAbout:
                        Views.about dlg2 = new Views.about();
                        dlg2.ShowDialog();
                        break;
                    case PkgCmdIDList.cmdidUpdateWorkItemTypes:
                        Views.UpdateWorkItemTypes dlg3 = new Views.UpdateWorkItemTypes(TeamExplorerIntegrator);
                        dlg3.ShowDialog();
                        break;        
                    case PkgCmdIDList.cmdidSCSearchLargeFiles:
                        Views.FileSearch dlg4 = new Views.FileSearch(TeamExplorerIntegrator);
                        dlg4.ShowDialog();
                        break;
                    case PkgCmdIDList.cmdidFindInFiles: //Deliberate fallthrough
                    case PkgCmdIDList.cmdidFindInFilesSCEContext:
                        Views.FindInFiles dlg10 = new Views.FindInFiles(TeamExplorerIntegrator);
                        dlg10.ShowDialog();
                        break;

                    case PkgCmdIDList.cmdidBuildControllers:
                        Views.BuildControllers dlg5 = new Views.BuildControllers(TeamExplorerIntegrator);
                        dlg5.ShowDialog();
                        break;
                    case PkgCmdIDList.cmdidTestAttachments:
                        Views.TestAttachmentSize dlgTA = new Views.TestAttachmentSize(TeamExplorerIntegrator);
                        dlgTA.ShowDialog();
                        break;

                    case PkgCmdIDList.cmdidFailedLogins:
                        Views.FailedLogins dlg6 = new Views.FailedLogins(TeamExplorerIntegrator);
                        dlg6.ShowDialog();
                        break;
                    case PkgCmdIDList.cmdidSubscriptions:
                        Views.Subscriptions dlg7 = new Views.Subscriptions(TeamExplorerIntegrator);
                        dlg7.ShowDialog();
                        break;
                }
            }
            catch (Exception ex)
            { 
                Trace.WriteLine("***** MATTIAS **** " + ex.Message);

                Guid clsid = Guid.Empty;
                int result;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                           0,
                           ref clsid,
                           "IterationManager",
                           string.Format(CultureInfo.CurrentCulture, "Msg:  {0}.Stacktrace: {1}", ex.Message, ex.StackTrace),
                           string.Empty,
                           0,
                           OLEMSGBUTTON.OLEMSGBUTTON_OK,
                           OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                           OLEMSGICON.OLEMSGICON_INFO,
                           0,        // false
                           out result));
            }

            if (false)
            { 
                Guid clsid = Guid.Empty;
                int result;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                           0,
                           ref clsid,
                           "IterationManager",
                           string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.ToString()),
                           string.Empty,
                           0,
                           OLEMSGBUTTON.OLEMSGBUTTON_OK,
                           OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                           OLEMSGICON.OLEMSGICON_INFO,
                           0,        // false
                           out result));
            }
        }
    }
}
