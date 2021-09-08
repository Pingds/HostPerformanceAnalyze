using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HostPerformanceAnalyze
{
    public class LogModel : INotifyPropertyChanged
    {

        #region properties

        private string time;

        public string Time
        {
            get { return time; }
            set
            {
                time = value;
                OnPropertyChanged();
            }
        }

        private string content;

        public string Content
        {
            get { return content; }
            set
            {
                content = value;
                OnPropertyChanged();
            }
        }


        #endregion

        public LogModel(string content)
        {
            Time = DateTime.Now.ToString("HH:mm:ss");
            Content = content;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged<T>(ref T propertyValue, T newValue, [CallerMemberName] string propertyName = null)
        {
            propertyValue = newValue;
            OnPropertyChanged(propertyName);
        }
        #endregion
    }
}
