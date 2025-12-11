var lines = File.ReadAllLines("input.txt");
var height = lines.Length;
var width = lines[0].Length;
var startCol = lines[0].IndexOf('S');

// Part 1: Count beam splits (beams merge at same position)
int Part1()
{
    var beams = new HashSet<int> { startCol };
    var splits = 0;

    for (var row = 0; row < height - 1; row++)
    {
        var nextRow = lines[row + 1];
        var newBeams = new HashSet<int>();

        foreach (var col in beams)
        {
            if (nextRow[col] == '^')
            {
                newBeams.Add(col - 1);
                newBeams.Add(col + 1);
                splits++;
            }
            else
            {
                newBeams.Add(col);
            }
        }

        beams = newBeams;
    }

    return splits;
}

// Part 2: Count timelines (many-worlds interpretation with memoization)
long Part2()
{
    var memo = new Dictionary<(int Row, int Col), long>();

    long Timelines(int row, int col)
    {
        if (row == height - 1)
            return 1L;

        if (memo.TryGetValue((row, col), out var cached))
            return cached;

        var result = lines[row + 1][col] == '^'
            ? Timelines(row + 1, col - 1) + Timelines(row + 1, col + 1)
            : Timelines(row + 1, col);

        memo[(row, col)] = result;
        return result;
    }

    return Timelines(0, startCol);
}

Console.WriteLine($"Part 1: {Part1()}");
Console.WriteLine($"Part 2: {Part2()}");
