namespace mskold.TfsAdminToolKit.ViewModels
{ 
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Sogeti.SourceControlWrapper;
    using Sogeti.VSExtention;

    internal class FileSearchViewModel : INotifyPropertyChanged
    { 
        public TeamExplorerIntergator teamExplorer;
        private string _fileNameFilter;
        private SCFolder _rootFld;
        private List<SCFile> _files;
        private fileSizeUnits _sizeUnit;

        public FileSearchViewModel()
        { 
            FoundFiles = new List<SCFile>();
        }
        
        #region Public properties
        public string FileNameFilter
        { 
            get { return _fileNameFilter; }

            set
            { 
                _fileNameFilter = value;
                NotifyPropertyChanged("FileNameFilter");
            }
        }

        public long FileSize { get; set; }

        public fileSizeUnits FileSizeUnit
        { 
            get
            { 
                return _sizeUnit;
            }

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

        public SCFolder RootFolder
        { 
            get { return _rootFld; }

            set
            { 
                _rootFld = value;
                NotifyPropertyChanged("RootFolders");
            }
        }

        public List<SCFile> FoundFiles
        { 
            get { return _files; }

            set
            { 
                _files = value;
                NotifyPropertyChanged("FoundFiles");
            }
        }
#endregion

        public long MinFileSizeInBytes()
        {
            return SCFileSize.GetSizeInBytes(FileSize, FileSizeUnit);
        }

        public void AddFoundFile(SCFile file)
        { 
            FoundFiles.Add(file);
            NotifyPropertyChanged("FoundFiles");
        }

        public void AddFoundFiles(List<SCFile> fileLst)
        { 
            FoundFiles.AddRange(fileLst);
            NotifyPropertyChanged("FoundFiles");
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

        public void Load(TeamExplorerIntergator te)
        {
            teamExplorer = te;
            FileNameFilter = "*.*";

            FileSize = 1;
            FileSizeUnit = fileSizeUnits.MB;
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
    }
}
