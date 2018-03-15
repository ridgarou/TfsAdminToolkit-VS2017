namespace mskold.TfsAdminToolKit.Views
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using mskold.TfsAdminToolKit.ViewModels;
    using Sogeti.SourceControlWrapper;
    using Sogeti.VSExtention;

    /// <summary>
    /// Interaction logic for FolderSizes.xaml
    /// </summary>
    /// 
    public partial class FolderSizes : Window
    {
        private FolderSizesViewModel vm;

        public FolderSizes()
        {
            InitializeComponent();
        }

        public FolderSizes(TeamExplorerIntergator te)
        {
            InitializeComponent();

            vm = new FolderSizesViewModel();
            vm.Load(te);
            this.DataContext = vm;          
        }

        public void DoCalcTree()
        {
            CallbackDelegate callback = new CallbackDelegate(UpdateStatus);
            
            SCWrapper scWrp = new SCWrapper(vm.teamExplorer);
            List<SCFolder> selectedFld = vm.RootFolder.Folders[0].Folders.FindAll(delegate(SCFolder f) { return f.IsSelected; });

            for (int i = 0; i < selectedFld.Count; i++)
            {
                SCFolder fld = selectedFld[i];
                Dispatcher.Invoke(callback, System.Windows.Threading.DispatcherPriority.Background, new object[] { i * 100, fld.FolderPath });

                scWrp.CalcSize(ref fld);
                fld.SetInclusiveSize();
            }

            vm.RootFolder.Folders[0].SetInclusiveSize();
            Dispatcher.Invoke(callback, System.Windows.Threading.DispatcherPriority.Background, new object[] { 100 * selectedFld.Count, "Done" });
        }

        public void UpdateStatus(double value, string currentOperation)
        {
            progressBar.Value = value;
            lblProgress.Content = "Calculating: " + currentOperation;
            if (progressBar.Value >= progressBar.Maximum)
            {
                progressBar.Visibility = Visibility.Collapsed;
                lblProgress.Visibility = Visibility.Collapsed;

                cmdStart.Visibility = Visibility.Visible;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

        private void treeView2_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            vm.SelectedFolder = treeView2.SelectedItem as SCFolder;
        }

        private void cmdStart_Click(object sender, RoutedEventArgs e)
        {
            cmdStart.Visibility = Visibility.Hidden;
            progressBar.Visibility = Visibility.Visible;
            lblProgress.Visibility = Visibility.Visible;

            progressBar.Maximum = 100 * vm.RootFolder.Folders[0].Folders.FindAll(delegate(SCFolder f) { return f.IsSelected; }).Count;
            Thread workerThread = new Thread((ThreadStart)delegate { DoCalcTree(); });
            workerThread.Start();
        }

        private void chkAll_Click(object sender, RoutedEventArgs e)
        {
            vm.InvertProjectSelection();
        }
    }
}
