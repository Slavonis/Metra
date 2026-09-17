const readline = require('readline');

// Пользовательские подпрограммы (функции)
function calcFactorial(n) {
    if (n <= 1) return 1;
    let result = 1;
    for (let i = 2; i <= n; i++) {
        result *= i;
    }
    return result;
}

function processArrayData(arr) {
    let sum = 0;
    let max = arr[0];
    let index = 0;

    while (index < arr.length) {
        sum += arr[index];
        if (arr[index] > max) {
            max = arr[index];
        }
        index++;
    }

    const average = sum / arr.length;
    return { sum, max, average };
}

function checkEvenOrOdd(num) {
    switch (num % 2) {
        case 0:
            return "even";
        case 1:
        case -1:
            return "odd";
        default:
            return "unknown";
    }
}

function analyzeNumber(value) {
    try {
        if (typeof value !== 'number' || isNaN(value)) {
            throw new Error("Invalid number input");
        }

        const isPositive = value >= 0 && value !== 0;
        const factVal = value <= 10 && value >= 0 ? calcFactorial(value) : null;
        const typeStr = checkEvenOrOdd(Math.floor(value));

        return {
            isPositive: isPositive,
            factorial: factVal,
            parity: typeStr
        };
    } catch (err) {
        console.log("Error occurred:", err.message);
        return null;
    }
}

function main() {
    const numbersList = [5, 12, 3, 8, 20, 1];
    const stats = processArrayData(numbersList);

    console.log("Array Stats - Sum:", stats.sum);
    console.log("Array Stats - Max:", stats.max);
    console.log("Array Stats - Average:", stats.average);

    const targetNum = 5;
    const analysisResult = analyzeNumber(targetNum);

    if (analysisResult !== null) {
        console.log("Analysis for target:", targetNum);
        console.log("Is Positive:", analysisResult.isPositive);
        console.log("Factorial:", analysisResult.factorial);
        console.log("Parity:", analysisResult.parity);
    }

    let counter = 3;
    do {
        console.log("Countdown step:", counter);
        counter--;
    } while (counter > 0);

    process.exit(0);
}

main();