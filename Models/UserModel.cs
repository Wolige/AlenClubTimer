using AlenClubTimer.ViewModels;
using AlenClubTimer.Views;
using GalaSoft.MvvmLight.Command;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace AlenClubTimer.Models
{
    [DataContract]
    public class UserModel : INotifyPropertyChanged
    {
        private bool _isVisible;
        private int _progressBarValue;
        private int _progressBarMaxValue;
        private bool _isSetTimeReadOnly;
        private const string ip = "127.0.0.1";
        AddMinuteWindow addMinuteWindow;
        private const int port = 8080;
        public System.Timers.Timer t;
        private bool _isAddCommandVisible;

        [DataMember]
        private int monitoringNumber;

        public UserModel()
        {
            ProgressBarValue = 0;
            ProgressBarMaxValue = 1;
            if (!string.IsNullOrEmpty(File.ReadAllText("CashForHour.txt"))) CashForHour = int.Parse(File.ReadAllText("CashForHour.txt"));
            IsSetTimeReadOnly = false;
            EnterCommand = new RelayCommand(OnEnterCommand);
            ChangeCommand = new RelayCommand(() =>
            {
                IsSetTimeReadOnly = false;
                PlayMinute = 0;
                PlayHours = 0;
                OnPropertyChanged(nameof(PlayMinute));
                OnPropertyChanged(nameof(PlayHours));
            });
            StopCommand = new RelayCommand(() =>
            {
                Task.Run(() =>
                {
                    MessageBox.Show($"{Name} время окончена.");
                });
                OnStopCommand();
            });
            SetVipStatusCommand = new RelayCommand(OnSetVipStatusCommand);
            AddMinuteCommand = new RelayCommand(OnAddMinuteCommand);
        }

        private void OnAddMinuteCommand()
        {
            addMinuteWindow = new AddMinuteWindow(this);
            addMinuteWindow.Show();
        }

        public void CloseAddMinuteWindow()
        {
            if (addMinuteWindow.ShowActivated) addMinuteWindow.Close();
        }

        public void OnSetVipStatusCommand()
        {
            if (t != null)
            {
                t.Stop();
                t.Close();
            }
            IsAddCommandVisible = false;
            ProgressBarValue = 1;
            ProgressBarMaxValue = 1;
            PlayMinute = 0;
            PlayHours = 0;
            TotalValue = 0;
            Cash = 0;
            monitoringNumber = 0;
            TimePassed = 0;
            IsVisible = true;
            TimeLeft = "VIP";
            StartTime = DateTime.Now;
            MainPageViewModel.SendMsg("false", ID);
            IsSetTimeReadOnly = true;
            JsonUser.AddUserToWokingTimeHistory(this);
            VipTimeMonitoring();
            OnPropertyChanged(nameof(TimeLeft));
            OnPropertyChanged(nameof(TotalValue));
            OnPropertyChanged(nameof(StartTime));
            OnPropertyChanged(nameof(PlayMinute));
            OnPropertyChanged(nameof(PlayHours));
            OnPropertyChanged(nameof(TimePassed));
            OnPropertyChanged(nameof(Cash));
        }

        public void VipTimeMonitoring()
        {
            t = new System.Timers.Timer();
            t.Elapsed += new ElapsedEventHandler(t_VipStatus);
            t.Interval = 60_000;
            t.Start();
        }

        private void t_VipStatus(object sender, ElapsedEventArgs e)
        {
            monitoringNumber++;
            TimePassed = monitoringNumber;
            Cash = TimePassed * Math.Floor(CashForHour / 60);
            TotalValue = Cash;
            JsonUser.AddUserToWokingTimeHistory(this);
            OnPropertyChanged(nameof(Cash));
            OnPropertyChanged(nameof(TimePassed));
            OnPropertyChanged(nameof(TotalValue));
            t.Interval = 60_000;
            t.Start();
        }

        public void OnStopCommand()
        {
            ProgressBarValue = 0;
            ProgressBarMaxValue = 1;
            EndTime = DateTime.Now;
            JsonUser.AddUserToHistory(this);
            if (t != null)
            {
                t.Stop();
                t.Close();
            }
            OnEnding();
            JsonUser.RemoveUserFromWorkingTimeHistory(this);
        }

        public void OnEnterCommand()
        {
            PlayTime = ((PlayHours * 60) + PlayMinute).ToString();
            if (string.IsNullOrEmpty(PlayTime) || PlayTime == "0") return;
            if (IsSetTimeReadOnly) return;
            TotalValue = Math.Floor(int.Parse(PlayTime) * CashForHour / 60);
            MainPageViewModel.SendMsg(PlayTime, ID);
            ProgressBarValue = 0;
            IsVisible = true;
            IsAddCommandVisible = true;
            IsSetTimeReadOnly = true;
            StartTime = DateTime.Now;
            TimeLeft = PlayTime;
            TimePassed = 0;
            Cash = 0;
            monitoringNumber = 0;
            ProgressBarMaxValue = int.Parse(PlayTime);
            EndTime = DateTime.Now.AddMinutes(double.Parse(PlayTime));
            JsonUser.AddUserToWokingTimeHistory(this);
            TimeMonitoring();
            OnPropertyChanged(nameof(StartTime));
            OnPropertyChanged(nameof(TimeLeft));
            OnPropertyChanged(nameof(Cash));
            OnPropertyChanged(nameof(EndTime));
            OnPropertyChanged(nameof(TotalValue));
            OnPropertyChanged(nameof(TimePassed));
        }

        public void TimeMonitoring()
        {
            ProgressBarMaxValue = int.Parse(PlayTime);
            if (t != null)
            {
                t.Stop();
                t.Close();
            }
            TotalValue = Math.Floor(int.Parse(PlayTime) * CashForHour / 60);
            t = new System.Timers.Timer();
            t.AutoReset = false;
            t.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
            t.Interval = 60_000;
            t.Start();
        }

        public void OnEnding()
        {
            if (PlayTime != null && int.Parse(PlayTime) <= 0 && TimeLeft != "VIP")
            {
                Task.Run(() =>
                {
                    MessageBox.Show("Нельзя досрочно остановить, ведь вы не поставили время.", "Alenic");
                });
                return;
            }
            if (t != null)
            {
                t.Stop();
                t.Close();
            }
            PlayHours = 0;
            PlayMinute = 0;
            TimePassed = 0;
            TimeLeft = "0";
            ProgressBarValue = 0;
            ProgressBarMaxValue = 1;
            Cash = 0;
            IsSetTimeReadOnly = false;
            IsAddCommandVisible = false;
            IsVisible = false;
            MainPageViewModel.SendMsg("true", ID);
            OnPropertyChanged(nameof(PlayHours));
            OnPropertyChanged(nameof(PlayMinute));
            OnPropertyChanged(nameof(TimePassed));
            OnPropertyChanged(nameof(TimeLeft));
            OnPropertyChanged(nameof(ProgressBarValue));
            OnPropertyChanged(nameof(ProgressBarMaxValue));
            OnPropertyChanged(nameof(Cash));
        }

        public void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (TimePassed >= int.Parse(PlayTime) || TimePassed < 0)
            {
                Cash = TotalValue;
                t.Stop();
                t.Close();
                OnStopCommand();
                return;
            }
            monitoringNumber++;
            TimeLeft = (int.Parse(PlayTime) - monitoringNumber).ToString();
            TimePassed = monitoringNumber;
            ProgressBarValue = monitoringNumber;
            Cash = TimePassed * Math.Floor(CashForHour / 60);
            if (TimePassed >= int.Parse(PlayTime) || TimePassed < 0)
            {
                Cash = TotalValue;
                t.Stop();
                t.Close();
                OnStopCommand();
                return;
            }
            if (TimePassed % 5 == 0) MainPageViewModel.SendMsg(TimePassed.ToString(), ID);
            JsonUser.AddUserToWokingTimeHistory(this);
            OnPropertyChanged(nameof(TimeLeft));
            OnPropertyChanged(nameof(Cash));
            OnPropertyChanged(nameof(TimePassed));
            OnPropertyChanged(nameof(ProgressBarValue));
            t.Interval = 60_000;
            t.Start();
        }

        [DataMember]
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        [DataMember]
        public bool IsSetTimeReadOnly
        {
            get
            {
                return _isSetTimeReadOnly;
            }
            set
            {
                _isSetTimeReadOnly = value;
                OnPropertyChanged(nameof(IsSetTimeReadOnly));
            }
        }

        [DataMember]
        public bool IsAddCommandVisible
        {
            get
            {
                return _isAddCommandVisible;
            }
            set
            {
                if (_isAddCommandVisible != value)
                {
                    _isAddCommandVisible = value;
                    OnPropertyChanged(nameof(IsAddCommandVisible));
                }
            }
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string ConnectionId { get; set; }

        [DataMember]
        public double Cash { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime EndTime { get; set; }

        [DataMember]
        public string TimeLeft { get; set; }

        [DataMember]
        public int TimePassed { get; set; }

        [DataMember]
        public string PlayTime { get; set; }

        [DataMember]
        public static double CashForHour { get; set; }

        [DataMember]
        public int PlayMinute { get; set; }

        [DataMember]
        public int PlayHours { get; set; }

        [DataMember]
        public double TotalValue { get; set; }

        [DataMember]
        public int ProgressBarValue
        {
            get
            {
                return _progressBarValue;
            }
            set
            {
                _progressBarValue = value;
                OnPropertyChanged(nameof(ProgressBarValue));
            }
        }

        [DataMember]
        public int ProgressBarMaxValue
        {
            get
            {
                if (string.IsNullOrEmpty(PlayTime)) return 1;
                return _progressBarMaxValue;
            }
            set
            {
                _progressBarMaxValue = value;
                OnPropertyChanged(nameof(ProgressBarMaxValue));
            }
        }

        public ICommand EnterCommand { get; set; }
        public ICommand ChangeCommand { get; set; }
        public ICommand StopCommand { get; set; }
        public ICommand SetVipStatusCommand { get; set; }
        public ICommand AddMinuteCommand { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
