namespace mskold.TfsAdminToolKit.Views
{
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Xml;

    using mskold.TfsAdminToolKit.ViewModels;
    using Sogeti.VSExtention;

    /// <summary>
    /// Interaction logic for TestAttachmentSize.xaml
    /// </summary>
    public partial class TestAttachmentSize : Window
    {
        private TestAttachmentViewModel vm;

        public TestAttachmentSize()
        {
            InitializeComponent();
        }

        public TestAttachmentSize(TeamExplorerIntergator te)
        {
            InitializeComponent();

            vm = new TestAttachmentViewModel();
            vm.Load(te);
            this.DataContext = vm;
        }


        private void cmdDoClean_Click(object sender, RoutedEventArgs e)
        {
            CleanTeamProjets(true);
            MessageBox.Show("Done");
        }

        private void cmdCopyCommandToClipboard_Click(object sender, RoutedEventArgs e)
        {
            CleanTeamProjets(false);
            MessageBox.Show("Done");
        }

        private void CleanTeamProjets(bool execute)
        {
            string path = "";
            if (vm.TestAttatchmentCleanerPath != null)
            {
                path = vm.TestAttatchmentCleanerPath.Replace(vm.TestAttatchmentCleanerPath.Split('\\').LastOrDefault(), string.Empty);
            }


            string total = "CD " + path + "\r\n\r\n";
            foreach (TeamProjectAttachemntInfo tpa in vm.RootFolder)
            {
                if (tpa.IsSelected)
                {
                    string s = GenerateTestAttatcmentCleanerStatement(tpa, path);
                    if (execute)
                    {
                        RunCmd cmd = new RunCmd();
                        cmd.ExecuteCommandSync(s, path);
                    }
                    else
                    {
                        total += "REM " + tpa.TeamProjectName + "\r\n";
                        total += s + "\r\n\r\n";
                    }
                }
            }

            if (!execute)
            {
                Clipboard.SetText(total);
            }
        }

        private string GenerateTestAttatcmentCleanerStatement(TeamProjectAttachemntInfo tpa, string path)
        {

            string s = "";
            string config = "Config.xml";
            if(vm.SelectedConfig!=null)
            {
                config = vm.SelectedConfig;
                
                if (vm.OverrideDate)
                {
                    string configPath = config.Substring(0, config.LastIndexOf('\\'));

                    
                    int start=vm.OverrideDateStart;
                    int period=(vm.OverrideDateEnd - vm.OverrideDateStart)/ vm.OverrideDateRuns;

                    for (int i = 0; i < vm.OverrideDateRuns; i++)
                    {
                    
                        XmlDocument doc = new XmlDocument();
                        doc.Load(path + @"\" + vm.SelectedConfig);
                        OverRideAgeSettingsInConfig(start, period, doc);
                        if (vm.OverrideExtensions)
                        {
                            XmlNodeList lst = doc.SelectNodes("/DeletionCriteria/Attachment/Extensions/Include");
                            if (lst.Count > 0)
                            {
                                foreach (XmlNode n in lst)
                                {
                                    n.ParentNode.RemoveChild(n);
                                }
                            }

                            foreach (SelectableFileExt ext in vm.OverrideFileExt)
                            {
                                if (ext.isSelected)
                                {
                                    XmlElement nExt = doc.CreateElement("Include");
                                    nExt.Attributes.Append(doc.CreateAttribute("value"));
                                    nExt.Attributes["value"].Value = ext.Key;
                                    doc.SelectNodes("/DeletionCriteria/Attachment/Extensions")[0].AppendChild(nExt);
                                }
                            }
                        }

                        config = configPath + @"\" + tpa.TeamProjectName + "_" + i + ".xml";
                        doc.Save(@"c:\temp\" + config);


                        start += period+1;

                    }
                }
            
            }



            if(vm.TestAttatchmentCleanerPath !=null)
            {
                s = vm.TestAttatchmentCleanerPath.Replace(path, string.Empty);
            }
            s += @" attachmentcleanup " + "/collection:" + vm.teamExplorer.tpCollection.Uri;
            s += @" /teamproject:""" + tpa.TeamProjectName + @"""";
            s += @" /settingsfile:""" + config + @"""";
            s += @" /mode:""" + vm.SelectedMode + @"""";

            return s;
        }

        private static void OverRideAgeSettingsInConfig(int start, int period, XmlDocument doc)
        {
            XmlNodeList lst = doc.SelectNodes("/DeletionCriteria/TestRun/AgeInDays ");
            XmlElement nAgeInDays;
            if (lst.Count == 0)
            {
                nAgeInDays = doc.CreateElement("AgeInDays");
                nAgeInDays.Attributes.Append(doc.CreateAttribute("OlderThan"));
                nAgeInDays.Attributes.Append(doc.CreateAttribute("NewerThan"));

                doc.SelectNodes("/DeletionCriteria/TestRun")[0].AppendChild(nAgeInDays);
            }
            else
            {
                nAgeInDays = (XmlElement)lst[0];
            }
            nAgeInDays.Attributes["OlderThan"].Value = start.ToString();
            nAgeInDays.Attributes["NewerThan"].Value = (start + period).ToString();

            //Check and remove created by 
            lst = doc.SelectNodes("/DeletionCriteria/TestRun/AgeInDays ");
            if (lst.Count == 0)
            {
                lst[0].ParentNode.RemoveChild(lst[0]);
            }
        }

        private void cmdStart_Click(object sender, RoutedEventArgs e)
        {
            Thread workerThread = new Thread((ThreadStart)delegate { vm.DoCalcTree(); });
            workerThread.Start();
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            vm.InvertRootFolderSelection();
            dgrProjects.GetBindingExpression(DataGrid.ItemsSourceProperty).UpdateTarget();
        }
    }
}
