// Advent of Code 2025 - Day 07
// Tachyon beam splitting in the manifold

use std::collections::{HashMap, HashSet};
use std::fs;

/// Part 1: Count beam splits (beams merge at same position)
fn part1(lines: &[&str]) -> u32 {
    let height = lines.len();
    let start_col = lines[0].find('S').unwrap();

    let mut beams: HashSet<usize> = HashSet::new();
    beams.insert(start_col);
    let mut splits = 0;

    for row in 0..height - 1 {
        let next_row = lines[row + 1].as_bytes();
        let mut new_beams: HashSet<usize> = HashSet::new();

        for &col in &beams {
            if next_row[col] == b'^' {
                splits += 1;
                new_beams.insert(col - 1);
                new_beams.insert(col + 1);
            } else {
                new_beams.insert(col);
            }
        }
        beams = new_beams;
    }

    splits
}

/// Part 2: Count timelines (many-worlds interpretation with memoization)
fn part2(lines: &[&str]) -> i64 {
    let height = lines.len();
    let start_col = lines[0].find('S').unwrap();

    let mut memo: HashMap<(usize, usize), i64> = HashMap::new();

    fn timelines(
        row: usize,
        col: usize,
        lines: &[&str],
        height: usize,
        memo: &mut HashMap<(usize, usize), i64>,
    ) -> i64 {
        if row == height - 1 {
            return 1;
        }

        let key = (row, col);
        if let Some(&val) = memo.get(&key) {
            return val;
        }

        let result = if lines[row + 1].as_bytes()[col] == b'^' {
            timelines(row + 1, col - 1, lines, height, memo)
                + timelines(row + 1, col + 1, lines, height, memo)
        } else {
            timelines(row + 1, col, lines, height, memo)
        };

        memo.insert(key, result);
        result
    }

    timelines(0, start_col, lines, height, &mut memo)
}

fn main() {
    let input = fs::read_to_string("input.txt").expect("Failed to read input.txt");
    let lines: Vec<&str> = input.lines().filter(|line| !line.is_empty()).collect();

    println!("Part 1: {}", part1(&lines));
    println!("Part 2: {}", part2(&lines));
}
