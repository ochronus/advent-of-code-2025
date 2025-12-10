// Advent of Code 2025 - Day 10: Factory
//
// Part 1: Gaussian Elimination over GF(2) for binary toggle problem
// Part 2: Integer Linear Programming with Fraction-Free Gaussian Elimination

open System.IO
open System.Text.RegularExpressions
open System.Collections.Generic

type Machine =
    { TargetLights: byte[]
      NumLights: int
      Buttons: int[] list
      Joltages: int64[] }

// Parsing
module Parser =
    let private extractPattern line =
        let patternMatch = Regex.Match(line, @"\[([.#]+)\]")

        patternMatch.Groups.[1].Value
        |> Seq.map (function
            | '#' -> 1uy
            | _ -> 0uy)
        |> Seq.toArray

    let private extractButtons line =
        Regex.Matches(line, @"\(([0-9,]*)\)")
        |> Seq.cast<Match>
        |> Seq.map (fun m ->
            let content = m.Groups.[1].Value

            if System.String.IsNullOrWhiteSpace(content) then
                [||]
            else
                content.Split(',') |> Array.map int)
        |> Seq.toList

    let private extractJoltages line =
        let joltageMatch = Regex.Match(line, @"\{([0-9,]+)\}")

        if joltageMatch.Success then
            joltageMatch.Groups.[1].Value.Split(',') |> Array.map int64
        else
            [||]

    let parseMachine (line: string) =
        let targetLights = extractPattern line

        { TargetLights = targetLights
          NumLights = targetLights.Length
          Buttons = extractButtons line
          Joltages = extractJoltages line }

// GF(2) Gaussian Elimination for Part 1
module GF2Solver =
    type private Matrix = byte[][]

    let private buildMatrix numLights numButtons (buttons: int[] list) (target: byte[]) =
        let matrix = Array.init numLights (fun _ -> Array.zeroCreate (numButtons + 1))

        buttons
        |> List.iteri (fun buttonIdx lights ->
            lights
            |> Array.iter (fun light ->
                if light < numLights then
                    matrix.[light].[buttonIdx] <- 1uy))

        target |> Array.iteri (fun light value -> matrix.[light].[numButtons] <- value)

        matrix

    let private findPivot (matrix: Matrix) startRow col =
        matrix
        |> Array.indexed
        |> Array.skip startRow
        |> Array.tryPick (fun (row, rowData) -> if rowData.[col] = 1uy then Some row else None)

    let private swapRows (matrix: Matrix) row1 row2 =
        if row1 <> row2 then
            let temp = matrix.[row1]
            matrix.[row1] <- matrix.[row2]
            matrix.[row2] <- temp

    let private eliminateColumn (matrix: Matrix) pivotRow col numButtons =
        for r in 0 .. matrix.Length - 1 do
            if r <> pivotRow && matrix.[r].[col] = 1uy then
                for k in col..numButtons do
                    matrix.[r].[k] <- matrix.[r].[k] ^^^ matrix.[pivotRow].[k]

    let private gaussianElimination (matrix: Matrix) numButtons =
        let pivotMap = Dictionary<int, int>()
        let mutable currentRow = 0

        for col in 0 .. numButtons - 1 do
            match findPivot matrix currentRow col with
            | Some pivotRow when currentRow < matrix.Length ->
                swapRows matrix currentRow pivotRow
                eliminateColumn matrix currentRow col numButtons
                pivotMap.[col] <- currentRow
                currentRow <- currentRow + 1
            | _ -> ()

        pivotMap

    let private isInconsistent (matrix: Matrix) pivotRow numButtons =
        matrix
        |> Array.skip pivotRow
        |> Array.exists (fun row -> row.[numButtons] = 1uy)

    let private identifyFreeVariables numButtons (pivotMap: Dictionary<int, int>) =
        [ 0 .. numButtons - 1 ] |> List.filter (pivotMap.ContainsKey >> not)

    let private backSubstitute (matrix: Matrix) (pivotMap: Dictionary<int, int>) freeVars mask =
        let numButtons = matrix.[0].Length - 1
        let solution = Array.zeroCreate numButtons

        // Set free variables based on mask
        freeVars
        |> List.iteri (fun bitIdx varIdx ->
            if (mask >>> bitIdx) &&& 1 = 1 then
                solution.[varIdx] <- 1uy)

        // Compute pivot variables
        pivotMap
        |> Seq.iter (fun (KeyValue(pivotCol, pivotRow)) ->
            let value =
                freeVars
                |> List.fold
                    (fun acc freeVar ->
                        if matrix.[pivotRow].[freeVar] = 1uy then
                            acc ^^^ solution.[freeVar]
                        else
                            acc)
                    matrix.[pivotRow].[numButtons]

            solution.[pivotCol] <- value)

        solution

    let private countPresses (solution: byte[]) =
        solution
        |> Array.sumBy (function
            | 1uy -> 1
            | _ -> 0)

    let solve (target: byte[]) (buttons: int[] list) numLights =
        let numButtons = buttons.Length
        let matrix = buildMatrix numLights numButtons buttons target
        let pivotMap = gaussianElimination matrix numButtons
        let pivotRow = pivotMap.Count

        if isInconsistent matrix pivotRow numButtons then
            None
        else
            let freeVars = identifyFreeVariables numButtons pivotMap
            let numFree = freeVars.Length

            [ 0 .. (1 <<< numFree) - 1 ]
            |> List.map (backSubstitute matrix pivotMap freeVars >> countPresses)
            |> List.min
            |> Some

