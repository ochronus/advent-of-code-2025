package main

import (
	"fmt"
	"os"
	"strconv"
	"strings"
)

func main() {
	content, err := os.ReadFile("input.txt")
	if err != nil {
		fmt.Fprintf(os.Stderr, "Failed to read input.txt: %v\n", err)
		os.Exit(1)
	}

	// Split and filter empty lines
	allLines := strings.Split(string(content), "\n")
	var lines []string
	for _, line := range allLines {
		if line != "" {
			lines = append(lines, line)
		}
	}
	h := len(lines)

	// Find max width
	w := 0
	for _, line := range lines {
		if len(line) > w {
			w = len(line)
		}
	}

	// Pad lines to same width
	grid := make([][]rune, h)
	for i, line := range lines {
		row := make([]rune, w)
		for j := 0; j < w; j++ {
			if j < len(line) {
				row[j] = rune(line[j])
			} else {
				row[j] = ' '
			}
		}
		grid[i] = row
	}

	// Get a column as runes
	getCol := func(c int) []rune {
		col := make([]rune, h)
		for r := 0; r < h; r++ {
			col[r] = grid[r][c]
		}
		return col
	}

	// Check if column is all spaces (separator)
	isSep := func(c int) bool {
		col := getCol(c)
		for _, ch := range col {
			if ch != ' ' {
				return false
			}
		}
		return true
	}

	// Find problem column groups
	var problems [][]int
	var current []int

	for c := 0; c < w; c++ {
		if isSep(c) {
			if len(current) > 0 {
				problems = append(problems, current)
				current = nil
			}
		} else {
			current = append(current, c)
		}
	}
	if len(current) > 0 {
		problems = append(problems, current)
	}

	// Solve a problem
	solve := func(cols []int, vertical bool) int64 {
		// Find operator
		var op func(int64, int64) int64
		for _, c := range cols {
			ch := grid[h-1][c]
			if ch == '+' {
				op = func(a, b int64) int64 { return a + b }
				break
			} else if ch == '*' {
				op = func(a, b int64) int64 { return a * b }
				break
			}
		}
		if op == nil {
			return 0
		}

		var nums []int64

		if vertical {
			// Each column is a number read top-to-bottom
			for _, c := range cols {
				var sb strings.Builder
				for r := 0; r < h-1; r++ {
					ch := grid[r][c]
					if ch >= '0' && ch <= '9' {
						sb.WriteRune(ch)
					}
				}
				s := sb.String()
				if s != "" {
					n, err := strconv.ParseInt(s, 10, 64)
					if err == nil {
						nums = append(nums, n)
					}
				}
			}
		} else {
			// Each row is a number read left-to-right
			for r := 0; r < h-1; r++ {
				var sb strings.Builder
				for _, c := range cols {
					sb.WriteRune(grid[r][c])
				}
				s := strings.TrimSpace(sb.String())
				if s != "" {
					n, err := strconv.ParseInt(s, 10, 64)
					if err == nil {
						nums = append(nums, n)
					}
				}
			}
		}

		if len(nums) == 0 {
			return 0
		}

		result := nums[0]
		for i := 1; i < len(nums); i++ {
			result = op(result, nums[i])
		}
		return result
	}

	// Part 1: sum of all problems solved horizontally (left-to-right)
	var part1 int64
	for _, p := range problems {
		part1 += solve(p, false)
	}

	// Part 2: sum of all problems solved vertically (top-to-bottom), in reverse order
	var part2 int64
	for i := len(problems) - 1; i >= 0; i-- {
		part2 += solve(problems[i], true)
	}

	fmt.Printf("Part 1: %d\n", part1)
	fmt.Printf("Part 2: %d\n", part2)
}
