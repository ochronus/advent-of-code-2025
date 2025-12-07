open System.IO
open System.Collections.Generic

let lines = File.ReadAllLines(Path.Combine(__SOURCE_DIRECTORY__, "input.txt"))
let height = lines.Length
let width = lines.[0].Length
let startCol = lines.[0].IndexOf('S')

// Part 1: Count beam splits (beams merge at same position)
let part1 () =
    let rec simulate row beams splits =
        if row >= height - 1 then
            splits
        else
            let nextRow = lines.[row + 1]

            let newBeams, newSplits =
                beams
                |> Set.fold
                    (fun (acc, s) col ->
                        if nextRow.[col] = '^' then
                            (acc |> Set.add (col - 1) |> Set.add (col + 1), s + 1)
                        else
                            (Set.add col acc, s))
                    (Set.empty, splits)

            simulate (row + 1) newBeams newSplits

    simulate 0 (Set.singleton startCol) 0

// Part 2: Count timelines (many-worlds interpretation with memoization)
let part2 () =
    let memo = Dictionary<int * int, int64>()

    let rec timelines row col =
        if row = height - 1 then
            1L
        else
            match memo.TryGetValue((row, col)) with
            | true, v -> v
            | false, _ ->
                let result =
                    match lines.[row + 1].[col] with
                    | '^' -> timelines (row + 1) (col - 1) + timelines (row + 1) (col + 1)
                    | _ -> timelines (row + 1) col

                memo.[(row, col)] <- result
                result

    timelines 0 startCol

printfn "Part 1: %d" (part1 ())
printfn "Part 2: %d" (part2 ())
