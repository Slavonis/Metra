using System;
using System.IO;
using System.Linq;
using HalsteadAnalyzer.Core;

// Консольный вариант анализатора (для проверки логики без графического интерфейса).
// Использование:  dotnet run --project src/HalsteadAnalyzer.Cli -- <файл.js>

if (args.Length < 1)
{
    Console.WriteLine("Использование: HalsteadAnalyzer.Cli <путь к .js файлу>");
    return 1;
}

string path = args[0];
if (!File.Exists(path))
{
    Console.WriteLine($"Файл не найден: {path}");
    return 1;
}

string code = File.ReadAllText(path);
var r = HalsteadCalculator.Analyze(code);

Console.WriteLine($"Файл: {path}");
Console.WriteLine(new string('=', 64));

Console.WriteLine("\n  ОПЕРАТОРЫ                       |   ОПЕРАНДЫ");
Console.WriteLine("  j  Оператор            f1j      |   i  Операнд             f2i");
Console.WriteLine("  " + new string('-', 60));
int rows = Math.Max(r.Operators.Count, r.Operands.Count);
for (int k = 0; k < rows; k++)
{
    string left = k < r.Operators.Count
        ? $"{r.Operators[k].Index,3}  {r.Operators[k].Symbol,-18} {r.Operators[k].Frequency,3}"
        : new string(' ', 27);
    string right = k < r.Operands.Count
        ? $"{r.Operands[k].Index,3}  {r.Operands[k].Symbol,-18} {r.Operands[k].Frequency,3}"
        : "";
    Console.WriteLine($"  {left}  |   {right}");
}

Console.WriteLine("\n  " + new string('-', 60));
Console.WriteLine("  6 БАЗОВЫХ МЕТРИК:");
Console.WriteLine($"    η1 (словарь операторов)      = {r.Eta1}");
Console.WriteLine($"    η2 (словарь операндов)       = {r.Eta2}");
Console.WriteLine($"    N1 (всего операторов)        = {r.N1}");
Console.WriteLine($"    N2 (всего операндов)         = {r.N2}");
Console.WriteLine($"    f1j — столбец частот операторов (см. таблицу выше)");
Console.WriteLine($"    f2i — столбец частот операндов  (см. таблицу выше)");

Console.WriteLine("\n  3 РАСШИРЕННЫЕ МЕТРИКИ:");
Console.WriteLine($"    η = η1 + η2 (словарь программы) = {r.Eta}");
Console.WriteLine($"    N = N1 + N2 (длина программы)   = {r.N}");
Console.WriteLine($"    V = N·log2(η) (объём программы) = {r.Volume:F2}");

return 0;
