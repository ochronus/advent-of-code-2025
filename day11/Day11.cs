var graph = File.ReadAllLines("input.txt")
    .Where(line => !string.IsNullOrWhiteSpace(line))
    .Select(line =>
    {
        var parts = line.Split(": ");
        return (Node: parts[0], Neighbors: parts[1].Split(' '));
    })
    .ToDictionary(x => x.Node, x => x.Neighbors);

long CountPaths(string start, string goal)
{
    var memo = new Dictionary<string, long>();

    long Dfs(string node)
    {
        if (node == goal) return 1L;
        if (memo.TryGetValue(node, out var cached)) return cached;

        long count = 0;
        if (graph.TryGetValue(node, out var neighbors))
        {
            foreach (var n in neighbors)
                count += Dfs(n);
        }

        memo[node] = count;
        return count;
    }

    return Dfs(start);
}

long CountPathsVia(string start, string goal, string via1, string via2)
{
    var memo = new Dictionary<(string, int), long>();

    long Dfs(string node, int mask)
    {
        if (node == via1) mask |= 1;
        if (node == via2) mask |= 2;

        if (node == goal)
            return mask == 3 ? 1L : 0L;

        var key = (node, mask);
        if (memo.TryGetValue(key, out var cached)) return cached;

        long count = 0;
        if (graph.TryGetValue(node, out var neighbors))
        {
            foreach (var n in neighbors)
                count += Dfs(n, mask);
        }

        memo[key] = count;
        return count;
    }

    return Dfs(start, 0);
}

Console.WriteLine($"Part 1: {CountPaths("you", "out")}");
Console.WriteLine($"Part 2: {CountPathsVia("svr", "out", "dac", "fft")}");
