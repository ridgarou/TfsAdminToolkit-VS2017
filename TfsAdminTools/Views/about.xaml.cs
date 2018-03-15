namespace mskold.TfsAdminToolKit.Views
{ 
    using System.Windows;

    /// <summary>
    /// Interaction logic for about.xaml
    /// </summary>
    public partial class about : Window
    { 
        public about()
        { 
            InitializeComponent();

            lblVersion.Content = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); 
        }

        private void cmdClose_Click(object sender, RoutedEventArgs e)
        { 
            Close();
        }
    }
}
