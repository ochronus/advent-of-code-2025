using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

var machines = File.ReadAllLines("input.txt")
    .Where(line => !string.IsNullOrWhiteSpace(line))
    .Select(ParseMachine)
    .ToArray();

var part1 = 0;
foreach (var m in machines)
{
    var result = SolveGF2(m.TargetLights, m.Buttons, m.NumLights);
    if (result.HasValue)
        part1 += result.Value;
}

var part2 = 0L;
foreach (var m in machines)
{
    if (m.Joltages.Length > 0)
    {
        var result = SolveILP(m.Joltages, m.Buttons);
        if (result.HasValue)
            part2 += result.Value;
    }
}

Console.WriteLine($"Part 1: {part1}");
Console.WriteLine($"Part 2: {part2}");

(byte[] TargetLights, int NumLights, int[][] Buttons, long[] Joltages) ParseMachine(string line)
{
    var patternMatch = Regex.Match(line, @"\[([.#]+)\]");
    var pattern = patternMatch.Groups[1].Value;
    var targetLights = new byte[pattern.Length];
    for (var i = 0; i < pattern.Length; i++)
        targetLights[i] = pattern[i] == '#' ? (byte)1 : (byte)0;

    var buttonMatches = Regex.Matches(line, @"\(([0-9,]*)\)");
    var buttons = new int[buttonMatches.Count][];
    for (var i = 0; i < buttonMatches.Count; i++)
    {
        var content = buttonMatches[i].Groups[1].Value;
        if (string.IsNullOrWhiteSpace(content))
            buttons[i] = [];
        else
        {
            var parts = content.Split(',');
            buttons[i] = new int[parts.Length];
            for (var j = 0; j < parts.Length; j++)
                buttons[i][j] = int.Parse(parts[j]);
        }
    }

    var joltageMatch = Regex.Match(line, @"\{([0-9,]+)\}");
    long[] joltages;
    if (joltageMatch.Success)
    {
        var parts = joltageMatch.Groups[1].Value.Split(',');
        joltages = new long[parts.Length];
        for (var i = 0; i < parts.Length; i++)
            joltages[i] = long.Parse(parts[i]);
    }
    else
    {
        joltages = [];
    }

    return (targetLights, targetLights.Length, buttons, joltages);
}

int? SolveGF2(byte[] target, int[][] buttons, int numLights)
{
    var numButtons = buttons.Length;
    var matrix = new byte[numLights][];
    for (var i = 0; i < numLights; i++)
        matrix[i] = new byte[numButtons + 1];

    for (var buttonIdx = 0; buttonIdx < numButtons; buttonIdx++)
    {
        var btn = buttons[buttonIdx];
        for (var li = 0; li < btn.Length; li++)
        {
            var light = btn[li];
            if (light < numLights)
                matrix[light][buttonIdx] = 1;
        }
    }

    for (var light = 0; light < numLights; light++)
        matrix[light][numButtons] = target[light];

    var pivotCols = new int[numLights];
    var pivotRows = new int[numButtons];
    for (var i = 0; i < numButtons; i++) pivotRows[i] = -1;
    var pivotCount = 0;
    var currentRow = 0;

    for (var col = 0; col < numButtons; col++)
    {
        var pivotRow = -1;
        for (var r = currentRow; r < numLights; r++)
        {
            if (matrix[r][col] == 1) { pivotRow = r; break; }
        }

        if (pivotRow >= 0 && currentRow < numLights)
        {
            if (pivotRow != currentRow)
            {
                var temp = matrix[currentRow];
                matrix[currentRow] = matrix[pivotRow];
                matrix[pivotRow] = temp;
            }

            var pivotRowData = matrix[currentRow];
            for (var r = 0; r < numLights; r++)
            {
                if (r != currentRow && matrix[r][col] == 1)
                {
                    var rowData = matrix[r];
                    for (var k = col; k <= numButtons; k++)
                        rowData[k] ^= pivotRowData[k];
                }
            }

            pivotCols[pivotCount] = col;
            pivotRows[col] = currentRow;
            pivotCount++;
            currentRow++;
        }
    }

    for (var r = currentRow; r < numLights; r++)
    {
        if (matrix[r][numButtons] == 1)
            return null;
    }

    var freeVars = new int[numButtons - pivotCount];
    var freeCount = 0;
    for (var c = 0; c < numButtons; c++)
    {
        if (pivotRows[c] == -1)
            freeVars[freeCount++] = c;
    }

    var minPresses = int.MaxValue;
    var numCombinations = 1 << freeCount;
    var solution = new byte[numButtons];

    for (var mask = 0; mask < numCombinations; mask++)
    {
        Array.Clear(solution);

        for (var bitIdx = 0; bitIdx < freeCount; bitIdx++)
        {
            if (((mask >> bitIdx) & 1) == 1)
                solution[freeVars[bitIdx]] = 1;
        }

        for (var p = 0; p < pivotCount; p++)
        {
            var pivotCol = pivotCols[p];
            var row = pivotRows[pivotCol];
            var rowData = matrix[row];
            byte value = rowData[numButtons];
            for (var fi = 0; fi < freeCount; fi++)
            {
                var freeVar = freeVars[fi];
                if (rowData[freeVar] == 1)
                    value ^= solution[freeVar];
            }
            solution[pivotCol] = value;
        }

        var presses = 0;
        for (var i = 0; i < numButtons; i++)
            presses += solution[i];

        if (presses < minPresses)
            minPresses = presses;
    }

    return minPresses;
}

