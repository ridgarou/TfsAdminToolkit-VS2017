namespace mskold.TfsAdminToolKit.Views
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using mskold.TfsAdminToolKit.ViewModels;
    using Sogeti.SourceControlWrapper;
    using Sogeti.VSExtention;

    /// <summary>
    /// Interaction logic for FileSearch.xaml
    /// </summary>
    public partial class FileSearch : Window
    {
        private FileSearchViewModel vm;

        public FileSearch()
        {
            InitializeComponent();
        }

        public FileSearch(TeamExplorerIntergator te)
        {
            InitializeComponent();

            progressBar.Visibility = Visibility.Collapsed;
            lblProgress.Visibility = Visibility.Collapsed;
            
            vm = new FileSearchViewModel();
            vm.Load(te);
            this.DataContext = vm;
        }

        public void DoSearch()
        {
            CallbackDelegate callback = new CallbackDelegate(UpdateStatus);

            SCWrapper scWrp = new SCWrapper(vm.teamExplorer);
            Wildcard fileNameWildCard = new Wildcard(vm.FileNameFilter);

            for (int i = 0; i < vm.RootFolder.Folders[0].Folders.Count; i++)
            {
                SCFolder fld = vm.RootFolder.Folders[0].Folders[i];
                Dispatcher.Invoke(callback, System.Windows.Threading.DispatcherPriority.Background, new object[] { i * 100, fld.FolderPath });

                vm.AddFoundFiles(scWrp.SearchForFile(fld, fileNameWildCard, vm.MinFileSizeInBytes()));
            }

            Dispatcher.Invoke(callback, System.Windows.Threading.DispatcherPriority.Background, new object[] {100 * vm.RootFolder.Folders[0].Folders.Count, "Done" });
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
            progressBar.Maximum = 100 * vm.RootFolder.Folders[0].Folders.Count;

            progressBar.Visibility = Visibility.Visible;
            lblProgress.Visibility = Visibility.Visible;

            Thread workerThread = new Thread((ThreadStart)delegate { DoSearch(); });
            workerThread.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
