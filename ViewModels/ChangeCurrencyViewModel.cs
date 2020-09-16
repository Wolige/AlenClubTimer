using AlenClubTimer.Models;
using GalaSoft.MvvmLight.Command;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace AlenClubTimer.ViewModels
{
    public class ChangeCurrencyViewModel : INotifyPropertyChanged
    {
        private int _selectedMinute;
        private double _writtenValue;

        public ChangeCurrencyViewModel()
        {
            WrittenValue = UserModel.CashForHour;
            SelectedMinute = 60;
            GetWrittenValue();
            CashCommand = new RelayCommand(OnCashCommand);
        }

        private void GetWrittenValue()
        {
            if (File.Exists("CashForHour.txt"))
            {
                string text = File.ReadAllText("CashForHour.txt");
                if (text != null)
                {
                    WrittenValue = int.Parse(text);
                    UserModel.CashForHour = WrittenValue;
                }
            }
        }

        private void OnCashCommand()
        {
            if (WrittenValue < 50) WrittenValue = 50;
            if (SelectedMinute == 30)
            {
                double number = WrittenValue * 2;
                UserModel.CashForHour = number;
                LabelContent = $"За час поставлено: {number}";
                LabelForeground = Brushes.LightGreen;
                NotifyPropertyChanged(nameof(LabelContent));
                NotifyPropertyChanged(nameof(LabelForeground));
                using (StreamWriter streamWriter = new StreamWriter("CashForHour.txt"))
                {
                    streamWriter.Write(UserModel.CashForHour);
                }
                return;
            }
            LabelContent = $"За час поставлено: {WrittenValue}";
            UserModel.CashForHour = WrittenValue;
            LabelForeground = Brushes.LightGreen;
            using (StreamWriter streamWriter = new StreamWriter("CashForHour.txt"))
            {
                streamWriter.Write(UserModel.CashForHour);
            }
            NotifyPropertyChanged(nameof(LabelContent));
            NotifyPropertyChanged(nameof(LabelForeground));
        }

        public int SelectedMinute
        { 
            get
            {
                return _selectedMinute;
            }
            set
            {
                if (_selectedMinute != value)
                {
                    _selectedMinute = value;
                }
                NotifyPropertyChanged(nameof(SelectedMinute));
            }
        }

        public double WrittenValue
        {
            get
            {
                return _writtenValue;
            }
            set
            {
                if (_writtenValue != value)
                {
                    _writtenValue = value;
                }
                NotifyPropertyChanged(nameof(WrittenValue));
            }
        }

        public string LabelContent { get; set; }
        public Brush LabelForeground { get; set; }

        public ICommand CashCommand { get; set; }



        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
