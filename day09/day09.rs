use std::fs;

fn parse_input(input: &str) -> Vec<(i64, i64)> {
    input
        .lines()
        .filter_map(|line| {
            let parts: Vec<&str> = line.split(',').collect();
            if parts.len() == 2 {
                Some((parts[0].parse().ok()?, parts[1].parse().ok()?))
            } else {
                None
            }
        })
        .collect()
}

fn rectangle_area((x1, y1): (i64, i64), (x2, y2): (i64, i64)) -> i64 {
    ((x2 - x1).abs() + 1) * ((y2 - y1).abs() + 1)
}

fn part1(tiles: &[(i64, i64)]) -> i64 {
    let n = tiles.len();
    let mut max_area = 0;

    for i in 0..n - 1 {
        for j in i + 1..n {
            let area = rectangle_area(tiles[i], tiles[j]);
            max_area = max_area.max(area);
        }
    }

    max_area
}

// Point-in-polygon using ray casting algorithm
fn is_inside_polygon((px, py): (i64, i64), polygon: &[(i64, i64)]) -> bool {
    let n = polygon.len();
    let mut inside = false;
    let mut j = n - 1;

    for i in 0..n {
        let (xi, yi) = polygon[i];
        let (xj, yj) = polygon[j];

        if ((yi > py) != (yj > py)) && (px < (xj - xi) * (py - yi) / (yj - yi) + xi) {
            inside = !inside;
        }
        j = i;
    }

    inside
}

// Check if point is on a line segment (for rectilinear edges)
fn is_on_segment((px, py): (i64, i64), (x1, y1): (i64, i64), (x2, y2): (i64, i64)) -> bool {
    let min_x = x1.min(x2);
    let max_x = x1.max(x2);
    let min_y = y1.min(y2);
    let max_y = y1.max(y2);

    if px < min_x || px > max_x || py < min_y || py > max_y {
        return false;
    }

    if x1 == x2 {
        px == x1 && py >= min_y && py <= max_y
    } else if y1 == y2 {
        py == y1 && px >= min_x && px <= max_x
    } else {
        false
    }
}

// Check if point is on polygon boundary
fn is_on_polygon_boundary((px, py): (i64, i64), polygon: &[(i64, i64)]) -> bool {
    let n = polygon.len();
    (0..n).any(|i| {
        let j = (i + 1) % n;
        is_on_segment((px, py), polygon[i], polygon[j])
    })
}

// Check if point is inside or on polygon
fn is_inside_or_on_polygon(point: (i64, i64), polygon: &[(i64, i64)]) -> bool {
    is_inside_polygon(point, polygon) || is_on_polygon_boundary(point, polygon)
}

fn part2(tiles: &[(i64, i64)]) -> i64 {
    let n = tiles.len();
    let mut max_area = 0;

    // Extract all unique x and y coordinates from polygon vertices
    let all_x: Vec<i64> = {
        let mut xs: Vec<i64> = tiles.iter().map(|&(x, _)| x).collect();
        xs.sort_unstable();
        xs.dedup();
        xs
    };

    let all_y: Vec<i64> = {
        let mut ys: Vec<i64> = tiles.iter().map(|&(_, y)| y).collect();
        ys.sort_unstable();
        ys.dedup();
        ys
    };

    for i in 0..n - 1 {
        for j in i + 1..n {
            let (x1, y1) = tiles[i];
            let (x2, y2) = tiles[j];

            let min_x = x1.min(x2);
            let max_x = x1.max(x2);
            let min_y = y1.min(y2);
            let max_y = y1.max(y2);

            // Get critical coordinates within rectangle bounds
            let critical_x: Vec<i64> = all_x
                .iter()
                .filter(|&&x| x >= min_x && x <= max_x)
                .copied()
                .collect();

            let critical_y: Vec<i64> = all_y
                .iter()
                .filter(|&&y| y >= min_y && y <= max_y)
                .copied()
                .collect();

            // Check all critical points
            let all_valid = critical_x.iter().all(|&x| {
                critical_y
                    .iter()
                    .all(|&y| is_inside_or_on_polygon((x, y), tiles))
            });

            if all_valid {
                let area = rectangle_area(tiles[i], tiles[j]);
                max_area = max_area.max(area);
            }
        }
    }

    max_area
}

fn main() {
    let input = fs::read_to_string("input.txt").expect("Failed to read input file");
    let tiles = parse_input(&input);

    let part1_result = part1(&tiles);
    println!("Part 1: {}", part1_result);

    let part2_result = part2(&tiles);
    println!("Part 2: {}", part2_result);
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_example() {
        let tiles = vec![
            (7, 1),
            (11, 1),
            (11, 7),
            (9, 7),
            (9, 5),
            (2, 5),
            (2, 3),
            (7, 3),
        ];

        assert_eq!(part1(&tiles), 50);
        assert_eq!(part2(&tiles), 24);
    }
}
