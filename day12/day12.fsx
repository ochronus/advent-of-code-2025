// Advent of Code 2025 - Day 12: Christmas Tree Farm
// Polyomino packing problem with rotation and reflection

open System.IO

type Shape = (int * int) list

type Region =
    { Width: int
      Height: int
      ShapeCounts: int[] }

module Shape =
    let parse (lines: string list) : Shape =
        lines
        |> List.mapi (fun row line ->
            line
            |> Seq.mapi (fun col c -> if c = '#' then Some(row, col) else None)
            |> Seq.choose id
            |> Seq.toList)
        |> List.concat

    let normalize (shape: Shape) : Shape =
        let minRow = shape |> List.map fst |> List.min
        let minCol = shape |> List.map snd |> List.min
        shape |> List.map (fun (r, c) -> (r - minRow, c - minCol)) |> List.sort

    let allOrientations (shape: Shape) : Shape list =
        let rotate90 = List.map (fun (r, c) -> (c, -r))
        let reflect = List.map (fun (r, c) -> (r, -c))

        let rotations s =
            s |> Seq.unfold (fun s -> Some(s, rotate90 s)) |> Seq.take 4 |> Seq.toList

        (rotations shape) @ (rotations (reflect shape))
        |> List.map normalize
        |> List.distinct

module Parser =
    let parse (lines: string[]) =
        let text = String.concat "\n" lines
        let sections = text.Split("\n\n")

        let shapes =
            sections
            |> Array.filter (fun s -> s.Contains(":") && not (s.Contains("x")))
            |> Array.map (fun section ->
                section.Split('\n')
                |> Array.skip 1
                |> Array.filter (fun s -> s.Length > 0)
                |> Array.toList
                |> Shape.parse)
            |> Array.toList

        let regions =
            lines
            |> Array.filter (fun line -> line.Contains("x") && line.Contains(":"))
            |> Array.map (fun line ->
                let parts = line.Split(": ")
                let dims = parts.[0].Split('x')

                { Width = int dims.[0]
                  Height = int dims.[1]
                  ShapeCounts = parts.[1].Split(' ') |> Array.map int })
            |> Array.toList

        (shapes, regions)

module Solver =
    let private canPlace (grid: bool[,]) (shape: Shape) row col width height =
        shape
        |> List.forall (fun (dr, dc) ->
            let r, c = row + dr, col + dc
            r >= 0 && r < height && c >= 0 && c < width && not grid.[r, c])

    let private placeShape (grid: bool[,]) (shape: Shape) row col =
        shape |> List.iter (fun (dr, dc) -> grid.[row + dr, col + dc] <- true)

    let private removeShape (grid: bool[,]) (shape: Shape) row col =
        shape |> List.iter (fun (dr, dc) -> grid.[row + dr, col + dc] <- false)

    let canFitAll (region: Region) (allOrientations: Shape list[]) =
        let grid = Array2D.create region.Height region.Width false

        let shapesToPlace =
            region.ShapeCounts
            |> Array.mapi (fun idx count -> Array.create count idx)
            |> Array.concat

        let totalCellsNeeded =
            region.ShapeCounts
            |> Array.mapi (fun idx count -> count * (allOrientations.[idx].[0].Length))
            |> Array.sum

        if totalCellsNeeded > region.Width * region.Height then
            false
        elif shapesToPlace.Length = 0 then
            true
        else
            // Convert orientations to arrays for efficient indexing
            let orientArrays = allOrientations |> Array.map List.toArray

            // Precompute max extents for each orientation
            let orientExtents =
                orientArrays
                |> Array.map (fun orients ->
                    orients
                    |> Array.map (fun orient ->
                        let maxRow = orient |> List.map fst |> List.max
                        let maxCol = orient |> List.map snd |> List.max
                        (maxRow, maxCol)))

            let numShapes = shapesToPlace.Length

            // Track state at each depth: (orientationIndex, row, col)
            let stateOrient = Array.create numShapes 0
            let stateRow = Array.create numShapes 0
            let stateCol = Array.create numShapes -1 // -1 means start fresh
            let placedOrient: Shape[] = Array.create numShapes Unchecked.defaultof<Shape>

            let mutable depth = 0
            let mutable found = false

            while not found && depth >= 0 do
                let shapeIdx = shapesToPlace.[depth]
                let orientations = orientArrays.[shapeIdx]
                let extents = orientExtents.[shapeIdx]

                // If we had placed something at this depth before backtracking, remove it
                if stateCol.[depth] >= 0 && not (obj.ReferenceEquals(placedOrient.[depth], null)) then
                    removeShape grid placedOrient.[depth] stateRow.[depth] stateCol.[depth]

                let mutable placedAtThisDepth = false
                let mutable oi = stateOrient.[depth]
                let mutable startRow = stateRow.[depth]
                let mutable startCol = stateCol.[depth] + 1 // Try next column (or 0 if was -1)

                while not placedAtThisDepth && oi < orientations.Length do
                    let orientation = orientations.[oi]
                    let (maxRow, maxCol) = extents.[oi]
                    let maxR = region.Height - 1 - maxRow
                    let maxC = region.Width - 1 - maxCol

                    let mutable r = startRow
                    let mutable c = startCol

                    while not placedAtThisDepth && r <= maxR do
                        while not placedAtThisDepth && c <= maxC do
                            if canPlace grid orientation r c region.Width region.Height then
                                placeShape grid orientation r c
                                stateOrient.[depth] <- oi
                                stateRow.[depth] <- r
                                stateCol.[depth] <- c
                                placedOrient.[depth] <- orientation
                                placedAtThisDepth <- true
                            else
                                c <- c + 1

                        if not placedAtThisDepth then
                            r <- r + 1
                            c <- 0

                    if not placedAtThisDepth then
                        oi <- oi + 1
                        startRow <- 0
                        startCol <- 0

                if placedAtThisDepth then
                    if depth = numShapes - 1 then
                        found <- true
                    else
                        depth <- depth + 1
                        stateOrient.[depth] <- 0
                        stateRow.[depth] <- 0
                        stateCol.[depth] <- -1
                        placedOrient.[depth] <- Unchecked.defaultof<Shape>
                else
                    // Backtrack
                    stateOrient.[depth] <- 0
                    stateRow.[depth] <- 0
                    stateCol.[depth] <- -1
                    placedOrient.[depth] <- Unchecked.defaultof<Shape>
                    depth <- depth - 1

            found

// Main
let (shapes, regions) =
    Path.Combine(__SOURCE_DIRECTORY__, "input.txt")
    |> File.ReadAllLines
    |> Parser.parse

let allOrientations = shapes |> List.map Shape.allOrientations |> List.toArray

let part1 =
    regions
    |> List.filter (fun region -> Solver.canFitAll region allOrientations)
    |> List.length

printfn "Part 1: %d" part1
