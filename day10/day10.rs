use std::collections::HashMap;
use std::fs;

#[derive(Debug, Clone)]
struct Machine {
    target_lights: Vec<u8>,
    buttons: Vec<Vec<usize>>,
    joltages: Vec<i64>,
    num_lights: usize,
}

fn parse_input(input: &str) -> Vec<Machine> {
    input
        .lines()
        .filter(|line| !line.trim().is_empty())
        .map(|line| {
            let parts: Vec<&str> = line.split_whitespace().collect();

            // Parse indicator lights pattern [.##.]
            let lights_str = parts[0].trim_matches(|c| c == '[' || c == ']');
            let num_lights = lights_str.len();
            let target_lights: Vec<u8> = lights_str
                .chars()
                .map(|c| if c == '#' { 1 } else { 0 })
                .collect();

            // Parse buttons (0,1,2)
            let buttons: Vec<Vec<usize>> = parts[1..parts.len() - 1]
                .iter()
                .map(|button_str| {
                    button_str
                        .trim_matches(|c| c == '(' || c == ')')
                        .split(',')
                        .filter_map(|s| s.parse().ok())
                        .collect()
                })
                .collect();

            // Parse joltages {3,5,4,7}
            let joltages: Vec<i64> = parts
                .last()
                .unwrap()
                .trim_matches(|c| c == '{' || c == '}')
                .split(',')
                .filter_map(|s| s.parse().ok())
                .collect();

            Machine {
                target_lights,
                buttons,
                joltages,
                num_lights,
            }
        })
        .collect()
}

fn solve_part1(target: &[u8], buttons: &[Vec<usize>], num_lights: usize) -> Option<usize> {
    let num_buttons = buttons.len();

    // Build matrix: rows are lights, columns are buttons
    // matrix[light][button] = 1 if button affects light
    let mut matrix = vec![vec![0u8; num_buttons + 1]; num_lights];

    for (button_idx, button) in buttons.iter().enumerate() {
        for &light in button {
            if light < num_lights {
                matrix[light][button_idx] = 1;
            }
        }
    }

    // Set target values in augmented column
    for (light, &val) in target.iter().enumerate() {
        matrix[light][num_buttons] = val;
    }

    // Gaussian elimination to RREF over GF(2)
    let mut pivot_row = 0;
    let mut pivot_col_to_row = HashMap::new();

    for col in 0..num_buttons {
        if pivot_row >= num_lights {
            break;
        }

        // Find pivot in current column
        let mut row = pivot_row;
        while row < num_lights && matrix[row][col] == 0 {
            row += 1;
        }

        if row < num_lights {
            // Swap rows to bring pivot to position
            matrix.swap(pivot_row, row);

            // Eliminate all other rows using XOR (GF(2) addition)
            for r in 0..num_lights {
                if r != pivot_row && matrix[r][col] == 1 {
                    for k in col..=num_buttons {
                        matrix[r][k] ^= matrix[pivot_row][k];
                    }
                }
            }

            pivot_col_to_row.insert(col, pivot_row);
            pivot_row += 1;
        }
    }

    // Check for inconsistency
    for row in &matrix[pivot_row..num_lights] {
        if row[num_buttons] == 1 {
            return None;
        }
    }

    // Identify free variables
    let free_vars: Vec<usize> = (0..num_buttons)
        .filter(|col| !pivot_col_to_row.contains_key(col))
        .collect();

    // Try all combinations to find minimum Hamming weight
    let num_free = free_vars.len();
    let mut min_presses = usize::MAX;

    for mask in 0..(1u64 << num_free) {
        let mut solution = vec![0u8; num_buttons];

        // Set free variables based on bitmask
        for (bit_idx, &var_idx) in free_vars.iter().enumerate() {
            if (mask >> bit_idx) & 1 == 1 {
                solution[var_idx] = 1;
            }
        }

        // Back-substitute to find pivot variable values
        for (&pivot_col, &pivot_row) in &pivot_col_to_row {
            let mut val = matrix[pivot_row][num_buttons];
            for &free_var in &free_vars {
                if matrix[pivot_row][free_var] == 1 {
                    val ^= solution[free_var];
                }
            }
            solution[pivot_col] = val;
        }

        // Count button presses
        let presses = solution.iter().filter(|&&v| v == 1).count();
        min_presses = min_presses.min(presses);
    }

    Some(min_presses)
}

