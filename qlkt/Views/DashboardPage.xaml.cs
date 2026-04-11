using System;
using System.Windows.Controls;
using MilitaryRewardApp.Data;

namespace MilitaryRewardApp.Views
{
    public partial class DashboardPage : UserControl
    {
        private readonly DatabaseContext _db;

        public DashboardPage()
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
                var units = await _db.ExecuteScalarAsync("SELECT COUNT(*) FROM Units");

                txtTotalSoldiers.Text = soldiers?.ToString() ?? "0";
                txtTotalRewards.Text = rewards?.ToString() ?? "0";
                txtTotalUnits.Text = units?.ToString() ?? "0";
            }
            catch (Exception ex)
            {
                // In a real app, log this
                Console.WriteLine(ex.Message);
            }
        }
    }
}
