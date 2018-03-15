namespace mskold.TfsAdminToolKit.ViewModels
{ 
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using Sogeti.VSExtention;

    public class FailedLoginsViewModel : INotifyPropertyChanged
    { 
        public TeamExplorerIntergator teamExplorer;
        private string _userName;
        private List<string> _foundEntries;

        public FailedLoginsViewModel()
        { 
        }

        #region Public properties

        public string UserName
        { 
            get 
            { 
                return _userName;
            }

            set 
            { 
                _userName = value;
                NotifyPropertyChanged("UserName");
            }
        }

        public List<string> FoundEntries
        {
            get
            {
                return _foundEntries;
            }

            set
            {
                _foundEntries = value;
                NotifyPropertyChanged("FoundFiles");
            }
        }
        #endregion

        public void Load(TeamExplorerIntergator te)
        {
            teamExplorer = te;
        }

        public void SearchFailedLogin(string username, CallbackDelegate callback)
        { 
            string machineName = teamExplorer.tpCollection.Uri.DnsSafeHost;
            if (machineName == "localhost")
            { 
                machineName = Environment.MachineName;
            }

            EventLog secLog = new EventLog("Security", machineName);

            int i = 0;
            int x = 0;
            double progress = 1000 / secLog.Entries.Count;
            foreach (EventLogEntry e in secLog.Entries)
            { 
                if (x > secLog.Entries.Count / 1000) 
                { 
                    callback(i * progress, "Searching ");
                    x = 0;
                }

                if (e.Message.Contains(username))
                { 
                    FoundEntries.Add(e.Message);
                }

                i++;
                x++;
            }
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
    }
}
