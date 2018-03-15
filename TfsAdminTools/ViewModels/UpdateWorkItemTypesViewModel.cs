namespace mskold.TfsAdminToolKit.ViewModels
{ 
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
    using System.Xml.Linq;
    using Sogeti.QueryWrapper;
    using Sogeti.VSExtention;
    
    public class UpdateWorkItemTypesViewModel : INotifyPropertyChanged
    { 
        private TeamExplorerIntergator teamExplorer;
        private ObservableCollection<Proj> _projects;
        private List<WIT> _wits;

        private string _template;
        private string _sourceProject;

        public UpdateWorkItemTypesViewModel()
        { 
        }

        public string Template
        { 
            get { return _template; }
            set
            {
                _template = value; 
                NotifyPropertyChanged("Template");
            }
        }

        public List<string> Templates
        { 
            get
            { 
                List<string> lst = new List<string>();
                if (teamExplorer != null)
                { 
                    TPWiWrapper wp = new TPWiWrapper(teamExplorer);
                    lst = wp.GetTemplates();
                }

                return lst;
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

        public ObservableCollection<Proj> Projects
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
            CreateBackup = true;
            BackupFolder = @"C:\temp";

            Projects = Proj.LoadProjects(teamExplorer);
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
    }
}
