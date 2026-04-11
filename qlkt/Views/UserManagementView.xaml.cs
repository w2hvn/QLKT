using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using QLKT.Data;

namespace QLKT.Views
{
    public partial class UserManagementView : UserControl
    {
        private readonly DatabaseContext _db;

        public UserManagementView()
        {
            InitializeComponent();
            _db = new DatabaseContext();
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                string query = "SELECT UserID, Username, FullName, Role, Status FROM Users";
                DataTable dt = await _db.ExecuteQueryAsync(query);
                var users = new List<UserItem>();

                foreach (DataRow row in dt.Rows)
                {
                    users.Add(new UserItem
                    {
                        ID = row["UserID"].ToString(),
                        Username = row["Username"].ToString(),
                        FullName = row["FullName"].ToString(),
                        Role = row["Role"].ToString(),
                        Status = row["Status"].ToString()
                    });
                }

                dgUsers.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách người dùng: " + ex.Message);
            }
        }
    }

    public class UserItem
    {
        public string ID { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
    }
}