long? SolveILP(long[] target, int[][] buttons)
{
    var numRequirements = target.Length;
    var numButtons = buttons.Length;

    var matrix = new long[numRequirements][];
    for (var i = 0; i < numRequirements; i++)
        matrix[i] = new long[numButtons + 1];

    for (var buttonIdx = 0; buttonIdx < numButtons; buttonIdx++)
    {
        var btn = buttons[buttonIdx];
        for (var ri = 0; ri < btn.Length; ri++)
        {
            var req = btn[ri];
            if (req < numRequirements)
                matrix[req][buttonIdx] = 1;
        }
    }

    for (var req = 0; req < numRequirements; req++)
        matrix[req][numButtons] = target[req];

    var pivotCols = new int[numRequirements];
    var rowToPivotCol = new int[numRequirements];
    for (var i = 0; i < numRequirements; i++) rowToPivotCol[i] = -1;
    var colToPivotRow = new int[numButtons];
    for (var i = 0; i < numButtons; i++) colToPivotRow[i] = -1;

    var numPivots = 0;
    var currentRow = 0;

    for (var col = 0; col < numButtons; col++)
    {
        var pivotRow = -1;
        for (var r = currentRow; r < numRequirements; r++)
        {
            if (matrix[r][col] != 0) { pivotRow = r; break; }
        }

        if (pivotRow >= 0 && currentRow < numRequirements)
        {
            if (pivotRow != currentRow)
            {
                var temp = matrix[currentRow];
                matrix[currentRow] = matrix[pivotRow];
                matrix[pivotRow] = temp;
            }

            var pivotVal = matrix[currentRow][col];
            var pivotRowData = matrix[currentRow];

            for (var r = currentRow + 1; r < numRequirements; r++)
            {
                var rowData = matrix[r];
                if (rowData[col] != 0)
                {
                    var factor = rowData[col];
                    for (var k = col; k <= numButtons; k++)
                        rowData[k] = rowData[k] * pivotVal - pivotRowData[k] * factor;
                }
            }

            pivotCols[numPivots] = col;
            colToPivotRow[col] = currentRow;
            rowToPivotCol[currentRow] = col;
            numPivots++;
            currentRow++;
        }
    }

    for (var r = numPivots; r < numRequirements; r++)
    {
        if (matrix[r][numButtons] != 0)
            return null;
    }

    var freeVars = new int[numButtons - numPivots];
    var freeCount = 0;
    for (var c = 0; c < numButtons; c++)
    {
        if (colToPivotRow[c] == -1)
            freeVars[freeCount++] = c;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    long? TryBackSubstitute(long[] solution, long currentSum, long bestSoFar)
    {
        for (var p = numPivots - 1; p >= 0; p--)
        {
            var pivotCol = pivotCols[p];
            var row = colToPivotRow[pivotCol];
            var rowData = matrix[row];

            var pivotVal = rowData[pivotCol];
            var rhs = rowData[numButtons];

            for (var col = pivotCol + 1; col < numButtons; col++)
                rhs -= rowData[col] * solution[col];

            if (rhs % pivotVal != 0)
                return null;

            var value = rhs / pivotVal;
            if (value < 0)
                return null;

            solution[pivotCol] = value;
            currentSum += value;

            // Early termination: if we already exceed the best, stop
            if (bestSoFar >= 0 && currentSum >= bestSoFar)
                return null;
        }

        return currentSum;
    }

    if (freeCount == 0)
    {
        var solution = new long[numButtons];
        return TryBackSubstitute(solution, 0, -1);
    }

    var searchLimit = freeCount > 1 ? 200 : 20000;
    long minTotal = long.MaxValue;
    var solution2 = new long[numButtons];

    if (freeCount == 1)
    {
        var varIdx = freeVars[0];
        for (var value = 0L; value <= searchLimit; value++)
        {
            // Early termination: free var value already exceeds best
            if (value >= minTotal) break;

            Array.Clear(solution2);
            solution2[varIdx] = value;
            var result = TryBackSubstitute(solution2, value, minTotal);
            if (result.HasValue && result.Value < minTotal)
                minTotal = result.Value;
        }
    }
    else if (freeCount == 2)
    {
        var varIdx0 = freeVars[0];
        var varIdx1 = freeVars[1];
        for (var v0 = 0L; v0 <= searchLimit; v0++)
        {
            // Early termination
            if (v0 >= minTotal) break;

            for (var v1 = 0L; v1 <= searchLimit; v1++)
            {
                var freeSum = v0 + v1;
                if (freeSum >= minTotal) break;

                Array.Clear(solution2);
                solution2[varIdx0] = v0;
                solution2[varIdx1] = v1;
                var result = TryBackSubstitute(solution2, freeSum, minTotal);
                if (result.HasValue && result.Value < minTotal)
                    minTotal = result.Value;
            }
        }
    }
    else
    {
        // General case with pruning
        var indices = new long[freeCount];

        void Search(int depth, long currentSum)
        {
            if (currentSum >= minTotal) return;

            if (depth == freeCount)
            {
                Array.Clear(solution2);
                for (var fi = 0; fi < freeCount; fi++)
                    solution2[freeVars[fi]] = indices[fi];

                var result = TryBackSubstitute(solution2, currentSum, minTotal);
                if (result.HasValue && result.Value < minTotal)
                    minTotal = result.Value;
                return;
            }

            for (var value = 0L; value <= searchLimit; value++)
            {
                if (currentSum + value >= minTotal) break;
                indices[depth] = value;
                Search(depth + 1, currentSum + value);
            }
        }

        Search(0, 0);
    }

    return minTotal == long.MaxValue ? null : minTotal;
}
