var ranges = File.ReadAllText("input.txt")
    .Trim()
    .TrimEnd(',')
    .Split(',')
    .Select(range =>
    {
        var parts = range.Split('-');
        return (Start: long.Parse(parts[0]), Stop: long.Parse(parts[1]));
    })
    .ToArray();

IEnumerable<long> FindInvalidIds((long Start, long Stop) range, bool exactlyTwoReps)
{
    var (start, stop) = range;
    var startDigits = start.ToString().Length;
    var stopDigits = stop.ToString().Length;

    for (var digitCount = startDigits; digitCount <= stopDigits; digitCount++)
    {
        for (var patternLen = 1; patternLen <= digitCount / 2; patternLen++)
        {
            if (digitCount % patternLen != 0) continue;

            var reps = digitCount / patternLen;
            if ((exactlyTwoReps && reps != 2) || (!exactlyTwoReps && reps < 2)) continue;

            var minPattern = patternLen == 1 ? 1L : (long)Math.Pow(10, patternLen - 1);
            var maxPattern = (long)Math.Pow(10, patternLen) - 1;

            for (var pattern = minPattern; pattern <= maxPattern; pattern++)
            {
                var fullStr = string.Concat(Enumerable.Repeat(pattern.ToString(), reps));
                var fullNum = long.Parse(fullStr);
                if (fullNum >= start && fullNum <= stop)
                {
                    yield return fullNum;
                }
            }
        }
    }
}

long Solve(bool exactlyTwoReps) =>
    ranges
        .SelectMany(r => FindInvalidIds(r, exactlyTwoReps))
        .Distinct()
        .Sum();

Console.WriteLine($"Part 1: {Solve(true)}");
Console.WriteLine($"Part 2: {Solve(false)}");
