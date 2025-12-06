open System
open System.IO

let lines = File.ReadAllLines "input.txt"
let h, w = lines.Length, lines |> Array.map (_.Length) |> Array.max
let grid = lines |> Array.map (fun s -> s.PadRight w)

let col c =
    [| for r in 0 .. h - 1 -> grid.[r].[c] |]

let isSep c = col c |> Array.forall ((=) ' ')

let problems =
    let rec go c acc ps =
        if c >= w then
            if acc = [] then ps else List.rev acc :: ps
        elif isSep c then
            if acc = [] then
                go (c + 1) [] ps
            else
                go (c + 1) [] (List.rev acc :: ps)
        else
            go (c + 1) (c :: acc) ps

    go 0 [] [] |> List.rev

let solve vertical cols =
    let op =
        cols
        |> List.pick (fun c ->
            match grid.[h - 1].[c] with
            | '+' -> Some (+)
            | '*' -> Some (*)
            | _ -> None)

    let nums =
        if vertical then
            cols
            |> List.choose (fun c ->
                let s = String(col c |> Array.take (h - 1) |> Array.filter Char.IsDigit)
                if s = "" then None else Some(int64 s))
        else
            [ for r in 0 .. h - 2 ->
                  let s =
                      cols |> List.map (fun c -> grid.[r].[c]) |> Array.ofList |> String |> _.Trim()

                  if s = "" then None else Some(int64 s) ]
            |> List.choose id

    nums |> List.reduce op

printfn "Part 1: %d" (problems |> List.sumBy (solve false))
printfn "Part 2: %d" (problems |> List.rev |> List.sumBy (solve true))
