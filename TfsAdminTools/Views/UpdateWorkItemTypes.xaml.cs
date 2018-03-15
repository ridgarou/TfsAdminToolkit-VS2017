namespace mskold.TfsAdminToolKit.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using mskold.TfsAdminToolKit.ViewModels;
    using Sogeti.VSExtention;

    /// <summary>
    /// Interaction logic for UpdateWorkItemTypes.xaml
    /// </summary>
    public partial class UpdateWorkItemTypes : Window
    {
        private UpdateWorkItemTypesViewModel vm;

        public UpdateWorkItemTypes()
        {
            InitializeComponent();
        }

        public UpdateWorkItemTypes(TeamExplorerIntergator te)
        {
            InitializeComponent();

            vm = new UpdateWorkItemTypesViewModel();

            vm.Load(te);
            this.DataContext = vm;
        }

        private void cmdOk_Click(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor(this))
            {
                vm.cmdUpdate();
            }
            
            MessageBox.Show("Update done");
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }
}
