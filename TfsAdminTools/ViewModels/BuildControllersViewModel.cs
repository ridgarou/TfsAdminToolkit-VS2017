namespace mskold.TfsAdminToolKit.ViewModels
{ 
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Sogeti.BuildWrapper;
    using Sogeti.VSExtention;

    public class BuildControllersViewModel : INotifyPropertyChanged
    { 
        public TeamExplorerIntergator teamExplorer;
        private List<BuildController> _lstBuildControllers;
        private List<BuildAgent> _lstBuildAgents;

        public BuildControllersViewModel()
        { 
            BuildControllers = new List<BuildController>();
        }

        #region Public properties
        public List<BuildController> LoadControllers()
        { 
                BuildWrapper buidlWrp = new BuildWrapper(teamExplorer);

                return buidlWrp.GetBuildControlles();
        }

        public List<BuildController> BuildControllers
        { 
            get
            { 
                if (_lstBuildControllers == null)
                { 
                }

                return _lstBuildControllers;
            }

            set
            { 
                _lstBuildControllers = value;
                NotifyPropertyChanged("BuildControllers");
            }
        }

        public List<BuildAgent> BuildAgents
        { 
            get
            { 
                return _lstBuildAgents;
            }

            set
            { 
                _lstBuildAgents = value;
                NotifyPropertyChanged("BuildAgents");
            }
        }
        #endregion

        public void Load(TeamExplorerIntergator te)
        {
            teamExplorer = te;

            BuildControllers = LoadControllers();
            BuildAgents = LoadAgents();
        }

        public List<BuildAgent> LoadAgents()
        {
            BuildWrapper buidlWrp = new BuildWrapper(teamExplorer);

            return buidlWrp.GetBuildAgents();
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

        internal void cmdTestConnection()
        {
            BuildWrapper wrp = new BuildWrapper(teamExplorer);

            foreach (BuildController b in BuildControllers)
            {
                wrp.TestConnection(b);
            }
        }

        internal void cmdRestart()
        {
            BuildWrapper wrp = new BuildWrapper(teamExplorer);

            foreach (BuildController b in BuildControllers)
            {
                wrp.EnableController(b, false);
                wrp.EnableController(b, true);
            }
        }
    }
}
