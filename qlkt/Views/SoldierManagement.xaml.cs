using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using QLKT.Data;
using QLKT.Models;
using MySqlConnector;

namespace QLKT.Views
{
    public partial class SoldierManagement : UserControl
    {
        private readonly DatabaseContext _db;
        private int? _selectedSoldierId = null;

        public SoldierManagement()
        {
            InitializeComponent();
            _db = new DatabaseContext();
            LoadUnits();
            LoadSoldiers();
        }

        private async void LoadUnits()
        {
            try
            {
                var dt = await _db.ExecuteQueryAsync("SELECT UnitID, UnitName FROM Units");
                cboUnits.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải đơn vị: " + ex.Message); }
        }

        private async void LoadSoldiers(string search = "")
        {
            try
            {
                string query = @"SELECT s.*, u.UnitName 
                               FROM Soldiers s 
                               LEFT JOIN Units u ON s.UnitID = u.UnitID";

                if (!string.IsNullOrEmpty(search))
                {
                    query += $" WHERE s.FullName LIKE '%{search}%' OR s.SoldierCode LIKE '%{search}%' OR s.Rank LIKE '%{search}%'";
                }

                query += " ORDER BY s.SoldierID DESC";

                var dt = await _db.ExecuteQueryAsync(query);

                // Manually mapping to Model List is cleaner for DataGrid binding but DataTable works too.
                // Using DataTable directly for simplicity as requested.
                // However, the model has 'UnitName' property. DataTable columns match the query.
                dgSoldiers.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải danh sách: " + ex.Message); }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSoldiers(txtSearch.Text);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            LoadSoldiers();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _selectedSoldierId = null;
            txtCode.Text = "";
            txtName.Text = "";
            cboGender.SelectedIndex = -1;
            txtRank.Text = "";
            txtPosition.Text = "";
            cboUnits.SelectedIndex = -1;
            txtEthnicity.Text = "";
            txtReligion.Text = "";
            txtAcademic.Text = "";
            txtPolitical.Text = "";
            txtSchools.Text = "";
            txtHistory.Text = "";
        }

        private void dgSoldiers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSoldiers.SelectedItem is DataRowView row)
            {
                _selectedSoldierId = Convert.ToInt32(row["SoldierID"]);
                txtCode.Text = row["SoldierCode"].ToString();
                txtName.Text = row["FullName"].ToString();
                cboGender.Text = row["Gender"].ToString();
                txtRank.Text = row["Rank"].ToString();
                txtPosition.Text = row["Position"].ToString();
                cboUnits.SelectedValue = row["UnitID"];
                txtEthnicity.Text = row["Ethnicity"].ToString();
                txtReligion.Text = row["Religion"].ToString();
                txtAcademic.Text = row["AcademicLevel"].ToString();
                txtPolitical.Text = row["PoliticalTheory"].ToString();
                txtSchools.Text = row["SchoolsAttended"].ToString();
                txtHistory.Text = row["WorkHistory"].ToString();
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || cboUnits.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng nhập tên và chọn đơn vị.");
                return;
            }

            try
            {
                string gender = (cboGender.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@Code", txtCode.Text),
                    new MySqlParameter("@Name", txtName.Text),
                    new MySqlParameter("@Gender", gender),
                    new MySqlParameter("@Rank", txtRank.Text),
                    new MySqlParameter("@Pos", txtPosition.Text),
                    new MySqlParameter("@Unit", cboUnits.SelectedValue),
                    new MySqlParameter("@Eth", txtEthnicity.Text),
                    new MySqlParameter("@Rel", txtReligion.Text),
                    new MySqlParameter("@Acad", txtAcademic.Text),
                    new MySqlParameter("@Pol", txtPolitical.Text),
                    new MySqlParameter("@Schools", txtSchools.Text),
                    new MySqlParameter("@Hist", txtHistory.Text)
                };

                string query;
                if (_selectedSoldierId == null)
                {
                    // Insert
                    query = @"INSERT INTO Soldiers (SoldierCode, FullName, Gender, `Rank`, Position, UnitID, Ethnicity, Religion, AcademicLevel, PoliticalTheory, SchoolsAttended, WorkHistory) 
                              VALUES (@Code, @Name, @Gender, @Rank, @Pos, @Unit, @Eth, @Rel, @Acad, @Pol, @Schools, @Hist)";
                }
                else
                {
                    // Update
                    query = @"UPDATE Soldiers SET SoldierCode=@Code, FullName=@Name, Gender=@Gender, `Rank`=@Rank, Position=@Pos, UnitID=@Unit, 
                              Ethnicity=@Eth, Religion=@Rel, AcademicLevel=@Acad, PoliticalTheory=@Pol, SchoolsAttended=@Schools, WorkHistory=@Hist 
                              WHERE SoldierID = " + _selectedSoldierId;
                }

                await _db.ExecuteNonQueryAsync(query, parameters);
                MessageBox.Show("Lưu thành công!");
                LoadSoldiers(txtSearch.Text);
                Clear_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu: " + ex.Message);
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSoldierId == null)
            {
                MessageBox.Show("Vui lòng chọn quân nhân để xóa.");
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    await _db.ExecuteNonQueryAsync($"DELETE FROM Soldiers WHERE SoldierID = {_selectedSoldierId}");
                    LoadSoldiers(txtSearch.Text);
                    Clear_Click(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xóa: " + ex.Message);
                }
            }
        }
    }
}
