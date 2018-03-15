namespace mskold.TfsAdminToolKit.ViewModels
{ 
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Windows;
    using Sogeti.SourceControlWrapper;
    using Sogeti.VSExtention;

    public class FindInFilesViewModel : INotifyPropertyChanged
    { 
        public TeamExplorerIntergator teamExplorer;

        public List<string> RecentFileNames { get; set; }
        public List<string> RecentTextSearches { get; set; }

        private string _fileNameFilter;
        private ObservableCollection<SCFolder> lstRootFolders;
        private SCFolder _SelectedTeamProject;
        private ObservableCollection<SCFile> _files;
        private fileSizeUnits _sizeUnit;

        

        public FindInFilesViewModel()
        {
            RecentFileNames = new List<string>();


            RecentTextSearches = new List<string>();
            FoundFiles = new ObservableCollection<SCFile>();
            SearchHistory = true;
            Progress = new ProgressIndication();
        }

        #region Public Properties
        public ProgressIndication Progress { get; set; }

        public string FileNameFilter
        { 
            get
            { 
                return _fileNameFilter;
            }

            set
            { 
                _fileNameFilter = value;
                NotifyPropertyChanged("FileNameFilter");
            }
        }

        public bool SearchHistory { get; set; }

        public string SearchText { get; set; }

        public bool isCaseSensitive { get; set; }

        public bool useRegExp { get; set; }

        public fileSizeUnits FileSizeUnit
        { 
            get { return _sizeUnit; }
            set
            { 
                fileSizeUnits u = _sizeUnit;
                if (value != _sizeUnit)
                { 
                    _sizeUnit = value;
                }

                if (u != value)
                { 
                    NotifyPropertyChanged("FileSizeUnit");
                }
            }
        }

        public SCFolder SelectedTeamProject
        { 
            get
            { 
                return _SelectedTeamProject;
            }

            set
            { 
                _SelectedTeamProject = value;
                NotifyPropertyChanged("SelectedTeamProject");
            }
        }

        public SCFolder SelectedFolder
        {
            get;
            set;
        }

        public SCFolder CollectionRootFolder { get; set; }

        public ObservableCollection<SCFolder> RootFolders
        { 
            get
            { 
                return lstRootFolders;
            }

            set
            {
                lstRootFolders = value;
                NotifyPropertyChanged("RootFolders");
            }
        }

        public ObservableCollection<SCFile> FoundFiles
        { 
            get
            { 
                return _files;
            }

            set
            { 
                _files = value;
                NotifyPropertyChanged("FoundFiles");
            }
        }
        #endregion
        public void Load(TeamExplorerIntergator te)
        { 
            teamExplorer = te;
            FileNameFilter = "*.*";

            RecentFileNames =  Convert2List(Properties.Settings.Default.RecentFileNames);


            SearchText = string.Empty;
            FileSizeUnit = fileSizeUnits.MB;
            RootFolders = new ObservableCollection<SCFolder>();
            if(te.SelectedSourceControlFolder!=null)
            {
                foreach (var f in te.SelectedSourceControlFolder)
                {
                    RootFolders.Add(new SCFolder {FolderPath = f});
                }                
            }
            
            CollectionRootFolder = LoadRootFolder();
        }

        private List<string> Convert2List(System.Collections.Specialized.StringCollection stringCollection)
        {
                List<string> lst= new List<string>();

                foreach(string s in stringCollection )
                {
                    lst.Add(s);
                }
                return lst;
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

        public void DoSearch()
        {
            UpdateRecentValues();

            Progress.Max = 10000;
            Progress.JobProgress = 0;
            Progress.JobStatus = JobStatus.inProgress;

            SCWrapper scWrp = new SCWrapper(this.teamExplorer);
            Wildcard fileNameWildCard = new Wildcard(this.FileNameFilter);
            this.FoundFiles = new ObservableCollection<SCFile>();

            CancelCallbackNotify notify = new CancelCallbackNotify();
            notify._delegate = DispatchUpdateStatus;
            notify.start = 0;
            notify.range = Progress.Max;

            SearchCondition s = new SearchCondition(this.SearchText, this.isCaseSensitive, this.useRegExp);

            List<SCFolder> foldersToSearch= new List<SCFolder>();
            foreach (SCFolder fld in RootFolders)
            {
                foldersToSearch.Add(fld);
            }

            scWrp.SearchForFileContent( foldersToSearch, fileNameWildCard, s, this.SearchHistory, notify);

            Progress.JobStatus = JobStatus.notStarted;
            
        }

        public void UpdateRecentValues()
        {
            RecentFileNames.Add(FileNameFilter);

            Properties.Settings.Default.RecentFileNames.Clear();
            Properties.Settings.Default.RecentFileNames.AddRange(RecentFileNames.ToArray());

            Properties.Settings.Default.Save();
            RecentFileNames = Convert2List(Properties.Settings.Default.RecentFileNames);

        }

        public bool DispatchUpdateStatus(double value, string currentOperation, ref List<SCFile> lst)
        {
            try
            {
                CallbackCancelDelegate callback = new CallbackCancelDelegate(UpdateStatus);

                return (bool)Application.Current.Dispatcher.Invoke(callback, System.Windows.Threading.DispatcherPriority.Background, new object[] { value, currentOperation, lst });

            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public bool UpdateStatus(double value, string currentOperation, ref List<SCFile> lst )
        {
            try
            {
                Progress.JobProgress = value;
                
                if (lst != null)
                {

                    foreach (SCFile f in lst)
                    {
                        FoundFiles.Add(f);
                    }
                    lst.Clear();
                }
                
                NotifyPropertyChanged("FoundFiles");
                
                if (Progress.Cancel)
                {
                    Progress.JobStatus = JobStatus.done;
                    Progress.CurrentOperation = "Canceling... ";
                }
                else
                {
                    Progress.CurrentOperation = "Searching: " + currentOperation;
                }

                if (Progress.JobProgress >= Progress.Max)
                {
                    Progress.JobStatus = JobStatus.notStarted;
                }

            }
            catch (Exception ex)
            {
            //TOdo Erro handling
            }

            return Progress.Cancel;


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

        internal void RemoveRootFolder(SCFolder folder)
        {
            this.RootFolders.Remove(folder);
            NotifyPropertyChanged("RootFolders");
        }

        internal void AddRootFolder(SCFolder folder)
        {
            this.RootFolders.Add(folder);
            NotifyPropertyChanged("RootFolders");
        }

        internal void ShowFile(SCFile file)
        {
            //Download to temp
            string tempFileName = string.Format(@"{0}{1}_{2}.{3}", Path.GetTempPath(), file.FileNameWithoutExt, file.ChangesetId, file.FileExt);
            SCWrapper scWrp = new SCWrapper(this.teamExplorer);
            scWrp.DownLoadFile(tempFileName, file.ItemId, file.ChangesetId);

            // Open Document 
            int lineNo= Convert.ToInt32(file.Comment);
            VSIntegration.OpenDocumentAndNavigateTo(tempFileName, lineNo, 0);

        }
    }
}
