namespace Sogeti.SourceControlWrapper
{ 
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows.Data;
    using System.Xml;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.VersionControl.Client;
    using Microsoft.TeamFoundation.VersionControl.Common;
    using mskold.TfsAdminToolKit;
    using Sogeti.VSExtention;

    public enum fileSizeUnits
    { 
        bytes,
        KB,
        MB,
        GB,
        TB,
        PB
    }

    public class EnumDescriptionConverter : IValueConverter
    { 
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        { 
            if (value is fileSizeUnits)
            { 
                if (targetType == typeof(IEnumerable) && parameter.ToString() == "List")
                { 
                    List<string> lst = new List<string>();
                    foreach (var item in Enum.GetNames(value.GetType()))
                    { 
                        lst.Add(item);
                    }

                    return lst;
                }
                else
                { 
                    foreach (var item in Enum.GetValues(typeof(fileSizeUnits)))
                    { 
                        var asstring = (item as Enum).ToString();
                        if (asstring == value.ToString())
                        { 
                            return item;
                        }
                    }
                }
            }

            return (value as Enum).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { 
            if (targetType == typeof(fileSizeUnits))
            { 
                foreach (var item in Enum.GetValues(targetType))
                { 
                    var asstring = (item as Enum).ToString();
                    if (asstring == (string)value)
                    { 
                        return item;
                    }
                }
            }
            else
            { 
                foreach (var item in Enum.GetValues(targetType))
                { 
                    var asstring = (item as Enum).ToString();
                    if (asstring == (string)value)
                    { 
                        return item;
                    }
                }
            }

            return null;
        }

        #endregion
    }

