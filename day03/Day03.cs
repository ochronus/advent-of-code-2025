var input = File.ReadAllLines("input.txt").Where(line => line.Length > 0).ToArray();

// Part 1: Pick exactly 2 batteries to form the largest 2-digit number
int FindMaxJoltage(string bank)
{
    var digits = bank.ToCharArray();
    var n = digits.Length;
    var maxJoltage = 0;

    for (var i = 0; i < n - 1; i++)
    {
        for (var j = i + 1; j < n; j++)
        {
            var joltage = (digits[i] - '0') * 10 + (digits[j] - '0');
            if (joltage > maxJoltage)
                maxJoltage = joltage;
        }
    }

    return maxJoltage;
}

// Part 2: Pick exactly 12 batteries to form the largest 12-digit number
long FindMaxJoltage12(string bank)
{
    var digits = bank.ToCharArray();
    var n = digits.Length;
    const int numToSelect = 12;

    // Greedy approach: at each step, pick the largest digit possible
    // while ensuring we have enough digits left to complete the selection
    var result = new char[numToSelect];
    var startIdx = 0;

    for (var remaining = numToSelect; remaining > 0; remaining--)
    {
        var endIdx = n - remaining;
        var maxDigit = '0';
        var maxPos = startIdx;

        for (var i = startIdx; i <= endIdx; i++)
        {
            if (digits[i] > maxDigit)
            {
                maxDigit = digits[i];
                maxPos = i;
            }
        }

        result[numToSelect - remaining] = maxDigit;
        startIdx = maxPos + 1;
    }

    return long.Parse(new string(result));
}

// Part 1
var totalJoltage = input.Sum(FindMaxJoltage);
Console.WriteLine($"Part 1 - Total output joltage: {totalJoltage}");

// Part 2
var totalJoltage2 = input.Sum(FindMaxJoltage12);
Console.WriteLine($"Part 2 - Total output joltage: {totalJoltage2}");
