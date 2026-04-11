using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace QLKT.Views
{
    public partial class RewardListView : UserControl
    {
        public ObservableCollection<RewardItem> Rewards { get; set; }

        public RewardListView()
        {
            InitializeComponent();
            LoadData();
            dgRewardList.ItemsSource = Rewards;
        }

        private void LoadData()
        {
            Rewards = new ObservableCollection<RewardItem>
            {
                new RewardItem { STT = 1, Name = "Đặng Văn Tú", Rank = "Trung tá", Unit = "Sư đoàn 301", Status = "Đã phê duyệt", StatusBackground = "#C6A87C", StatusForeground = "White" },
                new RewardItem { STT = 2, Name = "Lê Thị Mai", Rank = "Đại úy", Unit = "Quân đoàn 1", Status = "Đang chờ", StatusBackground = "#001529", StatusForeground = "White" },
                new RewardItem { STT = 3, Name = "Nguyễn Hữu Minh", Rank = "Thượng úy", Unit = "Sư đoàn 324", Status = "Đang đề xuất", StatusBackground = "White", StatusForeground = "#C6A87C" },
                new RewardItem { STT = 4, Name = "Trần Đức Hoài", Rank = "Thiếu tá", Unit = "Sư đoàn 301", Status = "Đã phê duyệt", StatusBackground = "#C6A87C", StatusForeground = "White" },
                new RewardItem { STT = 5, Name = "Lê Thị Mai", Rank = "Đại úy", Unit = "Quân đoàn 1", Status = "Đang chờ", StatusBackground = "#001529", StatusForeground = "White" },
                new RewardItem { STT = 6, Name = "Nguyễn Hữu Tú", Rank = "Thượng úy", Unit = "Sư đoàn 324", Status = "Đã phê duyệt", StatusBackground = "#C6A87C", StatusForeground = "White" }
            };
        }
    }

    public class RewardItem
    {
        public int STT { get; set; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public string Unit { get; set; }
        public string Status { get; set; }
        public string StatusBackground { get; set; }
        public string StatusForeground { get; set; }
    }
}
