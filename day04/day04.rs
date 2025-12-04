use std::fs;

const DIRECTIONS: [(i32, i32); 8] = [
    (-1, 0),  // N
    (-1, 1),  // NE
    (0, 1),   // E
    (1, 1),   // SE
    (1, 0),   // S
    (1, -1),  // SW
    (0, -1),  // W
    (-1, -1), // NW
];

fn parse_grid(input: &str) -> Vec<Vec<char>> {
    input
        .trim()
        .lines()
        .map(|line| line.chars().collect())
        .collect()
}

fn count_adjacent_rolls(grid: &[Vec<char>], row: usize, col: usize) -> usize {
    let rows = grid.len() as i32;
    let cols = grid[0].len() as i32;

    DIRECTIONS
        .iter()
        .filter(|(dr, dc)| {
            let new_row = row as i32 + dr;
            let new_col = col as i32 + dc;

            new_row >= 0
                && new_row < rows
                && new_col >= 0
                && new_col < cols
                && grid[new_row as usize][new_col as usize] == '@'
        })
        .count()
}

fn find_accessible_rolls(grid: &[Vec<char>]) -> Vec<(usize, usize)> {
    let rows = grid.len();
    let cols = grid[0].len();

    (0..rows)
        .flat_map(|row| (0..cols).map(move |col| (row, col)))
        .filter(|&(row, col)| grid[row][col] == '@' && count_adjacent_rolls(grid, row, col) < 4)
        .collect()
}

fn solve_part1(grid: &[Vec<char>]) -> usize {
    find_accessible_rolls(grid).len()
}

fn solve_part2(grid: &[Vec<char>]) -> usize {
    let mut grid = grid.to_vec();
    let mut total_removed = 0;

    loop {
        let accessible = find_accessible_rolls(&grid);

        if accessible.is_empty() {
            break;
        }

        for (row, col) in &accessible {
            grid[*row][*col] = '.';
        }

        total_removed += accessible.len();
    }

    total_removed
}

fn main() {
    let input = fs::read_to_string("input.txt").expect("Failed to read input.txt");
    let grid = parse_grid(&input);

    println!("Part 1: {}", solve_part1(&grid));
    println!("Part 2: {}", solve_part2(&grid));
}
