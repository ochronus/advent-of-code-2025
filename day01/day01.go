package main

import (
	"bufio"
	"fmt"
	"os"
	"strconv"
)

func parseLine(line string) int {
	if len(line) == 0 {
		return 0
	}

	direction := line[0]
	value, err := strconv.Atoi(line[1:])
	if err != nil {
		return 0
	}

	if direction == 'L' {
		return -value
	}
	return value
}

func countCrossings(dial, movement int) int {
	abs := movement
	if abs < 0 {
		abs = -abs
	}
	fullRotations := abs / 100

	sign := 1
	if movement < 0 {
		sign = -1
	}
	remainder := movement - sign*100*fullRotations
	newPos := dial + remainder

	boundaryCrossing := 0
	if remainder < 0 && dial > 0 && newPos <= 0 {
		boundaryCrossing = 1
	} else if remainder > 0 && newPos > 99 {
		boundaryCrossing = 1
	}

	return fullRotations + boundaryCrossing
}

func normalizeDial(dial int) int {
	return ((dial % 100) + 100) % 100
}

func main() {
	file, err := os.Open("input.txt")
	if err != nil {
		fmt.Fprintf(os.Stderr, "Failed to open input.txt: %v\n", err)
		os.Exit(1)
	}
	defer file.Close()

	dial := 50
	zeros := 0
	crossings := 0

	scanner := bufio.NewScanner(file)
	for scanner.Scan() {
		line := scanner.Text()
		if len(line) == 0 {
			continue
		}

		movement := parseLine(line)
		crossings += countCrossings(dial, movement)
		dial = normalizeDial(dial + movement)
		if dial == 0 {
			zeros++
		}
	}

	if err := scanner.Err(); err != nil {
		fmt.Fprintf(os.Stderr, "Error reading file: %v\n", err)
		os.Exit(1)
	}

	fmt.Println(zeros)
	fmt.Println(crossings)
}
