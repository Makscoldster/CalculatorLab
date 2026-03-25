using System.Globalization;

namespace CalculatorLab
{
    // ── 1. Snapshot стану ──────────────────────────────────────────────────────
    public record CalculatorState
    {
        public double CurrentValue { get; init; }
        public string CurrentOperator { get; init; } = "";
        public string DisplayText { get; init; } = "0";
        public string Expression { get; init; } = "";
        public bool NewEntry { get; init; } = true;
        public bool HasError { get; init; }
        public bool NewExpression { get; init; } = true;
    }

    // ── 2. Command interface ───────────────────────────────────────────────────
    public interface ICalculatorCommand
    {
        void Execute(CalculatorModel model);
    }

    // ── 3. Command history (Undo / Redo) ──────────────────────────────────────
    public class CommandHistory
    {
        private readonly Stack<CalculatorState> _undoStack = new();
        private readonly Stack<CalculatorState> _redoStack = new();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void Push(CalculatorState stateBefore)
        {
            _undoStack.Push(stateBefore);
            _redoStack.Clear();          // нова команда скидає redo
        }

        public CalculatorState Undo(CalculatorState current)
        {
            var prev = _undoStack.Pop();
            _redoStack.Push(current);
            return prev;
        }

        public CalculatorState Redo(CalculatorState current)
        {
            var next = _redoStack.Pop();
            _undoStack.Push(current);
            return next;
        }

        public void StackClear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }


    public class AppendNumberCommand(string digit) : ICalculatorCommand
    {
        public void Execute(CalculatorModel m) => m.AppendNumberInternal(digit);
    }

    public class AppendDotCommand : ICalculatorCommand
    {
        public void Execute(CalculatorModel m) => m.AppendDotInternal();
    }

    public class SetOperatorCommand(string op) : ICalculatorCommand
    {
        public void Execute(CalculatorModel m) => m.SetOperatorInternal(op);
    }

    public class CalculateCommand : ICalculatorCommand
    {
        public void Execute(CalculatorModel m) => m.CalculateInternal();
    }

    public class BackspaceCommand : ICalculatorCommand
    {
        public void Execute(CalculatorModel m) => m.BackspaceInternal();
    }

    public class ToggleSignCommand : ICalculatorCommand
    {
        public void Execute(CalculatorModel m) => m.ToggleSignInternal();
    }

    public class PercentCommand : ICalculatorCommand
    {
        public void Execute(CalculatorModel m) => m.PercentInternal();
    }
    public class NaturalLogCommand : ICalculatorCommand
    {
        public void Execute(CalculatorModel m) => m.NaturalLogInternal();
    }

    // ── 5. CalculatorModel ────────────────────────────────────────────────────
    public class CalculatorModel
    {
        private CalculatorState _state = new();
        private readonly CommandHistory _history = new();

        // Публічні властивості для прив'язки до UI
        public string DisplayText => _state.DisplayText;
        public string ExpressionText => _state.Expression;
        public bool CanUndo => _history.CanUndo;
        public bool CanRedo => _history.CanRedo;

        // ── Публічне API (зберігає snapshot → виконує команду) ────────────────

        public void Execute(ICalculatorCommand command)
        {
            _history.Push(_state);
            command.Execute(this);
        }

        // Зручні обгортки для UI / клавіатури
        public void AppendNumber(string d) => Execute(new AppendNumberCommand(d));
        public void AppendDot() => Execute(new AppendDotCommand());
        public void SetOperator(string op) => Execute(new SetOperatorCommand(op));
        public void Calculate() => Execute(new CalculateCommand());
        public void Backspace() => Execute(new BackspaceCommand());
        public void ToggleSign() => Execute(new ToggleSignCommand());
        public void Percent() => Execute(new PercentCommand());
        public void NaturalLog() => Execute(new NaturalLogCommand());



        public void Undo()
        {
            if (_history.CanUndo)
                _state = _history.Undo(_state);
        }

        public void Redo()
        {
            if (_history.CanRedo)
                _state = _history.Redo(_state);
        }

        public void ClearAll()
        {
            _history.StackClear();
            _state = new CalculatorState();
        }

        // ── Internal mutation methods (викликаються лише з команд) ────────────

        internal void AppendNumberInternal(string number)
        {
            var s = _state;

            if (s.HasError)
            {
                _state = new CalculatorState { DisplayText = number, NewEntry = false, NewExpression = true };
                return;
            }

            if (s.NewEntry)
            {
                string expr = s.Expression.TrimEnd().EndsWith("=") ? number : s.Expression + number;
                _state = s with
                {
                    DisplayText = number,
                    NewEntry = false,
                    NewExpression = false,
                    Expression = expr
                };
            }
            else
            {
                if (s.DisplayText.Length >= 15) return;
                if (s.DisplayText == "0" && number == "0") return;
                if (s.DisplayText == "0" && number != ".")
                {
                    _state = s with { DisplayText = number };
                    return;
                }
                _state = s with
                {
                    DisplayText = s.DisplayText + number,
                    Expression = s.Expression + number
                };
            }
        }

