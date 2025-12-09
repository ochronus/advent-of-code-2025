# Day 9: Movie Theater

## Problem Summary

### Part 1
Find the largest rectangle that can be formed using any two red tiles as opposite corners.

**Input:** List of coordinates representing red tiles on a grid.

**Output:** Maximum area of any rectangle formed by two red tiles as opposite corners.

### Part 2
Find the largest rectangle using two red tiles as opposite corners, but the rectangle can only contain red or green tiles.

**Additional constraints:**
- Red tiles (in input order) form a closed rectilinear polygon
- Green tiles are those on the edges between consecutive red tiles and all tiles inside the polygon
- The list wraps around (first tile connects to last tile)
- Adjacent tiles in the list are always on the same row or column (rectilinear polygon)

## Approach

### Part 1: Unrestricted Rectangles

1. Parse all red tile coordinates from input
2. For each pair of red tiles (i, j):
   - Calculate rectangle area = `(|x2 - x1| + 1) × (|y2 - y1| + 1)`
   - The +1 accounts for inclusive tile counting (discrete grid positions)
3. Return maximum area found

**Time Complexity:** O(n²) where n is the number of red tiles

**Key Insight:** Since we're counting discrete tiles on a grid, the area between coordinates (x1, y1) and (x2, y2) includes all tiles from x1 to x2 and y1 to y2 inclusive, hence the +1 in each dimension.

**Important:** Use 64-bit integers to avoid overflow, as areas can exceed 32-bit integer limits.

### Part 2: Polygon-Constrained Rectangles

1. Parse red tiles to form a rectilinear polygon (vertices in order)
2. For each pair of red tiles as potential rectangle corners:
   - Determine rectangle bounds [minX, maxX] × [minY, maxY]
   - Find all critical points to check: the grid formed by polygon vertex coordinates that fall within the rectangle
   - Verify all critical points are inside or on the polygon boundary
   - If valid, record the area
3. Return maximum valid area

**Critical Point Optimization:** For a rectilinear polygon (only horizontal/vertical edges), we don't need to check every tile in the rectangle. We only need to check points where the polygon's topology can change - i.e., at the x and y coordinates of polygon vertices. This reduces checking from potentially billions of points to a manageable number.

**Point-in-Polygon Testing:**
- **Ray Casting Algorithm:** Cast a horizontal ray from the test point to infinity, count edge crossings
  - Odd crossings = inside
  - Even crossings = outside
- **Boundary Check:** Also check if point lies on any polygon edge
  - For rectilinear polygons, this is simple: check if point is on a horizontal or vertical line segment

**Time Complexity:** O(n² × m × k × p) where:
- n = number of red tiles
- m = average number of critical x-coordinates per rectangle
- k = average number of critical y-coordinates per rectangle
- p = number of polygon edges (= n)

In practice, m and k are much smaller than the full rectangle dimensions, making this efficient.

## Algorithm Details

### Rectangle Area Calculation
```
area = (abs(x2 - x1) + 1) × (abs(y2 - y1) + 1)
```

### Point-in-Polygon (Ray Casting)
```
For each edge (i, i+1) of polygon:
    If horizontal ray from point crosses edge:
        Toggle inside flag
Return inside flag
```

### Point-on-Edge (Rectilinear)
```
For edge from (x1, y1) to (x2, y2):
    If x1 == x2 (vertical edge):
        Point (px, py) is on edge if px == x1 and min(y1,y2) <= py <= max(y1,y2)
    If y1 == y2 (horizontal edge):
        Point (px, py) is on edge if py == y1 and min(x1,x2) <= px <= max(x1,x2)
```

### Critical Points Selection
```
criticalX = {x : x is a polygon vertex x-coord and minX <= x <= maxX}
criticalY = {y : y is a polygon vertex y-coord and minY <= y <= maxY}
checkPoints = criticalX × criticalY (Cartesian product)
```

## Results

- **Part 1:** 4,749,672,288
- **Part 2:** 1,479,665,889

## Key Insights

1. **Integer Overflow:** With coordinates around 98,000, maximum areas can exceed 2³¹, requiring 64-bit integers.

2. **Rectilinear Polygon Property:** Since all edges are axis-aligned, we can optimize validation by only checking polygon vertex coordinates within the rectangle bounds.

3. **Boundary Inclusion:** Points on polygon edges count as "inside" for our purposes, since edge tiles are green.

4. **Grid Discrete Nature:** When calculating areas between coordinates, we must account for inclusive counting (+1 in each dimension).

## Runtime Comparison

All solutions were compiled with optimizations and run on the same machine with the same input.

| Language | Runtime | Relative Speed | Notes |
|----------|---------|----------------|-------|
| **Zig** | 1.43s | 1.00x (fastest) | Compiled with `-O ReleaseFast`, excellent performance |
| **Go** | 2.98s | 2.08x | Excellent performance, simple implementation |
| **Rust** | 18.68s | 13.07x | Compiled with `-O` flag |
| **F#** | 32.19s | 22.51x | Running via `dotnet fsi` (interpreted) |

### Analysis

**Zig's Performance:**
- Fastest implementation with aggressive optimizations
- `-O ReleaseFast` mode provides maximum speed
- Manual memory management with ArrayListUnmanaged
- Clean, low-level code with minimal overhead

**Go's Performance:**
- Excellent performance despite garbage collection
- Efficient memory allocation and runtime
- Simple, idiomatic implementation
- Fast compilation

**Rust Performance:**
- Slower than expected, possibly due to:
  - Using `rustc -O` instead of `cargo build --release`
  - Could benefit from explicit optimization of inner loops
  - May need profiling to identify bottlenecks
- Expected to match Zig performance with proper release build

**F# Performance:**
- Running through the F# interactive interpreter
- Would be significantly faster if compiled to native code
- Functional style remains readable despite lower performance
- Interpreted execution adds significant overhead

### Optimization Opportunities

1. **Part 2 dominates runtime** - The nested loops checking critical points are expensive
2. **Spatial indexing** could reduce Part 2 complexity significantly
3. **Early termination** when a rectangle is found to be invalid
4. **Parallel processing** of rectangle pairs (especially suitable for Go)