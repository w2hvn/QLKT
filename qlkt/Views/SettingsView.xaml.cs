using System.Windows;
using System.Windows.Controls;

namespace QLKT.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã lưu cài đặt!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
