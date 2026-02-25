using System.Windows;

namespace Noten.App.Views;

public partial class PromptWindow : Window
{
    public string Value => ValueTextBox.Text;

    public PromptWindow(string title, string label, string initialValue)
    {
        InitializeComponent();
        Title = title;
        PromptTextBlock.Text = label;
        ValueTextBox.Text = initialValue;
        ValueTextBox.Focus();
        ValueTextBox.SelectAll();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
