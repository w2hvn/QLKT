using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using QLKT.Data;

namespace QLKT.Views
{
    public partial class SoldierProfileView : UserControl
    {
        private readonly DatabaseContext _db;
        public event EventHandler<int> OnCreateProposalRequested;
        private int _currentSoldierId;

        public SoldierProfileView()
        {
            InitializeComponent();
            _db = new DatabaseContext();
        }

        public async void LoadProfile(int soldierId)
        {
            _currentSoldierId = soldierId;
            try
            {
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
                    txtRank.Text = row["Rank"].ToString();
                    txtRank2.Text = row["Rank"].ToString();
                    txtSoldierCode.Text = row["SoldierCode"].ToString();
                    txtEthnicity.Text = row["Ethnicity"].ToString();
                    txtReligion.Text = row["Religion"].ToString();
                    txtUnit.Text = row["UnitName"].ToString();
                    txtPosition.Text = row["Position"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải hồ sơ: " + ex.Message);
            }
        }

        private void BtnCreateProposal_Click(object sender, RoutedEventArgs e)
        {
            OnCreateProposalRequested?.Invoke(this, _currentSoldierId);
        }
    }
}
