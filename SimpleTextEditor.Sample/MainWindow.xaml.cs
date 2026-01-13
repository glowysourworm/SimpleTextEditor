using System.Windows;

namespace SimpleTextEditor.Sample
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Editor.SetText("Welcome to Simple Text Editor!");
        }
    }
}