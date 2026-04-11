using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace QLKT.Views
{
    public partial class ApprovalProcessView : UserControl
    {
        public ApprovalProcessView()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var pending = new List<PendingItem>
            {
                new PendingItem { ID = "DX-01", Name = "Nguyễn Văn A", Unit = "Tiểu đoàn 1", Date = "15/10/2023" },
                new PendingItem { ID = "DX-02", Name = "Lê Hoàng Tú", Unit = "Phòng Tham mưu", Date = "14/10/2023" },
                new PendingItem { ID = "DX-03", Name = "Trần Thị Lan", Unit = "Bệnh xá", Date = "12/10/2023" }
            };
            dgPending.ItemsSource = pending;
        }

        private void dgPending_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPending.SelectedItem is PendingItem item)
            {
                txtEmptyState.Visibility = Visibility.Collapsed;
                pnlDetails.Visibility = Visibility.Visible;
                txtSubject.Text = $"ĐX: Khen thưởng cho {item.Name} ({item.Unit})";
            }
            else
            {
                txtEmptyState.Visibility = Visibility.Visible;
                pnlDetails.Visibility = Visibility.Hidden;
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Cập nhật trạng thái thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class PendingItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Date { get; set; }
    }
}
