using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace calculat
{
    public partial class MainPage : ContentPage
    {
        private List<string> calcItems = new(); 
        private string currentNumber = "";
        private bool errorMode = false; // флаг ошибки
        private string _displayText = "0";
        private string decSep = ".";

        public string displayText
        {
            get => _displayText;
            set
            {
                if (_displayText != value)
                {
                    _displayText = value;
                    OnPropertyChanged();
                }
            }
        }

        // верхний текст
        private string _fullExp = "";
        public string fullExp
        {
            get => _fullExp;
            set
            {
                if (_fullExp != value)
                {
                    _fullExp = value;
                    OnPropertyChanged();
                }
            }
        }

        private string lastOp = "";         // последний оператор
        private double lastOperand = 0.0;   // последний операнд
        private bool justEquals = false;    // флаг, что только что нажимали =

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        // обработчик нажатия цифровых кнопок (0-9)
        private void NumBtnClick(object sender, EventArgs e)
        {

            if (errorMode)
            {
                ClearAll();
            }

            // проверяем, что клик пришёл от кнопки
            if (sender is Button btn)
            {
                if (justEquals)
                {
                    ClearAll();
                }

                currentNumber += btn.Text;
                displayText = currentNumber;

                justEquals = false;

                ShowFullExpression();
            }
        }

        // обработчик нажатия операторов +, -, x, /
        private void OpBtnClick(object sender, EventArgs e)
        {
            if (errorMode)
            {
                ClearAll();
            }

            if (sender is Button btn)
            {
                string op = btn.Text;

                
                if (justEquals) // если только что былo =, и на экране результат, начинаем новое выражение с этого результата
                {
                    calcItems.Clear();
                    calcItems.Add(displayText);
                    currentNumber = "";
                    justEquals = false;
                }

                if (!string.IsNullOrEmpty(currentNumber))
                {
                    calcItems.Add(currentNumber);
                    currentNumber = "";
                }

                if (calcItems.Count >= 3)
                {
                    ComputePartial();
                }

                else if (calcItems.Count > 0 &&
                         IsOperator(calcItems[calcItems.Count - 1]) &&
                         string.IsNullOrEmpty(currentNumber))
                {
                    calcItems[calcItems.Count - 1] = op;
                    ShowFullExpression();
                    return;
                }

                calcItems.Add(op);

                lastOp = op; // запомним для возможного повторного =

                ShowFullExpression();
            }
        }

        // обработчик нажатия =
        private void EqBtnClick(object sender, EventArgs e)
        {
            if (errorMode)
            {
                ClearAll();
                return;
            }

            // если уже нажимали =, то повторяем операцию 
            if (justEquals)
            {
                
                if (double.TryParse(displayText, NumberStyles.Any, CultureInfo.InvariantCulture, out double currVal)) // берём число на экране
                {
                    if (!string.IsNullOrEmpty(lastOp))
                    {
                        try
                        {
                            double newVal = DoCalc(currVal, lastOperand, lastOp);
                            displayText = newVal.ToString(CultureInfo.InvariantCulture);
                        }
                        catch (Exception ex)
                        {
                            ShowError($"ошибка: {ex.Message}");
                        }
                    }
                }
                return;
            }

            // если в currentNumber остались цифры, добавим в список
            if (!string.IsNullOrEmpty(currentNumber))
            {
                calcItems.Add(currentNumber);
                currentNumber = "";
            }

            if (calcItems.Count == 0)
            {
                ShowError("сначала введите выражение");
                return;
            }

            if (IsOperator(calcItems[calcItems.Count - 1]) && calcItems.Count == 2)
            {
                calcItems.Add(calcItems[0]);
            }

            try
            {

                double leftVal = double.Parse(calcItems[0].Replace(",", "."), CultureInfo.InvariantCulture);
                double rightVal = double.Parse(calcItems[2].Replace(",", "."), CultureInfo.InvariantCulture);
                string theOp = calcItems[1];

                double result = DoCalc(leftVal, rightVal, theOp);
                displayText = result.ToString(CultureInfo.InvariantCulture);

                
                if (calcItems.Count >= 3) // сохраним для повтора =
                {
                    lastOp = calcItems[1];
                    double.TryParse(calcItems[calcItems.Count - 1].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out lastOperand);
                }
                else
                {
                    lastOp = "";
                    lastOperand = 0.0;
                }

                calcItems.Clear();

                currentNumber = displayText;

                justEquals = true; // ставим флаг, что только что было =
                ShowFullExpression();
            }
            catch (Exception ex)
            {
                ShowError($"ошибка: {ex.Message}");
            }
        }

        // обработчик +/-
        private void SignBtnClick(object sender, EventArgs e)
        {
            if (errorMode)
            {
                ClearAll();
                return;
            }
 
            if (!string.IsNullOrEmpty(currentNumber)) // если есть число, меняем знак у него
            {
                if (double.TryParse(currentNumber.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double num))
                {
                    num = -num;
                    currentNumber = num.ToString(CultureInfo.InvariantCulture);
                    displayText = currentNumber;
                    ShowFullExpression();
                }
            }
            else
            {
  
                if (double.TryParse(displayText.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double scrNum)) //меняем знак у того, что на экране
                {
                    scrNum = -scrNum;
                    displayText = scrNum.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        // обработчик нажатия на .
        private void DotBtnClick(object sender, EventArgs e)
           
        {
            if (errorMode)
            {
                ClearAll();
                return;
            }

            if (justEquals)
            {
                ClearAll();
            }

            if (!currentNumber.Contains(decSep))
            {
                if (string.IsNullOrEmpty(currentNumber))
                {
                    currentNumber = "0" + decSep;
                }
                else
                {
                    currentNumber += decSep;
                }

                displayText = currentNumber;
                ShowFullExpression();
            }
        }

        // обработчик нажатия на ,
        private void Zap (object sender, EventArgs e)

        {
            if (errorMode)
            {
                ClearAll();
                return;
            }

            if (justEquals)
            {
                ClearAll();
            }

            if (!currentNumber.Contains(','))
            {
                if (string.IsNullOrEmpty(currentNumber))
                {
                    currentNumber = "0,";
                }
                else
                {
                    currentNumber += ',';
                }

                displayText = currentNumber;
                ShowFullExpression();
            }
        }

        // обработчик очистки
        private void ClrBtnClick(object sender, EventArgs e)
        {
            ClearAll();
        }

        // метод, где происходит действие левое оператор правое
        private double DoCalc(double left, double right, string op)
        {
            return op switch
            {
                "+" => left + right,
                "-" => left - right,
                "x" => left * right,
                "/" => Math.Abs(right) < 1e-15
                        ? throw new DivideByZeroException("деление на ноль")
                        : left / right,
                _ => throw new Exception("неизвестный оператор")
            };
        }

        // проверка, является ли строка оператором
        private bool IsOperator(string token)
        {
            return token == "+" || token == "-" || token == "x" || token == "/";
        }

        // очистка всего
        private void ClearAll()
        {
            calcItems.Clear();
            currentNumber = "";
            displayText = "0";
            fullExp = "";
            errorMode = false;
            lastOp = "";
            lastOperand = 0.0;
            justEquals = false;
        }

        // вывод ошибки
        private void ShowError(string message)
        {
            displayText = message;
            errorMode = true;
        }

        // обновляем строку полного выражения
        private void ShowFullExpression()
        {
            List<string> preview = new List<string>(calcItems);

            if (!string.IsNullOrEmpty(currentNumber))
            {
                preview.Add(currentNumber);
            }

            fullExp = string.Join(" ", preview);
        }

        // частичное вычисление 12 + 3 -> сразу 15, если нажали ещё +
        private void ComputePartial()
        {
            // 2 элемент — оператор
            if (!IsOperator(calcItems[1])) return;

            try
            {
                double left = double.Parse(calcItems[0].Replace(",", "."), CultureInfo.InvariantCulture);
                double right = double.Parse(calcItems[2].Replace(",", "."), CultureInfo.InvariantCulture);
                string op = calcItems[1];

                double result = DoCalc(left, right, op);

                calcItems.Clear();
                string rStr = result.ToString(CultureInfo.InvariantCulture);
                calcItems.Add(rStr);

                displayText = rStr;
                ShowFullExpression();
            }
            catch (Exception ex)
            {
                ShowError($"ошибка: {ex.Message}");
            }
        }
    }
}
