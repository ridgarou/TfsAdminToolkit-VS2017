using Sogeti.SourceControlWrapper;
using Sogeti.SourceControlWrapper;

namespace mskold.TfsAdminToolKit
{ 
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Sogeti.QueryWrapper;
    using Sogeti.VSExtention;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
  
    public delegate void CallbackDelegate(double value, string currentOperation);
    
    public delegate bool CallbackCancelDelegate(double value, string currentOperation , ref List<SCFile> lst);

    public class CancelCallbackNotify
    { 
        public CallbackCancelDelegate _delegate;
        public double start;
        public double range;
        public double current;
        public bool isCanceled;

        public bool DoCallback(double value, string operation, ref List<SCFile> lst)
        { 
            if (_delegate != null)
            { 
                current = start + (value * range);
                isCanceled= _delegate(current, operation, ref lst);
                lst.Clear();
                return isCanceled;
            }

            return false;
        }
    }

    public class Proj :INotifyPropertyChanged
    { 
        public string ProjectName { get; set; }

        public bool IsSelected { get; set; }

        public bool result 
        {
            get 
            {
                return _result;
            }

            set
            {
                _result = value;
                NotifyPropertyChanged("result");
                NotifyPropertyChanged("Failed");
                NotifyPropertyChanged("Passed");
            }
        }

        public string logfile { 
            get 
            {
                return _logfile;
            }

            set
            {
                _logfile = value;
                NotifyPropertyChanged("logfile");
            }
        }

        public bool Passed { get { return logfile != null && result == true; } }
        public bool Failed { get { return logfile != null && result == false; } }

        public static ObservableCollection<Proj> LoadProjects(TeamExplorerIntergator teamExplorer)
        {
            ObservableCollection<Proj> lst = new ObservableCollection<Proj>();

            if (teamExplorer != null)
            { 
                TPWiWrapper wp = new TPWiWrapper(teamExplorer);
                foreach (string s in wp.GetProjects())
                { 
                    lst.Add(new Proj { ProjectName = s, IsSelected = false });
                }
            }

            return lst;
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

        private string _logfile;
        private bool _isSelected;
        private bool _result;
        private string _projectname;

    }

    public class WIT
    { 
        public string Name { get; set; }

        public string xml { get; set; }

        public bool IsSelected { get; set; }

        public static List<WIT> LoadWITList(TeamExplorerIntergator teamExplorer, string teamProject)
        { 
            List<WIT> lst = new List<WIT>();
            if (teamExplorer != null)
            { 
                TPWiWrapper wp = new TPWiWrapper(teamExplorer);
                foreach (string name in wp.GetWorkItemTypes(teamProject))
                { 
                    lst.Add(new WIT { Name = name, xml = wp.ExportWIT(teamProject, name, false) });
                }
            }

            return lst;
        }




    }
}
