namespace mskold.TfsAdminToolKit.ViewModels
{ 
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Xml.Linq;
    using Sogeti.QueryWrapper;
    using Sogeti.VSExtention;
    
    using System.Collections.ObjectModel;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Client.ProjectSettings;
    using Microsoft.TeamFoundation.Framework.Common;
    using Microsoft.TeamFoundation.Framework.Client;


    internal class UpdateReportsViewModel : INotifyPropertyChanged
    {
        private TeamExplorerIntergator teamExplorer;
        private ObservableCollection<Proj> _projects;
        private string _template;
        private bool _UpdateReports;
        private bool _UpdatePortals;

        public UpdateReportsViewModel()
        { 
        }


        public bool UpdateReports
        {
            get { return _UpdateReports; }
            set
            {
                _UpdateReports = value;
                NotifyPropertyChanged("UpdateReports");
            }
        }

        public bool UpdatePortals
        {
            get { return _UpdatePortals; }
            set
            {
                _UpdatePortals = value;
                NotifyPropertyChanged("UpdatePortals");
            }
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

        public ObservableCollection<Proj> Projects
        { 
            get { return _projects; }
            set
            { 
                _projects = value;
                NotifyPropertyChanged("Projects");
            }
        }

        public void Load(TeamExplorerIntergator te)
        {
            teamExplorer = te;

            Projects =  Proj.LoadProjects(teamExplorer);
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

        internal void cmdDoUpdate()
        {

            string logDir= Path.GetTempPath() + @"\File_BatchNewTeamProject_Logs";

            if (!System.IO.Directory.Exists(logDir))
            {
                System.IO.Directory.CreateDirectory(logDir);
            }
           

            foreach (Proj p in Projects)
            { 
                if (p.IsSelected)
                { 
                    string sProject = p.ProjectName;
                    p.logfile = logDir;

                    string tempConfigFile = Path.GetTempPath() + @"\" + Guid.NewGuid().ToString() + ".xml";
                    XNamespace ns = "ProjectCreationSettingsFileSchema.xsd";
                    
                    XDocument x = new XDocument(
                        new XElement(ns + "Project",
                                     new XElement(ns + "TFSName",
                                                  new XText(teamExplorer.tpCollection.Uri.ToString())), 
                                                  new XElement(ns + "LogFolder",
                                                  new XText( p.logfile)),
                                                  new XElement(ns + "ProjectName",
                                                  new XText(sProject)),
                                     new XElement(ns + "AddFeaturesToExistingProject", new XText("true")),
                                     new XElement(ns + "ProjectReportsEnabled", new XText(UpdateReports?"true":"false")),
                                     new XElement(ns + "ProjectReportsForceUpload", new XText(UpdateReports ? "true" : "false")),
                                     new XElement(ns + "ProjectSiteEnabled", new XText(UpdatePortals ? "true" : "false")),
                                     new XElement(ns + "ProcessTemplateName", new XText(Template))));

                    x.Save(tempConfigFile);
                    p.logfile= p.logfile + @"\" + p.ProjectName + ".log";

                    teamExplorer.ExecuteCommands("File.BatchNewTeamProject", tempConfigFile);

                    p.result= ScanLogFile(p.logfile);

                }
            }
            NotifyPropertyChanged("Projects");
        }

        private bool ScanLogFile(string logfile)
        {
            try
            {
                using (StreamReader sr = new StreamReader(logfile))
                {
                    while (sr.Peek() >= 0)
                    {
                        String line = sr.ReadLine();
                        if (line.Contains("Team Project Batch Creation succeeded"))
                        {
                            return true;
                        }

                    }
                    
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            
            return false;
        }

        internal void CalculateFolderTree()
        { 
            throw new NotImplementedException();
        }
    }
}
