// small.js — небольшой тестовый файл для анализатора метрик Холстеда.
// Вычисление факториала и суммы факториалов чисел от 1 до n.

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
        // factorial(k) — вызов внутри выражения: и оператор, и операнд
        sum = sum + factorial(k);
        k = k + 1;
    }
    return sum;
}

var total = sumOfFactorials(5);
// console.log(...) — вызов-инструкция: только оператор
console.log("Сумма факториалов равна " + total);
