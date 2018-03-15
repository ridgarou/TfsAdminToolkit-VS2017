namespace mskold.TfsAdminToolKit.ViewModels
{
    using System.ComponentModel;

    public enum JobStatus
    {
        /// <summary>
        /// notStarted
        /// </summary>
        notStarted,

        /// <summary>
        /// inProgress
        /// </summary>        
        inProgress,

        /// <summary>
        /// done
        /// </summary>
        done
    }

    public class ProgressIndication : INotifyPropertyChanged
    {
        #region "Private members"

        private JobStatus status;
        private double progress;
        private string curOp;
        private double maxvalue;
      

        #endregion

        public ProgressIndication()
        {
            Max = 100;
        }

        #region "Public properties"

        public JobStatus JobStatus
        {
            get
            {
                return this.status;
            }

            set
            {
                this.status = value;
                if(value != JobStatus.done)
                {
                    this.Cancel = false;
                }
                this.NotifyPropertyChanged("JobStatus");
                this.NotifyPropertyChanged("ShowProgress");
                this.NotifyPropertyChanged("ShowForm");
                this.NotifyPropertyChanged("IsJobDone");
            }
        }

        public bool ShowProgress
        {
            get { return this.JobStatus == JobStatus.inProgress || this.JobStatus == JobStatus.done; }
        }

        public bool Cancel { get; set; }
        
        public string CurrentOperation  { 
            get
            {
                return curOp;
            }
            set 
            { 
                curOp= value; 
                NotifyPropertyChanged("CurrentOperation");
            }
        }

        public bool ShowForm
        {
            get { return this.JobStatus == JobStatus.notStarted; }
        }

        public bool IsJobDone
        {
            get { return this.JobStatus == JobStatus.done; }
        }

        public double JobProgress
        {
            get
            {
                return this.progress;
            }

            set
            {
                this.progress = value;
                this.NotifyPropertyChanged("JobProgress");
            }
        }

        public double Max 
        { 
            get
            { 
                return maxvalue; 
            }

            set
            {
                maxvalue = value;
                NotifyPropertyChanged("Max");
            }
        }

        #endregion

        public void Step()
        {
            JobProgress += 1 * 100 / Max;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion
    }
}