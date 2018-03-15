namespace mskold.TfsAdminToolKit.Views
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using mskold.TfsAdminToolKit.ViewModels;
    using Sogeti.VSExtention;

    /// <summary>
    /// Interaction logic for Subscribtions.xaml
    /// </summary>
    public partial class Subscriptions : Window
    {
        private SubscriptionsViewModel vm;

        public Subscriptions()
        {
            InitializeComponent();
        }

        public Subscriptions(TeamExplorerIntergator te)
        {
            InitializeComponent();

            vm = new SubscriptionsViewModel();
            vm.Load(te);
            this.DataContext = vm;
        }

        private void cmdRefresh_Click(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor(this))
            {
                vm.Refresh();
            }
        }

        private void cmdUnsubscribe_Click(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor(this))
            {
                vm.SelectedSubscriptions = new List<TfsSubscriptionsItem>();
     
                vm.SelectedSubscriptions.AddRange(dataGrid1.SelectedItems.OfType<TfsSubscriptionsItem>());
                vm.Unsubscribe();
            }
        }
    }
}
