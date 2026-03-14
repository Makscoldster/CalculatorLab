using System;
using System.Globalization;

namespace CalculatorLab
{
    public class CalculatorModel
    {
        private double _currentValue = 0;
        private string _currentOperator = "";
        private string _displayText = "0";
        private bool _newEntry = true; // Флаг для нового введення після оператора
        private bool _hasError = false;

        public string DisplayText => _displayText;

        public void AppendNumber(string number)
        {
            if (_hasError) return;

            if (_newEntry)
            {
                _displayText = number;
                _newEntry = false;
            }
            else
            {
                // Додаємо до поточного, обмежуємо довжину
                if (_displayText.Length < 15)
                    _displayText += number;
            }
        }

        public void AppendDot()
        {
            if (_hasError || _displayText.Contains(".")) return;

            if (_newEntry)
            {
                _displayText = "0.";
                _newEntry = false;
            }
            else
            {
                _displayText += ".";
            }
        }

        public void SetOperator(string op)
        {
            if (_hasError) return;

            if (!string.IsNullOrEmpty(_currentOperator) && !_newEntry)
            {
                // Якщо є попередній оператор, виконаємо обчислення
                Calculate();
            }

            _currentValue = GetCurrentNumber();
            _currentOperator = op;
            _newEntry = true;
        }

        public void Calculate()
        {
            if (_hasError || string.IsNullOrEmpty(_currentOperator)) return;

            double secondValue = GetCurrentNumber();
            double result = _currentOperator switch
            {
                "+" => _currentValue + secondValue,
                "−" => _currentValue - secondValue,
                "×" => _currentValue * secondValue,
                "÷" => _hasError ? 0 : _currentValue / secondValue,
                _ => _currentValue
            };

            if (_currentOperator == "÷" && secondValue == 0)
            {
                _displayText = "Error";
                _hasError = true;
                _currentOperator = "";
                return;
            }

            _displayText = FormatNumber(result);
            _currentValue = result;
            _currentOperator = "";
            _newEntry = true;
        }

        public void ClearEntry() // Поки CE = очищення поточного введення (замість Undo)
        {
            _displayText = "0";
            _newEntry = true;
        }

        public void ClearAll()
        {
            _currentValue = 0;
            _currentOperator = "";
            _displayText = "0";
            _newEntry = true;
            _hasError = false;
        }

        private double GetCurrentNumber()
        {
            return double.TryParse(_displayText, NumberStyles.Any, CultureInfo.InvariantCulture, out double num) ? num : 0;
        }

        private string FormatNumber(double num)
        {
            if (num == (int)num)
                return ((int)num).ToString();
            return num.ToString("G15", CultureInfo.InvariantCulture);
        }
    }
}
