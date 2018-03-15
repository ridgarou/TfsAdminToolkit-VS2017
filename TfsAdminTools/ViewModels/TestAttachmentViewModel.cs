namespace mskold.TfsAdminToolKit.ViewModels
{ 
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using Sogeti.VSExtention;

    public class TestAttachmentViewModel : INotifyPropertyChanged
    { 
        public TeamExplorerIntergator teamExplorer;
        private string _testAttatchmentCleanerPath;
        private List<TeamProjectAttachemntInfo> _rootFld;

        private TeamProjectAttachemntInfo _selectedProject;

        private TeamProjectAttachemntInfo _totalAttachments;

        public TestAttachmentViewModel()
        { 
            string expectedTestAttacmenCleanerPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Microsoft\Test Attachment Cleaner\tcmpt.exe";

            if (File.Exists(expectedTestAttacmenCleanerPath))
            {
                TestAttatchmentCleanerPath = expectedTestAttacmenCleanerPath;

            }
            else
            {
                expectedTestAttacmenCleanerPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Microsoft Team Foundation Server 2012 Power Tools\Test Attachment Cleaner\tcmpt.exe";

                if (File.Exists(expectedTestAttacmenCleanerPath))
                {
                    TestAttatchmentCleanerPath = expectedTestAttacmenCleanerPath;

                }
            }
            OverrideDateStart = 1;
            OverrideDateStart=999;
            OverrideDateRuns=1;

            SelectedMode = "preview";
       }

        #region Public Properties 
        public ProgressIndication Progress { get; set; }

        public bool TotalExist
        {
            get { return (TotalAttachments != null); }
        }
        public TeamProjectAttachemntInfo TotalAttachments
        {
            get
            {
                return _totalAttachments;
            }

            set
            {
                _totalAttachments = value;
                NotifyPropertyChanged("TotalAttachments");
                NotifyPropertyChanged("TotalExist");
            }
        }

        public List<TeamProjectAttachemntInfo> RootFolder
        { 
            get
            { 
                return _rootFld;
            }

            set
            { 
                _rootFld = value;
                NotifyPropertyChanged("RootFolder");
            }
        }

         public TeamProjectAttachemntInfo SelectedProject
        { 
            get
            { 
                return _selectedProject;
            }

            set
            { 
                _selectedProject = value;

                List<SelectableFileExt> lst = new List<SelectableFileExt>();

                foreach (KeyValuePair<string, long> p in _selectedProject.FileExt)
                {
                    lst.Add(new SelectableFileExt { Key = p.Key, isSelected= true });
                }

                OverrideFileExt = lst;

                NotifyPropertyChanged("SelectedProject");
                NotifyPropertyChanged("OverrideFileExt");
            }
        }

        public bool TestAttachmentPowerToolInstalled
        { 
            get { return TestAttatchmentCleanerPath != null; }
        }

        public bool NoTestAttachmentPowerToolInstalled
        { 
            get { return TestAttatchmentCleanerPath == null; }
        }

        public bool OverrideDate { get; set; }
        public int OverrideDateStart { get; set; }
        public int OverrideDateEnd { get; set; }
        public int OverrideDateRuns { get; set; }

        public  List<SelectableFileExt> OverrideFileExt { get; set; }
        
        public bool OverrideExtensions { get; set; }

        public List<string> TestCleanerConfigs
        { 
            get
            { 
                List<string> lst = new List<string>();
                if (TestAttachmentPowerToolInstalled)
                { 
                    string path = TestAttatchmentCleanerPath.Replace(TestAttatchmentCleanerPath.Split('\\').LastOrDefault(), string.Empty);

                    foreach (string s in Directory.EnumerateFiles(path, "*.xml", SearchOption.AllDirectories))
                    { 
                        lst.Add(s.Replace(path, string.Empty));
                    }
                }

                return lst;
            }
        }

        public string SelectedConfig { get; set; }

        public List<string> TestCleanerModes
        { 
            get
            { 
                List<string> lst = new List<string>();
             
                        lst.Add("preview");
                        lst.Add("delete");
                return lst;
            }
        }

        public string SelectedMode { get; set; }

        public string TestAttatchmentCleanerPath
        { 
            get
            { 
             return _testAttatchmentCleanerPath;
            }

            set
            { 
                _testAttatchmentCleanerPath = value;
            }
        }

        #endregion

        #region Public methods
        public void Load(TeamExplorerIntergator te)
        {
            teamExplorer = te;
            Progress= new ProgressIndication();
            RootFolder = LoadProjects();

        }

        public List<TeamProjectAttachemntInfo> LoadProjects()
        {
            List<TeamProjectAttachemntInfo> lst = new List<TeamProjectAttachemntInfo>();

            if (teamExplorer != null)
            {
                TestWrapper tstWrp = new TestWrapper(teamExplorer);

                foreach (string s in tstWrp.GeTeamProjects())
                {
                    lst.Add(new TeamProjectAttachemntInfo { TeamProjectName = s, IsSelected = true });
                }
            }
            else
            {
                lst.Add(new TeamProjectAttachemntInfo { TeamProjectName = "Design time ?" });
            }

            return lst;
        }

        public void DoCalcTree()
        {
            TotalAttachments=null;

            CallbackDelegate callback = new CallbackDelegate(UpdateStatus);

            Progress.Max = 100 * RootFolder.Count(x => x.IsSelected == true);
            Progress.JobStatus=JobStatus.inProgress;

            TestWrapper tstWrp = new TestWrapper(teamExplorer);

            for (int i = 0; i < RootFolder.Count; i++)
            {
                TeamProjectAttachemntInfo fld = RootFolder[i];
                if (fld.IsSelected)
                {
                    fld.ClearSizes();

                    UpdateStatus(i * 100, fld.TeamProjectName);

                    tstWrp.CalcSize(ref fld, "{0:yy-MM}");
                    fld.IsSelected = fld.Size > 0 ? true : false;
                }
            }


            Progress.CurrentOperation = "Calculating Totals";

            TeamProjectAttachemntInfo tpaTotal = new TeamProjectAttachemntInfo();
            tpaTotal.CalcTotal(RootFolder);
            TotalAttachments = tpaTotal;
            
            Progress.JobStatus = JobStatus.notStarted;
        }

        public void UpdateStatus(double value, string currentOperation)
        {
            Progress.JobProgress = value;
            Progress.CurrentOperation = "Calculating: " + currentOperation;
        }


        #endregion

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

        internal void InvertRootFolderSelection()
        { 
            List<TeamProjectAttachemntInfo> lst = RootFolder;
            foreach (TeamProjectAttachemntInfo i in lst)
            { 
                i.IsSelected = !i.IsSelected;
            }

            RootFolder = lst;
        }
    }
    public class SelectableFileExt
    {
        public string Key {get;set;}
        public bool isSelected { get; set; }
    }


}
