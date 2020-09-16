using AlenClubTimer.Models;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AlenClubTimer.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged 
    {
        private static int nextId = 1;
        private static int port = 8080;
        private bool _isHomePageVisible;
        private bool _isHistoryPageVisible;
        private bool _isChangeCurrencyVisible;
        private static Dictionary<int, TcpClient> clientlist;
        TcpListener server;

        public MainPageViewModel()
        {
            Version = "1.5.2";
            File.WriteAllText(@"./AppVersion.txt", Version);
            Title = "AlenClubTimer " + Version;
            CheckUpdates();
            IsHomePageVisible = true;
            HistoryUsers = new ObservableCollection<UserModel>();
            Users = new ObservableCollection<UserModel>();
            TimeHistoryUsers = JsonUser.GetWorkingTimeHistoryUsers();
            UpdatePageCommand = new RelayCommand<object>(OnUpdatePageCommand);
            clientlist = new Dictionary<int, TcpClient>();
            server = new TcpListener(IPAddress.Any, port);
            Task thread = new Task(ServerTuning);
            thread.Start();
            Task ConnectionThread = new Task(ConnectionCheck);
            ConnectionThread.Start();
        }
        
        private void CheckUpdates()
        {
            WebClient webClient = new WebClient();

            webClient.DownloadFile("https://alenclubtimer.000webhostapp.com/Update/ClubTimerVersion.txt", "ClubTimerVersion.txt");
            string version = File.ReadAllText("ClubTimerVersion.txt");
            if (!Version.Contains(version))
            {
                if (MessageBox.Show("Хотите скачать обновление?", "ClubTimer", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) 
                {
                    Version = version;
                    Process.Start("UpdateProject");
                    Environment.Exit(0);
                }
            }
        }

        internal static void OnClosingEvent(object sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        public string Title { get; set; }

        public string Version { get; set; }

        public bool IsHomePageVisible
        { 
            get
            {
                return _isHomePageVisible;
            }
            set
            {
                _isHomePageVisible = value;
                NotifyPropertyChanged(nameof(IsHomePageVisible));
            }
        }
        public bool IsHistoryPageVisible
        { 
            get
            {
                return _isHistoryPageVisible;
            }
            set
            {
                _isHistoryPageVisible = value;
                NotifyPropertyChanged(nameof(IsHistoryPageVisible));
            }
        }
        public bool IsChangeCurrencyVisible
        { 
            get
            {
                return _isChangeCurrencyVisible;
            }
            set
            {
                _isChangeCurrencyVisible = value;
                NotifyPropertyChanged(nameof(IsChangeCurrencyVisible));
            }
        }

        public string ClientMessage { get; set; }
        public double TotalValue { get; set; }

        public static ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<UserModel> HistoryUsers { get; set; }
        public ObservableCollection<UserModel> TimeHistoryUsers { get; set; }
        private void ServerTuning()
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            while (true)
            {
                try
                {
                    TcpClient client = server.AcceptTcpClient();
                    try
                    {
                        var buffer = new byte[256];
                        var givenMessage = new StringBuilder();
                        do
                        {
                            int size = client.Client.Receive(buffer);
                            givenMessage.Append(Encoding.UTF8.GetString(buffer, 0, size));
                        }
                        while (client.Client.Available > 0);
                        App.Current.Dispatcher.Invoke((System.Action)delegate
                        {
                            ClientMessage = givenMessage.ToString();
                        });
                        var user = Users.FirstOrDefault(x => x.Name == ClientMessage);
                        if (user == null)
                        {
                            int number = Connect(ClientMessage);
                            clientlist.Add(number, client);
                            if (Users[Users.Count - 1].TimeLeft != null && Users[Users.Count - 1].TimeLeft != "VIP" )
                            {
                                SendMsg(Users[Users.Count - 1].TimeLeft, Users[Users.Count - 1].ID);
                                Users[Users.Count - 1].TimeMonitoring();
                            }
                            else if (Users[Users.Count - 1].TimeLeft == "VIP")
                            {
                                Users[Users.Count - 1].VipTimeMonitoring();
                                SendMsg("false", Users[Users.Count - 1].ID);
                            }
                            else
                            {
                                Users[Users.Count - 1].OnStopCommand();
                            }
                        }
                        else
                        {
                            clientlist[user.ID] = client;
                        }
                        Thread.Sleep(300);
                    }
                    catch (Exception)
                    {
                        server.Stop();
                        client.Close();
                        server.Server.Close();
                        continue;
                    }
                }
                catch (Exception)
                {
                    Thread.Sleep(1500);
                    continue;
                }
            }
        }

        private void ConnectionCheck()
        {
            while (true)
            {
                Dictionary<int, TcpClient> _clients = clientlist;
                List<UserModel> _users = Users.ToList();
                if (_users.Count != 0 && clientlist != null)
                {
                    try
                    {
                        foreach (var item in _clients)
                        {
                            if (IsClientConnected(item.Value))
                            {
                                continue;
                            }
                            else
                            {
                                clientlist.Remove(item.Key);
                                var user = Users.SingleOrDefault(x => x.ID == item.Key);
                                if (user != null)
                                {
                                    App.Current.Dispatcher.Invoke((System.Action)delegate
                                    {
                                        Users.Remove(user);
                                    });
                                }
                                _clients = clientlist;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    
                    Thread.Sleep(50);
                }
                Thread.Sleep(200);
            }
        }

        private void OnUpdatePageCommand(object obj)
        {
            if (obj.ToString() == "Home")
            {
                IsHomePageVisible = true;
                IsHistoryPageVisible = false;
                IsChangeCurrencyVisible = false;
            }
            else if (obj.ToString() == "History")
            {
                HistoryUsers = new ObservableCollection<UserModel>();
                TotalValue = 0;
                ObservableCollection<UserModel> users = new ObservableCollection<UserModel>();
                IsHistoryPageVisible = true;
                IsHomePageVisible = false;
                IsChangeCurrencyVisible = false;
                ObservableCollection<UserModel> historyUsers = Task.Run(() => JsonUser.GetHistoryUsers()).Result;
                if (HistoryUsers == historyUsers) return;
                foreach (var item in historyUsers)
                {
                    HistoryUsers.Add(item);
                }
                if (HistoryUsers.Any(x => x.EndTime.Day != DateTime.Now.Day))
                {
                    var removeUsers = HistoryUsers.Where(x => x.EndTime.Day != DateTime.Now.Day).ToArray();
                    foreach (var item in removeUsers)
                    {
                        HistoryUsers.Remove(item);
                        NotifyPropertyChanged(nameof(HistoryUsers));
                    }
                    JsonUser.AddCollectionToHistory(HistoryUsers);
                }
                foreach (var item in HistoryUsers)
                {
                    TotalValue += item.Cash;
                }
                NotifyPropertyChanged(nameof(HistoryUsers));
                NotifyPropertyChanged(nameof(TotalValue));
            }
            else if(obj.ToString() == "ChangeCurrency")
            {
                IsChangeCurrencyVisible = true;
                IsHistoryPageVisible = false;
                IsHomePageVisible = false;
            }
        }

        public static void SendMsg(string msg, int ID)
        {
            int number = 0;
            while (true)
            {
                try
                {
                    number++;
                    TcpClient client = clientlist[ID];
                    byte[] message = Encoding.UTF8.GetBytes(msg);
                    client.Client.Send(message);
                    byte[] data = new byte[256];
                    StringBuilder response = new StringBuilder();
                    client.Client.ReceiveTimeout = 500;
                    do
                    {
                        int bytes = client.Client.Receive(data);
                        response.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    } 
                    while (client.Client.Available > 0);
                    if (response.ToString() == msg) return;
                    Thread.Sleep(350);
                    continue;
                }
                catch (Exception)
                {
                    if (number == 15) return;
                    continue;
                }
            }
        }

        public ICommand AddCommand { get; set; }
        public ICommand UpdatePageCommand { get; set; }

        public int Connect(string name)
        {
            UserModel user = new UserModel()
            {
                ID = nextId,
                Name = name,
            };
            nextId++;

            App.Current.Dispatcher.Invoke((System.Action)delegate
            {
                var oldUser = TimeHistoryUsers.SingleOrDefault(x => x.Name == name);
                if (oldUser == null)
                {
                    Users.Add(user);
                }
                else
                {
                    oldUser.ID = user.ID;
                    Users.Add(oldUser);
                    NotifyPropertyChanged(nameof(Users));
                }
            });
            return user.ID;
        }

        public void Disconnect(int id)
        {
            var user = Users.FirstOrDefault(x => x.ID == id);
            if (user != null)
            {
                App.Current.Dispatcher.Invoke((System.Action)delegate
                {
                    Users.Remove(user);
                    HistoryUsers.Add(user);
                    NotifyPropertyChanged(nameof(HistoryUsers));
                });
            }
        }

        public static bool IsClientConnected(TcpClient client)
        {
            try
            {
                if (client != null && client.Client != null && client.Client.Connected)
                {
                    if (client.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];
                        if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
