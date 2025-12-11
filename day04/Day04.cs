var input = File.ReadAllText("input.txt");
var grid = input.Trim().Split('\n').Select(line => line.ToCharArray()).ToArray();

int[] dr = [-1, -1, 0, 1, 1, 1, 0, -1];
int[] dc = [0, 1, 1, 1, 0, -1, -1, -1];

int CountAdjacentRolls(char[][] g, int row, int col)
{
    var rows = g.Length;
    var cols = g[0].Length;
    var count = 0;

    for (var i = 0; i < 8; i++)
    {
        var newRow = row + dr[i];
        var newCol = col + dc[i];

        if (newRow >= 0 && newRow < rows && newCol >= 0 && newCol < cols && g[newRow][newCol] == '@')
            count++;
    }

    return count;
}

List<(int Row, int Col)> FindAccessibleRolls(char[][] g)
{
    var rows = g.Length;
    var cols = g[0].Length;
    var accessible = new List<(int, int)>();

    for (var row = 0; row < rows; row++)
    {
        for (var col = 0; col < cols; col++)
        {
            if (g[row][col] == '@' && CountAdjacentRolls(g, row, col) < 4)
                accessible.Add((row, col));
        }
    }

    return accessible;
}

// Part 1: Count accessible rolls
var part1 = FindAccessibleRolls(grid).Count;
Console.WriteLine($"Part 1: {part1}");

// Part 2: Iteratively remove accessible rolls until no more can be removed
var mutableGrid = grid.Select(row => (char[])row.Clone()).ToArray();
var totalRemoved = 0;

while (true)
{
    var accessible = FindAccessibleRolls(mutableGrid);
    if (accessible.Count == 0)
        break;

    foreach (var (row, col) in accessible)
        mutableGrid[row][col] = '.';

    totalRemoved += accessible.Count;
}

Console.WriteLine($"Part 2: {totalRemoved}");
