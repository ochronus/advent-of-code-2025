// Advent of Code 2025 - Day 03
// Find maximum joltage from battery banks

open System.IO

// Part 1: Pick exactly 2 batteries to form the largest 2-digit number
let findMaxJoltage (bank: string) =
    let digits = bank |> Seq.toArray
    let n = digits.Length
    
    let mutable maxJoltage = 0
    
    for i in 0 .. n - 2 do
        for j in i + 1 .. n - 1 do
            let joltage = (int (digits.[i]) - int '0') * 10 + (int (digits.[j]) - int '0')
            if joltage > maxJoltage then
                maxJoltage <- joltage
    
    maxJoltage

// Part 2: Pick exactly 12 batteries to form the largest 12-digit number
let findMaxJoltage12 (bank: string) =
    let digits = bank |> Seq.toArray
    let n = digits.Length
    let numToSelect = 12
    
    // Greedy approach: at each step, pick the largest digit possible
    // while ensuring we have enough digits left to complete the selection
    let rec selectDigits startIdx remaining acc =
        if remaining = 0 then
            acc
        else
            // We need to leave enough digits for the remaining selections
            // So we can only look from startIdx to (n - remaining)
            let endIdx = n - remaining
            
            // Find the position of the maximum digit in the valid range
            let mutable maxDigit = '0'
            let mutable maxPos = startIdx
            for i in startIdx .. endIdx do
                if digits.[i] > maxDigit then
                    maxDigit <- digits.[i]
                    maxPos <- i
            
            selectDigits (maxPos + 1) (remaining - 1) (acc + string maxDigit)
    
    let result = selectDigits 0 numToSelect ""
    int64 result

let input = File.ReadAllLines("input.txt") |> Array.filter (fun line -> line.Length > 0)

// Part 1
let totalJoltage = 
    input 
    |> Array.map findMaxJoltage
    |> Array.sum

printfn "Part 1 - Total output joltage: %d" totalJoltage

// Part 2
let totalJoltage2 = 
    input 
    |> Array.map findMaxJoltage12
    |> Array.sum

printfn "Part 2 - Total output joltage: %d" totalJoltage2
