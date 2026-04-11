using System.Collections.Generic;
using System.Windows.Controls;

namespace QLKT.Views
{
    public partial class UserManagementView : UserControl
    {
        public UserManagementView()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var users = new List<UserItem>
            {
                new UserItem { ID = "NV001", FullName = "Nguyễn Văn Hùng", Username = "hungnv", Role = "Quản trị viên", Status = "Hoạt động" },
                new UserItem { ID = "NV002", FullName = "Trần Thị Lan", Username = "lantt", Role = "Người dùng", Status = "Hoạt động" },
                new UserItem { ID = "NV003", FullName = "Lê Hoàng Tú", Username = "tulh", Role = "Người dùng", Status = "Khóa" }
            };
            dgUsers.ItemsSource = users;
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
