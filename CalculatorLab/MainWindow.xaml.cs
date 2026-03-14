using CalculatorLab;
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
            FocusManager.SetFocusedElement(this, this); // Автофокус на вікно
            this.Focus(); // Додатковий фокус
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

        // === Клавіатура ===

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Ігноруємо, якщо фокус не на вікні або є помилка
            if (_model.DisplayText == "Error") return;

            e.Handled = true; // Блокуємо повторну обробку

                              // === ПРІОРИТЕТ: Оператори ===
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                // Операторы: поддержка NumPad и основных клавиш как в Windows Calculator
                switch (e.Key)
                {
                    // Плюс: Numpad '+' или Shift+'=' (Key.OemPlus с Shift)
                    case Key.Add:
                    case Key.OemPlus when (Keyboard.Modifiers & ModifierKeys.Shift) != 0:
                        _model.SetOperator("+");
                        return;

                    // Минус: Numpad '-' или '-' на основной клавиатуре
                    case Key.Subtract:
                    case Key.OemMinus:
                        _model.SetOperator("−");
                        return;

                    // Умножение: Numpad '*' или Shift+'8' (Key.D8 с Shift)
                    case Key.Multiply:
                    case Key.D8 when (Keyboard.Modifiers & ModifierKeys.Shift) != 0:
                        _model.SetOperator("×");
                        return;

                    // Деление: Numpad '/' или '/' на основной клавиатуре (Oem2 / OemQuestion)
                    case Key.Divide:
                    case Key.OemQuestion:
                    case Key.OemBackslash: // на некоторых раскладках встречается как '/'
                        _model.SetOperator("÷");
                        return;
                }
            }

            

            switch (e.Key)
            {
                // Цифри 0-9
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

                // Крапка (десяткова)
                case Key.Decimal or Key.OemPeriod: _model.AppendDot(); break;


                // Enter = обчислення
                case Key.Enter or Key.Return: _model.Calculate(); break;

                // CE (Backspace)
                case Key.Back: _model.ClearEntry(); break;

                // C (Escape)
                case Key.Escape: _model.ClearAll(); break;
            }

            UpdateDisplay();
        }

    }
}