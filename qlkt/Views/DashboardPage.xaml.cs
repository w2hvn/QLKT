using System;
using System.Windows.Controls;
using QLKT.Data;

namespace QLKT.Views
{
    public partial class DashboardView : UserControl
    {
        private readonly DatabaseContext _db;

        public DashboardView()
        {
            InitializeComponent();
            _db = new DatabaseContext();
            LoadStats();
        }

        private async void LoadStats()
        {
            try
            {
                var soldiers = await _db.ExecuteScalarAsync("SELECT COUNT(*) FROM Soldiers");
                var rewards = await _db.ExecuteScalarAsync("SELECT COUNT(*) FROM Rewards");
                // For 'Pending', we assume a status field or similar, or just a sample count for now
                // var pending = await _db.ExecuteScalarAsync("SELECT COUNT(*) FROM Rewards WHERE Status = 'Pending'");

                txtTotalSoldiers.Text = string.Format("{0:N0}", soldiers ?? 0);
                txtTotalRewards.Text = string.Format("{0:N0}", rewards ?? 0);
                txtPendingRewards.Text = "5"; // Placeholder as requested by the UI design
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
