namespace mskold.TfsAdminToolKit.Views
{ 
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using mskold.TfsAdminToolKit.ViewModels;
    using Sogeti.VSExtention;

    /// <summary>
    /// Interaction logic for FileSearch.xaml
    /// </summary>
    public partial class FailedLogins : Window
    { 
        private FailedLoginsViewModel vm;

        public FailedLogins()
        { 
            InitializeComponent();
        }

        public FailedLogins(TeamExplorerIntergator te)
        { 
            InitializeComponent();

            progressBar.Visibility = Visibility.Collapsed;
            lblProgress.Visibility = Visibility.Collapsed;
            
            vm = new FailedLoginsViewModel();
            vm.Load(te);
            this.DataContext = vm;
        }

        public void DoSearch()
        { 
            CallbackDelegate callback = new CallbackDelegate(DispatcherStatusUpdate);
            vm.SearchFailedLogin(vm.UserName, callback);
            
            Dispatcher.Invoke(callback, System.Windows.Threading.DispatcherPriority.Background, new object[] {1000, "Done" });
        }

        public void DispatcherStatusUpdate(double value, string currentOp)
        { 
            CallbackDelegate callback = new CallbackDelegate(UpdateStatus);
            Dispatcher.Invoke(callback, System.Windows.Threading.DispatcherPriority.Background, new object[] { value, currentOp });
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        { 
        }

        private void cmdSearch_Click(object sender, RoutedEventArgs e)
        { 
            progressBar.Maximum = 1000;

            progressBar.Visibility = Visibility.Visible;
            lblProgress.Visibility = Visibility.Visible;

            Thread workerThread = new Thread((ThreadStart)delegate { DoSearch(); });
            workerThread.Start();
        }
    }
}
