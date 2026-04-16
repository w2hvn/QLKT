using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using QLKT.Data;

namespace QLKT.Views
{
    public partial class SoldierProfileView : UserControl
    {
        private readonly DatabaseContext _db;
        public event EventHandler<int> OnCreateProposalRequested;
        private int _currentSoldierId = 0;
        private bool _isInternalChange = false;

        public SoldierProfileView()
        {
            InitializeComponent();
            _db = new DatabaseContext();
            LoadFilters();
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
                cboUnit.ItemsSource = dtUnits.DefaultView;
                cboUnit.SelectedIndex = 0;

                // Positions
                var dtPos = await _db.ExecuteQueryAsync("SELECT DISTINCT Position FROM Soldiers WHERE Position IS NOT NULL AND Position <> ''");
                var positions = new List<string> { "Tất cả chức vụ" };
                foreach (DataRow row in dtPos.Rows) positions.Add(row[0].ToString());
                cboPosition.ItemsSource = positions;
                cboPosition.SelectedIndex = 0;

                // Ranks
                var dtRanks = await _db.ExecuteQueryAsync("SELECT DISTINCT `Rank` FROM Soldiers WHERE `Rank` IS NOT NULL AND `Rank` <> ''");
                var ranks = new List<string> { "Tất cả quân hàm" };
                foreach (DataRow row in dtRanks.Rows) ranks.Add(row[0].ToString());
                cboRank.ItemsSource = ranks;
                cboRank.SelectedIndex = 0;
            }
            catch { }
        }

        public async void LoadProfile(int soldierId)
        {
            if (soldierId <= 0) return;
            _currentSoldierId = soldierId;
            btnCreateProposal.IsEnabled = true;

            try
            {
                // 1. Basic Info
                string query = @"
                    SELECT s.*, u.UnitName
                    FROM Soldiers s
                    LEFT JOIN Units u ON s.UnitID = u.UnitID
                    WHERE s.SoldierID = @Id";

                var param = new MySqlConnector.MySqlParameter[]
                {
                    new MySqlConnector.MySqlParameter("@Id", soldierId)
                };

                DataTable dt = await _db.ExecuteQueryAsync(query, param);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    txtFullName.Text = row["FullName"].ToString();
                    txtRankDisplay.Text = row["Rank"].ToString();
                    txtRankDetail.Text = row["Rank"].ToString();
                    txtSoldierCode.Text = row["SoldierCode"].ToString();
                    txtEthnicity.Text = row["Ethnicity"].ToString();
                    txtReligion.Text = row["Religion"].ToString();
                    txtUnitDetail.Text = row["UnitName"]?.ToString() ?? "N/A";
                    txtPosition.Text = row["Position"]?.ToString() ?? "N/A";
                }

                // 2. Timeline & Achievements
                string timelineQ = @"
                    SELECT p.DateProposed, c.CategoryName, p.Reason
                    FROM Proposals p
                    JOIN RewardCategories c ON p.CategoryID = c.CategoryID
                    WHERE p.SoldierID = @Id AND p.Status = 'Đã phê duyệt'
                    ORDER BY p.DateProposed DESC";

                DataTable dtTimeline = await _db.ExecuteQueryAsync(timelineQ, param);
                var timelineItems = new List<object>();
                pnlTopAchievements.Children.Clear();

                if (dtTimeline.Rows.Count > 0)
                {
                    foreach (DataRow row in dtTimeline.Rows)
                    {
                        timelineItems.Add(new
                        {
                            Date = Convert.ToDateTime(row["DateProposed"]).ToString("dd/MM/yyyy"),
                            Title = row["CategoryName"].ToString(),
                            Reason = row["Reason"].ToString()
                        });

                        // Add to top achievements summary (first 3)
                        if (pnlTopAchievements.Children.Count < 3)
                        {
                            pnlTopAchievements.Children.Add(new TextBlock
                            {
                                Text = "• " + row["CategoryName"].ToString(),
                                Margin = new Thickness(0, 0, 0, 4),
                                TextWrapping = TextWrapping.Wrap
                            });
                        }
                    }
                    icTimeline.ItemsSource = timelineItems;
                    txtNoTimeline.Visibility = Visibility.Collapsed;
                }
                else
                {
                    icTimeline.ItemsSource = null;
                    txtNoTimeline.Visibility = Visibility.Visible;
                    pnlTopAchievements.Children.Add(new TextBlock
                    {
                        Text = "Chưa có thông tin khen thưởng.",
                        Foreground = System.Windows.Media.Brushes.Gray,
                        FontStyle = FontStyles.Italic
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải hồ sơ: " + ex.Message);
            }
        }

        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInternalChange) return;

            string keyword = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                txtSearchHint.Visibility = Visibility.Visible;
                popResults.IsOpen = false;
                return;
            }

            txtSearchHint.Visibility = Visibility.Collapsed;
            if (keyword.Length >= 2)
            {
                try
                {
                    string query = @"
                        SELECT s.SoldierID, s.FullName, s.SoldierCode, u.UnitName
                        FROM Soldiers s
                        LEFT JOIN Units u ON s.UnitID = u.UnitID
                        WHERE (s.FullName LIKE @K OR s.SoldierCode LIKE @K)";

                    var paramsList = new List<MySqlConnector.MySqlParameter>
                    {
                        new MySqlConnector.MySqlParameter("@K", $"%{keyword}%")
                    };

                    if (cboUnit.SelectedValue != null && (int)cboUnit.SelectedValue != -1)
                    {
                        query += " AND s.UnitID = @UnitID";
                        paramsList.Add(new MySqlConnector.MySqlParameter("@UnitID", cboUnit.SelectedValue));
                    }

                    if (cboPosition.SelectedItem != null && cboPosition.SelectedItem.ToString() != "Tất cả chức vụ")
                    {
                        query += " AND s.Position = @Pos";
                        paramsList.Add(new MySqlConnector.MySqlParameter("@Pos", cboPosition.SelectedItem.ToString()));
                    }

                    if (cboRank.SelectedItem != null && cboRank.SelectedItem.ToString() != "Tất cả quân hàm")
                    {
                        query += " AND s.Rank = @Rank";
                        paramsList.Add(new MySqlConnector.MySqlParameter("@Rank", cboRank.SelectedItem.ToString()));
                    }

                    query += " LIMIT 10";

                    DataTable dt = await _db.ExecuteQueryAsync(query, paramsList.ToArray());
                    var results = new List<object>();
                    foreach (DataRow row in dt.Rows)
                    {
                        results.Add(new
                        {
                            SoldierID = Convert.ToInt32(row["SoldierID"]),
                            FullName = row["FullName"].ToString(),
                            SoldierCode = row["SoldierCode"].ToString(),
                            UnitName = row["UnitName"]?.ToString() ?? ""
                        });
                    }

                    if (results.Count > 0)
                    {
                        lstResults.ItemsSource = results;
                        popResults.IsOpen = true;
                    }
                    else popResults.IsOpen = false;
                }
                catch { }
            }
            else popResults.IsOpen = false;
        }

        private void lstResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstResults.SelectedItem != null)
            {
                dynamic selected = lstResults.SelectedItem;
                _isInternalChange = true;
                txtSearch.Text = selected.FullName;
                _isInternalChange = false;
                popResults.IsOpen = false;
                LoadProfile(selected.SoldierID);
            }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            // Trigger search again with new filters
            txtSearch_TextChanged(null, null);
        }

        private void BtnCreateProposal_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSoldierId > 0)
                OnCreateProposalRequested?.Invoke(this, _currentSoldierId);
        }
    }
}