    public class DivideBy2Converter : IValueConverter
    { 
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        { 
            if (value is int || value is double)
            { 
                double d = 0;

                double.TryParse(value.ToString(), out d);

                return d * System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { 
            return null;
        }

        #endregion
    }

    public class FileSizeConverter : IValueConverter
    { 
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        { 
            if (value is int || value is double || value is long)
            { 
                return SCFileSize.SizeTxt(System.Convert.ToInt64(value));
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { 
            return null;
        }

        #endregion
    }

    public class SearchCondition
    { 
        private bool isCaseSensitive;
        private Regex regexpSearch;
        private string txtSearch;
        private bool useRegexp;

        public SearchCondition(string pattern, bool caseSensive, bool useReqExpMatching)
        { 
            isCaseSensitive = caseSensive;
            useRegexp = useReqExpMatching;

            if (useRegexp)
            { 
                regexpSearch = new Regex(pattern);
            }
            else
            { 
                txtSearch = pattern;
            }
        }

        public bool Matches(string s)
        { 
            bool ret = false;
            if (s == null)
            {
                return false;
            }
            if (useRegexp)
            { 
                ret = regexpSearch.IsMatch(s);
            }
            else
            { 
                if (!isCaseSensitive)
                { 
                    ret = s.ToUpper().Contains(txtSearch.ToUpper());
                }
                else
                { 
                    ret = s.Contains(txtSearch);
                }
            }

            return ret;
        }

        public int MatchStart(string s)
        {
            int ret=-1;
            if (useRegexp)
            { 
                ret = regexpSearch.Match(s).Index;
            }
            else
            { 
                if (!isCaseSensitive)
                { 
                    ret = s.ToUpper().IndexOf(txtSearch.ToUpper());
                }
                else
                { 
                    ret = s.IndexOf(txtSearch);
                }
            }

            return ret;
        }

        
        public int MatchEnd(string s)
        {
        
            int ret=-1;
            if (useRegexp)
            { 
                Match theMatch=  regexpSearch.Match(s);
                ret = theMatch.Index + theMatch.Length;
            }
            else
            { 
                if (!isCaseSensitive)
                { 
                    ret = s.ToUpper().IndexOf(txtSearch.ToUpper())+txtSearch.Length;
                }
                else
                { 
                    ret = s.IndexOf(txtSearch)+txtSearch.Length;
                }
            }

            return ret;
        }


    }

    public class Wildcard : Regex
    { 
        public Wildcard(string pattern)
            : base(WildcardToRegex(pattern))
        { 
        }

        public Wildcard(string pattern, RegexOptions options)
            : base(WildcardToRegex(pattern), options)
        { 
        }

        public static string WildcardToRegex(string pattern)
        { 
            string s=null;
            if(pattern.StartsWith("@"))
            {
                s= pattern.Substring(2);
            }
            else
            {
                s= "^(" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".").Replace(";", "|") + ")$";
            }
                return s;
        }
    }

    public class SCFileSize
    { 
        public static string SizeTxt(long theSize)
        { 
            string s = string.Empty;
            long d = theSize;
            if (theSize != 0)
            { 
                string[] size = Enum.GetNames(typeof(fileSizeUnits));
                long last = d;
                int i = 0;
                do
                { 
                    last = d;
                    d = d / 1024;
                    i++;
                } while (d > 1);
                s = string.Format("{0:#,0.##} {1}", last, size[i - 1]);
            }

            return s;
        }

        public static long GetSizeInBytes(long size, fileSizeUnits unit)
        { 
            long d = size;
            string[] sizeLst = Enum.GetNames(typeof(fileSizeUnits));
            int i = 0;
            while (sizeLst[i] != unit.ToString())
            { 
                d *= 1024;
                i++;
            }

            return d;
        }
    }

    public class WI
    { 
        public string AreaPath;
        public string Description;
        public string IterationPath;
        public string Title;
        public string Type;
        public Dictionary<string, object> customFields;

        public WI()
        { 
            customFields = new Dictionary<string, object>();
        }
    }

    public class SCFolder : INotifyPropertyChanged
    { 
        public SCFolder()
        { 
            Folders = new List<SCFolder>();
            FileTypes = new Dictionary<string, long>();
            InclusiveFileTypes = new Dictionary<string, long>();
        }

        public string FolderName
        { 
            get { return FolderPath.Split('/').Last(); }
        }

        public bool IsSelected 
        { 
            get
            {
                return _isSelected;
            }

            set
            {
                _isSelected = value;
                NotifyPropertyChanged("IsSelected");
            }
        }

        public string FolderPath { get; set; }

        public long SizeInclusive { get; set; }

        public long SizeExclusive { get; set; }

        public List<SCFolder> Folders { get; set; }

        public Dictionary<string, long> FileTypes { get; set; }

        public Dictionary<string, long> InclusiveFileTypes { get; set; }

        public string SizeTxt
        { 
            get { return SCFileSize.SizeTxt(SizeInclusive); }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void AddFolder(SCFolder fld)
        { 
            Folders.Add(fld);
            NotifyPropertyChanged("Folders");
        }

        public void SetInclusiveSize()
        { 
            long inclusive = 0;
            InclusiveFileTypes.Clear();
            foreach (string key in FileTypes.Keys)
            { 
                InclusiveFileTypes.Add(key, FileTypes[key]);
            }

            foreach (SCFolder fld in Folders)
            { 
                inclusive += fld.SizeInclusive;
                foreach (string key in fld.InclusiveFileTypes.Keys)
                { 
                    if (!InclusiveFileTypes.Keys.Contains(key))
                    { 
                        InclusiveFileTypes.Add(key, 0);
                    }

                    InclusiveFileTypes[key] += fld.InclusiveFileTypes[key];
                }
            }

            SizeInclusive = inclusive + SizeExclusive;
            NotifyPropertyChanged("SizeTxt");
        }

        public void NotifyPropertyChanged(string info)
        { 
            if (PropertyChanged != null)
            { 
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


        private bool _isSelected;

    }

    public class SCFile : INotifyPropertyChanged
    { 
        public SCFile()
        { 
        }

        public string FileName
        { 
            get { return FilePath.Split('/').Last(); }
        }

        public string FilePath { get; set; }

        public string Committer { get; set; }

        public DateTime CheckInDate { get; set; }

        public string Comment { get; set; }

        public bool Deleted { get; set; }

        public int ChangesetId { get; set; }
        
        public int ItemId { get; set; }

        public long Size { get; set; }

        public string SizeTxt
        { 
            get { return SCFileSize.SizeTxt(Size); }
        }

        public string PreviewText
        {
            get;set;
        }




        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public static void AddFileToList(SCFile file, List<SCFile> lst)
        { 
            lst.Add(file);
            file.NotifyPropertyChanged("FoundFiles");
        }

        public void NotifyPropertyChanged(string info)
        { 
            if (PropertyChanged != null)
            { 
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public string FileExt 
        {
            get
            {
                return FileName.Substring(FileName.LastIndexOf('.')+1);
            }
        }

        public string FileNameWithoutExt
        {
            get
            {
                return FileName.Substring(0, FileName.LastIndexOf('.'));
            }
        }


    }

    public class SCWrapper
    {
        protected VersionControlServer scSrv;
        protected string teamProjectName;
        protected TfsTeamProjectCollection tpCollection;

        public SCWrapper(string tpCollectionUrl, string aTeamProjectName)
        {
            tpCollection = new TfsTeamProjectCollection(new Uri(tpCollectionUrl));
            teamProjectName = aTeamProjectName;

            scSrv = (VersionControlServer)tpCollection.GetService(typeof(VersionControlServer));
        }

        public SCWrapper(TeamExplorerIntergator teamExplorer)
        {
            tpCollection = teamExplorer.tpCollection;
            teamProjectName = teamExplorer.tpName;

            scSrv = (VersionControlServer)tpCollection.GetService(typeof(VersionControlServer));
        }

        public string RootFolder()
        {
            return scSrv.GetTeamProject(teamProjectName).ServerItem;
        }

        public List<SCFolder> GetRootFolders()
        {
            List<SCFolder> lst = new List<SCFolder>();
            ItemSet items = scSrv.GetItems("$\\", RecursionType.OneLevel);

            foreach (Item itm in items.Items)
            {
                if (itm.ServerItem != @"$/")
                {
                    if (itm.ItemType == ItemType.Folder)
                    {
                        lst.Add(new SCFolder { FolderPath = itm.ServerItem });
                    }
                }
            }

            return lst;
        }

        public void CalcSize(ref SCFolder fld)
        {
            ItemSet items = scSrv.GetItems(fld.FolderPath, VersionSpec.Latest, RecursionType.OneLevel, DeletedState.NonDeleted, ItemType.Any);

            long totalExclusive = 0;
            long totalInclusive = 0;

            foreach (Item itm in items.Items)
            {
                switch (itm.ItemType)
                {
                    case ItemType.File:
                        long size = itm.ContentLength;
                        totalExclusive += size;
                        string fileExt = itm.ServerItem.Split('.').Last();
                        if (!fld.FileTypes.Keys.Contains(fileExt))
                        {
                            fld.FileTypes.Add(fileExt, 0);
                        }

                        fld.FileTypes[fileExt] += size;

                        break;
                    case ItemType.Folder:
                        if (itm.ServerItem != fld.FolderPath)
                        {
                            SCFolder child = new SCFolder { FolderPath = itm.ServerItem };
                            CalcSize(ref child);
                            fld.SetInclusiveSize();
                            fld.AddFolder(child);
                            totalInclusive += child.SizeInclusive;
                        }

                        break;
                }
            }

            fld.SizeExclusive = totalExclusive;
            fld.SetInclusiveSize();
        }

        public void DownLoadFile(string filename, int itemId, int changesetId)
        {
            Item itm = scSrv.GetItem(itemId, changesetId);
            itm.DownloadFile(filename);
        }



        public List<SCFile> SearchForFile(SCFolder fld, Wildcard fileNameFilter, long minSize)
        {
            ItemSet items = scSrv.GetItems(fld.FolderPath, VersionSpec.Latest, RecursionType.OneLevel, DeletedState.NonDeleted, ItemType.Any);

            List<SCFile> lst = new List<SCFile>();

            foreach (Item itm in items.Items)
            {
                switch (itm.ItemType)
                {
                    case ItemType.File:
                        if (itm.ContentLength > minSize)
                        {
                            if (fileNameFilter.IsMatch(itm.ServerItem))
                            {
                                
                                SCFile file = new SCFile();
                                file.ItemId = itm.ItemId;
                                file.ChangesetId = itm.ChangesetId;
                                file.FilePath = itm.ServerItem;
                                file.Size = itm.ContentLength;

                                Changeset chgSet = itm.VersionControlServer.GetChangeset(itm.ChangesetId);

                                file.Committer = chgSet.Committer;
                                file.CheckInDate = itm.CheckinDate;

                                file.Comment = chgSet.Comment;
                                if (itm.DeletionId != null)
                                {
                                    file.Deleted = itm.DeletionId != 0;
                                }

                                SCFile.AddFileToList(file, lst);
                            }
                        }

                        break;
                    case ItemType.Folder:
                        if (itm.ServerItem != fld.FolderPath)
                        {
                            SCFolder child = new SCFolder { FolderPath = itm.ServerItem };
                            lst.AddRange(SearchForFile(child, fileNameFilter, minSize));
                        }

                        break;
                }
            }

            return lst;
        }

        public List<SCFile> SearchForCheckIn(SCFolder fld, Wildcard user, Wildcard comment, Wildcard fileNameFilter, SearchCondition search, bool bSearchInHistory, CancelCallbackNotify callback)
        {
            QueryHistoryParameters qhp = new QueryHistoryParameters(fld.FolderPath, RecursionType.None);
            List<SCFile> lstFoundFiles= new List<SCFile>();
            IEnumerable<Changeset> changesets = null;

            int i = 0;
            foreach (Changeset chgset in changesets)
            {
                i++;
                if (user.IsMatch(chgset.Committer))
                {
                    if (callback.DoCallback((double)i / changesets.Count(), chgset.ChangesetId.ToString(), ref lstFoundFiles))
                    {
                        return lstFoundFiles;
                    }

                    if (comment.IsMatch(chgset.Comment))
                    {
                        SCFile file = new SCFile();
                        file.ChangesetId = chgset.ChangesetId;
                        file.Comment = chgset.Comment.ToString();
                        file.FilePath = chgset.Committer;
                        SCFile.AddFileToList(file, lstFoundFiles);
                    }
                }
            }
            return lstFoundFiles;
        }

        public bool FileExist(string fileName)
        {
            return scSrv.ServerItemExists(fileName, ItemType.File);
        }

        public Stream Get(string filename)
        {
            Item itm = scSrv.GetItem(filename);
            return itm.DownloadFile();
        }

        public void Save(string filename, XmlDocument xml)
        {
            // Using temporary directory for the workspace mapping
            var dir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

            string serverPath = filename.Substring(0, filename.LastIndexOf(@"/"));
            string fileNamePart = filename.Replace(serverPath + @"/", string.Empty);

            if (!dir.Exists)
            {
                dir.Create();
            }

            try
            {
                // Query for workspaces and delete if found.	    
                string username = null;

                if (tpCollection.Credentials is System.Net.NetworkCredential)
                {
                    System.Net.NetworkCredential nw = tpCollection.Credentials as System.Net.NetworkCredential;
                    if (nw.UserName != string.Empty)
                    {
                        username = nw.Domain + @"\" + nw.UserName;
                    }
                }

                if (username == null)
                {
                    username = Environment.UserName;
                }

                var ws = scSrv.QueryWorkspaces("temp", username, Environment.MachineName).FirstOrDefault();
                if (ws != null)
                {
                    ws.Delete();
                }

                // Create the workspace with a mapping to the temporary folder.
                ws = scSrv.CreateWorkspace("temp", username, string.Empty, new WorkingFolder[] { new WorkingFolder(serverPath, dir.FullName) });
                ws.Get(new GetRequest(filename, RecursionType.None, VersionSpec.Latest), GetOptions.None);

                // Create a file and add it as a pending change.
                var file = Path.Combine(dir.FullName, fileNamePart);
                FileInfo fi = new FileInfo(file);
                if (fi.Exists)
                {
                    fi.Attributes = FileAttributes.Normal;
                    xml.Save(file);
                    ws.PendEdit(file);
                }
                else
                {
                    xml.Save(file);
                    ws.PendAdd(file);
                }

                // Finally check-in, don't trigger a Continuous Integration build and override gated check-in.
                var wip = new WorkspaceCheckInParameters(ws.GetPendingChanges(), "***NO_CI***")
                              {
                                  // Enable the override of gated check-in when the server supports gated check-ins.
                                  OverrideGatedCheckIn =
                                      ((CheckInOptions2)scSrv.SupportedFeatures & CheckInOptions2.OverrideGatedCheckIn) ==
                                      CheckInOptions2.OverrideGatedCheckIn,
                                  PolicyOverride = new PolicyOverrideInfo("Check-in from IterationnManaager.", null)
                              };

                ws.CheckIn(wip);
            }
            finally
            {
                if (dir.Exists)
                {
                    dir.Attributes = FileAttributes.Normal;
                    dir.Delete(true);
                }
            }
        }

        public List<SCFile> SearchForFileContent(List<SCFolder> fldLst, Wildcard fileNameFilter, SearchCondition search, bool bSearchInHistory, CancelCallbackNotify callback)
        {
            int i = 0;

            List<SCFile> lstFoundFiles = new List<SCFile>(); 

            foreach (SCFolder fld  in fldLst)
            {
                callback.DoCallback((double)i / fldLst.Count(), fld.FolderPath, ref lstFoundFiles);
                CancelCallbackNotify notify = new CancelCallbackNotify();
                notify._delegate = callback._delegate;
                notify.start = callback.current;
                notify.range = callback.range/fldLst.Count;
                notify.isCanceled = callback.isCanceled;

                lstFoundFiles.AddRange(SearchForFileContent(fld, fileNameFilter, search, bSearchInHistory, notify));
                i ++;
                if (notify.isCanceled)
                {
                    break;
                }
                
            }
            return lstFoundFiles;
        }

        public List<SCFile> SearchForFileContent(SCFolder fld, Wildcard fileNameFilter, SearchCondition search, bool bSearchInHistory, CancelCallbackNotify callback)
        {
            ItemSet items = scSrv.GetItems(fld.FolderPath, VersionSpec.Latest, RecursionType.OneLevel, DeletedState.Any, ItemType.Any);
            List<SCFile> lstFoundFiles = new List<SCFile>();
            Regex t = new Regex(@"^(.*\.vb|.*\.xaml)$");
            bool b = t.IsMatch("foo.vb");

            int i = 0;
            foreach (Item itmLatest in items.Items)
            {
                if (callback.isCanceled)
                {
                    break;
                }
                switch (itmLatest.ItemType)
                {
                    case ItemType.File:
                        i++;
                        if (fileNameFilter.IsMatch(itmLatest.ServerItem))
                        {
                            if (callback.DoCallback((double)i / items.Items.Count(), itmLatest.ServerItem, ref lstFoundFiles))
                            {
                                return lstFoundFiles;
                            }
                            else 
                            
                        
                            if (bSearchInHistory)
                            {
                                foreach (Changeset ch in scSrv.QueryHistory(itmLatest.ServerItem,
                                                                            VersionSpec.Latest,
                                                                            0,
                                                                            RecursionType.Full,
                                                                            string.Empty,
                                                                            null,
                                                                            VersionSpec.Latest,
                                                                            int.MaxValue,
                                                                            true,
                                                                            true))
                                {
                                    Item myItm = scSrv.GetItem(itmLatest.ItemId, ch.ChangesetId);
                                    if (myItm != null)
                                    {
                                        FindInItem(search, ref lstFoundFiles, myItm);
                                    }
                                }
                            }
                            else
                            {
                                FindInItem(search, ref lstFoundFiles, itmLatest);
                            }
                        }

                        break;
                    case ItemType.Folder:
                        if (itmLatest.ServerItem != fld.FolderPath)
                        {
                            i++;
                            if (callback.DoCallback((double)i / items.Items.Count(), itmLatest.ServerItem, ref lstFoundFiles))
                            {
                                return lstFoundFiles;
                            }
                            else
                            {
                                SCFolder child = new SCFolder { FolderPath = itmLatest.ServerItem };
                                CancelCallbackNotify subCallback = new CancelCallbackNotify();
                                subCallback._delegate = callback._delegate;
                                subCallback.start = callback.current;
                                subCallback.range = 1.0 / (items.Items.Count() - 1);

                                lstFoundFiles.AddRange( SearchForFileContent(child, fileNameFilter, search, bSearchInHistory, subCallback));

                                
                            }
                        }

                        break;
                } 
            }

            callback.DoCallback(1, "Done searching " + fld.FolderPath, ref lstFoundFiles);
            return lstFoundFiles;
        }

        private static void FindInItem(SearchCondition search,  ref List<SCFile> lstFoundFiles,Item itm)
        {
            
            try
            {
                Stream fileStream = itm.DownloadFile();
                StreamReader r = new StreamReader(fileStream);
                int lineNo = 0;
                do
                {
                    lineNo++;
                    string s = r.ReadLine();
                    if (search.Matches(s))
                    {
                        SCFile file = new SCFile();
                        file.FilePath = itm.ServerItem;
                        file.Size = itm.ContentLength;
                        file.ItemId = itm.ItemId;
                        file.ChangesetId = itm.ChangesetId;
                        file.Comment = lineNo.ToString();



                        file.PreviewText = ReadPreviewTextFromStream(itm, lineNo, search, 5);

                        lstFoundFiles.Add(file);

                        break;
                    }
                } while (!r.EndOfStream);
                r.Dispose();
                fileStream.Dispose();
            }
            catch (Exception ex)
            {
                SCFile file = new SCFile();
                file.FilePath = itm.ServerItem;
                file.Size = itm.ContentLength;
                file.Comment ="ERROR reading file content: " + ex.Message;
                lstFoundFiles.Add(file);
            }
        }

        private static string ReadPreviewTextFromStream( Item itm, int lineNo, SearchCondition search, int showExtraLines)
        {
            string preview = string.Empty;
             Stream fileStream = itm.DownloadFile();
            StreamReader rTmp = new StreamReader(fileStream);
         
            int readToLine= lineNo;
            if (readToLine > showExtraLines)
            {
                readToLine -= showExtraLines;
            }
            else{
                readToLine=0;
            }

            for(int i=0; i<readToLine ; i++)
            {
                rTmp.ReadLine(); //Just waste them
            }
           
            for (int i = 0; i < showExtraLines * 2 && !rTmp.EndOfStream; i++)
            {
                string s= rTmp.ReadLine() + "\r\n";
                if (search.Matches(s))
                {
                    int start = search.MatchStart(s);
                    int end = search.MatchEnd(s);

                    string pre = s.Substring(0, start);
                    string hit = s.Substring(start, end - start);
                    string post = s.Substring(end);

                    s = string.Format(@"{0}<Bold><Span Background=""Salmon"">{1}</Span></Bold>{2}", pre, hit, post);
                }
            
                preview += s;
                
            }

           

            rTmp.Dispose();
            fileStream.Dispose();
            return preview;            
        }
    }


}