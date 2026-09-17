using System;
using System.Collections.Generic;

namespace HalsteadAnalyzer.Core;

/// <summary>Строка частотной таблицы (оператор или операнд + число вхождений).</summary>
public sealed class FrequencyEntry
{
    public int Index { get; set; }        // порядковый номер (j или i)
    public string Symbol { get; set; }    // сам оператор / операнд
    public int Frequency { get; set; }    // f1j или f2i — число вхождений

    public FrequencyEntry(int index, string symbol, int frequency)
    {
        Index = index;
        Symbol = symbol;
        Frequency = frequency;
    }
}

/// <summary>
/// Результат расчёта метрик Холстеда: частотные таблицы операторов и
/// операндов, 6 базовых и 3 расширенные (производные) метрики.
/// </summary>
public sealed class HalsteadResult
{
    // Частотные таблицы (уже отсортированы по убыванию частоты).
    public List<FrequencyEntry> Operators { get; } = new();
    public List<FrequencyEntry> Operands { get; } = new();

    // ---- 6 базовых метрик ----
    public int Eta1 { get; set; }  // η1 — число уникальных операторов
    public int Eta2 { get; set; }  // η2 — число уникальных операндов
    public int N1 { get; set; }    // N1 — общее число операторов
    public int N2 { get; set; }    // N2 — общее число операндов
    // f1j и f2i — это столбцы Frequency в таблицах Operators / Operands.

    // ---- 3 расширенные (производные) метрики ----
    public int Eta => Eta1 + Eta2;                 // η  — словарь программы
    public int N => N1 + N2;                        // N  — длина программы
    public double Volume =>                         // V  — объём программы
        Eta > 0 ? N * Math.Log2(Eta) : 0.0;
}
