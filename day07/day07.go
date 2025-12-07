package main

import (
	"bufio"
	"fmt"
	"os"
	"strings"
)

type Pos struct {
	row, col int
}

// Part 1: Count beam splits (beams merge at same position)
func part1(lines []string) int {
	height := len(lines)
	startCol := strings.IndexByte(lines[0], 'S')

	beams := map[int]bool{startCol: true}
	splits := 0

	for row := 0; row < height-1; row++ {
		nextRow := lines[row+1]
		newBeams := make(map[int]bool)

		for col := range beams {
			if nextRow[col] == '^' {
				splits++
				newBeams[col-1] = true
				newBeams[col+1] = true
			} else {
				newBeams[col] = true
			}
		}
		beams = newBeams
	}

	return splits
}

// Part 2: Count timelines (many-worlds interpretation with memoization)
func part2(lines []string) int64 {
	height := len(lines)
	startCol := strings.IndexByte(lines[0], 'S')

	memo := make(map[Pos]int64)

	var timelines func(row, col int) int64
	timelines = func(row, col int) int64 {
		if row == height-1 {
			return 1
		}

		pos := Pos{row, col}
		if val, ok := memo[pos]; ok {
			return val
		}

		var result int64
		if lines[row+1][col] == '^' {
			result = timelines(row+1, col-1) + timelines(row+1, col+1)
		} else {
			result = timelines(row+1, col)
		}

		memo[pos] = result
		return result
	}

	return timelines(0, startCol)
}

func main() {
	file, err := os.Open("input.txt")
	if err != nil {
		fmt.Fprintf(os.Stderr, "Failed to open input.txt: %v\n", err)
		os.Exit(1)
	}
	defer file.Close()

	var lines []string
	scanner := bufio.NewScanner(file)
	for scanner.Scan() {
		line := scanner.Text()
		if len(line) > 0 {
			lines = append(lines, line)
		}
	}

	if err := scanner.Err(); err != nil {
		fmt.Fprintf(os.Stderr, "Error reading file: %v\n", err)
		os.Exit(1)
	}

	fmt.Printf("Part 1: %d\n", part1(lines))
	fmt.Printf("Part 2: %d\n", part2(lines))
}
