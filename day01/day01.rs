use std::fs;

fn parse_line(line: &str) -> i32 {
    let direction = line.chars().next().unwrap();
    let value: i32 = line[1..].parse().unwrap();
    if direction == 'L' {
        -value
    } else {
        value
    }
}

fn count_crossings(dial: i32, movement: i32) -> i32 {
    let full_rotations = movement.abs() / 100;
    let sign = if movement >= 0 { 1 } else { -1 };
    let remainder = movement - sign * 100 * full_rotations;
    let new_pos = dial + remainder;

    let boundary_crossing = if remainder < 0 && dial > 0 && new_pos <= 0 {
        1
    } else if remainder > 0 && new_pos > 99 {
        1
    } else {
        0
    };

    full_rotations + boundary_crossing
}

fn normalize_dial(dial: i32) -> i32 {
    ((dial % 100) + 100) % 100
}

fn main() {
    let content = fs::read_to_string("input.txt").expect("Failed to read input.txt");

    let (_, zeros, crossings) = content.lines().filter(|line| !line.is_empty()).fold(
        (50, 0, 0),
        |(dial, zeros, crossings), line| {
            let movement = parse_line(line);
            let new_dial = normalize_dial(dial + movement);
            let new_zeros = if new_dial == 0 { zeros + 1 } else { zeros };
            let new_crossings = crossings + count_crossings(dial, movement);
            (new_dial, new_zeros, new_crossings)
        },
    );

    println!("{}", zeros);
    println!("{}", crossings);
}
