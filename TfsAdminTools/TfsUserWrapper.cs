namespace mskold.TfsAdminToolKit
{ 
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Framework.Client;
    using Microsoft.TeamFoundation.Server;
    using Microsoft.TeamFoundation.VersionControl.Client;
    using Sogeti.VSExtention;

    public class TfsUser
    { 
        public string SID { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public List<string> Roles { get; set; }

        public string AccountName { get; set; }

        public TfsUser()
        { 
            Roles = new List<string>();
        }
    }

    public class TfsSubscriptionsItem
    { 
        public int ID { get; set; }

        public string Conditionstring { get; set; }

        public string Subscriber { get; set; }

        public string Schedule { get; set; }

        public string EventTypeName { get; set; }

        public string EmailType { get; set; }

        public string Tag { get; set; }

        public string Email { get; set; }

        public TfsUser User { get; set; }
    }

    public class TFSUserWrapper
    { 
        private TfsTeamProjectCollection tpCollection;
        private string teamProjectName;
        private IGroupSecurityService secSrv;

        public TFSUserWrapper(string tpCollectionUrl, string aTeamProjectName)
        { 
            tpCollection = new TfsTeamProjectCollection(new Uri(tpCollectionUrl));
            teamProjectName = aTeamProjectName;

            secSrv = (IGroupSecurityService)tpCollection.GetService(typeof(IGroupSecurityService));
        }

        public TFSUserWrapper(TeamExplorerIntergator teamExplorer)
        { 
            tpCollection = teamExplorer.tpCollection;
            teamProjectName = teamExplorer.tpName;

            secSrv = (IGroupSecurityService)tpCollection.GetService(typeof(IGroupSecurityService));
        }

        public List<string> Groups()
        { 
            List<string> lst = new List<string>();
            TeamProject tp = GetTeamProject();

            Identity[] appGroups = secSrv.ListApplicationGroups(tp.ArtifactUri.AbsoluteUri);

            foreach (Identity group in appGroups)
            { 
                lst.Add(group.DisplayName);
            }

            return lst;
        }

        public List<TfsUser> Users()
        { 
            List<TfsUser> lst = new List<TfsUser>();
            TeamProject tp = GetTeamProject();

            Identity[] appGroups = secSrv.ListApplicationGroups(tp.ArtifactUri.AbsoluteUri);

            foreach (Identity group in appGroups)
            { 
                Identity[] groupMembers = secSrv.ReadIdentities(SearchFactor.Sid, new string[] { group.Sid }, QueryMembership.Direct);

                foreach (Identity member in groupMembers)
                { 
                    if (member.Members != null)
                    { 
                        foreach (string memberSid in member.Members)
                        { 
                            lst.Add(GetTfsUser(memberSid));
                        }
                    }
                }
            }

            return lst;
        }

        public List<TfsSubscriptionsItem> ListSubscriptions(ref List<TfsUser> lstUser, string tpName = null)
        { 
            IEventService es = tpCollection.GetService(typeof(IEventService)) as IEventService;

            string filter = string.Format("\"PortfolioProject\" = '{0}'", teamProjectName);

            DeliveryPreference del = new DeliveryPreference();

            del.Schedule = DeliverySchedule.Immediate;
            del.Type = DeliveryType.EmailHtml;

            List<TfsSubscriptionsItem> lst = new List<TfsSubscriptionsItem>();

            string sTEAMPROJECTNAME = string.Empty;
            if (tpName != null)
            { 
                sTEAMPROJECTNAME = teamProjectName.ToUpper();
            }

            foreach (Subscription s in es.GetAllEventSubscriptions())
            { 
                if (sTEAMPROJECTNAME == string.Empty || s.ConditionString.ToUpper().Contains("'" + sTEAMPROJECTNAME + "'"))
                { 
                    lst.Add(new TfsSubscriptionsItem()
                                { 
                                    ID = s.ID,
                                    Conditionstring = s.ConditionString,
                                    Tag = ParseTag(s.Tag),
                                    Subscriber = s.Subscriber,
                                    EmailType = s.DeliveryPreference.Type.ToString(),
                                    EventTypeName = s.EventType,
                                    User = GetTfsUserFromList(s.Subscriber, ref lstUser),
                                    Email = s.DeliveryPreference.Address
                                });
                }
            }

            return lst;
        }

        public TfsUser GetTfsUser(string p)
        { 
            Identity memberInfo = secSrv.ReadIdentity(SearchFactor.Sid, p, QueryMembership.Direct);

            if (memberInfo != null)
            { 
                TfsUser usr = new TfsUser()
                                  { 
                                      SID = memberInfo.Sid,
                                      DisplayName = memberInfo.DisplayName,
                                      Email = memberInfo.MailAddress,
                                      AccountName = memberInfo.AccountName
                                  };

                Identity[] gorupInfo = secSrv.ReadIdentities(SearchFactor.Sid, memberInfo.MemberOf, QueryMembership.None);
                foreach (Identity g in gorupInfo)
                { 
                    usr.Roles.Add(g.DisplayName);
                }

                return usr;
            }
            else
            { 
                TfsUser usr = new TfsUser()
                                  { 
                                      SID = p,
                                      DisplayName = p
                                  };
                return usr;
            }
        }

        internal void Subscribe(TfsUser u, string tag, string eventType, string delType, string filter)
        { 
            IEventService es = tpCollection.GetService(typeof(IEventService)) as IEventService;

            DeliveryPreference del = new DeliveryPreference();
            del.Schedule = DeliverySchedule.Immediate;
            del.Type = (DeliveryType)Enum.Parse(typeof(DeliveryType), delType);
            del.Address = u.Email;

            es.SubscribeEvent(u.SID, eventType, filter, del, tag);
        }

        internal void UnSubscribe(int id)
        { 
            IEventService es = tpCollection.GetService(typeof(IEventService)) as IEventService;

            es.UnsubscribeEvent(id);
        }

        private TfsUser GetTfsUserFromList(string sidSubscriber, ref List<TfsUser> lstUser)
        {
            if (lstUser == null)
            {
                return null;
            }
            else
            {
                TfsUser usr = (from u in lstUser where u.SID == sidSubscriber select u).FirstOrDefault();

                if (usr == null)
                {
                    usr = GetTfsUser(sidSubscriber);
                    lstUser.Add(usr);
                }

                return usr;
            }
        }

        private string ParseTag(string p)
        {
            string s = p;
            if (p.StartsWith("<PT"))
            {
                s = p.Substring(p.IndexOf(@"N") + 3);
                s = s.Substring(1, s.IndexOf(@"""") - 1);
            }

            return s;
        }

        private TeamProject GetTeamProject()
        {
            VersionControlServer versionControl = (VersionControlServer)tpCollection.GetService(typeof(VersionControlServer));
            TeamProject teamProject = versionControl.GetTeamProject(teamProjectName);
            return teamProject;
        }
    }
}
