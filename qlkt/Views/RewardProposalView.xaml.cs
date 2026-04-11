using System.Windows;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace QLKT.Views
{
    public partial class RewardProposalView : UserControl
    {
        public event EventHandler OnCreateNewProposalRequested;

        public RewardProposalView()
        {
            InitializeComponent();
            LoadData();
        }

        private void BtnCreateNew_Click(object sender, RoutedEventArgs e)
        {
            OnCreateNewProposalRequested?.Invoke(this, EventArgs.Empty);
        }

        private void LoadData()
        {
            var proposals = new List<ProposalItem>
            {
                new ProposalItem { Id = "DX-2023-01", FullName = "Nguyễn Văn A", Unit = "Tiểu đoàn 1, Trung đoàn 2", RewardType = "Bằng khen Bộ Quốc phòng", Date = "15/10/2023", Status = "Chờ phê duyệt", StatusCode = 0 },
                new ProposalItem { Id = "DX-2023-02", FullName = "Trần Thị B", Unit = "Phòng Tham mưu", RewardType = "Chiến sĩ thi đua", Date = "14/10/2023", Status = "Đã phê duyệt", StatusCode = 1 },
                new ProposalItem { Id = "DX-2023-03", FullName = "Lê Văn C", Unit = "Đại đội 3, Tiểu đoàn 1", RewardType = "Giấy khen Lữ đoàn", Date = "12/10/2023", Status = "Đã từ chối", StatusCode = 2 },
                new ProposalItem { Id = "DX-2023-04", FullName = "Phạm Văn D", Unit = "Tiểu đoàn 2, Trung đoàn 2", RewardType = "Huân chương Chiến công", Date = "10/10/2023", Status = "Chờ phê duyệt", StatusCode = 0 },
                new ProposalItem { Id = "DX-2023-05", FullName = "Hoàng Thị E", Unit = "Bệnh xá", RewardType = "Bằng khen Quân khu", Date = "08/10/2023", Status = "Đã phê duyệt", StatusCode = 1 },
                new ProposalItem { Id = "DX-2023-06", FullName = "Vũ Văn F", Unit = "Tiểu đoàn 1, Trung đoàn 2", RewardType = "Chiến sĩ thi đua", Date = "05/10/2023", Status = "Đã phê duyệt", StatusCode = 1 },
                new ProposalItem { Id = "DX-2023-07", FullName = "Đặng Văn G", Unit = "Đại đội 1, Tiểu đoàn 1", RewardType = "Giấy khen Trung đoàn", Date = "01/10/2023", Status = "Chờ phê duyệt", StatusCode = 0 },
            };

            ProposalsDataGrid.ItemsSource = proposals;
        }
    }

    public class ProposalItem
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Unit { get; set; }
        public string RewardType { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        public int StatusCode { get; set; }

        public SolidColorBrush StatusBackground
        {
            get
            {
                if (StatusCode == 0) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0")); // Orange light
                if (StatusCode == 1) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9")); // Green light
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE")); // Red light
            }
        }

        public SolidColorBrush StatusForeground
        {
            get
            {
                if (StatusCode == 0) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E65100")); // Orange
                if (StatusCode == 1) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32")); // Green
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828")); // Red
            }
        }
    }
}
