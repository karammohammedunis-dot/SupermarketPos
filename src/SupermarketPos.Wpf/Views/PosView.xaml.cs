using System.Windows.Controls;

namespace SupermarketPos.Wpf.Views;

public partial class PosView : UserControl
{
    public PosView()
    {
        InitializeComponent();
        Loaded += (_, _) => SearchBox.Focus();
    }
}
