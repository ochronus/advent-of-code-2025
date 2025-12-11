var lines = File.ReadAllLines("input.txt").Where(l => !string.IsNullOrEmpty(l)).ToArray();

int ParseLine(string line)
{
    var direction = line[0];
    var value = int.Parse(line[1..]);
    return direction == 'L' ? -value : value;
}

int CountCrossings(int dial, int movement)
{
    var fullRotations = Math.Abs(movement) / 100;
    var sign = movement >= 0 ? 1 : -1;
    var remainder = movement - sign * 100 * fullRotations;
    var newPos = dial + remainder;

    var boundaryCrossing =
        (remainder < 0 && dial > 0 && newPos <= 0) ? 1 :
        (remainder > 0 && newPos > 99) ? 1 : 0;

    return fullRotations + boundaryCrossing;
}

int NormalizeDial(int dial) => ((dial % 100) + 100) % 100;

var dial = 50;
var zeros = 0;
var crossings = 0;

foreach (var line in lines)
{
    var movement = ParseLine(line);
    var newDial = NormalizeDial(dial + movement);
    if (newDial == 0) zeros++;
    crossings += CountCrossings(dial, movement);
    dial = newDial;
}

Console.WriteLine(zeros);
Console.WriteLine(crossings);
