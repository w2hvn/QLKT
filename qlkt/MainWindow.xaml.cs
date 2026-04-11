using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QLKT.Views;

namespace QLKT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Global State
        public int SelectedSoldierId { get; set; }
        public int SelectedProposalId { get; set; }
        public string CurrentUser { get; set; }
        public string Filters { get; set; }

        private DashboardOverviewView _dashboardView = new DashboardOverviewView();
        private RewardProposalView _rewardProposalView = new RewardProposalView();
        private RewardListView _rewardListView = new RewardListView();
        private ApprovalProcessView _approvalView = new ApprovalProcessView();
        private CreateProposalView _createProposalView = new CreateProposalView();
        private ReportManagement _reportManagementView = new ReportManagement();
        private UserManagementView _userManagementView = new UserManagementView();
        private SoldierProfileView _soldierProfileView = new SoldierProfileView();
        private SettingsView _settingsView = new SettingsView();
        private CategoryView _categoryView = new CategoryView();

        public MainWindow()
        {
            InitializeComponent();

            ApplyRoleBasedAccess();
            LoadUserInfo();

            // Subscribe to events
            _rewardProposalView.OnCreateNewProposalRequested += RewardProposalView_OnCreateNewProposalRequested;
            _rewardListView.OnSoldierSelected += RewardListView_OnSoldierSelected;
            _soldierProfileView.OnCreateProposalRequested += SoldierProfileView_OnCreateProposalRequested;

            MainContent.Content = _dashboardView;
            UpdateSubNavStyle(borderSubNavOverview, txtSubNavOverview);
            UpdateSidebarStyle(btnNavOverview);
        }

        private void RewardListView_OnSoldierSelected(object sender, int soldierId)
        {
            SelectedSoldierId = soldierId;
            NavigateToSoldierProfile(soldierId);
        }

        private void LoadUserInfo()
        {
            CurrentUser = App.CurrentUserName;
            txtUserNameTop.Text = App.CurrentUserName;
            txtUserRoleTop.Text = App.CurrentUserRole;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void ApplyRoleBasedAccess()
        {
            // App.CurrentUserRole contains the role of the logged in user
            string role = App.CurrentUserRole?.ToLower() ?? "";

            if (role != "admin" && role != "quản trị viên")
            {
                btnNavApproval.Visibility = Visibility.Collapsed;
                btnNavUsers.Visibility = Visibility.Collapsed;
            }
        }

        private void RewardProposalView_OnCreateNewProposalRequested(object sender, EventArgs e)
        {
            // When user clicks "Tạo đề xuất mới" in the Proposal list view, navigate to the Create form view
            _createProposalView = new CreateProposalView(); // New instance to clear state
            MainContent.Content = _createProposalView;
            txtSubNavOverview.Text = "Tạo đề xuất mới";
            UpdateSubNavStyle(borderSubNavOverview, txtSubNavOverview);
            UpdateSidebarStyle(btnNavRewardCategory); // Focus the corresponding sidebar menu item
        }

        private void SoldierProfileView_OnCreateProposalRequested(object sender, int soldierId)
        {
            SelectedSoldierId = soldierId;
            _createProposalView = new CreateProposalView();
            _createProposalView.SetSoldier(soldierId);
            MainContent.Content = _createProposalView;
            txtSubNavOverview.Text = "Tạo đề xuất mới";
            UpdateSubNavStyle(borderSubNavOverview, txtSubNavOverview);
            UpdateSidebarStyle(btnNavRewardCategory);
        }

        private void UpdateSubNavStyle(Border activeBorder, TextBlock activeText)
        {
            // Reset all sub-navs
            borderSubNavOverview.BorderBrush = Brushes.Transparent;
            txtSubNavOverview.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
            txtSubNavOverview.FontWeight = FontWeights.SemiBold;

            // Set active
            if (activeBorder != null && activeText != null)
            {
                activeBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#001529"));
                activeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#001529"));
                activeText.FontWeight = FontWeights.Bold;
            }
        }

        private void UpdateSidebarStyle(Button activeButton)
        {
            btnNavOverview.Tag = "Secondary";
            btnNavRewardList.Tag = "Secondary";
            btnNavProposal.Tag = "Secondary";
            btnNavApproval.Tag = "Secondary";
            btnNavRewardCategory.Tag = "Secondary";
            btnNavReport.Tag = "Secondary";
            btnNavUsers.Tag = "Secondary";
            btnNavProfile.Tag = "Secondary";
            btnNavSettings.Tag = "Secondary";

            if (activeButton != null)
            {
                activeButton.Tag = "Dark";
            }
        }

        public void NavigateToSoldierProfile(int soldierId)
        {
            _soldierProfileView.LoadProfile(soldierId);
            MainContent.Content = _soldierProfileView;
            txtSubNavOverview.Text = "Hồ sơ cá nhân";
            UpdateSubNavStyle(borderSubNavOverview, txtSubNavOverview);
            UpdateSidebarStyle(null);
        }

        private void Nav_Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _dashboardView;
            txtSubNavOverview.Text = "Tổng quan";
            UpdateSubNavStyle(borderSubNavOverview, txtSubNavOverview);
            UpdateSidebarStyle(btnNavOverview);
        }

        private void Nav_RewardList_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _rewardListView;
            txtSubNavOverview.Text = "Danh sách Khen thưởng";
            UpdateSubNavStyle(borderSubNavOverview, txtSubNavOverview);
            UpdateSidebarStyle(btnNavRewardList);
        }

        private void Nav_Proposal_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _rewardProposalView;
            txtSubNavOverview.Text = "Đề xuất Khen thưởng";
            UpdateSubNavStyle(borderSubNavOverview, txtSubNavOverview);
            UpdateSidebarStyle(btnNavProposal);
        }

        private void Nav_Approval_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _approvalView;
            txtSubNavOverview.Text = "Phê duyệt";
            UpdateSubNavStyle(borderSubNavOverview, txtSubNavOverview);
            UpdateSidebarStyle(btnNavApproval);
        }

        private void Nav_RewardCategory_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _categoryView;
            txtSubNavOverview.Text = "Danh mục Khen thưởng";
            UpdateSubNavStyle(borderSubNavOverview, txtSubNavOverview);
            UpdateSidebarStyle(btnNavRewardCategory);
        }

        private void Nav_Report_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _reportManagementView;
            txtSubNavOverview.Text = "Thống kê & Báo cáo";
            UpdateSubNavStyle(borderSubNavOverview, txtSubNavOverview);
            UpdateSidebarStyle(btnNavReport);
        }

        private void Nav_Users_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _userManagementView;
            txtSubNavOverview.Text = "Quản lý Người dùng";
            UpdateSubNavStyle(borderSubNavOverview, txtSubNavOverview);
            UpdateSidebarStyle(btnNavUsers);
        }

        private void Nav_Profile_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _soldierProfileView;
            txtSubNavOverview.Text = "Thành tích cá nhân";
            UpdateSubNavStyle(borderSubNavOverview, txtSubNavOverview);
            UpdateSidebarStyle(btnNavProfile);
        }

        private void Nav_Settings_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _settingsView;
            txtSubNavOverview.Text = "Cài đặt";
            UpdateSubNavStyle(borderSubNavOverview, txtSubNavOverview);
            UpdateSidebarStyle(btnNavSettings);
        }
    }
}
