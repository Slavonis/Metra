function calculateSumAndFactorial(limit) {
    let totalSum = 0;
    let product = 1;

    for (let i = 1; i <= limit; i++) {
        totalSum += i;
        product *= i;
    }

    if (totalSum > 10) {
        console.log("Sum is large:", totalSum);
    } else {
        console.log("Sum is small:", totalSum);
    }

    return product;
}

const inputVal = 4;
const resultFact = calculateSumAndFactorial(inputVal);
console.log("Factorial result:", resultFact);