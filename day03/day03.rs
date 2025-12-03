// Advent of Code 2025 - Day 03
// Find maximum joltage from battery banks

use std::fs;

/// Part 1: Pick exactly 2 batteries to form the largest 2-digit number
fn find_max_joltage_2(bank: &str) -> u32 {
    let digits: Vec<u8> = bank.bytes().map(|b| b - b'0').collect();
    let n = digits.len();

    let mut max_joltage = 0;
    for i in 0..n - 1 {
        for j in i + 1..n {
            let joltage = digits[i] as u32 * 10 + digits[j] as u32;
            max_joltage = max_joltage.max(joltage);
        }
    }
    max_joltage
}

/// Part 2: Pick exactly 12 batteries to form the largest 12-digit number
/// Uses a greedy approach: at each step, pick the largest digit possible
/// while ensuring enough digits remain for the rest of the selection
fn find_max_joltage_12(bank: &str) -> u64 {
    let digits: Vec<u8> = bank.bytes().map(|b| b - b'0').collect();
    let n = digits.len();
    const NUM_TO_SELECT: usize = 12;

    let mut result: u64 = 0;
    let mut start_idx = 0;

    for remaining in (1..=NUM_TO_SELECT).rev() {
        // We can only look up to index (n - remaining) to leave enough for the rest
        let end_idx = n - remaining;

        // Find position of maximum digit in valid range (first occurrence on ties)
        let mut max_digit = 0u8;
        let mut max_pos = start_idx;
        for i in start_idx..=end_idx {
            if digits[i] > max_digit {
                max_digit = digits[i];
                max_pos = i;
            }
        }

        result = result * 10 + max_digit as u64;
        start_idx = max_pos + 1;
    }

    result
}

fn main() {
    let input = fs::read_to_string("input.txt").expect("Failed to read input.txt");
    let lines: Vec<&str> = input.lines().filter(|line| !line.is_empty()).collect();

    // Part 1
    let total_part1: u32 = lines.iter().map(|line| find_max_joltage_2(line)).sum();
    println!("Part 1 - Total output joltage: {}", total_part1);

    // Part 2
    let total_part2: u64 = lines.iter().map(|line| find_max_joltage_12(line)).sum();
    println!("Part 2 - Total output joltage: {}", total_part2);
}
