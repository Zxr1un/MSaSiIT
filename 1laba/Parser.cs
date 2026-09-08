using System.IO;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

/*
        !!ОСОБЕННОСТИ!!
1. Объявления переменных, классов, функций не являются ни операциями, ни операндами.
2. Но присваивание значений таким переменным/классам учитываются, как и операции внутри объявлений функций.
3. При вызове метода из класса и совпадении его с другим методом другого класса, данные методы будут считаться одним и тем же оператором
4. Импорт библиотек не является операцией, а сами библиотеки не являются операндами.

 
 
 */
namespace MetricCalculator.logic
{
    public class Parser
    {
        public Parser(List<string> bo, List<string> bw) { 
            if(bo != null && bo.Count > 0)
                BasicOperators = bo;
            if(bw != null && bw.Count > 0)
                BasicWords = bw;
            SortOperators();
        }

        //полный список подгружается из файла, тут отладочный синтаксис
        public List<string> BasicOperators = new List<string> { "+", "-", "*", "/", "=", "==", "!=", "<", ">", "<=", ">=", "++", "--", "(", ")", "{", "}", "[", "]", ".", ";", ":"};
        public List<string> BasicOperatorsWords = new List<string> {  "if", "else", "for", "while", "switch", "case",  "break", "continue", "return", "do"};
        public List<string> BasicWords = new List<string> {"public", "static", "void", "package", "import", "class", "int", "double"};

        List<string> tokens = new List<string>();
        public string ResultMetrics = "";


        public Dictionary<string, int> Operators = new Dictionary<string, int>();
        public Dictionary<string, int> Operands = new Dictionary<string, int>();

