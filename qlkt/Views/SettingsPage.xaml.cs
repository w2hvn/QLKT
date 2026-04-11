using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MilitaryRewardApp.Data;
using MySqlConnector;

namespace MilitaryRewardApp.Views
{
    public partial class SettingsPage : UserControl
    {
        private readonly DatabaseContext _db;
        private int? _selectedUserId = null;

        public SettingsPage()
        {
            InitializeComponent();
            _db = new DatabaseContext();
            LoadUsers();
        }

        private async void LoadUsers()
        {
            try
            {
                var dt = await _db.ExecuteQueryAsync("SELECT * FROM Users");
                dgUsers.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải user: " + ex.Message); }
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsers.SelectedItem is DataRowView row)
            {
                _selectedUserId = Convert.ToInt32(row["UserID"]);
                txtUsername.Text = row["Username"].ToString();
                txtPassword.Password = row["Password"].ToString();
                txtFullName.Text = row["FullName"].ToString();
                cboRole.Text = row["Role"].ToString();
            }
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Vui lòng nhập user/pass.");
                return;
            }

            try
            {
                string role = (cboRole.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "User";
                string query = "INSERT INTO Users (Username, Password, FullName, Role) VALUES (@U, @P, @F, @R)";

                var param = new MySqlParameter[]
                {
                    new MySqlParameter("@U", txtUsername.Text),
                    new MySqlParameter("@P", txtPassword.Password),
                    new MySqlParameter("@F", txtFullName.Text),
                    new MySqlParameter("@R", role)
                };

                await _db.ExecuteNonQueryAsync(query, param);
                LoadUsers();
                ClearForm();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi thêm: " + ex.Message); }
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUserId == null) return;
            try
            {
                string role = (cboRole.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "User";
                string query = "UPDATE Users SET Username=@U, Password=@P, FullName=@F, Role=@R WHERE UserID=@Id";

                var param = new MySqlParameter[]
                {
                    new MySqlParameter("@U", txtUsername.Text),
                    new MySqlParameter("@P", txtPassword.Password),
                    new MySqlParameter("@F", txtFullName.Text),
                    new MySqlParameter("@R", role),
                    new MySqlParameter("@Id", _selectedUserId)
                };

                await _db.ExecuteNonQueryAsync(query, param);
                LoadUsers();
                ClearForm();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi sửa: " + ex.Message); }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUserId == null) return;
            if (MessageBox.Show("Xóa user này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    await _db.ExecuteNonQueryAsync($"DELETE FROM Users WHERE UserID={_selectedUserId}");
                    LoadUsers();
                    ClearForm();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi xóa: " + ex.Message); }
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _selectedUserId = null;
            txtUsername.Text = "";
            txtPassword.Password = "";
            txtFullName.Text = "";
            cboRole.SelectedIndex = -1;
        }
    }
}
