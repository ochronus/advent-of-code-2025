var input = File.ReadAllText("input.txt");
var parts = input.Trim().Split("\n\n");

var ranges = parts[0].Split('\n')
    .Select(line =>
    {
        var p = line.Split('-');
        return (Start: long.Parse(p[0]), Stop: long.Parse(p[1]));
    })
    .ToArray();

var ingredients = parts[1].Split('\n')
    .Select(long.Parse)
    .ToArray();

bool IsFresh(long id) =>
    ranges.Any(r => id >= r.Start && id <= r.Stop);

// Part 1: Count fresh ingredients
var part1 = ingredients.Count(IsFresh);
Console.WriteLine($"Part 1: {part1}");

// Part 2: Merge overlapping ranges and count total unique IDs
(long Start, long Stop)[] MergeRanges((long Start, long Stop)[] r)
{
    if (r.Length == 0) return [];

    var sorted = r.OrderBy(x => x.Start).ToArray();
    var merged = new List<(long Start, long Stop)> { sorted[0] };

    for (var i = 1; i < sorted.Length; i++)
    {
        var (start, stop) = sorted[i];
        var last = merged[^1];

        if (start <= last.Stop + 1)
        {
            // Overlapping or adjacent, merge them
            merged[^1] = (last.Start, Math.Max(last.Stop, stop));
        }
        else
        {
            merged.Add((start, stop));
        }
    }

    return merged.ToArray();
}

var part2 = MergeRanges(ranges).Sum(r => r.Stop - r.Start + 1);
Console.WriteLine($"Part 2: {part2}");
