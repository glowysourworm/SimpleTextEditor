using System.Windows;

namespace SimpleTextEditor.Sample
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Editor.Loaded += Editor_Loaded;
        }

        private void Editor_Loaded(object sender, RoutedEventArgs e)
        {
            this.Editor.SetText("Welcome to  Simple Text Editor!");
        }
    }
}