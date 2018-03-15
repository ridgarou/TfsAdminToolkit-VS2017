namespace mskold.TfsAdminToolKit.Views
{ 
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using mskold.TfsAdminToolKit.ViewModels;
    using Sogeti.VSExtention;

    /// <summary>
    /// Interaction logic for BuildControllers.xaml
    /// </summary>
    public partial class BuildControllers : Window
    { 
        private BuildControllersViewModel vm;

        public BuildControllers()
        { 
            InitializeComponent();
        }

        public BuildControllers(TeamExplorerIntergator te)
        { 
            InitializeComponent();

            progressBar.Visibility = Visibility.Collapsed;
            lblProgress.Visibility = Visibility.Collapsed;

            vm = new BuildControllersViewModel();
            vm.Load(te);
            this.DataContext = vm;
        }

        public void DoSearch()
        { 
        }

        public void UpdateStatus(double value, string currentOperation)
        { 
            progressBar.Value = value;
            lblProgress.Content = "Searching: " + currentOperation;
            if (progressBar.Value >= progressBar.Maximum)
            { 
                progressBar.Visibility = Visibility.Collapsed;
                lblProgress.Visibility = Visibility.Collapsed;
            }

            dataGrid1.GetBindingExpression(DataGrid.ItemsSourceProperty).UpdateTarget();
        }

        private void cmdSearch_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Visibility = Visibility.Visible;
            lblProgress.Visibility = Visibility.Visible;

            Thread workerThread = new Thread((ThreadStart)delegate { DoSearch(); });
            workerThread.Start();
        }

        private void cmdRefresh_Click(object sender, RoutedEventArgs e)
        { 
            vm.BuildControllers = vm.LoadControllers();
        }

        private void cmdTestConnection_Click(object sender, RoutedEventArgs e)
        { 
            vm.cmdTestConnection();
        }

        private void cmdRestart_Click(object sender, RoutedEventArgs e)
        { 
            vm.cmdRestart();
        }
    }
}
