namespace mskold.TfsAdminToolKit.ViewModels
{ 
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Microsoft.TeamFoundation.Framework.Client;
    using Sogeti.VSExtention;

    public class SubscriptionsViewModel : INotifyPropertyChanged
    {
        public TeamExplorerIntergator teamExplorer;
        protected TFSUserWrapper usrWrp;
        private string _FilterDeliveryType;
        private string _FilterEventType;
        private string _FilterEmail;
        private string _FilterCondition;
        private List<TfsSubscriptionsItem> selectedSubscriptions;

        public SubscriptionsViewModel()
        { 
        }

        public List<TfsSubscriptionsItem> Subscriptions
        {
            get
            {
                if (usrWrp != null)
                {
                    List<TfsUser> lstUsr = Users;
                    if (lstUsr == null)
                    {
                        lstUsr = new List<TfsUser>();
                    }

                    List<TfsSubscriptionsItem> lst = usrWrp.ListSubscriptions(ref lstUsr);
                    Users = lstUsr;

                    return lst;
                }
                else
                {
                    return null;
                }
            }
        }

        public List<string> EventTypes
        {
            get
            {
                string[] fo = { "CheckinEvent", "WorkItemChangedEvent", "BuildCompletionEvent2", "BuildCompletionEvent", "BuildStatusChangeEvent" };

                return fo.ToList();
            }
        }

        public List<string> DeliveryTypes
        {
            get { return Enum.GetNames(typeof(DeliveryType)).ToList(); }
        }

        public string FilterEventType
        {
            get
            {
                return _FilterEventType;
            }

            set
            {
                _FilterEventType = value;
                NotifyPropertyChanged("FilterEventType");
                NotifyPropertyChanged("FilterSubscriptions");
            }
        }

        public List<TfsUser> Users { get; set; }

        public string FilterCondition
        {
            get
            {
                return _FilterCondition;
            }

            set
            {
                _FilterCondition = value;
                NotifyPropertyChanged("FilterCondition");
                NotifyPropertyChanged("FilterSubscriptions");
            }
        }

        public string FilterDeliveryType
        {
            get
            {
                return _FilterDeliveryType;
            }

            set
            {
                _FilterDeliveryType = value;
                NotifyPropertyChanged("FilterDeliveryType");
                NotifyPropertyChanged("FilterSubscriptions");
            }
        }

        public string FilterEmail
        {
            get
            {
                return _FilterEmail;
            }

            set
            {
                _FilterEmail = value;
                NotifyPropertyChanged("FilterEmail");
                NotifyPropertyChanged("FilterSubscriptions");
            }
        }

        public List<TfsSubscriptionsItem> FilterSubscriptions
        {
            get
            {
                if (Subscriptions != null)
                {
                    var x = from s in Subscriptions select s;
                    if (!string.IsNullOrEmpty(FilterEmail))
                    {
                        string FILTER = FilterEmail.ToUpper();
                        x = from s in x where s.Email.ToUpper().IndexOf(FILTER) >= 0 select s;
                    }

                    if (!string.IsNullOrEmpty(FilterCondition))
                    {
                        string FILTER = FilterCondition.ToUpper();
                        x = from s in x where s.Conditionstring.ToUpper().IndexOf(FILTER) >= 0 select s;
                    }

                    if (!string.IsNullOrEmpty(FilterDeliveryType))
                    {
                        x = from s in x where s.EmailType == FilterDeliveryType select s;
                    }

                    if (!string.IsNullOrEmpty(FilterEventType))
                    {
                        x = from s in x where s.EventTypeName == FilterEventType select s;
                    }

                    return x.ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        public List<TfsUser> UserLst
        {
            get;
            set;
        }

        public List<TfsSubscriptionsItem> SelectedSubscriptions
        {
            get
            {
                return selectedSubscriptions;
            }

            set
            {
                selectedSubscriptions = value;
                NotifyPropertyChanged("SelectedSubscriptions");
            }
        }

        public void Load(TeamExplorerIntergator te)
        { 
            teamExplorer = te;
            usrWrp = new TFSUserWrapper(teamExplorer);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string info)
        { 
            if (PropertyChanged != null)
            { 
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        internal void Refresh()
        { 
            NotifyPropertyChanged("Subscriptions");
            NotifyPropertyChanged("FilterSubscriptions");
        }

        internal void Unsubscribe()
        { 
            foreach (TfsSubscriptionsItem itm in SelectedSubscriptions)
            { 
                usrWrp.UnSubscribe(itm.ID);
            }

            Refresh();
        }

        internal TfsUser GetUserFromSid(string sidToAdd)
        { 
            return usrWrp.GetTfsUser(sidToAdd);
        }
    }
}
