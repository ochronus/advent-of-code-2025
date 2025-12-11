var boxes = File.ReadAllLines("input.txt")
    .Select(line => line.Split(','))
    .Where(parts => parts.Length == 3)
    .Select(parts => (X: long.Parse(parts[0]), Y: long.Parse(parts[1]), Z: long.Parse(parts[2])))
    .ToArray();

var n = boxes.Length;

long DistSq((long X, long Y, long Z) a, (long X, long Y, long Z) b)
{
    var dx = b.X - a.X;
    var dy = b.Y - a.Y;
    var dz = b.Z - a.Z;
    return dx * dx + dy * dy + dz * dz;
}

// All pairs sorted by distance
var pairs = new List<(long Dist, int I, int J)>();
for (var i = 0; i < n - 1; i++)
    for (var j = i + 1; j < n; j++)
        pairs.Add((DistSq(boxes[i], boxes[j]), i, j));

pairs.Sort((a, b) => a.Dist.CompareTo(b.Dist));

// Union-Find with path compression and union by rank
int[] parent = Enumerable.Range(0, n).ToArray();
int[] rank = new int[n];
int components = n;

int Find(int x)
{
    if (parent[x] != x)
        parent[x] = Find(parent[x]);
    return parent[x];
}

bool Union(int x, int y)
{
    var px = Find(x);
    var py = Find(y);
    if (px == py) return false;

    if (rank[px] < rank[py])
        parent[px] = py;
    else if (rank[px] > rank[py])
        parent[py] = px;
    else
    {
        parent[py] = px;
        rank[px]++;
    }
    components--;
    return true;
}

int[] CircuitSizes()
{
    return Enumerable.Range(0, n)
        .GroupBy(Find)
        .Select(g => g.Count())
        .OrderByDescending(x => x)
        .ToArray();
}

// Part 1: Connect 1000 shortest pairs
for (var i = 0; i < 1000; i++)
    Union(pairs[i].I, pairs[i].J);

var sizes = CircuitSizes();
var part1 = (long)sizes[0] * sizes[1] * sizes[2];

// Reset for Part 2
parent = Enumerable.Range(0, n).ToArray();
rank = new int[n];
components = n;

// Part 2: Find last connection that unifies all circuits
(int A, int B) lastPair = (0, 0);
foreach (var (_, a, b) in pairs)
{
    if (Union(a, b) && components == 1)
    {
        lastPair = (a, b);
        break;
    }
}

var part2 = boxes[lastPair.A].X * boxes[lastPair.B].X;

Console.WriteLine($"Part 1: {part1}");
Console.WriteLine($"Part 2: {part2}");
