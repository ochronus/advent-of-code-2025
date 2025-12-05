use std::fs;

fn parse_input(input: &str) -> (Vec<(i64, i64)>, Vec<i64>) {
    let parts: Vec<&str> = input.trim().split("\n\n").collect();

    let ranges = parts[0]
        .lines()
        .map(|line| {
            let (start, stop) = line.split_once('-').unwrap();
            (start.parse().unwrap(), stop.parse().unwrap())
        })
        .collect();

    let ingredients = parts[1].lines().map(|line| line.parse().unwrap()).collect();

    (ranges, ingredients)
}

fn is_fresh(ranges: &[(i64, i64)], id: i64) -> bool {
    ranges
        .iter()
        .any(|(start, stop)| id >= *start && id <= *stop)
}

fn solve_part1(ranges: &[(i64, i64)], ingredients: &[i64]) -> usize {
    ingredients
        .iter()
        .filter(|&&id| is_fresh(ranges, id))
        .count()
}

fn merge_ranges(ranges: &[(i64, i64)]) -> Vec<(i64, i64)> {
    if ranges.is_empty() {
        return vec![];
    }

    let mut sorted: Vec<_> = ranges.to_vec();
    sorted.sort_by_key(|(start, _)| *start);

    let mut merged: Vec<(i64, i64)> = vec![];

    for (start, stop) in sorted {
        if let Some((_, prev_stop)) = merged.last_mut() {
            if start <= *prev_stop + 1 {
                // Overlapping or adjacent, merge them
                *prev_stop = (*prev_stop).max(stop);
            } else {
                // No overlap, add new range
                merged.push((start, stop));
            }
        } else {
            merged.push((start, stop));
        }
    }

    merged
}

fn solve_part2(ranges: &[(i64, i64)]) -> i64 {
    merge_ranges(ranges)
        .iter()
        .map(|(start, stop)| stop - start + 1)
        .sum()
}

fn main() {
    let input = fs::read_to_string("input.txt").expect("Failed to read input.txt");
    let (ranges, ingredients) = parse_input(&input);

    println!("Part 1: {}", solve_part1(&ranges, &ingredients));
    println!("Part 2: {}", solve_part2(&ranges));
}
