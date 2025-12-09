package main

import (
	"bufio"
	"fmt"
	"os"
	"strconv"
	"strings"
)

type Point struct {
	x, y int64
}

func parseInput(filename string) ([]Point, error) {
	file, err := os.Open(filename)
	if err != nil {
		return nil, err
	}
	defer file.Close()

	var tiles []Point
	scanner := bufio.NewScanner(file)
	for scanner.Scan() {
		parts := strings.Split(scanner.Text(), ",")
		if len(parts) == 2 {
			x, err1 := strconv.ParseInt(parts[0], 10, 64)
			y, err2 := strconv.ParseInt(parts[1], 10, 64)
			if err1 == nil && err2 == nil {
				tiles = append(tiles, Point{x, y})
			}
		}
	}
	return tiles, scanner.Err()
}

func abs(n int64) int64 {
	if n < 0 {
		return -n
	}
	return n
}

func rectangleArea(p1, p2 Point) int64 {
	return (abs(p2.x-p1.x) + 1) * (abs(p2.y-p1.y) + 1)
}

func part1(tiles []Point) int64 {
	n := len(tiles)
	var maxArea int64

	for i := 0; i < n-1; i++ {
		for j := i + 1; j < n; j++ {
			area := rectangleArea(tiles[i], tiles[j])
			if area > maxArea {
				maxArea = area
			}
		}
	}

	return maxArea
}

func isInsidePolygon(p Point, polygon []Point) bool {
	n := len(polygon)
	inside := false
	j := n - 1

	for i := 0; i < n; i++ {
		xi, yi := polygon[i].x, polygon[i].y
		xj, yj := polygon[j].x, polygon[j].y

		if ((yi > p.y) != (yj > p.y)) && (p.x < (xj-xi)*(p.y-yi)/(yj-yi)+xi) {
			inside = !inside
		}
		j = i
	}

	return inside
}

func min(a, b int64) int64 {
	if a < b {
		return a
	}
	return b
}

func max(a, b int64) int64 {
	if a > b {
		return a
	}
	return b
}

func isOnSegment(p, p1, p2 Point) bool {
	minX := min(p1.x, p2.x)
	maxX := max(p1.x, p2.x)
	minY := min(p1.y, p2.y)
	maxY := max(p1.y, p2.y)

	if p.x < minX || p.x > maxX || p.y < minY || p.y > maxY {
		return false
	}

	if p1.x == p2.x {
		return p.x == p1.x && p.y >= minY && p.y <= maxY
	} else if p1.y == p2.y {
		return p.y == p1.y && p.x >= minX && p.x <= maxX
	}

	return false
}

func isOnPolygonBoundary(p Point, polygon []Point) bool {
	n := len(polygon)
	for i := 0; i < n; i++ {
		j := (i + 1) % n
		if isOnSegment(p, polygon[i], polygon[j]) {
			return true
		}
	}
	return false
}

func isInsideOrOnPolygon(p Point, polygon []Point) bool {
	return isInsidePolygon(p, polygon) || isOnPolygonBoundary(p, polygon)
}

func part2(tiles []Point) int64 {
	n := len(tiles)
	var maxArea int64

	// Extract unique x and y coordinates
	xSet := make(map[int64]bool)
	ySet := make(map[int64]bool)
	for _, tile := range tiles {
		xSet[tile.x] = true
		ySet[tile.y] = true
	}

	allX := make([]int64, 0, len(xSet))
	for x := range xSet {
		allX = append(allX, x)
	}

	allY := make([]int64, 0, len(ySet))
	for y := range ySet {
		allY = append(allY, y)
	}

	for i := 0; i < n-1; i++ {
		for j := i + 1; j < n; j++ {
			x1, y1 := tiles[i].x, tiles[i].y
			x2, y2 := tiles[j].x, tiles[j].y

			minX := min(x1, x2)
			maxX := max(x1, x2)
			minY := min(y1, y2)
			maxY := max(y1, y2)

			// Get critical coordinates within bounds
			var criticalX []int64
			for _, x := range allX {
				if x >= minX && x <= maxX {
					criticalX = append(criticalX, x)
				}
			}

			var criticalY []int64
			for _, y := range allY {
				if y >= minY && y <= maxY {
					criticalY = append(criticalY, y)
				}
			}

			// Check all critical points
			allValid := true
			for _, x := range criticalX {
				for _, y := range criticalY {
					if !isInsideOrOnPolygon(Point{x, y}, tiles) {
						allValid = false
						break
					}
				}
				if !allValid {
					break
				}
			}

			if allValid {
				area := rectangleArea(tiles[i], tiles[j])
				if area > maxArea {
					maxArea = area
				}
			}
		}
	}

	return maxArea
}

func main() {
	tiles, err := parseInput("input.txt")
	if err != nil {
		fmt.Fprintf(os.Stderr, "Error reading input: %v\n", err)
		os.Exit(1)
	}

	fmt.Printf("Part 1: %d\n", part1(tiles))
	fmt.Printf("Part 2: %d\n", part2(tiles))
}
