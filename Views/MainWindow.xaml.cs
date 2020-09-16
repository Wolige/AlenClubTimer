using AlenClubTimer.ViewModels;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.ComponentModel;

namespace AlenClubTimer.Views
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);


        const uint MF_BYCOMMAND = 0x00000000;
        const uint MF_GRAYED = 0x00000001;

        const uint SC_CLOSE = 0xF060;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainPageViewModel();
            Closing += MainPageViewModel.OnClosingEvent;
        }

        private void SetTimeTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]").IsMatch(e.Text);
            if (!((TextBox)sender).IsFocused && ((TextBox)sender).Text == string.Empty)
            {
                ((TextBox)sender).Text = "0";
            }
        }

        private void MoneyPageCommand_Click(object sender, RoutedEventArgs e)
        {
            ChangeCurrencyWindow changeCurrencyWindow = new ChangeCurrencyWindow();
            changeCurrencyWindow.Show();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Disable close button
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            IntPtr hMenu = GetSystemMenu(hwnd, false);
            if (hMenu != IntPtr.Zero)
            {
                EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
            }
        }
    }
}
