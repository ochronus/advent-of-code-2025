var lines = File.ReadAllLines("input.txt");
var h = lines.Length;
var w = lines.Max(s => s.Length);
var grid = lines.Select(s => s.PadRight(w)).ToArray();

char[] Col(int c) => Enumerable.Range(0, h).Select(r => grid[r][c]).ToArray();

bool IsSep(int c) => Col(c).All(ch => ch == ' ');

List<List<int>> GetProblems()
{
    var problems = new List<List<int>>();
    var acc = new List<int>();

    for (var c = 0; c < w; c++)
    {
        if (IsSep(c))
        {
            if (acc.Count > 0)
            {
                problems.Add(acc);
                acc = [];
            }
        }
        else
        {
            acc.Add(c);
        }
    }

    if (acc.Count > 0)
        problems.Add(acc);

    return problems;
}

long Solve(bool vertical, List<int> cols)
{
    Func<long, long, long>? op = null;
    foreach (var c in cols)
    {
        var ch = grid[h - 1][c];
        if (ch == '+') { op = (a, b) => a + b; break; }
        if (ch == '*') { op = (a, b) => a * b; break; }
    }

    List<long> nums;

    if (vertical)
    {
        nums = cols
            .Select(c => new string(Col(c).Take(h - 1).Where(char.IsDigit).ToArray()))
            .Where(s => s.Length > 0)
            .Select(long.Parse)
            .ToList();
    }
    else
    {
        nums = Enumerable.Range(0, h - 1)
            .Select(r => new string(cols.Select(c => grid[r][c]).ToArray()).Trim())
            .Where(s => s.Length > 0)
            .Select(long.Parse)
            .ToList();
    }

    return nums.Aggregate(op!);
}

var problems = GetProblems();

var part1 = problems.Sum(p => Solve(false, p));
Console.WriteLine($"Part 1: {part1}");

var part2 = problems.AsEnumerable().Reverse().Sum(p => Solve(true, p));
Console.WriteLine($"Part 2: {part2}");
