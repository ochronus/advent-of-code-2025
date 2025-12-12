// Advent of Code 2025 - Day 12: Christmas Tree Farm
// Polyomino packing problem with rotation and reflection
// Run with: cargo run

use std::collections::HashSet;
use std::fs;
use std::path::Path;

type Point = (i32, i32);
type Shape = Vec<Point>;

#[derive(Debug)]
struct Region {
    width: usize,
    height: usize,
    shape_counts: Vec<usize>,
}

mod shape {
    use super::*;

    pub fn parse(lines: &[&str]) -> Shape {
        let mut points = Vec::new();
        for (row, line) in lines.iter().enumerate() {
            for (col, ch) in line.chars().enumerate() {
                if ch == '#' {
                    points.push((row as i32, col as i32));
                }
            }
        }
        points
    }

    pub fn normalize(shape: &Shape) -> Shape {
        let min_row = shape.iter().map(|p| p.0).min().unwrap_or(0);
        let min_col = shape.iter().map(|p| p.1).min().unwrap_or(0);
        let mut normalized: Shape = shape
            .iter()
            .map(|p| (p.0 - min_row, p.1 - min_col))
            .collect();
        normalized.sort();
        normalized
    }

    fn rotate90(shape: &Shape) -> Shape {
        shape.iter().map(|p| (p.1, -p.0)).collect()
    }

    fn reflect(shape: &Shape) -> Shape {
        shape.iter().map(|p| (p.0, -p.1)).collect()
    }

    // Using iterator to generate rotations lazily - avoiding infinite collection issue!
    fn rotations(shape: &Shape) -> impl Iterator<Item = Shape> {
        let mut current = shape.clone();
        (0..4).map(move |i| {
            if i > 0 {
                current = rotate90(&current);
            }
            current.clone()
        })
    }

    pub fn all_orientations(shape: &Shape) -> Vec<Shape> {
        let mut seen = HashSet::new();
        let mut orientations = Vec::new();

        // 4 rotations of original
        for rotated in rotations(shape) {
            let normalized = normalize(&rotated);
            let key: Vec<_> = normalized.iter().copied().collect();
            if seen.insert(key) {
                orientations.push(normalized);
            }
        }

        // 4 rotations of reflected
        let reflected = reflect(shape);
        for rotated in rotations(&reflected) {
            let normalized = normalize(&rotated);
            let key: Vec<_> = normalized.iter().copied().collect();
            if seen.insert(key) {
                orientations.push(normalized);
            }
        }

        orientations
    }
}

mod parser {
    use super::*;

    pub fn parse(content: &str) -> (Vec<Shape>, Vec<Region>) {
        let sections: Vec<&str> = content.split("\n\n").collect();

        let shapes: Vec<Shape> = sections
            .iter()
            .filter(|s| s.contains(':') && !s.contains('x'))
            .map(|section| {
                let lines: Vec<&str> = section.lines().skip(1).filter(|l| !l.is_empty()).collect();
                shape::parse(&lines)
            })
            .collect();

        let regions: Vec<Region> = content
            .lines()
            .filter(|line| line.contains('x') && line.contains(':'))
            .map(|line| {
                let parts: Vec<&str> = line.split(": ").collect();
                let dims: Vec<&str> = parts[0].split('x').collect();
                let counts: Vec<usize> = parts[1].split(' ').map(|s| s.parse().unwrap()).collect();
                Region {
                    width: dims[0].parse().unwrap(),
                    height: dims[1].parse().unwrap(),
                    shape_counts: counts,
                }
            })
            .collect();

        (shapes, regions)
    }
}

mod solver {
    use super::*;

    fn can_place(
        grid: &[Vec<bool>],
        shape: &Shape,
        row: i32,
        col: i32,
        width: usize,
        height: usize,
    ) -> bool {
        for &(dr, dc) in shape {
            let r = row + dr;
            let c = col + dc;
            if r < 0
                || r >= height as i32
                || c < 0
                || c >= width as i32
                || grid[r as usize][c as usize]
            {
                return false;
            }
        }
        true
    }

    fn place_shape(grid: &mut [Vec<bool>], shape: &Shape, row: i32, col: i32) {
        for &(dr, dc) in shape {
            grid[(row + dr) as usize][(col + dc) as usize] = true;
        }
    }

    fn remove_shape(grid: &mut [Vec<bool>], shape: &Shape, row: i32, col: i32) {
        for &(dr, dc) in shape {
            grid[(row + dr) as usize][(col + dc) as usize] = false;
        }
    }

