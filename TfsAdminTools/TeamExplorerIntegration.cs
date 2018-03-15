using System.Collections.Generic;

namespace Sogeti.VSExtention
{ 
    using System;
    using System.Diagnostics;
    using EnvDTE;
    using EnvDTE80;
    using EnvDTE90;
    using EnvDTE100;
    using System.Linq;

    using Extensibility;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Common;
    using Microsoft.VisualStudio.TeamFoundation;
    using Microsoft.VisualStudio.TeamFoundation.VersionControl;

    public class TeamExplorerIntergator 
    { 
        private IVsTeamExplorer IvsTeamExpl;
        private DTE2 _applicationobject;
        private VersionControlExt srcCtrlExplorer;
        private TeamFoundationServerExt m_tfsExt;
        private TfsTeamProjectCollection m_tfs;


        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary> 
        public TeamExplorerIntergator(EnvDTE.IVsExtensibility extensibility, IVsTeamExplorer te)
        {
            IvsTeamExpl = te;

            // get IDE Globals object and DTE from that
            EnvDTE80.DTE2 dte2 = extensibility.GetGlobalsObject(null).DTE as EnvDTE80.DTE2;
            _applicationobject = dte2;

            Debug.Assert(dte2 != null, "No DTE2");

            TeamFoundationServerExt tfsExt = (TeamFoundationServerExt)dte2.Application.GetObject("Microsoft.VisualStudio.TeamFoundation.TeamFoundationServerExt");
            this.srcCtrlExplorer = (VersionControlExt)dte2.Application.GetObject("Microsoft.VisualStudio.TeamFoundation.VersionControl.VersionControlExt");

            DoConnect(tfsExt);
        } 

        public TfsTeamProjectCollection tpCollection
        { 
            get
            { 
                return m_tfs;
            }
        }

        public string tpName { get; set; }


        public string CurrentSourceControlFolder
        {
            get
            {
                VersionControlExplorerItem itm = this.srcCtrlExplorer.Explorer.SelectedItems.First(i => i.IsFolder == true);
                if (itm == null)
                {
                    return this.srcCtrlExplorer.Explorer.CurrentFolderItem.SourceServerPath;
                }
                else
                {
                    return itm.SourceServerPath;
                }
            }
        }

        public List<string> SelectedSourceControlFolder
        {
            get
            {
                List<string> lst=new List<string>();
                
                if (this.srcCtrlExplorer.Explorer.SelectedItems.Count() == 0)
                {
                    if (this.srcCtrlExplorer.Explorer.CurrentFolderItem == null)
                    {
                        lst = null;
                    }
                    else
                    {
                        lst.Add(this.srcCtrlExplorer.Explorer.CurrentFolderItem.SourceServerPath);
                    }
                    
                }
                else
                {
                    lst.AddRange(this.srcCtrlExplorer.Explorer.SelectedItems.Select( x => x.SourceServerPath).ToList());
                }
                return lst;
            }
        }


        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary> 
        /// <param term='application'>Root object of the host application.</param> 
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param> 
        /// <param term='addInInst'>object representing this Add-in.</param> 
        /// <seealso class='IDTExtensibility2' />     
        public void ExecuteCommands(string command, string parameter)
        { 
            _applicationobject.ExecuteCommand(command, parameter);
        }

        public void DoConnect(TeamFoundationServerExt tfsEx)
        { 
            try
            { 
                m_tfsExt = tfsEx;
               
                if (null != m_tfsExt)
                { 
                    m_tfsExt.ProjectContextChanged += new EventHandler(m_tfsExt_ProjectContextChanged);

                    if (null != m_tfsExt.ActiveProjectContext)
                    { 
                        // Run the event handler without the event actually having fired, so we pick up the initial state. 
                        m_tfsExt_ProjectContextChanged(null, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            { 
                Trace.WriteLine("***** MATTIAS **** " + ex.Message);
            }
        }

        public void RefreshTeamExplorer()
        { 
            //this.srcCtrlExplorer.Explorer.VsWindowFrame.
            if (IvsTeamExpl != null)
            {
                IvsTeamExpl.RefreshTeamExplorerView();
            }
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary> 
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param> 
        /// <param term='custom'>Array of parameters that are host application specific.</param> 
        /// <seealso class='IDTExtensibility2' /> 
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom) 
        { 
            // Unhook the ProjectContextChanged event handler. 
            if (null != m_tfsExt) 
            { 
                m_tfsExt.ProjectContextChanged -= new EventHandler(m_tfsExt_ProjectContextChanged); 
                m_tfsExt = null; 
            } 
        } 

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary> 
        /// <param term='custom'>Array of parameters that are host application specific.</param> 
        /// <seealso class='IDTExtensibility2' />        
        public void OnAddInsUpdate(ref Array custom) 
        { 
        } 

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary> 
        /// <param term='custom'>Array of parameters that are host application specific.</param> 
        /// <seealso class='IDTExtensibility2' /> 
        public void OnStartupComplete(ref Array custom) 
        { 
        } 

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary> 
        /// <param term='custom'>Array of parameters that are host application specific.</param> 
        /// <seealso class='IDTExtensibility2' /> 
        public void OnBeginShutdown(ref Array custom) 
        { 
        } 

        /// <summary> 
        /// Raised by the TFS Visual Studio integration package when the active project context changes. 
        /// </summary> 
        /// <param name="sender"></param> 
        /// <param name="e"></param> 
        private void m_tfsExt_ProjectContextChanged(object sender, EventArgs e) 
        { 
            try 
            { 
                if (null != m_tfsExt.ActiveProjectContext && 
                    !string.IsNullOrEmpty(m_tfsExt.ActiveProjectContext.DomainUri)) 
                { 
                    SwitchToTfs(TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(m_tfsExt.ActiveProjectContext.DomainUri)));
                    tpName = m_tfsExt.ActiveProjectContext.ProjectName;
                } 
                else 
                { 
                    SwitchToTfs(null); 
                } 
            } 
            catch (Exception ex) 
            { 
            } 
        } 

        private void SwitchToTfs(TfsTeamProjectCollection tfs) 
        { 
            if (object.ReferenceEquals(m_tfs, tfs)) 
            { 
                // No work to do; could be a team project switch only 
                return; 
            }

            m_tfs = tfs;
        }
    } 
} 
