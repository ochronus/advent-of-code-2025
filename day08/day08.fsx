open System.IO

// Parse input
let boxes =
    File.ReadAllLines(Path.Combine(__SOURCE_DIRECTORY__, "input.txt"))
    |> Array.choose (fun line ->
        match line.Split(',') with
        | [| x; y; z |] -> Some(int64 x, int64 y, int64 z)
        | _ -> None)

let n = boxes.Length

// Squared distance (avoids floating point)
let distSq (x1, y1, z1) (x2, y2, z2) =
    let dx, dy, dz = x2 - x1, y2 - y1, z2 - z1
    dx * dx + dy * dy + dz * dz

// All pairs sorted by distance
let pairs =
    [| for i in 0 .. n - 2 do
           for j in i + 1 .. n - 1 -> (distSq boxes[i] boxes[j], i, j) |]
    |> Array.sortBy (fun (d, _, _) -> d)

// Union-Find with path compression and union by rank
type UnionFind(size: int) =
    let parent = Array.init size id
    let rank = Array.zeroCreate<int> size
    let mutable components = size

    let rec find x =
        if parent[x] <> x then
            parent[x] <- find parent[x]

        parent[x]

    member _.Union(x, y) =
        let px, py = find x, find y

        if px = py then
            false
        else
            if rank[px] < rank[py] then
                parent[px] <- py
            elif rank[px] > rank[py] then
                parent[py] <- px
            else
                parent[py] <- px
                rank[px] <- rank[px] + 1

            components <- components - 1
            true

    member _.Find(x) = find x
    member _.Components = components

    member _.CircuitSizes() =
        [| 0 .. size - 1 |]
        |> Array.countBy find
        |> Array.map snd
        |> Array.sortDescending

// Part 1: Connect 1000 shortest pairs
let uf1 = UnionFind(n)
pairs[..999] |> Array.iter (fun (_, a, b) -> uf1.Union(a, b) |> ignore)

let sizes = uf1.CircuitSizes()
let part1 = int64 sizes[0] * int64 sizes[1] * int64 sizes[2]

// Part 2: Find last connection that unifies all circuits
let uf2 = UnionFind(n)

let lastPair =
    pairs
    |> Array.pick (fun (_, a, b) ->
        if uf2.Union(a, b) && uf2.Components = 1 then
            Some(a, b)
        else
            None)

let (x1, _, _), (x2, _, _) = boxes[fst lastPair], boxes[snd lastPair]
let part2 = x1 * x2

printfn "Part 1: %d" part1
printfn "Part 2: %d" part2
