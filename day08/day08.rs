// Advent of Code 2025 - Day 08
// Junction box circuits with Union-Find

use std::fs;

struct UnionFind {
    parent: Vec<usize>,
    rank: Vec<usize>,
    components: usize,
}

impl UnionFind {
    fn new(n: usize) -> Self {
        Self {
            parent: (0..n).collect(),
            rank: vec![0; n],
            components: n,
        }
    }

    fn find(&mut self, x: usize) -> usize {
        if self.parent[x] != x {
            self.parent[x] = self.find(self.parent[x]); // Path compression
        }
        self.parent[x]
    }

    fn union(&mut self, x: usize, y: usize) -> bool {
        let px = self.find(x);
        let py = self.find(y);

        if px == py {
            return false;
        }

        // Union by rank
        match self.rank[px].cmp(&self.rank[py]) {
            std::cmp::Ordering::Less => self.parent[px] = py,
            std::cmp::Ordering::Greater => self.parent[py] = px,
            std::cmp::Ordering::Equal => {
                self.parent[py] = px;
                self.rank[px] += 1;
            }
        }
        self.components -= 1;
        true
    }

    fn circuit_sizes(&mut self) -> Vec<usize> {
        let mut counts = std::collections::HashMap::new();
        for i in 0..self.parent.len() {
            let root = self.find(i);
            *counts.entry(root).or_insert(0) += 1;
        }
        let mut sizes: Vec<_> = counts.values().copied().collect();
        sizes.sort_by(|a, b| b.cmp(a));
        sizes
    }
}

fn dist_sq(a: (i64, i64, i64), b: (i64, i64, i64)) -> i64 {
    let (dx, dy, dz) = (b.0 - a.0, b.1 - a.1, b.2 - a.2);
    dx * dx + dy * dy + dz * dz
}

fn main() {
    let input = fs::read_to_string("input.txt").expect("Failed to read input.txt");

    let boxes: Vec<(i64, i64, i64)> = input
        .lines()
        .filter(|line| !line.is_empty())
        .map(|line| {
            let parts: Vec<i64> = line.split(',').map(|s| s.parse().unwrap()).collect();
            (parts[0], parts[1], parts[2])
        })
        .collect();

    let n = boxes.len();

    // Generate all pairs sorted by distance
    let mut pairs: Vec<(i64, usize, usize)> = Vec::new();
    for i in 0..n - 1 {
        for j in i + 1..n {
            pairs.push((dist_sq(boxes[i], boxes[j]), i, j));
        }
    }
    pairs.sort_by_key(|p| p.0);

    // Part 1: Connect 1000 shortest pairs
    let mut uf1 = UnionFind::new(n);
    for (_, i, j) in pairs.iter().take(1000) {
        uf1.union(*i, *j);
    }

    let sizes = uf1.circuit_sizes();
    let part1 = sizes[0] as i64 * sizes[1] as i64 * sizes[2] as i64;

    // Part 2: Find last connection that unifies all circuits
    let mut uf2 = UnionFind::new(n);
    let mut last_pair = (0, 0);

    for (_, i, j) in &pairs {
        if uf2.union(*i, *j) && uf2.components == 1 {
            last_pair = (*i, *j);
            break;
        }
    }

    let part2 = boxes[last_pair.0].0 * boxes[last_pair.1].0;

    println!("Part 1: {}", part1);
    println!("Part 2: {}", part2);
}
