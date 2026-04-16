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
            LoadFilters();
            LoadData();
        }

        private async void LoadFilters()
        {
            try
            {
                // Units
                var dtUnits = await _db.ExecuteQueryAsync("SELECT UnitID, UnitName FROM Units");
                DataRow drUnit = dtUnits.NewRow();
                drUnit["UnitID"] = -1;
                drUnit["UnitName"] = "Tất cả đơn vị";
                dtUnits.Rows.InsertAt(drUnit, 0);
                cboUnitFilter.ItemsSource = dtUnits.DefaultView;
                cboUnitFilter.SelectedIndex = 0;

                // Categories
                var dtCats = await _db.ExecuteQueryAsync("SELECT CategoryID, CategoryName FROM RewardCategories");
                DataRow drCat = dtCats.NewRow();
                drCat["CategoryID"] = -1;
                drCat["CategoryName"] = "Tất cả danh hiệu";
                dtCats.Rows.InsertAt(drCat, 0);
                cboCategoryFilter.ItemsSource = dtCats.DefaultView;
                cboCategoryFilter.SelectedIndex = 0;
            }
            catch { }
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            if (txtSearchPlaceholder != null)
                txtSearchPlaceholder.Visibility = string.IsNullOrEmpty(txtSearch.Text) ? Visibility.Visible : Visibility.Collapsed;

            LoadData();
        }

        private async void LoadData()
        {
            if (_db == null) return;

            try
            {
                string query = @"
                    SELECT s.SoldierID, s.FullName, s.Rank, s.Position, u.UnitName, c.CategoryName, p.DateProposed
                    FROM Proposals p
                    JOIN Soldiers s ON p.SoldierID = s.SoldierID
                    LEFT JOIN Units u ON s.UnitID = u.UnitID
                    LEFT JOIN RewardCategories c ON p.CategoryID = c.CategoryID
                    WHERE p.Status = 'Đã phê duyệt'";

                var parameters = new System.Collections.Generic.List<MySqlConnector.MySqlParameter>();

                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    query += " AND (s.FullName LIKE @Search OR s.SoldierCode LIKE @Search)";
                    parameters.Add(new MySqlConnector.MySqlParameter("@Search", $"%{txtSearch.Text.Trim()}%"));
                }

                if (cboUnitFilter.SelectedValue != null && (int)cboUnitFilter.SelectedValue != -1)
                {
                    query += " AND s.UnitID = @UnitID";
                    parameters.Add(new MySqlConnector.MySqlParameter("@UnitID", cboUnitFilter.SelectedValue));
                }

                if (cboCategoryFilter.SelectedValue != null && (int)cboCategoryFilter.SelectedValue != -1)
                {
                    query += " AND p.CategoryID = @CatID";
                    parameters.Add(new MySqlConnector.MySqlParameter("@CatID", cboCategoryFilter.SelectedValue));
                }

                query += " ORDER BY p.DateProposed DESC";

                DataTable dt = await _db.ExecuteQueryAsync(query, parameters.ToArray());
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
                        Position = row["Position"]?.ToString() ?? "",
                        Unit = row["UnitName"]?.ToString() ?? "",
                        CategoryName = row["CategoryName"]?.ToString() ?? "N/A",
                        Date = Convert.ToDateTime(row["DateProposed"]).ToString("dd/MM/yyyy")
                    });
                }

                txtPaginationSummary.Text = $"Hiển thị {Rewards.Count} kết quả";
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
        public string Position { get; set; }
        public string Unit { get; set; }
        public string CategoryName { get; set; }
        public string Date { get; set; }
    }
}
