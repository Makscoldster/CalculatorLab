using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CalculatorLab
{
    public partial class MainWindow : Window
    {
        private readonly CalculatorModel _model = new();

        public MainWindow()
        {
            InitializeComponent();
            UpdateDisplay();
            this.Focus();
        }

        // ── Display ────────────────────────────────────────────────────────────

        private void UpdateDisplay()
        {
            Display.Text = _model.DisplayText;
            ExpressionText.Text = _model.ExpressionText;

            // Підсвічуємо кнопки Undo/Redo якщо вони є у XAML
            if (BtnUndo is Button u) u.IsEnabled = _model.CanUndo;
            if (BtnRedo is Button r) r.IsEnabled = _model.CanRedo;
        }

        // ── Button handlers ────────────────────────────────────────────────────

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            var content = ((Button)sender).Content.ToString()!;
            if (content == ".")
                _model.AppendDot();
            else
                _model.AppendNumber(content);
            UpdateDisplay();
        }

        private void OperatorButton_Click(object sender, RoutedEventArgs e)
        {
            _model.SetOperator(((Button)sender).Content.ToString()!);
            UpdateDisplay();
        }

        private void EqualsButton_Click(object sender, RoutedEventArgs e)
        {
            _model.Calculate();
            UpdateDisplay();
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            _model.Backspace();
            UpdateDisplay();
        }

        private void ToggleSignButton_Click(object sender, RoutedEventArgs e)
        {
            _model.ToggleSign();
            UpdateDisplay();
        }

        private void PercentButton_Click(object sender, RoutedEventArgs e)
        {
            _model.Percent();
            UpdateDisplay();
        }

        // CE = Undo
        private void CEButton_Click(object sender, RoutedEventArgs e)
        {
            _model.Undo();
            UpdateDisplay();
        }

        // C = ClearAll
        private void CButton_Click(object sender, RoutedEventArgs e)
        {
            _model.ClearAll();
            UpdateDisplay();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            _model.Undo();
            UpdateDisplay();
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            _model.Redo();
            UpdateDisplay();
        }

        // ── Keyboard ───────────────────────────────────────────────────────────

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_model.DisplayText == "Error" && e.Key != Key.Escape && e.Key != Key.Delete)
                return;

            e.Handled = true;
            var mods = Keyboard.Modifiers;

            // Оператори з Shift
            if (mods == ModifierKeys.Shift)
            {
                switch (e.Key)
                {
                    case Key.OemPlus:                        // Shift+= → +
                    case Key.Add:
                        _model.SetOperator("+"); UpdateDisplay(); return;

                    case Key.D8:                             // Shift+8 → ×
                    case Key.Multiply:
                        _model.SetOperator("×"); UpdateDisplay(); return;

                    case Key.D5:                             // Shift+5 → %
                        _model.Percent(); UpdateDisplay(); return;
                }
            }

            // Ctrl+Z / Ctrl+Y
            if (mods == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.Z: _model.Undo(); UpdateDisplay(); return;
                    case Key.Y: _model.Redo(); UpdateDisplay(); return;
                }
            }

            switch (e.Key)
            {
                // Цифри
                case Key.D0 or Key.NumPad0: _model.AppendNumber("0"); break;
                case Key.D1 or Key.NumPad1: _model.AppendNumber("1"); break;
                case Key.D2 or Key.NumPad2: _model.AppendNumber("2"); break;
                case Key.D3 or Key.NumPad3: _model.AppendNumber("3"); break;
                case Key.D4 or Key.NumPad4: _model.AppendNumber("4"); break;
                case Key.D5 or Key.NumPad5: _model.AppendNumber("5"); break;
                case Key.D6 or Key.NumPad6: _model.AppendNumber("6"); break;
                case Key.D7 or Key.NumPad7: _model.AppendNumber("7"); break;
                case Key.D8 or Key.NumPad8: _model.AppendNumber("8"); break;
                case Key.D9 or Key.NumPad9: _model.AppendNumber("9"); break;

                // Крапка
                case Key.Decimal or Key.OemPeriod or Key.OemComma: _model.AppendDot(); break;

                // Оператори (без Shift / NumPad)
                case Key.Add: _model.SetOperator("+"); break;
                case Key.Subtract: _model.SetOperator("−"); break;
                case Key.OemMinus: _model.SetOperator("−"); break;
                case Key.Multiply: _model.SetOperator("×"); break;
                case Key.Divide: _model.SetOperator("÷"); break;
                case Key.OemQuestion: _model.SetOperator("÷"); break;  // '/' на деяких розкладках

                // Enter / = → обчислення
                case Key.Enter or Key.Return: _model.Calculate(); break;
                case Key.OemPlus when mods == ModifierKeys.None: _model.Calculate(); break; // '=' без Shift

                // Backspace → видалити останній символ
                case Key.Back: _model.Backspace(); break;

                // Escape → ClearAll
                case Key.Escape or Key.Delete: _model.ClearAll(); break;

                // F9 → зміна знаку (як у Windows Calculator)
                case Key.F9: _model.ToggleSign(); break;

                default: e.Handled = false; return;
            }

            UpdateDisplay();
        }
    }
}