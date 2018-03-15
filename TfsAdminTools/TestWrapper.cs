namespace mskold.TfsAdminToolKit
{ 
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.TestManagement.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;
    using Sogeti.SourceControlWrapper;
    using Sogeti.VSExtention;

    public class TeamProjectAttachemntInfo : INotifyPropertyChanged
    { 
        public Dictionary<string, long> _attachmentTypes;
        public Dictionary<string, long> _fileExt;
        public Dictionary<string, long> _time;
        private bool _isSelected;
        private long _size;

        public TeamProjectAttachemntInfo()
        {
            ClearSizes();
        }

        #region Public properties
        public string TeamProjectName { get; set; }

        public bool IsSelected
        { 
            get { return _isSelected; }

            set
            { 
                _isSelected = value;
                NotifyPropertyChanged("IsSelected");
            }
        }

        public long Size
        { 
            get { return _size; }
            set
            { 
                _size = value;
                NotifyPropertyChanged("Size");
                NotifyPropertyChanged("SizeTxt");
            }
        }

        public Dictionary<string, long> AttachmentTypes
        { 
            get { return _attachmentTypes; }
            set
            { 
                _attachmentTypes = value;
                NotifyPropertyChanged("AttachmentTypes");
            }
        }

        public Dictionary<string, long> FileExt
        { 
            get { return _fileExt; }
            set
            { 
                _fileExt = value;
                NotifyPropertyChanged("FileExt");
            }
        }

        public Dictionary<string, long> Time
        {
            get { return _time; }
            set
            {
                _time = value;
                NotifyPropertyChanged("Time");
            }
        }

        public string SizeTxt
        { 
            get { return SCFileSize.SizeTxt(Size); }
        }

        #endregion

        public void ClearSizes()
        {
            Size = 0;
            AttachmentTypes = new Dictionary<string, long>();
            FileExt = new Dictionary<string, long>();
            Time = new Dictionary<string, long>();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void CalcTotal(List<TeamProjectAttachemntInfo> lst)
        { 
            foreach (TeamProjectAttachemntInfo tpa in lst)
            { 
                Size += tpa.Size;

                foreach (string key in tpa.FileExt.Keys)
                { 
                    AddValueToKey(ref _fileExt, key, tpa.FileExt[key]);
                }

                foreach (string key in tpa.AttachmentTypes.Keys)
                { 
                    AddValueToKey(ref _attachmentTypes, key, tpa.AttachmentTypes[key]);
                }

                foreach (string key in tpa.Time.Keys)
                {
                    AddValueToKey(ref _time, key, tpa.Time[key]);
                }
            }
        }

        public void AddValueToKey(ref Dictionary<string, long> d, string ext, long value)
        { 
            if (!d.Keys.Contains(ext))
            { 
                d.Add(ext, 0);
            }

            d[ext] += value;
        }

        public void NotifyPropertyChanged(string info)
        { 
            if (PropertyChanged != null)
            { 
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    internal class TestWrapper
    { 
        protected string teamProjectName;
        protected ITestManagementService testSvc;
        protected TfsTeamProjectCollection tpCollection;

        public TestWrapper(string tpCollectionUrl, string aTeamProjectName)
        { 
            tpCollection = new TfsTeamProjectCollection(new Uri(tpCollectionUrl));
            teamProjectName = aTeamProjectName;

            testSvc = (ITestManagementService)tpCollection.GetService(typeof(ITestManagementService));
        }

        public TestWrapper(TeamExplorerIntergator teamExplorer)
        { 
            tpCollection = teamExplorer.tpCollection;
            teamProjectName = teamExplorer.tpName;

            testSvc = (ITestManagementService)tpCollection.GetService(typeof(ITestManagementService));
        }

        public List<string> GeTeamProjects()
        { 
            WorkItemStore wiStore = (WorkItemStore)tpCollection.GetService(typeof(WorkItemStore));

            List<string> lst = new List<string>();

            foreach (Project p in wiStore.Projects)
            { 
                lst.Add(p.Name);
            }

            return lst;
        }

        internal long CalcSize(ref TeamProjectAttachemntInfo fld, string timeUnitFormat)
        { 
            ITestManagementTeamProject tp = testSvc.GetTeamProject(fld.TeamProjectName);
            long size = 0;
            IEnumerable<ITestRun> lst = testSvc.QueryTestRuns(string.Format("SELECT * FROM TestRun WHERE TeamProject = '{0}' ", fld.TeamProjectName));
            foreach (ITestRun tr in lst)
            { 
                long curSize=CalcSizeInRuns(tp, tr, ref fld);
                size += curSize;
                string timeUnit = String.Format(timeUnitFormat, tr.DateCreated);

                fld.AddValueToKey(ref fld._time , timeUnit, curSize);

            }

            return size;
        }

        private long CalcSizeInRuns(ITestManagementTeamProject tp, ITestRun tr, ref TeamProjectAttachemntInfo fld)
        { 
            string s = "SELECT * FROM Attachment WHERE TestRunId = " + tr.Id.ToString();
            long size = 0;
            IList<ITestAttachment> list = tp.QueryAttachments(s);
            foreach (ITestAttachment a in list)
            {
                size += a.Length;
                fld.Size += a.Length;

                fld.AddValueToKey(ref fld._attachmentTypes, a.AttachmentType, a.Length);

                string ext = a.Name.Split('.').LastOrDefault();
                fld.AddValueToKey(ref fld._fileExt, ext, a.Length);
            }

            return size;
        }
    }
}