fn solve_part2(target: &[i64], buttons: &[Vec<usize>]) -> Option<i64> {
    let num_requirements = target.len();
    let num_buttons = buttons.len();

    // Build matrix: rows are requirements, columns are buttons
    // matrix[requirement][button] = 1 if button affects requirement
    let mut matrix = vec![vec![0i128; num_buttons + 1]; num_requirements];

    for (button_idx, button) in buttons.iter().enumerate() {
        for &req in button {
            if req < num_requirements {
                matrix[req][button_idx] = 1;
            }
        }
    }

    for (req, &val) in target.iter().enumerate() {
        matrix[req][num_buttons] = val as i128;
    }

    // Fraction-free Gaussian elimination (forward pass only)
    let mut pivot_row = 0;
    let mut pivot_col_to_row = HashMap::new();

    for col in 0..num_buttons {
        if pivot_row >= num_requirements {
            break;
        }

        // Find non-zero pivot
        let mut row = pivot_row;
        while row < num_requirements && matrix[row][col] == 0 {
            row += 1;
        }

        if row < num_requirements {
            matrix.swap(pivot_row, row);

            let pivot_val = matrix[pivot_row][col];

            // Eliminate rows below
            for r in pivot_row + 1..num_requirements {
                if matrix[r][col] != 0 {
                    let factor = matrix[r][col];
                    for k in col..=num_buttons {
                        matrix[r][k] = matrix[r][k] * pivot_val - matrix[pivot_row][k] * factor;
                    }
                }
            }

            pivot_col_to_row.insert(col, pivot_row);
            pivot_row += 1;
        }
    }

    // Check for inconsistency
    for row in &matrix[pivot_row..num_requirements] {
        if row[num_buttons] != 0 {
            return None;
        }
    }

    // Identify free variables
    let free_vars: Vec<usize> = (0..num_buttons)
        .filter(|col| !pivot_col_to_row.contains_key(col))
        .collect();

    // Search for minimum solution
    let mut min_total: Option<i64> = None;

    fn search(
        free_idx: usize,
        free_vars: &[usize],
        free_vals: &mut Vec<i64>,
        matrix: &[Vec<i128>],
        row_to_pivot_col: &HashMap<usize, usize>,
        num_buttons: usize,
        num_pivots: usize,
        min_total: &mut Option<i64>,
    ) {
        if free_idx == free_vars.len() {
            // Try to solve with current free variable values
            let mut solution = vec![0i128; num_buttons];

            // Set free variables
            for (i, &var) in free_vars.iter().enumerate() {
                solution[var] = free_vals[i] as i128;
            }

            // Back-substitute from bottom to top
            for row in (0..num_pivots).rev() {
                if let Some(&pivot_col) = row_to_pivot_col.get(&row) {
                    let pivot_val = matrix[row][pivot_col];
                    let mut rhs = matrix[row][num_buttons];

                    // Subtract known variable contributions
                    for col in pivot_col + 1..num_buttons {
                        rhs -= matrix[row][col] * solution[col];
                    }

                    // Check if solution is integral and non-negative
                    if rhs % pivot_val != 0 {
                        return; // Not integral
                    }

                    let val = rhs / pivot_val;
                    if val < 0 {
                        return; // Negative solution
                    }

                    solution[pivot_col] = val;
                }
            }

            // Calculate total button presses
            let total: i128 = solution.iter().sum();
            if total >= 0 && total < i64::MAX as i128 {
                let total_i64 = total as i64;
                if min_total.is_none() || total_i64 < min_total.unwrap() {
                    *min_total = Some(total_i64);
                }
            }
            return;
        }

        // Search range: larger for single free var, smaller for multiple
        let limit = if free_vars.len() > 1 { 200 } else { 20000 };
        for val in 0..=limit {
            free_vals[free_idx] = val;
            search(
                free_idx + 1,
                free_vars,
                free_vals,
                matrix,
                row_to_pivot_col,
                num_buttons,
                num_pivots,
                min_total,
            );
        }
    }

    // Build row -> pivot_col map once for back-substitution
    let mut row_to_pivot_col = HashMap::new();
    for (&col, &row) in &pivot_col_to_row {
        row_to_pivot_col.insert(row, col);
    }
    let num_pivots = pivot_col_to_row.len();

    let mut free_vals = vec![0i64; free_vars.len()];
    search(
        0,
        &free_vars,
        &mut free_vals,
        &matrix,
        &row_to_pivot_col,
        num_buttons,
        num_pivots,
        &mut min_total,
    );

    min_total
}

fn main() {
    let input = fs::read_to_string("input.txt").expect("Failed to read input.txt");
    let machines = parse_input(&input);

    // Part 1: Binary configuration using Gaussian elimination over GF(2)
    let part1_total: usize = machines
        .iter()
        .filter_map(|machine| {
            solve_part1(&machine.target_lights, &machine.buttons, machine.num_lights)
        })
        .sum();

    println!("Part 1: {}", part1_total);

    // Part 2: Integer linear programming with branch-and-bound search
    let part2_total: i64 = machines
        .iter()
        .filter_map(|machine| {
            if !machine.joltages.is_empty() {
                solve_part2(&machine.joltages, &machine.buttons)
            } else {
                Some(0)
            }
        })
        .sum();

    println!("Part 2: {}", part2_total);
}
