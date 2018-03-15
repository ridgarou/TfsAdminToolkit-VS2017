namespace mskold.TfsAdminToolKit.Views
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using mskold.TfsAdminToolKit.ViewModels;
    using Sogeti.SourceControlWrapper;
    using Sogeti.VSExtention;

    /// <summary>
    /// Interaction logic for FindInFiles.xaml
    /// </summary>
    public partial class FindInCheckIns : Window
    {
        private bool isCanceled = false;
        private FindInCheckInsViewModel vm;

        public FindInCheckIns()
        {
            InitializeComponent();
        }

        public FindInCheckIns(TeamExplorerIntergator te)
        {
            InitializeComponent();

            progressBar.Visibility = Visibility.Collapsed;
            lblProgress.Visibility = Visibility.Collapsed;
        
            vm = new FindInCheckInsViewModel();
            vm.Load(te);
            this.DataContext = vm;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void cmdSearch_Click(object sender, RoutedEventArgs e)
        {
            cmdStopSearch.Visibility = cmdSearch.Visibility;
            cmdSearch.Visibility = Visibility.Collapsed;
            isCanceled = false;

            progressBar.Maximum = 10000; 
            lblProgress.Content = "Searching...";
            progressBar.Visibility = Visibility.Visible;
            lblProgress.Visibility = Visibility.Visible;

            //Thread workerThread = new Thread((ThreadStart)delegate { vmDoSearch(); });
            //workerThread.Start();
        }

        private void cmdStopSearch_Click(object sender, RoutedEventArgs e)
        {
            cmdSearch.Visibility = cmdStopSearch.Visibility;
            cmdStopSearch.Visibility = Visibility.Collapsed;

            isCanceled = true;
        }
    }
}
