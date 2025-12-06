package main

import (
	"fmt"
	"os"
	"strconv"
	"strings"
)

type Range struct {
	Start int64
	Stop  int64
}

func parseInput(input string) []Range {
	var ranges []Range

	trimmed := strings.TrimSpace(input)
	trimmed = strings.TrimSuffix(trimmed, ",")

	parts := strings.Split(trimmed, ",")
	for _, part := range parts {
		rangeStr := strings.TrimSpace(part)
		if rangeStr == "" {
			continue
		}

		rangeParts := strings.Split(rangeStr, "-")
		if len(rangeParts) != 2 {
			continue
		}

		start, err1 := strconv.ParseInt(rangeParts[0], 10, 64)
		stop, err2 := strconv.ParseInt(rangeParts[1], 10, 64)
		if err1 != nil || err2 != nil {
			continue
		}

		ranges = append(ranges, Range{Start: start, Stop: stop})
	}

	return ranges
}

func countDigits(n int64) int {
	if n == 0 {
		return 1
	}
	count := 0
	for n > 0 {
		n /= 10
		count++
	}
	return count
}

func pow10(exp int) int64 {
	result := int64(1)
	for i := 0; i < exp; i++ {
		result *= 10
	}
	return result
}

func findInvalidIDs(start, stop int64, exactlyTwoReps bool) []int64 {
	var result []int64

	startDigits := countDigits(start)
	stopDigits := countDigits(stop)

	for digitCount := startDigits; digitCount <= stopDigits; digitCount++ {
		for patternLen := 1; patternLen <= digitCount/2; patternLen++ {
			if digitCount%patternLen != 0 {
				continue
			}

			reps := digitCount / patternLen
			if exactlyTwoReps && reps != 2 {
				continue
			}
			if !exactlyTwoReps && reps < 2 {
				continue
			}

			minPattern := int64(1)
			if patternLen > 1 {
				minPattern = pow10(patternLen - 1)
			}
			maxPattern := pow10(patternLen) - 1

			for pattern := minPattern; pattern <= maxPattern; pattern++ {
				// Build the full number by repeating the pattern
				fullNum := int64(0)
				multiplier := pow10(patternLen)
				for i := 0; i < reps; i++ {
					fullNum = fullNum*multiplier + pattern
				}

				if fullNum >= start && fullNum <= stop {
					result = append(result, fullNum)
				}
			}
		}
	}

	return result
}

func solve(ranges []Range, exactlyTwoReps bool) int64 {
	seen := make(map[int64]struct{})

	for _, r := range ranges {
		invalidIDs := findInvalidIDs(r.Start, r.Stop, exactlyTwoReps)
		for _, id := range invalidIDs {
			seen[id] = struct{}{}
		}
	}

	var sum int64
	for id := range seen {
		sum += id
	}

	return sum
}

func main() {
	content, err := os.ReadFile("input.txt")
	if err != nil {
		fmt.Fprintf(os.Stderr, "Failed to read input.txt: %v\n", err)
		os.Exit(1)
	}

	ranges := parseInput(string(content))

	part1 := solve(ranges, true)
	part2 := solve(ranges, false)

	fmt.Printf("Part 1: %d\n", part1)
	fmt.Printf("Part 2: %d\n", part2)
}
