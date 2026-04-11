using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QLKT.Data;

namespace QLKT.Views
{
    public partial class RewardListView : UserControl
    {
        public ObservableCollection<RewardItem> Rewards { get; set; }
        private readonly DatabaseContext _db;
        public event EventHandler<int> OnSoldierSelected;

        public RewardListView()
        {
            InitializeComponent();
            _db = new DatabaseContext();
            Rewards = new ObservableCollection<RewardItem>();
            dgRewardList.ItemsSource = Rewards;
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                string query = @"
                    SELECT s.SoldierID, s.FullName, s.Rank, u.UnitName, p.Status
                    FROM Proposals p
                    JOIN Soldiers s ON p.SoldierID = s.SoldierID
                    LEFT JOIN Units u ON s.UnitID = u.UnitID
                    WHERE p.Status = 'Đã phê duyệt'";

                DataTable dt = await _db.ExecuteQueryAsync(query);
                Rewards.Clear();
                int stt = 1;

                foreach (DataRow row in dt.Rows)
                {
                    Rewards.Add(new RewardItem
                    {
                        STT = stt++,
                        SoldierID = Convert.ToInt32(row["SoldierID"]),
                        Name = row["FullName"].ToString(),
                        Rank = row["Rank"].ToString(),
                        Unit = row["UnitName"]?.ToString() ?? "",
                        Status = row["Status"].ToString(),
                        StatusBackground = "#C6A87C",
                        StatusForeground = "White"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách khen thưởng: " + ex.Message);
            }
        }

        private void dgRewardList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgRewardList.SelectedItem is RewardItem item)
            {
                OnSoldierSelected?.Invoke(this, item.SoldierID);
            }
        }
    }

    public class RewardItem
    {
        public int STT { get; set; }
        public int SoldierID { get; set; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public string Unit { get; set; }
        public string Status { get; set; }
        public string StatusBackground { get; set; }
        public string StatusForeground { get; set; }
    }
}
