open System.IO

// Day 5: Cafeteria
// Determine which available ingredient IDs are fresh based on ranges

let parseInput (input: string) =
    let parts = input.Trim().Split("\n\n")

    let ranges =
        parts.[0].Split('\n')
        |> Array.map (fun line ->
            let parts = line.Split('-')
            (int64 parts.[0], int64 parts.[1]))

    let ingredients =
        parts.[1].Split('\n')
        |> Array.map int64

    (ranges, ingredients)

let isFresh (ranges: (int64 * int64)[]) (id: int64) =
    ranges |> Array.exists (fun (start, stop) -> id >= start && id <= stop)

let solvePart1 (ranges: (int64 * int64)[]) (ingredients: int64[]) =
    ingredients
    |> Array.filter (isFresh ranges)
    |> Array.length

// Part 2: Merge overlapping ranges and count total unique IDs
let mergeRanges (ranges: (int64 * int64)[]) =
    if Array.isEmpty ranges then
        [||]
    else
        let sorted = ranges |> Array.sortBy fst

        sorted
        |> Array.fold
            (fun acc (start, stop) ->
                match acc with
                | [] -> [ (start, stop) ]
                | (prevStart, prevStop) :: rest ->
                    if start <= prevStop + 1L then
                        // Overlapping or adjacent, merge them
                        (prevStart, max prevStop stop) :: rest
                    else
                        // No overlap, add new range
                        (start, stop) :: acc)
            []
        |> List.rev
        |> List.toArray

let solvePart2 (ranges: (int64 * int64)[]) =
    mergeRanges ranges
    |> Array.sumBy (fun (start, stop) -> stop - start + 1L)

// Check for test mode via command line argument
let args = System.Environment.GetCommandLineArgs()
let isTestMode = args |> Array.exists (fun arg -> arg = "--test" || arg = "-t")

let inputFile = if isTestMode then "test_input.txt" else "input.txt"
let inputPath = Path.Combine(__SOURCE_DIRECTORY__, inputFile)

if isTestMode then
    printfn "Running in TEST mode with %s" inputFile

let (ranges, ingredients) = File.ReadAllText(inputPath) |> parseInput

let part1Result = solvePart1 ranges ingredients
printfn "Part 1: %d" part1Result

let part2Result = solvePart2 ranges
printfn "Part 2: %d" part2Result

if isTestMode then
    let expected1 = 3
    let expected2 = 14L

    if part1Result = expected1 then
        printfn "✓ Part 1 test passed! Expected %d, got %d" expected1 part1Result
    else
        printfn "✗ Part 1 test FAILED! Expected %d, got %d" expected1 part1Result

    if part2Result = expected2 then
        printfn "✓ Part 2 test passed! Expected %d, got %d" expected2 part2Result
    else
        printfn "✗ Part 2 test FAILED! Expected %d, got %d" expected2 part2Result
