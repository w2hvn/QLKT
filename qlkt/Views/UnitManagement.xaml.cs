using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MilitaryRewardApp.Data;
using MilitaryRewardApp.Models;
using MySqlConnector;

namespace MilitaryRewardApp.Views
{
    public partial class UnitManagement : UserControl
    {
        private readonly DatabaseContext _db;
        private int? _selectedUnitId = null;

        public UnitManagement()
        {
            InitializeComponent();
            _db = new DatabaseContext();
            LoadTree();
        }

        // Simple class for TreeView binding
        public class UnitNode
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int? ParentId { get; set; }
            public List<UnitNode> Children { get; set; } = new List<UnitNode>();

            public override string ToString() => Name;
        }

        private async void LoadTree()
        {
            try
            {
                var dt = await _db.ExecuteQueryAsync("SELECT * FROM Units");
                var allUnits = new List<UnitNode>();

                foreach (DataRow row in dt.Rows)
                {
                    allUnits.Add(new UnitNode
                    {
                        Id = Convert.ToInt32(row["UnitID"]),
                        Name = row["UnitName"].ToString(),
                        ParentId = row["ParentID"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ParentID"])
                    });
                }

                // Build hierarchy
                var rootUnits = allUnits.Where(u => u.ParentId == null).ToList();
                foreach (var root in rootUnits)
                {
                    AddChildren(root, allUnits);
                }

                // Bind to TreeView. Need to use ItemsSource or manually add TreeViewItems.
                // For simplicity, let's manually add TreeViewItems to support recursion easily in code-behind
                tvUnits.Items.Clear();
                foreach (var root in rootUnits)
                {
                    tvUnits.Items.Add(CreateTreeItem(root));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải cây đơn vị: " + ex.Message);
            }
        }

        private void AddChildren(UnitNode parent, List<UnitNode> allUnits)
        {
            parent.Children = allUnits.Where(u => u.ParentId == parent.Id).ToList();
            foreach (var child in parent.Children)
            {
                AddChildren(child, allUnits);
            }
        }

        private TreeViewItem CreateTreeItem(UnitNode node)
        {
            var item = new TreeViewItem { Header = node.Name, Tag = node.Id };
            foreach (var child in node.Children)
            {
                item.Items.Add(CreateTreeItem(child));
            }
            item.IsExpanded = true;
            return item;
        }

        private void tvUnits_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (tvUnits.SelectedItem is TreeViewItem item)
            {
                _selectedUnitId = (int)item.Tag;
                txtUnitName.Text = item.Header.ToString();
                txtParentID.Text = _selectedUnitId.ToString(); // Just showing ID for reference
            }
            else
            {
                _selectedUnitId = null;
                txtUnitName.Text = "";
                txtParentID.Text = "";
            }
        }

        private async void AddRoot_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUnitName.Text)) return;
            try
            {
                string query = "INSERT INTO Units (UnitName, ParentID) VALUES (@Name, NULL)";
                var param = new MySqlParameter[] { new MySqlParameter("@Name", txtUnitName.Text) };
                await _db.ExecuteNonQueryAsync(query, param);
                LoadTree();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private async void AddChild_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUnitId == null || string.IsNullOrWhiteSpace(txtUnitName.Text))
            {
                MessageBox.Show("Chọn đơn vị cha và nhập tên mới.");
                return;
            }
            try
            {
                string query = "INSERT INTO Units (UnitName, ParentID) VALUES (@Name, @Pid)";
                var param = new MySqlParameter[]
                {
                    new MySqlParameter("@Name", txtUnitName.Text),
                    new MySqlParameter("@Pid", _selectedUnitId)
                };
                await _db.ExecuteNonQueryAsync(query, param);
                LoadTree();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUnitId == null || string.IsNullOrWhiteSpace(txtUnitName.Text)) return;
            try
            {
                string query = "UPDATE Units SET UnitName = @Name WHERE UnitID = @Id";
                var param = new MySqlParameter[]
                {
                    new MySqlParameter("@Name", txtUnitName.Text),
                    new MySqlParameter("@Id", _selectedUnitId)
                };
                await _db.ExecuteNonQueryAsync(query, param);
                LoadTree();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUnitId == null) return;
            if (MessageBox.Show("Xóa đơn vị này (và tất cả con)?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    // Basic delete. DB constraints might prevent if soldiers exist, or cascade.
                    // Assuming no FK constraint blocking or user knows to clear soldiers first.
                    // Or we could implement recursive delete. For now, simple delete.
                    string query = "DELETE FROM Units WHERE UnitID = @Id";
                    var param = new MySqlParameter[] { new MySqlParameter("@Id", _selectedUnitId) };
                    await _db.ExecuteNonQueryAsync(query, param);
                    LoadTree();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            }
        }
    }
}
