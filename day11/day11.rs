// Advent of Code 2025 - Day 11: Reactor
// Part 1: Count paths from "you" to "out"
// Part 2: Count paths from "svr" to "out" visiting both "dac" and "fft"

use std::collections::HashMap;
use std::fs;

type Graph<'a> = HashMap<&'a str, Vec<&'a str>>;

fn parse_graph(input: &str) -> Graph<'_> {
    input
        .lines()
        .filter(|line| !line.is_empty())
        .map(|line| {
            let (src, dests) = line.split_once(": ").unwrap();
            (src, dests.split_whitespace().collect())
        })
        .collect()
}

struct PathCounter<'a> {
    graph: &'a Graph<'a>,
    goal: &'a str,
    memo: HashMap<&'a str, i64>,
}

impl<'a> PathCounter<'a> {
    fn new(graph: &'a Graph<'a>, goal: &'a str) -> Self {
        Self {
            graph,
            goal,
            memo: HashMap::new(),
        }
    }

    fn count(&mut self, node: &'a str) -> i64 {
        if node == self.goal {
            return 1;
        }
        if let Some(&count) = self.memo.get(node) {
            return count;
        }

        let count = self
            .graph
            .get(node)
            .map(|neighbors| neighbors.iter().map(|n| self.count(n)).sum())
            .unwrap_or(0);

        self.memo.insert(node, count);
        count
    }
}

struct PathCounterVia<'a> {
    graph: &'a Graph<'a>,
    goal: &'a str,
    required: [&'a str; 2],
    memo: HashMap<(&'a str, u8), i64>,
}

impl<'a> PathCounterVia<'a> {
    fn new(graph: &'a Graph<'a>, goal: &'a str, required: [&'a str; 2]) -> Self {
        Self {
            graph,
            goal,
            required,
            memo: HashMap::new(),
        }
    }

    fn count(&mut self, node: &'a str, mask: u8) -> i64 {
        let mask =
            self.required.iter().enumerate().fold(
                mask,
                |m, (i, &req)| {
                    if node == req {
                        m | (1 << i)
                    } else {
                        m
                    }
                },
            );

        if node == self.goal {
            return if mask == 3 { 1 } else { 0 };
        }

        let key = (node, mask);
        if let Some(&count) = self.memo.get(&key) {
            return count;
        }

        let count = self
            .graph
            .get(node)
            .map(|neighbors| neighbors.iter().map(|n| self.count(n, mask)).sum())
            .unwrap_or(0);

        self.memo.insert(key, count);
        count
    }
}

fn main() {
    let input = fs::read_to_string("day11/input.txt").expect("Failed to read input");
    let graph = parse_graph(&input);

    let part1 = PathCounter::new(&graph, "out").count("you");
    let part2 = PathCounterVia::new(&graph, "out", ["dac", "fft"]).count("svr", 0);

    println!("Part 1: {part1}");
    println!("Part 2: {part2}");
}
