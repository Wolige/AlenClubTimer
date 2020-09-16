using AlenClubTimer.ViewModels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AlenClubTimer.Views
{
    /// <summary>
    /// Interaction logic for ChangeCurrencyWindow.xaml
    /// </summary>
    public partial class ChangeCurrencyWindow : Window
    {
        public ChangeCurrencyWindow()
        {
            InitializeComponent();
            DataContext = new ChangeCurrencyViewModel();
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
