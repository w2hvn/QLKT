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

        private int _selectedUserId = -1;

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

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsers.SelectedItem is UserItem user)
            {
                _selectedUserId = int.Parse(user.ID);
                txtFullName.Text = user.FullName;
                txtUsername.Text = user.Username;
                txtPassword.Password = ""; // For security, don't show password

                cboRole.Text = user.Role;
                cboStatus.Text = user.Status;

                txtFormTitle.Text = "CẬP NHẬT NGƯỜI DÙNG";
                btnDelete.Visibility = Visibility.Visible;
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.");
                return;
            }

            try
            {
                string role = (cboRole.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Người dùng";
                string status = (cboStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Hoạt động";

                if (_selectedUserId == -1)
                {
                    // Add new
                    string query = "INSERT INTO Users (Username, Password, FullName, Role, Status) VALUES (@Username, @Password, @FullName, @Role, @Status)";
                    var parameters = new MySqlConnector.MySqlParameter[]
                    {
                        new MySqlConnector.MySqlParameter("@Username", txtUsername.Text.Trim()),
                        new MySqlConnector.MySqlParameter("@Password", txtPassword.Password), // In production, hash this!
                        new MySqlConnector.MySqlParameter("@FullName", txtFullName.Text.Trim()),
                        new MySqlConnector.MySqlParameter("@Role", role),
                        new MySqlConnector.MySqlParameter("@Status", status)
                    };
                    await _db.ExecuteNonQueryAsync(query, parameters);
                    MessageBox.Show("Thêm người dùng thành công.");
                }
                else
                {
                    // Update
                    string query = "UPDATE Users SET Username=@Username, FullName=@FullName, Role=@Role, Status=@Status";
                    var parametersList = new List<MySqlConnector.MySqlParameter>
                    {
                        new MySqlConnector.MySqlParameter("@Username", txtUsername.Text.Trim()),
                        new MySqlConnector.MySqlParameter("@FullName", txtFullName.Text.Trim()),
                        new MySqlConnector.MySqlParameter("@Role", role),
                        new MySqlConnector.MySqlParameter("@Status", status),
                        new MySqlConnector.MySqlParameter("@Id", _selectedUserId)
                    };

                    if (!string.IsNullOrEmpty(txtPassword.Password))
                    {
                        query += ", Password=@Password";
                        parametersList.Add(new MySqlConnector.MySqlParameter("@Password", txtPassword.Password));
                    }

                    query += " WHERE UserID=@Id";
                    await _db.ExecuteNonQueryAsync(query, parametersList.ToArray());
                    MessageBox.Show("Cập nhật người dùng thành công.");
                }

                ResetForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu: " + ex.Message);
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUserId == -1) return;

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa người dùng này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    string query = "DELETE FROM Users WHERE UserID = @Id";
                    var parameters = new MySqlConnector.MySqlParameter[]
                    {
                        new MySqlConnector.MySqlParameter("@Id", _selectedUserId)
                    };
                    await _db.ExecuteNonQueryAsync(query, parameters);
                    MessageBox.Show("Đã xóa người dùng thành công.");
                    ResetForm();
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa: " + ex.Message);
                }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        private void ResetForm()
        {
            _selectedUserId = -1;
            txtFullName.Text = "";
            txtUsername.Text = "";
            txtPassword.Password = "";
            cboRole.SelectedIndex = -1;
            cboStatus.SelectedIndex = -1;
            txtFormTitle.Text = "THÊM NGƯỜI DÙNG MỚI";
            btnDelete.Visibility = Visibility.Collapsed;
            dgUsers.SelectedItem = null;
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
