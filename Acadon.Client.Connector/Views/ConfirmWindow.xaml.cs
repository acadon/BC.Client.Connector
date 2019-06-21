using System.Windows;

namespace Acadon.Client.Connector.Views
{
    /// <summary>
    /// Interaktionslogik für ConfirmWindow.xaml
    /// </summary>
    public partial class ConfirmWindow : Window
    {
        public string OperationName { get; set; } = "Demo Name";
        public string CompanyName { get; set; } = "Demo Name";
        public string CompanyNameQst {
            get {
                return string.Format(Properties.Resources.ExecuteQuestionPart1, CompanyName);
            }
        }
        public bool AlwaysYes { get; set; } = false;

        public ConfirmWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Click_Yes(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Click_Always(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            AlwaysYes = true;
            Close();
        }

        private void Click_No(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
