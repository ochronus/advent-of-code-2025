package main

import (
	"bufio"
	"fmt"
	"os"
	"sort"
	"strconv"
	"strings"
)

type Box struct {
	x, y, z int64
}

type Pair struct {
	distSq int64
	i, j   int
}

type UnionFind struct {
	parent     []int
	rank       []int
	components int
}

func NewUnionFind(n int) *UnionFind {
	parent := make([]int, n)
	for i := range parent {
		parent[i] = i
	}
	return &UnionFind{
		parent:     parent,
		rank:       make([]int, n),
		components: n,
	}
}

func (uf *UnionFind) Find(x int) int {
	if uf.parent[x] != x {
		uf.parent[x] = uf.Find(uf.parent[x]) // Path compression
	}
	return uf.parent[x]
}

func (uf *UnionFind) Union(x, y int) bool {
	px, py := uf.Find(x), uf.Find(y)
	if px == py {
		return false
	}

	// Union by rank
	if uf.rank[px] < uf.rank[py] {
		uf.parent[px] = py
	} else if uf.rank[px] > uf.rank[py] {
		uf.parent[py] = px
	} else {
		uf.parent[py] = px
		uf.rank[px]++
	}
	uf.components--
	return true
}

func (uf *UnionFind) CircuitSizes() []int {
	counts := make(map[int]int)
	for i := range uf.parent {
		counts[uf.Find(i)]++
	}

	sizes := make([]int, 0, len(counts))
	for _, size := range counts {
		sizes = append(sizes, size)
	}
	sort.Sort(sort.Reverse(sort.IntSlice(sizes)))
	return sizes
}

func distSq(a, b Box) int64 {
	dx := b.x - a.x
	dy := b.y - a.y
	dz := b.z - a.z
	return dx*dx + dy*dy + dz*dz
}

func main() {
	file, err := os.Open("input.txt")
	if err != nil {
		fmt.Fprintf(os.Stderr, "Failed to open input.txt: %v\n", err)
		os.Exit(1)
	}
	defer file.Close()

	var boxes []Box
	scanner := bufio.NewScanner(file)
	for scanner.Scan() {
		line := scanner.Text()
		if line == "" {
			continue
		}
		parts := strings.Split(line, ",")
		x, _ := strconv.ParseInt(parts[0], 10, 64)
		y, _ := strconv.ParseInt(parts[1], 10, 64)
		z, _ := strconv.ParseInt(parts[2], 10, 64)
		boxes = append(boxes, Box{x, y, z})
	}

	n := len(boxes)

	// Generate all pairs sorted by distance
	var pairs []Pair
	for i := 0; i < n-1; i++ {
		for j := i + 1; j < n; j++ {
			pairs = append(pairs, Pair{distSq(boxes[i], boxes[j]), i, j})
		}
	}
	sort.Slice(pairs, func(a, b int) bool {
		return pairs[a].distSq < pairs[b].distSq
	})

	// Part 1: Connect 1000 shortest pairs
	uf1 := NewUnionFind(n)
	for i := 0; i < 1000; i++ {
		uf1.Union(pairs[i].i, pairs[i].j)
	}

	sizes := uf1.CircuitSizes()
	part1 := int64(sizes[0]) * int64(sizes[1]) * int64(sizes[2])

	// Part 2: Find last connection that unifies all circuits
	uf2 := NewUnionFind(n)
	var lastA, lastB int
	for _, p := range pairs {
		if uf2.Union(p.i, p.j) && uf2.components == 1 {
			lastA, lastB = p.i, p.j
			break
		}
	}

	part2 := boxes[lastA].x * boxes[lastB].x

	fmt.Printf("Part 1: %d\n", part1)
	fmt.Printf("Part 2: %d\n", part2)
}
