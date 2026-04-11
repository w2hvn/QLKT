using System;
using System.Data;
using System.Windows;
using MilitaryRewardApp.Data;

namespace MilitaryRewardApp.Views
{
    public partial class LoginWindow : Window
    {
        private readonly DatabaseContext _db;

        public LoginWindow()
        {
            InitializeComponent();
            _db = new DatabaseContext();
            txtPassword.Password = "admin123"; // Auto-fill for demo
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblError.Text = "Vui lòng nhập đầy đủ thông tin.";
                lblError.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                // In production, use Hashing. Plaintext for this demo as requested.
                string query = "SELECT * FROM Users WHERE Username = @U AND Password = @P";
                var parameters = new MySqlConnector.MySqlParameter[]
                {
                    new MySqlConnector.MySqlParameter("@U", username),
                    new MySqlConnector.MySqlParameter("@P", password)
                };

                DataTable dt = await _db.ExecuteQueryAsync(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    App.CurrentUserRole = row["Role"].ToString();
                    App.CurrentUserName = row["FullName"].ToString();

                    MainWindow main = new MainWindow();
                    main.Show();
                    this.Close();
                }
                else
                {
                    lblError.Text = "Sai tên đăng nhập hoặc mật khẩu.";
                    lblError.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Lỗi kết nối: " + ex.Message;
                lblError.Visibility = Visibility.Visible;
            }
        }
    }
}