// Integer Linear Programming for Part 2
module ILPSolver =
    let private buildMatrix numRequirements numButtons (buttons: int[] list) (target: int64[]) =
        let matrix = Array.init numRequirements (fun _ -> Array.zeroCreate (numButtons + 1))

        buttons
        |> List.iteri (fun buttonIdx reqs ->
            reqs
            |> Array.iter (fun req ->
                if req < numRequirements then
                    matrix.[req].[buttonIdx] <- 1L))

        target |> Array.iteri (fun req value -> matrix.[req].[numButtons] <- value)

        matrix

    let private fractionFreeElimination (matrix: int64[][]) numButtons =
        let pivotMap = Dictionary<int, int>()
        let rowMap = Dictionary<int, int>()
        let mutable currentRow = 0

        for col in 0 .. numButtons - 1 do
            let pivotRow =
                matrix
                |> Array.indexed
                |> Array.skip currentRow
                |> Array.tryPick (fun (row, rowData) -> if rowData.[col] <> 0L then Some row else None)

            match pivotRow with
            | Some row when currentRow < matrix.Length ->
                // Swap rows
                if row <> currentRow then
                    let temp = matrix.[currentRow]
                    matrix.[currentRow] <- matrix.[row]
                    matrix.[row] <- temp

                let pivotVal = matrix.[currentRow].[col]

                // Eliminate below
                for r in currentRow + 1 .. matrix.Length - 1 do
                    if matrix.[r].[col] <> 0L then
                        let factor = matrix.[r].[col]

                        for k in col..numButtons do
                            matrix.[r].[k] <- matrix.[r].[k] * pivotVal - matrix.[currentRow].[k] * factor

                pivotMap.[col] <- currentRow
                rowMap.[currentRow] <- col
                currentRow <- currentRow + 1
            | _ -> ()

        (pivotMap, rowMap, currentRow)

    let private isInconsistent (matrix: int64[][]) pivotRow numButtons =
        matrix
        |> Array.skip pivotRow
        |> Array.exists (fun row -> row.[numButtons] <> 0L)

    let private tryBackSubstitute
        (matrix: int64[][])
        (rowMap: Dictionary<int, int>)
        numButtons
        numPivots
        freeVarValues
        =
        let solution = Array.zeroCreate numButtons

        // Set free variables
        freeVarValues
        |> Array.iteri (fun i (varIdx, value) -> solution.[varIdx] <- value)

        let rec substituteRow r =
            if r < 0 then
                Some solution
            else
                match rowMap.TryGetValue(r) with
                | false, _ -> substituteRow (r - 1)
                | true, pivotCol ->
                    let pivotVal = matrix.[r].[pivotCol]
                    let mutable rhs = matrix.[r].[numButtons]

                    for col in pivotCol + 1 .. numButtons - 1 do
                        rhs <- rhs - matrix.[r].[col] * solution.[col]

                    if rhs % pivotVal = 0L then
                        let value = rhs / pivotVal

                        if value >= 0L then
                            solution.[pivotCol] <- value
                            substituteRow (r - 1)
                        else
                            None
                    else
                        None

        substituteRow (numPivots - 1)

    let private searchSolutions matrix (freeVars: int list) numButtons rowMap numPivots =
        let searchLimit = if freeVars.Length > 1 then 200 else 20000
        let mutable minTotal = None

        let rec search freeIdx (partialVals: (int * int64)[]) =
            if freeIdx = freeVars.Length then
                match tryBackSubstitute matrix rowMap numButtons numPivots partialVals with
                | Some solution ->
                    let total = Array.sum solution

                    minTotal <-
                        match minTotal with
                        | None -> Some total
                        | Some current -> Some(min current total)
                | None -> ()
            else
                let varIdx = freeVars.[freeIdx]

                for value in 0L .. int64 searchLimit do
                    let newVals = Array.append partialVals [| (varIdx, value) |]
                    search (freeIdx + 1) newVals

        search 0 [||]
        minTotal

    let solve (target: int64[]) (buttons: int[] list) =
        let numRequirements = target.Length
        let numButtons = buttons.Length
        let matrix = buildMatrix numRequirements numButtons buttons target
        let (pivotMap, rowMap, numPivots) = fractionFreeElimination matrix numButtons

        if isInconsistent matrix numPivots numButtons then
            None
        else
            let freeVars = [ 0 .. numButtons - 1 ] |> List.filter (pivotMap.ContainsKey >> not)

            if freeVars.IsEmpty then
                tryBackSubstitute matrix rowMap numButtons numPivots [||]
                |> Option.map Array.sum
            else
                searchSolutions matrix freeVars numButtons rowMap numPivots

// Main
let machines =
    File.ReadAllLines(Path.Combine(__SOURCE_DIRECTORY__, "input.txt"))
    |> Array.filter (System.String.IsNullOrWhiteSpace >> not)
    |> Array.map Parser.parseMachine

let part1 =
    machines
    |> Array.choose (fun m -> GF2Solver.solve m.TargetLights m.Buttons m.NumLights)
    |> Array.sum

let part2 =
    machines
    |> Array.choose (fun m ->
        if m.Joltages.Length > 0 then
            ILPSolver.solve m.Joltages m.Buttons
        else
            Some 0L)
    |> Array.sum

printfn "Part 1: %d" part1
printfn "Part 2: %d" part2