    pub fn can_fit_all(region: &Region, all_orientations: &[Vec<Shape>]) -> bool {
        let mut grid = vec![vec![false; region.width]; region.height];

        // Build list of shape indices to place
        let shapes_to_place: Vec<usize> = region
            .shape_counts
            .iter()
            .enumerate()
            .flat_map(|(idx, &count)| std::iter::repeat(idx).take(count))
            .collect();

        // Early exit: check if total cells needed exceeds grid size
        let total_cells_needed: usize = region
            .shape_counts
            .iter()
            .enumerate()
            .map(|(idx, &count)| count * all_orientations[idx][0].len())
            .sum();

        if total_cells_needed > region.width * region.height {
            return false;
        }

        if shapes_to_place.is_empty() {
            return true;
        }

        // Precompute max extents for each orientation
        let orient_extents: Vec<Vec<(i32, i32)>> = all_orientations
            .iter()
            .map(|orients| {
                orients
                    .iter()
                    .map(|orient| {
                        let max_row = orient.iter().map(|p| p.0).max().unwrap_or(0);
                        let max_col = orient.iter().map(|p| p.1).max().unwrap_or(0);
                        (max_row, max_col)
                    })
                    .collect()
            })
            .collect();

        let num_shapes = shapes_to_place.len();

        // Track state at each depth
        let mut state_orient = vec![0usize; num_shapes];
        let mut state_row = vec![0i32; num_shapes];
        let mut state_col = vec![-1i32; num_shapes]; // -1 means start fresh
        let mut placed_orient: Vec<Option<usize>> = vec![None; num_shapes];

        let mut depth: i32 = 0;
        let mut found = false;

        while !found && depth >= 0 {
            let d = depth as usize;
            let shape_idx = shapes_to_place[d];
            let orientations = &all_orientations[shape_idx];
            let extents = &orient_extents[shape_idx];

            // If we had placed something at this depth before backtracking, remove it
            if state_col[d] >= 0 {
                if let Some(oi) = placed_orient[d] {
                    remove_shape(&mut grid, &orientations[oi], state_row[d], state_col[d]);
                }
            }

            let mut placed_at_this_depth = false;
            let mut oi = state_orient[d];
            let mut start_row = state_row[d];
            let mut start_col = state_col[d] + 1; // Try next column (or 0 if was -1)

            while !placed_at_this_depth && oi < orientations.len() {
                let orientation = &orientations[oi];
                let (max_row, max_col) = extents[oi];
                let max_r = region.height as i32 - 1 - max_row;
                let max_c = region.width as i32 - 1 - max_col;

                let mut r = start_row;
                let mut c = start_col;

                while !placed_at_this_depth && r <= max_r {
                    while !placed_at_this_depth && c <= max_c {
                        if can_place(&grid, orientation, r, c, region.width, region.height) {
                            place_shape(&mut grid, orientation, r, c);
                            state_orient[d] = oi;
                            state_row[d] = r;
                            state_col[d] = c;
                            placed_orient[d] = Some(oi);
                            placed_at_this_depth = true;
                        } else {
                            c += 1;
                        }
                    }

                    if !placed_at_this_depth {
                        r += 1;
                        c = 0;
                    }
                }

                if !placed_at_this_depth {
                    oi += 1;
                    start_row = 0;
                    start_col = 0;
                }
            }

            if placed_at_this_depth {
                if d == num_shapes - 1 {
                    found = true;
                } else {
                    depth += 1;
                    let next_d = depth as usize;
                    state_orient[next_d] = 0;
                    state_row[next_d] = 0;
                    state_col[next_d] = -1;
                    placed_orient[next_d] = None;
                }
            } else {
                // Backtrack
                state_orient[d] = 0;
                state_row[d] = 0;
                state_col[d] = -1;
                placed_orient[d] = None;
                depth -= 1;
            }
        }

        found
    }
}

fn main() {
    // Find input file
    let input_path = if Path::new("input.txt").exists() {
        "input.txt".to_string()
    } else if Path::new("day12/input.txt").exists() {
        "day12/input.txt".to_string()
    } else {
        panic!("Could not find input.txt");
    };

    let content = fs::read_to_string(&input_path).expect("Failed to read input file");
    let (shapes, regions) = parser::parse(&content);

    // Precompute all orientations for each shape
    let all_orientations: Vec<Vec<Shape>> =
        shapes.iter().map(|s| shape::all_orientations(s)).collect();

    // Count regions where all shapes can fit
    let part1 = regions
        .iter()
        .filter(|region| solver::can_fit_all(region, &all_orientations))
        .count();

    println!("Part 1: {}", part1);
}