        internal void AppendDotInternal()
        {
            var s = _state;
            if (s.HasError || s.DisplayText.Contains('.')) return;

            bool freshStart = s.NewExpression || s.Expression.TrimEnd().EndsWith("=");

            if (s.NewEntry)
            {
                _state = s with
                {
                    DisplayText = "0.",
                    NewEntry = false,
                    NewExpression = false,
                    Expression = freshStart ? "0." : s.Expression + "0."
                };
            }
            else
            {
                _state = s with
                {
                    DisplayText = s.DisplayText + ".",
                    Expression = s.Expression + "."
                };
            }
        }

        internal void SetOperatorInternal(string op)
        {
            var s = _state;
            if (s.HasError) return;

            // Якщо вже є незавершена операція — обчислюємо
            if (!string.IsNullOrEmpty(s.CurrentOperator) && !s.NewEntry)
                CalculateInternal();

            var cur = GetNumber(_state.DisplayText);
            _state = _state with
            {
                CurrentValue = cur,
                CurrentOperator = op,
                Expression = FormatNumber(cur) + " " + op + " ",
                NewEntry = true
            };
        }

        internal void CalculateInternal()
        {
            var s = _state;
            if (s.HasError || string.IsNullOrEmpty(s.CurrentOperator)) return;

            double b = GetNumber(s.DisplayText);

            if (s.CurrentOperator == "÷" && b == 0)
            {
                _state = s with { DisplayText = "Error", HasError = true, CurrentOperator = "" };
                return;
            }

            double result = s.CurrentOperator switch
            {
                "+" => s.CurrentValue + b,
                "−" => s.CurrentValue - b,
                "×" => s.CurrentValue * b,
                "÷" => s.CurrentValue / b,
                "xⁿ" => Math.Pow(s.CurrentValue, b),
                "ˣ√" => Math.Pow(s.CurrentValue, 1.0 / b),
                _ => s.CurrentValue
            };

            string expr = s.NewEntry
                            ? s.Expression + FormatNumber(s.CurrentValue) + " ="
                            : s.Expression.TrimEnd() + " =";

            _state = s with
            {
                DisplayText = FormatNumber(result),
                CurrentValue = result,
                CurrentOperator = "",
                Expression = expr,
                NewEntry = true,
                NewExpression = false
            };
        }

        internal void BackspaceInternal()
        {
            var s = _state;
            if (s.HasError || s.NewEntry || s.DisplayText == "0") return;

            string newDisplay = s.DisplayText.Length > 1
                ? s.DisplayText[..^1]
                : "0";

            if (newDisplay == "-") newDisplay = "0";

            string newExpr = s.Expression.Length > 0 && s.Expression.EndsWith(s.DisplayText[^1].ToString())
                ? s.Expression[..^1]
                : s.Expression;

            bool backToNew = newDisplay == "0";

            _state = s with { DisplayText = newDisplay, Expression = newExpr, NewEntry = backToNew };
        }

        internal void ToggleSignInternal()
        {
            var s = _state;
            if (s.HasError || s.DisplayText == "0" || s.DisplayText == "Error") return;

            double val = GetNumber(s.DisplayText) * -1;
            string newDisplay = FormatNumber(val);
            string newExpr = s.Expression.TrimEnd().EndsWith(s.DisplayText)
                ? s.Expression[..^s.DisplayText.Length] + newDisplay
                : s.Expression;
            _state = s with { DisplayText = newDisplay, Expression = newExpr };
        }

        internal void PercentInternal()
        {
            var s = _state;
            if (s.HasError) return;

            double val = GetNumber(s.DisplayText);
            double result = string.IsNullOrEmpty(s.CurrentOperator)
                ? val / 100
                : s.CurrentValue * val / 100;

            _state = s with
            {
                DisplayText = FormatNumber(result),
                Expression = s.Expression.TrimEnd() + FormatNumber(result)
            };
        }
        internal void NaturalLogInternal()
        {
            var s = _state;
            if (s.HasError) return;

            double val = GetNumber(s.DisplayText);
            if (val <= 0)
            {
                _state = s with { DisplayText = "Error", HasError = true };
                return;
            }

            double result = Math.Log(val);
            _state = s with
            {
                DisplayText = FormatNumber(result),
                Expression = "ln(" + s.DisplayText + ") ="
            };
        }

        private static double GetNumber(string text) =>
            double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double n) ? n : 0;

        private static string FormatNumber(double num)
        {
            if (double.IsInfinity(num) || double.IsNaN(num)) return "Error";
            if (num == Math.Truncate(num) && Math.Abs(num) < 1e15)
                return ((long)num).ToString();
            return num.ToString("G15", CultureInfo.InvariantCulture);
        }
    }
}