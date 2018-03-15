namespace mskold.TfsAdminToolKit.ViewModels
{ 
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Xml.Linq;
    using Sogeti.QueryWrapper;
    using Sogeti.SourceControlWrapper;
    using Sogeti.VSExtention;
    
    public class FolderSizesViewModel : INotifyPropertyChanged
    { 
        public TeamExplorerIntergator teamExplorer;
        private List<Proj> _projects;
        private List<WIT> _wits;
        private SCFolder _rootFld;
        private SCFolder _selectedFolder;
        private string _template;
        private string _sourceProject;

        public FolderSizesViewModel()
        { 
        }

        public SCFolder SelectedFolder
        { 
            get { return _selectedFolder; }
            set
            { 
                _selectedFolder = value;
                NotifyPropertyChanged("SelectedFolder");
            }
        }

        public SCFolder RootFolder
        { 
            get
            { 
                return _rootFld;
            }

            set
            { 
                _rootFld = value;
                NotifyPropertyChanged("RootFolders");
            }
        }

        public List<WIT> WorkItemTypes
        { 
            get
            { 
                return _wits;
            }

            set
            { 
                _wits = value;
                NotifyPropertyChanged("WorkItemTypes");
            }
        }

        public List<Proj> Projects
        { 
            get { return _projects; }
            set
            {
                _projects = value; 
                NotifyPropertyChanged("Projects");
            }
        }

        public bool CreateBackup { get; set; }

        public string BackupFolder { get; set; }

        public string SourceProject
        { 
            get { return _sourceProject; }
            set
            { 
                _sourceProject = value;
                NotifyPropertyChanged("SourceProject");
                WorkItemTypes = WIT.LoadWITList(teamExplorer, SourceProject);
            }
        }

        public void Load(TeamExplorerIntergator te)
        { 
            teamExplorer = te;
            RootFolder = LoadRootFolder();
        }

        public SCFolder LoadRootFolder()
        {
            SCFolder fldDummy = new SCFolder { FolderPath = @"DummyRoot" };

            if (teamExplorer != null)
            {
                SCFolder fld = new SCFolder { FolderPath = @"$/" + teamExplorer.tpCollection.Name };
                fldDummy.Folders.Add(fld);

                SCWrapper scWrap = new SCWrapper(teamExplorer);

                fld.Folders.AddRange(scWrap.GetRootFolders());
            }
            else
            {
                fldDummy.Folders.Add(new SCFolder { FolderPath = "Hej" });
                fldDummy.Folders.Add(new SCFolder { FolderPath = "Hej då" });
            }

            return fldDummy;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string info)
        { 
            if (PropertyChanged != null)
            { 
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        internal void cmdUpdate()
        { 
            TPWiWrapper wrp = new TPWiWrapper(teamExplorer);
            
            foreach (Proj p in Projects)
            { 
                if (p.IsSelected)
                { 
                    string curProj = p.ProjectName;
                      string projectDir = BackupFolder + @"\" + curProj;
                    if (!System.IO.Directory.Exists(projectDir))
                    { 
                        System.IO.Directory.CreateDirectory(projectDir);
                    }
                    
                    if (CreateBackup)
                    { 
                        foreach (WIT wit in WorkItemTypes)
                        { 
                            if (wit.IsSelected)
                            { 
                                if (wrp.WorkIteTypeExist(curProj, wit.Name))
                                { 
                                    string s = wrp.ExportWIT(curProj, wit.Name, true);
                                    XDocument x = new XDocument(); 
                                    x = XDocument.Parse(s);
                                    x.Save(projectDir + @"\" + wit.Name + ".xml");
                                }
                            }
                        }
                    }

                    foreach (WIT wit in WorkItemTypes)
                    { 
                        if (wit.IsSelected)
                        { 
                             wrp.ImportWIT(curProj, wit.xml);
                        }
                    }
                }
            }
        }

        internal void InvertProjectSelection()
        {
            foreach (SCFolder f in RootFolder.Folders[0].Folders)
            {
                f.IsSelected = !f.IsSelected;

            }
            NotifyPropertyChanged("RootFolder");
        }
    }
}
