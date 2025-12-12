// Advent of Code 2025 - Day 12: Christmas Tree Farm
// Polyomino packing problem with rotation and reflection
// Run with: dotnet run day12.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Main entry point (top-level statements must come first)
var scriptPath = AppContext.BaseDirectory;
var inputPath = Path.Combine(scriptPath, "input.txt");

// Try current directory if not found
if (!File.Exists(inputPath))
{
    inputPath = Path.Combine(Environment.CurrentDirectory, "input.txt");
}

var lines = File.ReadAllLines(inputPath);
var (shapes, regions) = Parser.Parse(lines);

// Precompute all orientations for each shape
var allOrientations = shapes
    .Select(s => Shape.AllOrientations(s).ToArray())
    .ToArray();

// Count regions where all shapes can fit
int part1 = regions
    .Count(region => Solver.CanFitAll(region, allOrientations));

Console.WriteLine($"Part 1: {part1}");

// Types
record struct Point(int Row, int Col);

record Region(int Width, int Height, int[] ShapeCounts);

// Shape utilities
static class Shape
{
    public static List<Point> Parse(string[] lines)
    {
        var points = new List<Point>();
        for (int row = 0; row < lines.Length; row++)
        {
            for (int col = 0; col < lines[row].Length; col++)
            {
                if (lines[row][col] == '#')
                {
                    points.Add(new Point(row, col));
                }
            }
        }
        return points;
    }

    public static List<Point> Normalize(List<Point> shape)
    {
        int minRow = shape.Min(p => p.Row);
        int minCol = shape.Min(p => p.Col);
        return shape
            .Select(p => new Point(p.Row - minRow, p.Col - minCol))
            .OrderBy(p => p.Row)
            .ThenBy(p => p.Col)
            .ToList();
    }

    public static List<Point> Rotate90(List<Point> shape) =>
        shape.Select(p => new Point(p.Col, -p.Row)).ToList();

    public static List<Point> Reflect(List<Point> shape) =>
        shape.Select(p => new Point(p.Row, -p.Col)).ToList();

    // Using lazy enumeration (IEnumerable) to avoid infinite list issue!
    static IEnumerable<List<Point>> GetRotations(List<Point> shape)
    {
        var current = shape;
        for (int i = 0; i < 4; i++)
        {
            yield return current;
            current = Rotate90(current);
        }
    }

    public static List<List<Point>> AllOrientations(List<Point> shape)
    {
        var orientations = new List<List<Point>>();

        // 4 rotations of original
        foreach (var rotated in GetRotations(shape))
        {
            orientations.Add(Normalize(rotated));
        }

        // 4 rotations of reflected
        foreach (var rotated in GetRotations(Reflect(shape)))
        {
            orientations.Add(Normalize(rotated));
        }

        // Remove duplicates (compare by content)
        return orientations
            .GroupBy(o => string.Join(",", o.Select(p => $"{p.Row}:{p.Col}")))
            .Select(g => g.First())
            .ToList();
    }
}

// Parser
static class Parser
{
    public static (List<List<Point>> Shapes, List<Region> Regions) Parse(string[] lines)
    {
        var text = string.Join("\n", lines);
        var sections = text.Split("\n\n");

        var shapes = sections
            .Where(s => s.Contains(":") && !s.Contains("x"))
            .Select(section =>
            {
                var sectionLines = section.Split('\n')
                    .Skip(1)
                    .Where(l => l.Length > 0)
                    .ToArray();
                return Shape.Parse(sectionLines);
            })
            .ToList();

        var regions = lines
            .Where(line => line.Contains("x") && line.Contains(":"))
            .Select(line =>
            {
                var parts = line.Split(": ");
                var dims = parts[0].Split('x');
                var counts = parts[1].Split(' ').Select(int.Parse).ToArray();
                return new Region(int.Parse(dims[0]), int.Parse(dims[1]), counts);
            })
            .ToList();

        return (shapes, regions);
    }
}

// Solver
static class Solver
{
    static bool CanPlace(bool[,] grid, List<Point> shape, int row, int col, int width, int height)
    {
        foreach (var p in shape)
        {
            int r = row + p.Row;
            int c = col + p.Col;
            if (r < 0 || r >= height || c < 0 || c >= width || grid[r, c])
                return false;
        }
        return true;
    }

