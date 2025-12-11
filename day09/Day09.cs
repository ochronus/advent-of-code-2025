using System.Runtime.CompilerServices;

var lines = File.ReadAllLines("input.txt");
var tileCount = 0;
foreach (var line in lines)
    if (line.Contains(',')) tileCount++;

var tilesX = new long[tileCount];
var tilesY = new long[tileCount];
var idx = 0;

foreach (var line in lines)
{
    var comma = line.IndexOf(',');
    if (comma > 0)
    {
        tilesX[idx] = long.Parse(line.AsSpan(0, comma));
        tilesY[idx] = long.Parse(line.AsSpan(comma + 1));
        idx++;
    }
}

var n = tileCount;

// Part 1: Find maximum area by checking all pairs
long maxArea = 0;
for (var i = 0; i < n - 1; i++)
{
    var ix = tilesX[i];
    var iy = tilesY[i];
    for (var j = i + 1; j < n; j++)
    {
        var area = (Math.Abs(tilesX[j] - ix) + 1) * (Math.Abs(tilesY[j] - iy) + 1);
        if (area > maxArea) maxArea = area;
    }
}

Console.WriteLine($"Part 1: {maxArea}");

// Extract unique x and y coordinates
var xSet = new HashSet<long>();
var ySet = new HashSet<long>();
for (var i = 0; i < n; i++)
{
    xSet.Add(tilesX[i]);
    ySet.Add(tilesY[i]);
}

var allX = new long[xSet.Count];
var allY = new long[ySet.Count];
xSet.CopyTo(allX);
ySet.CopyTo(allY);

// Shuffle arrays to match Go's random map iteration (better early termination)
var rng = new Random(42);
Shuffle(allX, rng);
Shuffle(allY, rng);

var allXLen = allX.Length;
var allYLen = allY.Length;

// Part 2: Rectangle must only contain red or green tiles
long maxAreaPart2 = 0;

for (var i = 0; i < n - 1; i++)
{
    var x1 = tilesX[i];
    var y1 = tilesY[i];

    for (var j = i + 1; j < n; j++)
    {
        var x2 = tilesX[j];
        var y2 = tilesY[j];

        var minX = x1 < x2 ? x1 : x2;
        var maxX = x1 > x2 ? x1 : x2;
        var minY = y1 < y2 ? y1 : y2;
        var maxY = y1 > y2 ? y1 : y2;

        // Check all critical points
        var allValid = true;
        for (var xi = 0; xi < allXLen; xi++)
        {
            var x = allX[xi];
            if (x < minX || x > maxX) continue;

            for (var yi = 0; yi < allYLen; yi++)
            {
                var y = allY[yi];
                if (y < minY || y > maxY) continue;

                if (!IsInsideOrOnPolygon(x, y, tilesX, tilesY, n))
                {
                    allValid = false;
                    break;
                }
            }

            if (!allValid) break;
        }

        if (allValid)
        {
            var area = (maxX - minX + 1) * (maxY - minY + 1);
            if (area > maxAreaPart2) maxAreaPart2 = area;
        }
    }
}

Console.WriteLine($"Part 2: {maxAreaPart2}");

static void Shuffle(long[] array, Random rng)
{
    var n = array.Length;
    for (var i = n - 1; i > 0; i--)
    {
        var j = rng.Next(i + 1);
        (array[i], array[j]) = (array[j], array[i]);
    }
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
static bool IsInsideOrOnPolygon(long px, long py, long[] tilesX, long[] tilesY, int n)
{
    // Ray casting for inside check
    var inside = false;
    var jj = n - 1;

    for (var ii = 0; ii < n; ii++)
    {
        var xi = tilesX[ii];
        var yi = tilesY[ii];
        var xj = tilesX[jj];
        var yj = tilesY[jj];

        if ((yi > py) != (yj > py) && px < (xj - xi) * (py - yi) / (yj - yi) + xi)
            inside = !inside;

        jj = ii;
    }

    if (inside) return true;

    // Check if on boundary
    for (var ii = 0; ii < n; ii++)
    {
        var next = ii + 1;
        if (next == n) next = 0;

        var x1 = tilesX[ii];
        var y1 = tilesY[ii];
        var x2 = tilesX[next];
        var y2 = tilesY[next];

        // Vertical edge
        if (x1 == x2 && px == x1)
        {
            var segMinY = y1 < y2 ? y1 : y2;
            var segMaxY = y1 > y2 ? y1 : y2;
            if (py >= segMinY && py <= segMaxY)
                return true;
        }
        // Horizontal edge
        else if (y1 == y2 && py == y1)
        {
            var segMinX = x1 < x2 ? x1 : x2;
            var segMaxX = x1 > x2 ? x1 : x2;
            if (px >= segMinX && px <= segMaxX)
                return true;
        }
    }

    return false;
}
