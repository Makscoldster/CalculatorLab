using CalculatorLab;
using System.Windows;
using System.Windows.Controls;

namespace CalculatorLab
{

    public partial class MainWindow : Window
    {
        private readonly CalculatorModel _model = new();

        public MainWindow()
        {
            InitializeComponent();
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            Display.Text = _model.DisplayText;
        }

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.Content.ToString() == ".")
                _model.AppendDot();
            else
                _model.AppendNumber(button.Content.ToString());
            UpdateDisplay();
        }

        private void OperatorButton_Click(object sender, RoutedEventArgs e)
        {
            _model.SetOperator(((Button)sender).Content.ToString());
            UpdateDisplay();
        }

        private void EqualsButton_Click(object sender, RoutedEventArgs e)
        {
            _model.Calculate();
            UpdateDisplay();
        }

        private void CEButton_Click(object sender, RoutedEventArgs e)
        {
            _model.ClearEntry(); // Поки просте CE
            UpdateDisplay();
        }

        private void CButton_Click(object sender, RoutedEventArgs e)
        {
            _model.ClearAll();
            UpdateDisplay();
        }
    }
}