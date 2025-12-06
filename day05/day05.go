package main

import (
	"fmt"
	"os"
	"sort"
	"strconv"
	"strings"
)

type Range struct {
	Start int64
	Stop  int64
}

func parseInput(input string) ([]Range, []int64) {
	var ranges []Range
	var ingredients []int64

	parts := strings.Split(strings.TrimSpace(input), "\n\n")
	if len(parts) < 2 {
		return ranges, ingredients
	}

	// Parse ranges
	for _, line := range strings.Split(parts[0], "\n") {
		line = strings.TrimSpace(line)
		if line == "" {
			continue
		}

		rangeParts := strings.Split(line, "-")
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

	// Parse ingredients
	for _, line := range strings.Split(parts[1], "\n") {
		line = strings.TrimSpace(line)
		if line == "" {
			continue
		}

		id, err := strconv.ParseInt(line, 10, 64)
		if err != nil {
			continue
		}

		ingredients = append(ingredients, id)
	}

	return ranges, ingredients
}

func isFresh(ranges []Range, id int64) bool {
	for _, r := range ranges {
		if id >= r.Start && id <= r.Stop {
			return true
		}
	}
	return false
}

func solvePart1(ranges []Range, ingredients []int64) int {
	count := 0
	for _, id := range ingredients {
		if isFresh(ranges, id) {
			count++
		}
	}
	return count
}

func mergeRanges(ranges []Range) []Range {
	if len(ranges) == 0 {
		return nil
	}

	// Create a copy and sort by start
	sorted := make([]Range, len(ranges))
	copy(sorted, ranges)
	sort.Slice(sorted, func(i, j int) bool {
		return sorted[i].Start < sorted[j].Start
	})

	var merged []Range
	for _, r := range sorted {
		if len(merged) == 0 {
			merged = append(merged, r)
		} else {
			last := &merged[len(merged)-1]
			if r.Start <= last.Stop+1 {
				// Overlapping or adjacent, merge them
				if r.Stop > last.Stop {
					last.Stop = r.Stop
				}
			} else {
				// No overlap, add new range
				merged = append(merged, r)
			}
		}
	}

	return merged
}

func solvePart2(ranges []Range) int64 {
	merged := mergeRanges(ranges)

	var total int64
	for _, r := range merged {
		total += r.Stop - r.Start + 1
	}

	return total
}

func main() {
	content, err := os.ReadFile("input.txt")
	if err != nil {
		fmt.Fprintf(os.Stderr, "Failed to read input.txt: %v\n", err)
		os.Exit(1)
	}

	ranges, ingredients := parseInput(string(content))

	part1 := solvePart1(ranges, ingredients)
	part2 := solvePart2(ranges)

	fmt.Printf("Part 1: %d\n", part1)
	fmt.Printf("Part 2: %d\n", part2)
}
