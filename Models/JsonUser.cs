using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlenClubTimer.Models
{
    public static class JsonUser
    {
        #region Working with UsersHistory
        /// <summary>
        /// Get users from "UsersInfo.json"
        /// </summary>
        /// <returns> List of Users </returns>
        public static ObservableCollection<UserModel> GetHistoryUsers()
        {
            ObservableCollection<UserModel> users = new ObservableCollection<UserModel>();
            if (File.Exists("UsersHistory.json") && new FileInfo("UsersHistory.json").Length != 0)
            {
                using (var reader = new StreamReader("UsersHistory.json"))
                {
                    while (true)
                    {
                        try
                        {
                            while (true)
                            {
                                users.Add(JsonConvert.DeserializeObject<UserModel>(reader.ReadLine()));
                            }
                        }
                        catch (Exception)
                        {
                            return users;
                        }
                    }
                }
            }
            return users ?? new ObservableCollection<UserModel>();
        }

        /// <summary>
        /// Refresh users
        /// </summary>
        /// <param name="users"> List of our users </param>
        /// <returns>if all is done true</returns>
        public static bool AddUserToHistory(UserModel user)
        {
            using (StreamWriter file = new StreamWriter("UsersHistory.json", true))
            {
                try
                {
                    string json = JsonConvert.SerializeObject(user);
                    file.WriteLine(json);
                    file.Close();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static void AddCollectionToHistory(ObservableCollection<UserModel> users)
        {
            using (StreamWriter file = new StreamWriter("UsersHistory.json"))
            {
                foreach (var item in users)
                {
                    string json = JsonConvert.SerializeObject(item);
                    file.WriteLine(json);
                }
                file.Close();
            }
        }

        public static void SetEmptyHistory()
        {
            using (StreamWriter file = new StreamWriter("UsersHistory.json"))
            {
                file.Write("");
                file.Close();
            }
        }
        #endregion

        public static bool AddUserToWokingTimeHistory(UserModel user)
        {
            ObservableCollection<UserModel> users = GetWorkingTimeHistoryUsers();
            if (users == null || users.Count == 0)
            {
                users.Add(user);
                AddCollectionToTimeHistory(users);
                return true;
            }
            else
            {
                var _user = users.SingleOrDefault(x => x.Name == user.Name);
                if (_user != null)
                {
                    users.Remove(_user);
                    users.Add(user);
                }
                else
                {
                    users.Add(user);
                }
                AddCollectionToTimeHistory(users);
                return true;
            }
        }

        public static void RemoveUserFromWorkingTimeHistory(UserModel user)
        {
            ObservableCollection<UserModel> users = GetWorkingTimeHistoryUsers();
            if (users != null || users.Count > 0)
            {
                var _user = users.SingleOrDefault(x => x.Name == user.Name);
                if (_user != null)
                {
                    users.Remove(_user);
                    AddCollectionToTimeHistory(users);
                }
            }
        }

        public static ObservableCollection<UserModel> GetWorkingTimeHistoryUsers()
        {
            ObservableCollection<UserModel> users = new ObservableCollection<UserModel>();
            if (File.Exists("UsersWokingTimeHistory.json") && new FileInfo("UsersWokingTimeHistory.json").Length != 0)
            {
                using (var reader = new StreamReader("UsersWokingTimeHistory.json"))
                {
                    while (true)
                    {
                        try
                        {
                            while (true)
                            {
                                users.Add(JsonConvert.DeserializeObject<UserModel>(reader.ReadLine()));
                            }
                        }
                        catch (Exception)
                        {
                            return users;
                        }
                    }
                }
            }
            return users ?? new ObservableCollection<UserModel>();
        }

        public static void AddCollectionToTimeHistory(ObservableCollection<UserModel> users)
        {
            using (StreamWriter file = new StreamWriter("UsersWokingTimeHistory.json"))
            {
                foreach (var item in users)
                {
                    string json = JsonConvert.SerializeObject(item);
                    file.WriteLine(json);
                }
                file.Close();
            }
        }
    }
}
