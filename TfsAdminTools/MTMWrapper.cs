namespace mskold.TfsAdminToolKit
{ 
    using System;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.TestManagement.Client;
    using Sogeti.VSExtention;

    public class MTMWrapper
    { 
		protected TfsTeamProjectCollection tpCollection;
		protected string teamProjectName;
        private ITestManagementService testSvc;

		public MTMWrapper(string tpCollectionUrl, string aTeamProjectName)
		{ 
            tpCollection = new TfsTeamProjectCollection(new Uri(tpCollectionUrl));
			teamProjectName = aTeamProjectName;

            testSvc = (ITestManagementService)tpCollection.GetService(typeof(ITestManagementService));
		}

        public MTMWrapper(TeamExplorerIntergator teamExplorer)
		{ 
            tpCollection = teamExplorer.tpCollection;
			teamProjectName = teamExplorer.tpName;

            testSvc = (ITestManagementService)tpCollection.GetService(typeof(ITestManagementService));
		}
    }
}
