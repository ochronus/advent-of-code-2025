open System.IO

let parseInput (input: string) =
    input.Trim().TrimEnd(',').Split(',')
    |> Array.map (fun range ->
        let parts = range.Split('-')
        (int64 parts.[0], int64 parts.[1]))

// Generate invalid IDs: numbers formed by repeating a pattern
// Part 1: exactly 2 repetitions (e.g., 1212)
// Part 2: at least 2 repetitions (e.g., 111, 1212, 123123123)
let findInvalidIds (start: int64, stop: int64) exactlyTwoReps =
    let startDigits = start.ToString().Length
    let stopDigits = stop.ToString().Length
    
    [ for digitCount in startDigits .. stopDigits do
        for patternLen in 1 .. digitCount / 2 do
            if digitCount % patternLen = 0 then
                let reps = digitCount / patternLen
                if (exactlyTwoReps && reps = 2) || (not exactlyTwoReps && reps >= 2) then
                    let minPattern = if patternLen = 1 then 1L else pown 10L (patternLen - 1)
                    let maxPattern = pown 10L patternLen - 1L
                    for pattern in minPattern .. maxPattern do
                        let fullStr = String.replicate reps (string pattern)
                        let fullNum = int64 fullStr
                        if fullNum >= start && fullNum <= stop then
                            yield fullNum ]

let solve inputFile exactlyTwoReps =
    let ranges = File.ReadAllText(inputFile) |> parseInput
    ranges
    |> Array.collect (fun r -> findInvalidIds r exactlyTwoReps |> List.toArray)
    |> Array.distinct
    |> Array.sum

let inputPath = Path.Combine(__SOURCE_DIRECTORY__, "input.txt")
printfn "Part 1: %d" (solve inputPath true)
printfn "Part 2: %d" (solve inputPath false)
