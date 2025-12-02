use std::collections::HashSet;
use std::fs;

fn parse_input(input: &str) -> Vec<(i64, i64)> {
    input
        .trim()
        .trim_end_matches(',')
        .split(',')
        .map(|range| {
            let (start, stop) = range.split_once('-').unwrap();
            (start.parse().unwrap(), stop.parse().unwrap())
        })
        .collect()
}

fn find_invalid_ids(start: i64, stop: i64, exactly_two_reps: bool) -> Vec<i64> {
    let start_digits = start.to_string().len();
    let stop_digits = stop.to_string().len();

    (start_digits..=stop_digits)
        .flat_map(|digit_count| {
            (1..=digit_count / 2).filter_map(move |pattern_len| {
                if digit_count % pattern_len != 0 {
                    return None;
                }
                let reps = digit_count / pattern_len;
                if exactly_two_reps && reps != 2 {
                    return None;
                }
                if !exactly_two_reps && reps < 2 {
                    return None;
                }

                let min_pattern = if pattern_len == 1 { 1 } else { 10_i64.pow((pattern_len - 1) as u32) };
                let max_pattern = 10_i64.pow(pattern_len as u32) - 1;

                Some((min_pattern..=max_pattern).filter_map(move |pattern| {
                    let full_str = pattern.to_string().repeat(reps);
                    let full_num: i64 = full_str.parse().unwrap();
                    if full_num >= start && full_num <= stop {
                        Some(full_num)
                    } else {
                        None
                    }
                }))
            })
            .flatten()
        })
        .collect()
}

fn solve(ranges: &[(i64, i64)], exactly_two_reps: bool) -> i64 {
    let all_invalid: HashSet<i64> = ranges
        .iter()
        .flat_map(|&(start, stop)| find_invalid_ids(start, stop, exactly_two_reps))
        .collect();

    all_invalid.iter().sum()
}

fn main() {
    let input = fs::read_to_string("input.txt").expect("Failed to read input.txt");
    let ranges = parse_input(&input);

    println!("Part 1: {}", solve(&ranges, true));
    println!("Part 2: {}", solve(&ranges, false));
}
