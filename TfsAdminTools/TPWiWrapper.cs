namespace Sogeti.QueryWrapper
{ 
    using System;
    using System.Collections.Generic;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Server;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;
    using Sogeti.VSExtention;

    public class WI
    { 
        public string Title;
        public string Description;
        public string Type;
        public string AreaPath;
        public string IterationPath;
        public Dictionary<string, object> customFields;

        public WI()
        { 
            customFields = new Dictionary<string, object>();
        }
    }

    public class TPWiWrapper
    { 
        protected TfsTeamProjectCollection tpCollection;
        protected string teamProjectName;
        protected WorkItemStore wiStore;
        protected QueryHierarchy qh;
        protected WorkItemTypeCollection wiTypes;

        public TPWiWrapper(string tpCollectionUrl, string aTeamProjectName)
        { 
            tpCollection = new TfsTeamProjectCollection(new Uri(tpCollectionUrl));
            teamProjectName = aTeamProjectName;
            
            wiStore = (WorkItemStore)tpCollection.GetService(typeof(WorkItemStore));
            qh = wiStore.Projects[teamProjectName].QueryHierarchy;
            wiTypes = wiStore.Projects[teamProjectName].WorkItemTypes;
        }

        public TPWiWrapper(TeamExplorerIntergator teamExpolorer)
        { 
            tpCollection = teamExpolorer.tpCollection;
            teamProjectName = teamExpolorer.tpName;

            wiStore = (WorkItemStore)tpCollection.GetService(typeof(WorkItemStore));
            qh = wiStore.Projects[teamProjectName].QueryHierarchy;
            wiTypes = wiStore.Projects[teamProjectName].WorkItemTypes;
        }

        public void ImportWIT(string projectName,  string witXml)
        { 
                wiStore.Projects[projectName].WorkItemTypes.Import(witXml);
        }
        
        public string ExportWIT(string projectName, string witName, bool includeGlobalListFlag)
        { 
            return wiStore.Projects[projectName].WorkItemTypes[witName].Export(includeGlobalListFlag).OuterXml;
        }

        public List<WI> GetInitialWorkItens()
        { 
            string s = "SELECT [System.TeamProject], [System.Id],  [System.AssignedTo], [System.Title], [System.CreatedBy], [System.CreatedDate] FROM WorkItems WHERE [System.TeamProject]='" + teamProjectName + "'  ORDER BY [System.Id]";

            WorkItemCollection existingWI = wiStore.Query(s);
            DateTime dtCreated = existingWI[0].CreatedDate;
            string sCreatedBy = existingWI[0].CreatedBy;

            List<WI> lst = new List<WI>();
                        
            foreach (WorkItem itm in existingWI)
            { 
                if (itm.CreatedBy == sCreatedBy && itm.CreatedDate == dtCreated)
                { 
                    WI wi = new WI { Title = itm.Title, Description = itm.Description, Type = itm.Type.Name, AreaPath = itm.AreaPath, IterationPath = itm.IterationPath };
                    foreach (Field fld in itm.Fields)
                    { 
                        if (fld.ReferenceName.IndexOf("System.") < 0 && fld.ReferenceName.IndexOf("Microsoft.") < 0)
                        { 
                            wi.customFields.Add(fld.ReferenceName, fld.Value);
                        }
                    }
                    
                    lst.Add(wi);
                }
                else
                { 
                    break;
                }
            }

            return lst;
        }

        public List<string> GetWorkItemTypes()
        { 
            List<string> lst = new List<string>();
            foreach (WorkItemType wit in wiTypes)
            { 
                lst.Add(wit.Name);
            }

            return lst;          
        }

        public List<string> GetWorkItemTypes(string teamProject)
        { 
            List<string> lst = new List<string>();
            foreach (WorkItemType wit in wiStore.Projects[teamProject].WorkItemTypes)
            { 
                lst.Add(wit.Name);
            }

            return lst;
        }

        public void CreatWorkItems(List<WI> lst)
        { 
            foreach (WI itm in lst)
            { 
                WorkItem newItm = new WorkItem(wiTypes[itm.Type]);
                newItm.Title = itm.Title;
                newItm.Description = itm.Description;
                newItm.AreaPath = itm.AreaPath;
                newItm.IterationPath = itm.IterationPath;
                foreach (KeyValuePair<string, object> kvp in itm.customFields)
                { 
                    newItm.Fields[kvp.Key].Value = kvp.Value;
                }
                    
                foreach (Field fld in newItm.Fields)
                { 
                    if (!fld.IsValid)
                    { 
                        Console.WriteLine(fld.Name + " " + fld.Value);
                    }
                }

                newItm.Save();
            }
        }

        public List<string> GetProjects()
        { 
            List<string> lst = new List<string>();
            foreach (Project p in wiStore.Projects)
            { 
                lst.Add(p.Name);
            }

            return lst;
        }

        public List<string> GetTemplates()
        { 
            List<string> lst = new List<string>();

            IProcessTemplates procTmplSrv = tpCollection.GetService<IProcessTemplates>();

            foreach (TemplateHeader header in procTmplSrv.TemplateHeaders())
            { 
                lst.Add(header.Name);
            }

            return lst;
        }

        public bool WorkIteTypeExist(string curProj, string witName)
        { 
            return wiStore.Projects[curProj].WorkItemTypes.Contains(witName);
        }

        public static string ReplaceFirst(string org, string search, string replace)
        {
            if (org.Contains(search))
            {
                int endOfSearch = org.IndexOf(search) + search.Length;
                string s = org.Substring(0, org.IndexOf(search) + search.Length);
                if (org.Length > endOfSearch + 1)
                {
                    s = s.Replace(search, replace);
                }

                string remaining = string.Empty;
                if (org.Length > endOfSearch)
                {
                    remaining = org.Substring(endOfSearch);
                }

                org = s + remaining;
            }

            return org;
        }

        internal void CreateWorkItem(string sprintWI, string title, string iterationPath,  Dictionary<string, object> fields)
        { 
            wiStore.RefreshCache();

            WorkItem newItm = new WorkItem(wiTypes[sprintWI]);
            newItm.Title = title;
            newItm.IterationPath = ReplaceFirst(iterationPath, @"\" + teamProjectName + @"\Iteration\", teamProjectName + @"\");
            
            foreach (KeyValuePair<string, object> kvp in fields)
            { 
                newItm.Fields[kvp.Key].Value = kvp.Value;
            }

            newItm.Save();        
        }

        internal List<string> GetWorkItemFields(string wiTypeName,  string wiFieldType = "")
        { 
            List<string> lst = new List<string>();
            WorkItemType wiType = wiTypes[0];

            if (wiTypes.Contains(wiTypeName))
            { 
                wiType = wiTypes[wiTypeName];
            }

            foreach (FieldDefinition fld in wiType.FieldDefinitions)
            { 
                if (wiFieldType == string.Empty || wiFieldType == fld.FieldType.ToString())
                { 
                    lst.Add(fld.ReferenceName);
                }
            }

            return lst;
        }
    }
}