    static void PlaceShape(bool[,] grid, List<Point> shape, int row, int col)
    {
        foreach (var p in shape)
            grid[row + p.Row, col + p.Col] = true;
    }

    static void RemoveShape(bool[,] grid, List<Point> shape, int row, int col)
    {
        foreach (var p in shape)
            grid[row + p.Row, col + p.Col] = false;
    }

    public static bool CanFitAll(Region region, List<Point>[][] allOrientations)
    {
        var grid = new bool[region.Height, region.Width];

        // Build list of shape indices to place
        var shapesToPlace = region.ShapeCounts
            .SelectMany((count, idx) => Enumerable.Repeat(idx, count))
            .ToArray();

        // Early exit: check if total cells needed exceeds grid size
        int totalCellsNeeded = region.ShapeCounts
            .Select((count, idx) => count * allOrientations[idx][0].Count)
            .Sum();

        if (totalCellsNeeded > region.Width * region.Height)
            return false;

        if (shapesToPlace.Length == 0)
            return true;

        // Precompute max extents for each orientation
        var orientExtents = allOrientations
            .Select(orients => orients
                .Select(orient => (
                    MaxRow: orient.Max(p => p.Row),
                    MaxCol: orient.Max(p => p.Col)))
                .ToArray())
            .ToArray();

        int numShapes = shapesToPlace.Length;

        // Track state at each depth
        var stateOrient = new int[numShapes];
        var stateRow = new int[numShapes];
        var stateCol = new int[numShapes];
        var placedOrient = new List<Point>?[numShapes];

        // Initialize stateCol to -1 (means start fresh)
        for (int i = 0; i < numShapes; i++)
            stateCol[i] = -1;

        int depth = 0;
        bool found = false;

        while (!found && depth >= 0)
        {
            int shapeIdx = shapesToPlace[depth];
            var orientations = allOrientations[shapeIdx];
            var extents = orientExtents[shapeIdx];

            // If we had placed something at this depth before backtracking, remove it
            if (stateCol[depth] >= 0 && placedOrient[depth] != null)
            {
                RemoveShape(grid, placedOrient[depth]!, stateRow[depth], stateCol[depth]);
            }

            bool placedAtThisDepth = false;
            int oi = stateOrient[depth];
            int startRow = stateRow[depth];
            int startCol = stateCol[depth] + 1; // Try next column (or 0 if was -1)

            while (!placedAtThisDepth && oi < orientations.Length)
            {
                var orientation = orientations[oi];
                var (maxRow, maxCol) = extents[oi];
                int maxR = region.Height - 1 - maxRow;
                int maxC = region.Width - 1 - maxCol;

                int r = startRow;
                int c = startCol;

                while (!placedAtThisDepth && r <= maxR)
                {
                    while (!placedAtThisDepth && c <= maxC)
                    {
                        if (CanPlace(grid, orientation, r, c, region.Width, region.Height))
                        {
                            PlaceShape(grid, orientation, r, c);
                            stateOrient[depth] = oi;
                            stateRow[depth] = r;
                            stateCol[depth] = c;
                            placedOrient[depth] = orientation;
                            placedAtThisDepth = true;
                        }
                        else
                        {
                            c++;
                        }
                    }

                    if (!placedAtThisDepth)
                    {
                        r++;
                        c = 0;
                    }
                }

                if (!placedAtThisDepth)
                {
                    oi++;
                    startRow = 0;
                    startCol = 0;
                }
            }

            if (placedAtThisDepth)
            {
                if (depth == numShapes - 1)
                {
                    found = true;
                }
                else
                {
                    depth++;
                    stateOrient[depth] = 0;
                    stateRow[depth] = 0;
                    stateCol[depth] = -1;
                    placedOrient[depth] = null;
                }
            }
            else
            {
                // Backtrack
                stateOrient[depth] = 0;
                stateRow[depth] = 0;
                stateCol[depth] = -1;
                placedOrient[depth] = null;
                depth--;
            }
        }

        return found;
    }
}
