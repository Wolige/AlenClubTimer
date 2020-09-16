using AlenClubTimer.Models;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AlenClubTimer.ViewModels
{
    public class AddMinuteViewModel
    {
        private UserModel userModel;
        public AddMinuteViewModel(UserModel _userModel)
        {
            userModel = _userModel;
            AcceptCommand = new RelayCommand(OnAcceptCommand);
        }

        private void OnAcceptCommand()
        {
            userModel.PlayHours += PlayHours;
            userModel.PlayMinute += PlayMinute;
            userModel.EndTime.AddMinutes(PlayMinute);
            userModel.IsSetTimeReadOnly = false;
            userModel.OnEnterCommand();
            userModel.CloseAddMinuteWindow();
            PlayMinute = 0;
            PlayHours = 0;
        }
        public ICommand AcceptCommand { get; set; }
        public int PlayMinute { get; set; }
        public int PlayHours { get; set; }
    }
}
