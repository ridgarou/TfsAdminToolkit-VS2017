namespace Sogeti.BuildWrapper
{ 
    using System;
    using System.Collections.Generic;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Client;
    using Sogeti.VSExtention;
    
    public class BuildAgent
    { 
        public string Name { get; set; }

        public bool Enabled { get; set; }

        public string StatusMsg { get; set; }

        public Uri Uri { get; set; }

        public string BuildDir { get; set; }

        public string Machine { get; set; }

        public List<string> Tags { get; set; }
    }

    public class BuildController
    { 
        public string Name { get; set; }

        public bool Enabled { get; set; }
        
        public string StatusMsg { get; set; }
        
        public Uri Uri { get; set; }
    }

    public class BuildWrapper
    { 
        private TfsTeamProjectCollection tpCollection;
        private string teamProjectName;
        private IBuildServer buildSrv;

   		public BuildWrapper(string tpCollectionUrl, string aTeamProjectName)
		{ 
			tpCollection = new TfsTeamProjectCollection(new Uri(tpCollectionUrl));
			teamProjectName = aTeamProjectName;

            buildSrv = (IBuildServer)tpCollection.GetService(typeof(IBuildServer));
		}

		public BuildWrapper(TeamExplorerIntergator teamExplorer)
		{ 
			tpCollection = teamExplorer.tpCollection;
			teamProjectName = teamExplorer.tpName;

            buildSrv = (IBuildServer)tpCollection.GetService(typeof(IBuildServer));
		}

        public List<BuildController> GetBuildControlles()
        { 
            List<BuildController> lst = new List<BuildController>();

            foreach (IBuildController bc in buildSrv.QueryBuildControllers())
            { 
                lst.Add(new BuildController { Name = bc.Name, 
                                                Enabled = bc.Enabled, 
                                                StatusMsg = bc.StatusMessage,
                                                Uri = bc.Uri });
            }

            return lst;
        }

        public List<BuildAgent> GetBuildAgents()
        { 
            List<BuildAgent> lst = new List<BuildAgent>();

            IBuildAgentSpec agentSpec = buildSrv.CreateBuildAgentSpec();
            
            foreach (IBuildAgent ba in buildSrv.QueryBuildAgents(agentSpec).Agents)
            { 
                lst.Add(new BuildAgent()
                { 
                    Name = ba.Name,
                    Enabled = ba.Enabled,
                    BuildDir = ba.BuildDirectory,
                    Uri = ba.Uri,
                    Tags = ba.Tags,
                    StatusMsg = ba.StatusMessage
                });
            }

            return lst;
        }

        public void TestConnection(BuildController b)
        { 
            IBuildController bc = buildSrv.GetBuildController(b.Uri, false);
            buildSrv.TestConnectionForBuildController(bc);
        }

        internal void EnableController(BuildController b, bool started)
        { 
            IBuildController bc = buildSrv.GetBuildController(b.Uri, false);
            bc.Enabled = started;

            buildSrv.SaveBuildControllers(new IBuildController[] { bc });
        }
    }
}
