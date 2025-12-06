use std::fs;

fn main() {
    let input = fs::read_to_string("input.txt").expect("Failed to read input.txt");
    let lines: Vec<&str> = input.lines().collect();
    let h = lines.len();
    let w = lines.iter().map(|l| l.len()).max().unwrap_or(0);

    // Pad lines to same width
    let grid: Vec<Vec<char>> = lines
        .iter()
        .map(|l| {
            let mut row: Vec<char> = l.chars().collect();
            row.resize(w, ' ');
            row
        })
        .collect();

    // Get a column as chars
    let col = |c: usize| -> Vec<char> { (0..h).map(|r| grid[r][c]).collect() };

    // Check if column is all spaces (separator)
    let is_sep = |c: usize| col(c).iter().all(|&ch| ch == ' ');

    // Find problem column groups
    let mut problems: Vec<Vec<usize>> = Vec::new();
    let mut current: Vec<usize> = Vec::new();

    for c in 0..w {
        if is_sep(c) {
            if !current.is_empty() {
                problems.push(current.clone());
                current.clear();
            }
        } else {
            current.push(c);
        }
    }
    if !current.is_empty() {
        problems.push(current);
    }

    // Solve a problem
    let solve = |cols: &[usize], vertical: bool| -> i64 {
        // Find operator
        let op: fn(i64, i64) -> i64 = cols
            .iter()
            .find_map(|&c| match grid[h - 1][c] {
                '+' => Some((|a, b| a + b) as fn(i64, i64) -> i64),
                '*' => Some((|a, b| a * b) as fn(i64, i64) -> i64),
                _ => None,
            })
            .expect("No operator found");

        let nums: Vec<i64> = if vertical {
            // Each column is a number read top-to-bottom
            cols.iter()
                .filter_map(|&c| {
                    let s: String = (0..h - 1)
                        .map(|r| grid[r][c])
                        .filter(|ch| ch.is_ascii_digit())
                        .collect();
                    if s.is_empty() {
                        None
                    } else {
                        s.parse().ok()
                    }
                })
                .collect()
        } else {
            // Each row is a number read left-to-right
            (0..h - 1)
                .filter_map(|r| {
                    let s: String = cols.iter().map(|&c| grid[r][c]).collect::<String>();
                    let trimmed = s.trim();
                    if trimmed.is_empty() {
                        None
                    } else {
                        trimmed.parse().ok()
                    }
                })
                .collect()
        };

        nums.into_iter().reduce(op).unwrap_or(0)
    };

    let part1: i64 = problems.iter().map(|p| solve(p, false)).sum();
    let part2: i64 = problems.iter().rev().map(|p| solve(p, true)).sum();

    println!("Part 1: {}", part1);
    println!("Part 2: {}", part2);
}
