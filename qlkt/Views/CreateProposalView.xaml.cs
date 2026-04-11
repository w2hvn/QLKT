using System.Windows;
using System.Windows.Controls;

namespace QLKT.Views
{
    public partial class CreateProposalView : UserControl
    {
        public CreateProposalView()
        {
            InitializeComponent();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã gửi đề xuất thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
