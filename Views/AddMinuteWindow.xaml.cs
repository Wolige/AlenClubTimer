using AlenClubTimer.Models;
using AlenClubTimer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AlenClubTimer.Views
{
    /// <summary>
    /// Interaction logic for AddMinuteWindow.xaml
    /// </summary>
    public partial class AddMinuteWindow : Window
    {
        public AddMinuteWindow(UserModel userModel)
        {
            InitializeComponent();
            DataContext = new AddMinuteViewModel(userModel);
        }

        private void SetTimeTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]").IsMatch(e.Text);
            if (!((TextBox)sender).IsFocused && ((TextBox)sender).Text == string.Empty)
            {
                ((TextBox)sender).Text = "0";
            }
        }
    }
}
