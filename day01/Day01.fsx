open System.IO

let parseLine (line: string) =
    let direction = line.[0]
    let value = int (line.Substring(1))
    if direction = 'L' then -value else value

let countCrossings dial movement =
    let fullRotations = abs movement / 100
    let sign = if movement >= 0 then 1 else -1
    let remainder = movement - sign * 100 * fullRotations
    let newPos = dial + remainder

    let boundaryCrossing =
        if remainder < 0 && dial > 0 && newPos <= 0 then 1
        elif remainder > 0 && newPos > 99 then 1
        else 0

    fullRotations + boundaryCrossing

let normalizeDial dial =
    ((dial % 100) + 100) % 100

let processLine (dial, zeros, crossings) line =
    let movement = parseLine line
    let newDial = normalizeDial (dial + movement)
    let newZeros = if newDial = 0 then zeros + 1 else zeros
    let newCrossings = crossings + countCrossings dial movement
    (newDial, newZeros, newCrossings)

let lines =
    File.ReadAllLines("input.txt")
    |> Array.filter ((<>) "")

let (_, result1, result2) =
    Array.fold processLine (50, 0, 0) lines

printfn "%d" result1
printfn "%d" result2
