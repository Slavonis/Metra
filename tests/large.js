// large.js — большой тестовый файл (около 95 строк) для анализатора Холстеда.
// Программа моделирует простую обработку списка студентов: вычисление
// средних баллов, классификацию и формирование итогового отчёта.
// В файле присутствуют все основные операторы языка и пользовательские
// подпрограммы (функции).

var PASS_MARK = 60;
var students = [];
var reportLines = [];

// Пользовательская подпрограмма: создание записи о студенте.
function makeStudent(name, scores) {
    var record = {
        name: name,
        scores: scores,
        average: 0,
        grade: "N/A"
    };
    return record;
}

// Пользовательская подпрограмма: среднее арифметическое массива чисел.
function average(values) {
    var sum = 0;
    var count = values.length;
    if (count === 0) {
        return 0;
    }
    for (var i = 0; i < count; i++) {
        sum += values[i];
    }
    return sum / count;
}

// Пользовательская подпрограмма: перевод балла в буквенную оценку.
function classify(avg) {
    var grade;
    switch (true) {
        case avg >= 90:
            grade = "A";
            break;
        case avg >= 75:
            grade = "B";
            break;
        case avg >= PASS_MARK:
            grade = "C";
            break;
        default:
            grade = "F";
    }
    return grade;
}

// Пользовательская подпрограмма: признак «сдал/не сдал» через тернарный оператор.
function isPassing(avg) {
    return (avg >= PASS_MARK) ? true : false;
}

// Наполнение массива студентов (вызовы makeStudent — часть выражения).
students[0] = makeStudent("Ann", [92, 88, 95]);
students[1] = makeStudent("Bob", [55, 70, 48]);
students[2] = makeStudent("Cid", [76, 61, 84]);

var index = 0;
do {
    var s = students[index];
    // average(...) и classify(...) — вызовы внутри выражений.
    s.average = average(s.scores);
    s.grade = classify(s.average);

    var status = isPassing(s.average) && s.average > 0;
    var flag = status ? "PASS" : "FAIL";

    // Побитовые операции для формирования условного кода студента.
    var code = (index << 4) | (s.grade.length & 0x0F);

    var line = s.name + ": avg=" + s.average + ", grade=" + s.grade +
               ", " + flag + ", code=" + code;
    reportLines.push(line);
    index = index + 1;
} while (index < students.length);

// Подсчёт числа сдавших с помощью цикла и вызова-выражения.
var passed = 0;
for (var j = 0; j < students.length; j++) {
    if (isPassing(students[j].average)) {
        passed = passed + 1;
    } else {
        passed = passed - 0;
    }
}

var summary = "Passed " + passed + " of " + students.length;
reportLines.push(summary);

// Вызовы-инструкции console.log — только операторы.
console.log(reportLines.join("\n"));
console.log(summary);
