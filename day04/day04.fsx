open System.IO

// Day 4: Printing Department
// Find rolls of paper (@) that can be accessed by forklifts
// A roll is accessible if there are fewer than 4 adjacent rolls (8-directional)

let parseGrid (input: string) =
    input.Trim().Split('\n') |> Array.map (fun line -> line.ToCharArray())

let countAdjacentRolls (grid: char[][]) (row: int) (col: int) =
    let rows = grid.Length
    let cols = grid.[0].Length

    // All 8 directions: N, NE, E, SE, S, SW, W, NW
    let directions =
        [| (-1, 0); (-1, 1); (0, 1); (1, 1); (1, 0); (1, -1); (0, -1); (-1, -1) |]

    directions
    |> Array.sumBy (fun (dr, dc) ->
        let newRow = row + dr
        let newCol = col + dc

        if newRow >= 0 && newRow < rows && newCol >= 0 && newCol < cols then
            if grid.[newRow].[newCol] = '@' then 1 else 0
        else
            0)

let findAccessibleRolls (grid: char[][]) =
    let rows = grid.Length
    let cols = grid.[0].Length

    [| for row in 0 .. rows - 1 do
           for col in 0 .. cols - 1 do
               if grid.[row].[col] = '@' then
                   let adjacentCount = countAdjacentRolls grid row col

                   if adjacentCount < 4 then
                       yield (row, col) |]

let solvePart1 (grid: char[][]) =
    findAccessibleRolls grid |> Array.length

// Part 2: Iteratively remove accessible rolls until no more can be removed
let solvePart2 (grid: char[][]) =
    // Make a mutable copy of the grid
    let mutableGrid = grid |> Array.map (fun row -> Array.copy row)

    let mutable totalRemoved = 0
    let mutable keepGoing = true

    while keepGoing do
        let accessible = findAccessibleRolls mutableGrid

        if accessible.Length = 0 then
            keepGoing <- false
        else
            // Remove all accessible rolls
            for (row, col) in accessible do
                mutableGrid.[row].[col] <- '.'

            totalRemoved <- totalRemoved + accessible.Length

    totalRemoved

// Check for test mode via command line argument
let args = System.Environment.GetCommandLineArgs()
let isTestMode = args |> Array.exists (fun arg -> arg = "--test" || arg = "-t")

let inputFile = if isTestMode then "test_input.txt" else "input.txt"
let inputPath = Path.Combine(__SOURCE_DIRECTORY__, inputFile)

if isTestMode then
    printfn "Running in TEST mode with %s" inputFile

let grid = File.ReadAllText(inputPath) |> parseGrid

let part1Result = solvePart1 grid
printfn "Part 1: %d" part1Result

let part2Result = solvePart2 grid
printfn "Part 2: %d" part2Result

if isTestMode then
    let expected1 = 13
    let expected2 = 43

    if part1Result = expected1 then
        printfn "✓ Part 1 test passed! Expected %d, got %d" expected1 part1Result
    else
        printfn "✗ Part 1 test FAILED! Expected %d, got %d" expected1 part1Result

    if part2Result = expected2 then
        printfn "✓ Part 2 test passed! Expected %d, got %d" expected2 part2Result
    else
        printfn "✗ Part 2 test FAILED! Expected %d, got %d" expected2 part2Result

