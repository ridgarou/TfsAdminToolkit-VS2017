namespace mskold.TfsAdminToolKit.Views
{
    using System.ComponentModel;
    using System.Diagnostics;

    using System.Windows;
    using System.Windows.Controls;
    using mskold.TfsAdminToolKit.ViewModels;
    using Sogeti.VSExtention;

    /// <summary>
    /// Interaction logic for UpdateReports.xaml
    /// </summary>
    public partial class UpdateReports : Window
    {
        private BackgroundWorker bgWorker;
        private UpdateReportsViewModel vm;

        public UpdateReports()
        {
            InitializeComponent();
        }

        public UpdateReports(TeamExplorerIntergator te)
        {
            InitializeComponent();

             vm = new UpdateReportsViewModel();

            vm.Load(te);
            this.DataContext = vm;
        }

        private void cmdOk_Click(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor(this))
            {
                vm.cmdDoUpdate();
            }
            MessageBox.Show("Update done");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ShowLogFile(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            ProcessStartInfo psi = new ProcessStartInfo(b.Tag.ToString());
            psi.UseShellExecute = true;
            Process.Start(psi);
        }
    }
}
