package main

import (
	"bufio"
	"fmt"
	"os"
)

var directions = [][2]int{
	{-1, 0},  // N
	{-1, 1},  // NE
	{0, 1},   // E
	{1, 1},   // SE
	{1, 0},   // S
	{1, -1},  // SW
	{0, -1},  // W
	{-1, -1}, // NW
}

func parseGrid(filename string) ([][]byte, error) {
	file, err := os.Open(filename)
	if err != nil {
		return nil, err
	}
	defer file.Close()

	var grid [][]byte
	scanner := bufio.NewScanner(file)
	for scanner.Scan() {
		line := scanner.Text()
		if len(line) == 0 {
			continue
		}
		grid = append(grid, []byte(line))
	}

	return grid, scanner.Err()
}

func countAdjacentRolls(grid [][]byte, row, col int) int {
	rows := len(grid)
	cols := len(grid[0])
	count := 0

	for _, dir := range directions {
		newRow := row + dir[0]
		newCol := col + dir[1]

		if newRow < 0 || newRow >= rows || newCol < 0 || newCol >= cols {
			continue
		}

		if grid[newRow][newCol] == '@' {
			count++
		}
	}

	return count
}

type Position struct {
	row, col int
}

func findAccessibleRolls(grid [][]byte) []Position {
	var accessible []Position
	rows := len(grid)
	cols := len(grid[0])

	for row := 0; row < rows; row++ {
		for col := 0; col < cols; col++ {
			if grid[row][col] == '@' && countAdjacentRolls(grid, row, col) < 4 {
				accessible = append(accessible, Position{row, col})
			}
		}
	}

	return accessible
}

func solvePart1(grid [][]byte) int {
	return len(findAccessibleRolls(grid))
}

func solvePart2(grid [][]byte) int {
	totalRemoved := 0

	for {
		accessible := findAccessibleRolls(grid)
		if len(accessible) == 0 {
			break
		}

		for _, pos := range accessible {
			grid[pos.row][pos.col] = '.'
		}

		totalRemoved += len(accessible)
	}

	return totalRemoved
}

func main() {
	grid, err := parseGrid("input.txt")
	if err != nil {
		fmt.Fprintf(os.Stderr, "Failed to read input.txt: %v\n", err)
		os.Exit(1)
	}

	// Part 1: run on original grid first
	part1 := solvePart1(grid)

	// Part 2: modifies grid in place
	part2 := solvePart2(grid)

	fmt.Printf("Part 1: %d\n", part1)
	fmt.Printf("Part 2: %d\n", part2)
}