        public void SortOperators()
        {
            BasicOperators.Sort((a, b) => b.Length.CompareTo(a.Length));
            BasicWords.Sort((a, b) => b.Length.CompareTo(a.Length));
        }
        public string PrintOperators()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string op in BasicOperators)
            {
                sb.Append("[" + op + "] ");
            }
            return sb.ToString();
        }
        public string PrintTokens()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string t in tokens)
            {
                sb.Append("[" + t + "] ");
            }
            return sb.ToString();
        }
        public void LoadInformation()
        {

            try
            {
                string text = File.ReadAllText(@"Syntacsis.txt");
                List<string> parts = text.Split("----------", StringSplitOptions.RemoveEmptyEntries).ToList();
                char[] splitSymbols = new char[] { '\r', '\n', ' ', '\"' };
                BasicOperators = parts[0].Split(splitSymbols, StringSplitOptions.RemoveEmptyEntries).ToList();
                BasicOperatorsWords = parts[1].Split(splitSymbols, StringSplitOptions.RemoveEmptyEntries).ToList();
                BasicWords = parts[2].Split(splitSymbols, StringSplitOptions.RemoveEmptyEntries).ToList();
                BasicOperators.Add("!)"); //Специальный технический оператор парсера
                SortOperators();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Чтение файлов не удалось. Ошибка: {ex.Message}. \n Попробуйте перезапустить программу и проверьте целостность файлов");
            }
            
        }


        public void Parse(string input)
        {
            Tokenize(input);
            BuildTable();
            CalculateMetrics();
        }

        public void Tokenize(string input)
        {
            try
            {
                StringBuilder firstSB = new StringBuilder();
                tokens.Clear();
                //избавляемся от кавычек, лишних пробелов и других спецсимволов
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] == '\"' )
                    {
                        if (!(i - 1 > -1 && input[i - 1] == '\\'))
                        {
                            if (firstSB.Length > 0)
                            {
                                tokens.Add(firstSB.ToString());
                                firstSB.Clear();
                            }
                            firstSB.Append(input[i]);
                            i++;
                            while (i < input.Length)
                            {
                                if (input[i] == '\"' && !(i - 1 > -1 && input[i - 1] == '\\')) break;
                                firstSB.Append(input[i]);
                                i++;
                            }
                            if (i < input.Length)
                            {
                                firstSB.Append(input[i]);
                            }
                            else
                            {
                                throw new Exception("Syntax error: Пропущена закрывающая двойная кавычка.");
                            }
                            if (firstSB.Length > 0)
                            {
                                tokens.Add(firstSB.ToString());
                                firstSB.Clear();
                                continue;
                            }
                            continue;
                        }
                        
                    }
                    if (input[i] == '\'')
                    {
                        if (!(i - 1 > -1 && input[i - 1] == '\\'))
                        {
                            if (firstSB.Length > 0)
                            {
                                tokens.Add(firstSB.ToString());
                                firstSB.Clear();
                            }
                            firstSB.Append(input[i]);
                            i++;
                            while (i < input.Length)
                            {
                                if (input[i] == '\'' && !(i - 1 > -1 && input[i - 1] == '\\')) break;
                                firstSB.Append(input[i]);
                                i++;
                            }
                            if (i < input.Length)
                            {
                                firstSB.Append(input[i]);
                            }
                            else
                            {
                                throw new Exception("Syntax error: Пропущена закрывающая одинарная кавычка.");
                            }
                            if (firstSB.Length > 0)
                            {
                                tokens.Add(firstSB.ToString());
                                firstSB.Clear();
                                continue;
                            }
                            continue;
                        }

                    }
                    if (new char[] { ' ', '\t', '\n', '\r' }.Contains(input[i]))
                    {
                        if (firstSB.Length > 0)
                        {
                            tokens.Add(firstSB.ToString());
                            firstSB.Clear();
                            continue;
                        }
                        continue;
                    }
                    firstSB.Append(input[i]);
                }
                if (firstSB.Length > 0)
                {
                    tokens.Add(firstSB.ToString());
                    firstSB.Clear();
                }
                List<string> correctTokens = new List<string>();
                //откидываем токены пометок спецсимволов
                foreach (string token in tokens)
                {
                    if (token.Length != 0)
                    {
                        if (token.StartsWith("\"") && token.EndsWith("\"") || token.StartsWith("\'") && token.EndsWith("\'"))
                        {
                            correctTokens.Add(token);
                            continue;
                        }
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < token.Length; i++)
                    {
                        bool matched = false;
                        foreach (string op in BasicOperators)
                        {
                            if (token.Substring(i).StartsWith(op))
                            {
                                if(op == ".")
                                {
                                    if(i - 1 > -1 && i + 1 < token.Length && char.IsDigit(token[i - 1]) && char.IsDigit(token[i + 1]))
                                    {
                                        sb.Append(op);
                                        matched = true;
                                        break;
                                    }
                                }
                                if (sb.Length > 0)
                                {
                                    correctTokens.Add(sb.ToString());
                                    sb.Clear();
                                }
                                correctTokens.Add(op);
                                i += op.Length - 1;
                                matched = true;
                                break;
                            }
                        }
                        if (!matched)
                        {
                            sb.Append(token[i]);
                        }
                    }
                    if (sb.Length > 0)
                    {
                        correctTokens.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                tokens = correctTokens;
                UpdateTokens();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
           

        }

        public void UpdateTokens()
        {
            try
            {
                //отработка скобок
                for (int i = 0; i < tokens.Count; i++)
                {
                    string token = tokens[i];
                    if (token == "(")
                    {
                        int including = 0;
                        bool isFunctionCall = false;
                        if (i > 0 && !BasicOperators.Contains(tokens[i - 1]) && tokens[i-1] != " ")
                        {
                            isFunctionCall = true;
                            tokens[i - 1] = tokens[i - 1] + "()";
                            tokens[i] = " ";
                        }
                        int counter = i + 1;
                        while (counter < tokens.Count && (tokens[counter] != ")" || including > 0))
                        {
                            if(tokens[counter] == "(")
                            {
                                including++;
                            }
                            else if(tokens[counter] == ")")
                            {
                                including--;
                            }
                            counter++;
                        }
                        if (counter >= tokens.Count || tokens[counter] != ")")
                        {
                            throw new Exception($"Синтаксическая ошибка: не закрыта '(' в токене {i}");
                        }
                        if (isFunctionCall)
                        {
                            //мой спец символ для окончания вызова функций. После обработки токенов удаляется.
                            tokens[counter] = "!)";
                        }
                        else tokens[counter] = " ";
                    }
                    else if (token == ")")
                    {
                        throw new Exception($"Синтаксическая ошибка: непарная закрывающая скобка ')' в токене {i}");
                    }
                    else if (token == "{")
                    {
                        int counter = i;
                        while (counter < tokens.Count && tokens[counter] != "}")
                        {
                            counter++;
                        }
                        if (counter >= tokens.Count || tokens[counter] != "}")
                        {
                            throw new Exception($"Синтаксическая ошибка: не закрыта '{{' в токене {i}");
                        }
                        tokens[counter] = " ";
                    }
                    else if (token == "}")
                    {
                        throw new Exception($"Синтаксическая ошибка: непарная закрывающая '}}' в токене {i}");
                    }
                    else if (token == "[")
                    {
                        int counter = i;
                        while (counter < tokens.Count && tokens[counter] != "]")
                        {
                            counter++;
                        }
                        if (counter >= tokens.Count || tokens[counter] != "]")
                        {
                            throw new Exception($"Синтаксическая ошибка: не закрыта '[' в токене {i}");
                        }
                        tokens[counter] = " ";
                    }
                    else if (token == "]")
                    {
                        throw new Exception($"Синтаксическая ошибка: непарная закрывающая ']' в токене {i}");
                    }
                    
                }
                //удаление пробелов (места, где стояли скобки) в токенах и обработка do-while
                for (int i = 0; i < tokens.Count; i++)
                {
                    
                    string token = tokens[i];
                    if(token == "case" || token == "case()")
                    {
                        int counter = i;
                        while (counter < tokens.Count && tokens[counter] != ":")
                        {
                            counter++;
                        }
                        if (counter >= tokens.Count || tokens[counter] != ":")
                        {
                            throw new Exception($"Пропущен ':' для 'case' в токене {i}");
                        }
                        tokens[counter] = " ";
                        //tokens[i] += "()";
                    }
                    if(token == "default")
                    {
                        int counter = i;
                        while (counter < tokens.Count && tokens[counter] != ":")
                        {
                            counter++;
                        }
                        if (counter >= tokens.Count || tokens[counter] != ":")
                        {
                            throw new Exception($"Пропущен ':' для 'default' в токене {i}");
                        }
                        tokens[counter] = " ";
                        //tokens[i] += "()";
                    }   
                    if (token == "do")
                    {
                        int counter = i;
                        while (counter < tokens.Count && tokens[counter] != "while()")
                        {
                            counter++;
                        }
                        if (counter >= tokens.Count || tokens[counter] != "while()")
                        {
                            throw new Exception($"Пропущен 'while' для 'do' в токене {i}");
                        }
                        tokens[i] += "While()";
                        tokens[counter] = " ";

                    }
                    if (tokens[i] == " ")
                    {
                        tokens.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
                //удаление после базовых слов объявления переменных
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (BasicWords.Contains(tokens[i]))
                    {
                        int DepthInsideFoo = 0;
                        bool IsSymbolAfterOperator = false; //чтобы оставить ; после присваивания, если оно есть
                        if (tokens[i].EndsWith("()"))
                        {
                            DepthInsideFoo++;
                        }
                        tokens.RemoveAt(i);
                        while (i < tokens.Count && !(tokens[i] == ";" || tokens[i] == "=" || tokens[i] == "{"))
                        {
                            if (tokens[i].EndsWith("()"))
                            {
                                DepthInsideFoo++;
                            }
                            //учитываем сложные конструкции типа a.b.c = 5; и a.b.c();
                            if (i + 1 < tokens.Count && tokens[i + 1] == ".")
                            {
                                int counter = i;
                                while (counter < tokens.Count && tokens[counter] != ";" && tokens[counter] != "=" && tokens[counter] != "{")
                                {
                                    counter++;
                                }
                                if(tokens[counter] == "=")
                                {
                                    IsSymbolAfterOperator = true;
                                    if (DepthInsideFoo > 0)
                                    {
                                        while (counter < tokens.Count && tokens[counter] != "!)")
                                        {
                                            counter++;
                                        }
                                        DepthInsideFoo--;
                                        i = counter;
                                    }
                                    else
                                    {
                                        while (counter < tokens.Count && tokens[counter] != ";")
                                        {
                                            counter++;
                                        }
                                        i = counter;
                                        break;
                                    }

                                        
                                }
                            }
                            //учитываем и обычные мат выражения после объявления;
                            if(i+1 < tokens.Count && tokens[i+1] == "=")
                            {
                                int counter = i;
                                IsSymbolAfterOperator = true;
                                if (DepthInsideFoo > 0)
                                {
                                    while (counter < tokens.Count && tokens[counter] != "!)")
                                    {
                                        counter++;
                                    }
                                    DepthInsideFoo--;
                                    i = counter;
                                }
                                else
                                {
                                    while (counter < tokens.Count && tokens[counter] != ";")
                                    {
                                        counter++;
                                    }
                                    i = counter;
                                    break;
                                }
                            }
                            
                            tokens.RemoveAt(i);
                        }
                        if (i < tokens.Count && tokens[i] == ";" && !IsSymbolAfterOperator)
                        {
                            tokens.RemoveAt(i);
                            i--;
                            IsSymbolAfterOperator = false;
                        }
                    }
                }
                //удаление спец символов окончания вызова функций
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (tokens[i] == "!)")
                    {
                        tokens.RemoveAt(i);
                        i--;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка уменьшения токенов: {ex.Message}");
            }

        }
        public void BuildTable()
        {
            Operands.Clear();
            Operators.Clear();
            foreach (string token in tokens)
            {
                if (BasicOperators.Contains(token) || BasicOperatorsWords.Contains(token) || token.EndsWith("()"))
                {
                    if (Operators.ContainsKey(token))
                    {
                        Operators[token]++;
                    }
                    else
                    {
                        Operators[token] = 1;
                    }
                }
                else
                {
                    if (Operands.ContainsKey(token))
                    {
                        Operands[token]++;
                    }
                    else
                    {
                        Operands[token] = 1;
                    }
                }
            }
        }

        public void CalculateMetrics()
        {
            int Dict = Operators.Count + Operands.Count;
            int progLong = 0;
            foreach(var operators in Operators)
            {
                progLong += operators.Value;
            }
            foreach(var operands in Operands)
            {
                progLong += operands.Value;
            }
            double volume = ((double)progLong * Math.Log2(Dict));
            ResultMetrics = "Метрики программы:\n" +
                            $"Длина программы: {progLong}\n" +
                            $"Словарь программы: {Dict}\n" +
                            $"Объем программы: {Math.Round(volume)}";
        }
    }
}
