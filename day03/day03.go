package main

import (
	"bufio"
	"fmt"
	"os"
)

// findMaxJoltage2 picks exactly 2 batteries to form the largest 2-digit number
func findMaxJoltage2(bank string) int {
	n := len(bank)
	maxJoltage := 0

	for i := 0; i < n-1; i++ {
		for j := i + 1; j < n; j++ {
			digitI := int(bank[i] - '0')
			digitJ := int(bank[j] - '0')
			joltage := digitI*10 + digitJ
			if joltage > maxJoltage {
				maxJoltage = joltage
			}
		}
	}

	return maxJoltage
}

// findMaxJoltage12 picks exactly 12 batteries to form the largest 12-digit number
// Uses a greedy approach: at each step, pick the largest digit possible
// while ensuring enough digits remain for the rest of the selection
func findMaxJoltage12(bank string) int64 {
	const numToSelect = 12
	n := len(bank)

	var result int64
	startIdx := 0

	for remaining := numToSelect; remaining > 0; remaining-- {
		// We can only look up to index (n - remaining) to leave enough for the rest
		endIdx := n - remaining

		// Find position of maximum digit in valid range
		maxDigit := byte(0)
		maxPos := startIdx
		for i := startIdx; i <= endIdx; i++ {
			digit := bank[i] - '0'
			if digit > maxDigit {
				maxDigit = digit
				maxPos = i
			}
		}

		result = result*10 + int64(maxDigit)
		startIdx = maxPos + 1
	}

	return result
}

func main() {
	file, err := os.Open("input.txt")
	if err != nil {
		fmt.Fprintf(os.Stderr, "Failed to open input.txt: %v\n", err)
		os.Exit(1)
	}
	defer file.Close()

	var totalPart1 int
	var totalPart2 int64

	scanner := bufio.NewScanner(file)
	for scanner.Scan() {
		line := scanner.Text()
		if len(line) == 0 {
			continue
		}

		totalPart1 += findMaxJoltage2(line)
		totalPart2 += findMaxJoltage12(line)
	}

	if err := scanner.Err(); err != nil {
		fmt.Fprintf(os.Stderr, "Error reading file: %v\n", err)
		os.Exit(1)
	}

	fmt.Printf("Part 1 - Total output joltage: %d\n", totalPart1)
	fmt.Printf("Part 2 - Total output joltage: %d\n", totalPart2)
}
