namespace mskold.TfsAdminToolKit.Views
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using mskold.TfsAdminToolKit.ViewModels;
    using Sogeti.SourceControlWrapper;
    using Sogeti.VSExtention;

    /// <summary>
    /// Interaction logic for FindInFiles.xaml
    /// </summary>
    public partial class FindInFiles : Window
    {
        private bool isCanceled = false;
        private FindInFilesViewModel vm;

        public FindInFiles()
        {
            InitializeComponent();

        }

        public FindInFiles(TeamExplorerIntergator te)
        {
            InitializeComponent();
       
            vm = new FindInFilesViewModel();
            vm.Load(te);
            this.DataContext = vm;
        }

        private void cmdSearch_Click(object sender, RoutedEventArgs e)
        {


            cmdStopSearch.Visibility = cmdSearch.Visibility;
            cmdSearch.Visibility = Visibility.Collapsed;

            Thread workerThread = new Thread((ThreadStart)delegate { vm.DoSearch(); });
            
            workerThread.Start();

            
        }

        private void cmdStopSearch_Click(object sender, RoutedEventArgs e)
        {
            cmdSearch.Visibility = cmdStopSearch.Visibility;
            cmdStopSearch.Visibility = Visibility.Collapsed;

            vm.Progress.Cancel = true;
        }

        private void cmdRemove_Click(object sender, RoutedEventArgs e)
        {
            vm.RemoveRootFolder(vm.SelectedFolder);
        }

        private void cmdAdd_Click(object sender, RoutedEventArgs e)
        {
            foreach (var i in lstTeamProjects.SelectedItems)
            {
                //vm.AddRootFolder(vm.SelectedTeamProject);
                vm.AddRootFolder((SCFolder)i);
            }
            
        }

        private void grdFoundFiles_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(dgrFoundFiles.SelectedItem!=null)
            { 
                vm.ShowFile((SCFile)dgrFoundFiles.SelectedItem);
            }

        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            vm.ShowFile((SCFile)dgrFoundFiles.SelectedItem);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }

    public static class TextblockFormatedTextBehaviour
    {
        public static string GetFormattedText(DependencyObject obj)
        {
            return (string)obj.GetValue(FormattedTextProperty);
        }

        public static void SetFormattedText(DependencyObject obj, string value)
        {
            obj.SetValue(FormattedTextProperty, value);
        }

        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.RegisterAttached("FormattedText",
            typeof(string),
            typeof(TextblockFormatedTextBehaviour),
            new UIPropertyMetadata("", FormattedTextChanged));

        static Inline Traverse(string value)
        {
            // Get the sections/inlines
            string[] sections = SplitIntoSections(value);

            // Check for grouping
            if (sections.Length.Equals(1))
            {
                string section = sections[0];
                string token; // E.g <Bold>
                string tokenAttrib; // E.g Background="RED"
                int tokenStart, tokenEnd, contentStart;// Where the token/section starts and ends.

                // Check for token
                if (GetTokenInfo(section, out token, out tokenAttrib, out tokenStart, out contentStart, out tokenEnd))
                {
                    // Get the content to further examination
                    string content = token.Length.Equals(tokenEnd - tokenStart) ?
                        null :
                        section.Substring(contentStart, section.Length - 1 - contentStart - token.Length);

                    switch (token)
                    {
                        case "<Bold>":
                            return new Bold(Traverse(content));
                        case "<Italic>":
                            return new Italic(Traverse(content));
                        case "<Underline>":
                            return new Underline(Traverse(content));
                        case "<Span>":
                            {
                                Span span=new Span(Traverse(content));
                                if(tokenAttrib!=null)
                                {
                                    if(tokenAttrib.Contains("Background"))
                                    {
                                        string color = tokenAttrib.Split('=')[1];
                                        color= color.Replace(@"""", "");
                                        color = color.Replace(@">", "");
                                        span.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
                                       // span.Background = System.Convert("Red", typeof(VsBrushes) );
                                    }
                                }
                                return span;
                            }
                            
                        case "<LineBreak/>":
                            return new LineBreak();
                        default:
                            return new Run(section);
                    }
                }
                else return new Run(section);
            }
            else // Group together
            {
                Span span = new Span();

                foreach (string section in sections)
                    span.Inlines.Add(Traverse(section));

                return span;
            }
        }

        /// <summary>
        /// Examines the passed string and find the first token, where it begins and where it ends.
        /// </summary>
        /// <param name="value">The string to examine.</param>
        /// <param name="token">The found token.</param>
        /// <param name="startIndex">Where the token begins.</param>
        /// <param name="endIndex">Where the end-token ends.</param>
        /// <returns>True if a token was found.</returns>
        static bool GetTokenInfo(string value, out string token, out string tokenAttrib, out int startIndex, out int startIndexContent, out int endIndex)
        {
            token = null;
            tokenAttrib = null;
            startIndexContent = -1;
             
            endIndex = -1;
            startIndex=-1;
            
            string tokenElement=null;
            string endToken = null;
            do
            {
                do
                {
                    startIndex = value.IndexOf("<", startIndex + 1);

                } while (startIndex >= 0 && value.Substring(startIndex, 2) == "</");



                int startTokenEndIndex = value.IndexOf(">", startIndex + 1);

                // No token here
                if (startIndex < 0)
                    return false;

                // No token here
                if (startTokenEndIndex < 0)
                    return false;

                token = value.Substring(startIndex, startTokenEndIndex - startIndex + 1);
                startIndexContent = startTokenEndIndex + 1;

                //Mattias 
                tokenElement = token;
                if (token.Contains(" "))
                {
                    tokenElement = token.Split(' ')[0] + ">";
                    tokenAttrib = token.Replace(tokenElement, "");

                }

                // Check for closed token. E.g. <LineBreak/>
                if (token.EndsWith("/>"))
                {
                    endIndex = startIndex + token.Length;
                    return true;
                }

                endToken= tokenElement.Insert(1, "/");

            } while (!value.Contains(endToken));

            // Detect nesting;
            int nesting = 0;
            int temp_startTokenIndex = -1;
            int temp_endTokenIndex = -1;
            int pos = 0;
            do
            {
                temp_startTokenIndex = value.IndexOf(token, pos);
                temp_endTokenIndex = value.IndexOf(endToken, pos);

                if (temp_startTokenIndex >= 0 && temp_startTokenIndex < temp_endTokenIndex)
                {
                    nesting++;
                    pos = temp_startTokenIndex + token.Length;
                }
                else if (temp_endTokenIndex >= 0 && nesting > 0)
                {
                    nesting--;
                    pos = temp_endTokenIndex + endToken.Length;
                }
                else // Invalid tokenized string
                    return GetTokenInfo(value.Substring(startIndexContent),  out token, out tokenAttrib, out startIndex, out startIndexContent, out endIndex);

            } while (nesting > 0);


           

            endIndex = pos;
            
            token = tokenElement;

            return true;
        }

        /// <summary>
        /// Splits the string into sections of tokens and regular text.
        /// </summary>
        /// <param name="value">The string to split.</param>
        /// <returns>An array with the sections.</returns>
        static string[] SplitIntoSections(string value)
        {
            List<string> sections = new List<string>();

            while (!string.IsNullOrEmpty(value))
            {
                string token;
                string tokenAttrib;
                int tokenStartIndex, tokenEndIndex, contentStart;

                // Check if this is a token section
                if (GetTokenInfo(value, out token, out tokenAttrib, out tokenStartIndex, out contentStart,  out tokenEndIndex))
                {
                    // Add pretext if the token isn't from the start
                    if (tokenStartIndex > 0)
                        sections.Add(value.Substring(0, tokenStartIndex));

                    sections.Add(value.Substring(tokenStartIndex, tokenEndIndex - tokenStartIndex));
                    value = value.Substring(tokenEndIndex); // Trim away
                }
                else
                { // No tokens, just add the text
                    sections.Add(value);
                    value = null;
                }
            }

            return sections.ToArray();
        }

        private static void FormattedTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            string value = e.NewValue as string;

            TextBlock textBlock = sender as TextBlock;

            if (textBlock != null)
            {
                textBlock.Inlines.Clear();
                textBlock.Inlines.Add(Traverse(value));
            }
        }
    }
}
