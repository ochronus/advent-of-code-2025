open System.IO

// Calculate rectangle area between two tiles (treated as opposite corners)
// Area includes all tiles in the bounding rectangle
let rectangleArea (x1, y1) (x2, y2) =
    (abs (x2 - x1) + 1L) * (abs (y2 - y1) + 1L)

// Parse input - read red tile coordinates
let tiles =
    File.ReadAllLines(Path.Combine(__SOURCE_DIRECTORY__, "input.txt"))
    |> Array.map (fun line ->
        match line.Split(',') with
        | [| x; y |] -> (int64 x, int64 y)
        | _ -> failwithf "Invalid input: %s" line)

// Part 1: Find maximum area by checking all pairs of red tiles
let maxArea =
    let n = tiles.Length

    [| for i in 0 .. n - 2 do
           for j in i + 1 .. n - 1 -> rectangleArea tiles[i] tiles[j] |]
    |> Array.max

printfn "Part 1: %d" maxArea

// Part 2: Rectangle must only contain red or green tiles
// Green tiles are on edges between consecutive red tiles and inside the polygon

// Check if a point is on a line segment between two points
let isOnSegment (px, py) (x1, y1) (x2, y2) =
    let minX, maxX = min x1 x2, max x1 x2
    let minY, maxY = min y1 y2, max y1 y2

    if px < minX || px > maxX || py < minY || py > maxY then
        false
    elif x1 = x2 then
        px = x1 && minY <= py && py <= maxY
    elif y1 = y2 then
        py = y1 && minX <= px && px <= maxX
    else
        false // Problem states adjacent tiles are on same row or column

// Point-in-polygon using ray casting algorithm
let isInsidePolygon (px, py) (polygon: (int64 * int64)[]) =
    let n = polygon.Length
    let mutable inside = false
    let mutable j = n - 1

    for i in 0 .. n - 1 do
        let (xi, yi) = polygon[i]
        let (xj, yj) = polygon[j]

        // Check if ray from point to right crosses edge
        if ((yi > py) <> (yj > py)) && (px < (xj - xi) * (py - yi) / (yj - yi) + xi) then
            inside <- not inside

        j <- i

    inside

// Check if a point is on any edge of the polygon
let isOnPolygonBoundary (px, py) (polygon: (int64 * int64)[]) =
    let n = polygon.Length

    [| 0 .. n - 1 |]
    |> Array.exists (fun i ->
        let j = (i + 1) % n
        isOnSegment (px, py) polygon[i] polygon[j])

// Check if a point is inside or on the polygon boundary
let isInsideOrOnPolygon point polygon =
    isInsidePolygon point polygon || isOnPolygonBoundary point polygon

// Find maximum area for rectangles containing only red or green tiles
let maxAreaPart2 =
    let n = tiles.Length

    // Get all unique x and y coordinates from polygon vertices
    let allX = tiles |> Array.map fst |> Set.ofArray
    let allY = tiles |> Array.map snd |> Set.ofArray

    [| for i in 0 .. n - 2 do
           for j in i + 1 .. n - 1 do
               let (x1, y1) = tiles[i]
               let (x2, y2) = tiles[j]

               let minX, maxX = min x1 x2, max x1 x2
               let minY, maxY = min y1 y2, max y1 y2

               // Get critical x and y coordinates within the rectangle bounds
               // These are the polygon vertex coordinates that fall within the rectangle
               let criticalX = allX |> Set.filter (fun x -> x >= minX && x <= maxX) |> Set.toList
               let criticalY = allY |> Set.filter (fun y -> y >= minY && y <= maxY) |> Set.toList

               // Check all critical points (grid formed by polygon vertices within rectangle)
               // For a rectilinear polygon, checking these points is sufficient
               let allPointsValid =
                   criticalX
                   |> List.forall (fun x -> criticalY |> List.forall (fun y -> isInsideOrOnPolygon (x, y) tiles))

               if allPointsValid then
                   yield rectangleArea tiles[i] tiles[j] |]
    |> Array.max

printfn "Part 2: %d" maxAreaPart2
