using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HalsteadAnalyzer.Core;

namespace HalsteadAnalyzer.Gui;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        OpenButton.Click += OnOpen;
        AnalyzeButton.Click += OnAnalyze;
        SampleButton.Click += OnSample;
        ClearButton.Click += OnClear;
    }

    // Открытие файла .js через системный диалог.
    private async void OnOpen(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите файл JavaScript",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("JavaScript") { Patterns = new[] { "*.js", "*.mjs", "*.cjs" } },
                new FilePickerFileType("Все файлы") { Patterns = new[] { "*.*" } }
            }
        });

        var file = files?.FirstOrDefault();
        if (file == null) return;

        try
        {
            await using var stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream);
            SourceBox.Text = await reader.ReadToEndAsync();
            FileLabel.Text = "Файл: " + file.Name;
            Analyze();
        }
        catch (Exception ex)
        {
            FileLabel.Text = "Ошибка чтения файла: " + ex.Message;
        }
    }

    private void OnAnalyze(object? sender, RoutedEventArgs e) => Analyze();

    private void OnClear(object? sender, RoutedEventArgs e)
    {
        SourceBox.Text = string.Empty;
        FileLabel.Text = "Файл не выбран";
        OperatorsGrid.ItemsSource = null;
        OperandsGrid.ItemsSource = null;
        OperatorTotals.Text = "η1 = 0     N1 = 0";
        OperandTotals.Text = "η2 = 0     N2 = 0";
        EtaValue.Text = "0";
        LengthValue.Text = "0";
        VolumeValue.Text = "0";
    }

    private void OnSample(object? sender, RoutedEventArgs e)
    {
        SourceBox.Text = SampleCode;
        FileLabel.Text = "Загружен встроенный пример";
        Analyze();
    }

    // Запуск анализа и вывод результатов в таблицы и панель метрик.
    private void Analyze()
    {
        string code = SourceBox.Text ?? string.Empty;
        HalsteadResult r = HalsteadCalculator.Analyze(code);

        OperatorsGrid.ItemsSource = new List<FrequencyEntry>(r.Operators);
        OperandsGrid.ItemsSource = new List<FrequencyEntry>(r.Operands);

        OperatorTotals.Text = $"η1 = {r.Eta1}     N1 = {r.N1}";
        OperandTotals.Text = $"η2 = {r.Eta2}     N2 = {r.N2}";

        EtaValue.Text = r.Eta.ToString();
        LengthValue.Text = r.N.ToString();
        VolumeValue.Text = r.Volume.ToString("F2");
    }

    // Встроенный пример (аналог small.js).
    private const string SampleCode =
@"// Вычисление факториала и суммы факториалов чисел от 1 до n.
function factorial(n) {
    var result = 1;
    for (var i = 2; i <= n; i++) {
        result = result * i;
    }
    return result;
}

function sumOfFactorials(n) {
    var sum = 0;
    var k = 1;
    while (k <= n) {
        sum = sum + factorial(k);
        k = k + 1;
    }
    return sum;
}

var total = sumOfFactorials(5);
console.log(""Сумма факториалов равна "" + total);
";
}
