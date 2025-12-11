// Advent of Code 2025 - Day 11: Reactor
// Part 1: Count paths from "you" to "out"
// Part 2: Count paths from "svr" to "out" visiting both "dac" and "fft"

open System.IO

let parseGraph lines =
    lines
    |> Array.filter (System.String.IsNullOrWhiteSpace >> not)
    |> Array.map (fun (line: string) ->
        let parts = line.Split(": ")
        parts.[0], parts.[1].Split(' ') |> Array.toList)
    |> Map.ofArray

let countPaths graph start goal =
    let memo = System.Collections.Generic.Dictionary<string, int64>()

    let rec dfs node =
        if node = goal then
            1L
        elif memo.ContainsKey(node) then
            memo.[node]
        else
            let count =
                graph
                |> Map.tryFind node
                |> Option.map (List.sumBy dfs)
                |> Option.defaultValue 0L

            memo.[node] <- count
            count

    dfs start

let countPathsVia graph start goal via1 via2 =
    let memo = System.Collections.Generic.Dictionary<string * int, int64>()

    let rec dfs node mask =
        let mask =
            mask
            |> (if node = via1 then (|||) 1 else id)
            |> (if node = via2 then (|||) 2 else id)

        if node = goal then
            if mask = 3 then 1L else 0L
        else
            let key = (node, mask)

            match memo.TryGetValue(key) with
            | true, count -> count
            | false, _ ->
                let count =
                    graph
                    |> Map.tryFind node
                    |> Option.map (List.sumBy (fun n -> dfs n mask))
                    |> Option.defaultValue 0L

                memo.[key] <- count
                count

    dfs start 0

let graph =
    Path.Combine(__SOURCE_DIRECTORY__, "input.txt")
    |> File.ReadAllLines
    |> parseGraph

printfn "Part 1: %d" (countPaths graph "you" "out")
printfn "Part 2: %d" (countPathsVia graph "svr" "out" "dac" "fft")
