const std = @import("std");

const Box = struct {
    x: i64,
    y: i64,
    z: i64,
};

const Pair = struct {
    dist_sq: i64,
    i: usize,
    j: usize,
};

const UnionFind = struct {
    parent: []usize,
    rank: []usize,
    components: usize,
    allocator: std.mem.Allocator,

    fn init(allocator: std.mem.Allocator, n: usize) !UnionFind {
        const parent = try allocator.alloc(usize, n);
        const rank = try allocator.alloc(usize, n);

        for (0..n) |i| {
            parent[i] = i;
            rank[i] = 0;
        }

        return UnionFind{
            .parent = parent,
            .rank = rank,
            .components = n,
            .allocator = allocator,
        };
    }

    fn deinit(self: *UnionFind) void {
        self.allocator.free(self.parent);
        self.allocator.free(self.rank);
    }

    fn find(self: *UnionFind, x: usize) usize {
        if (self.parent[x] != x) {
            self.parent[x] = self.find(self.parent[x]); // Path compression
        }
        return self.parent[x];
    }

    fn merge(self: *UnionFind, x: usize, y: usize) bool {
        const px = self.find(x);
        const py = self.find(y);

        if (px == py) {
            return false;
        }

        // Union by rank
        if (self.rank[px] < self.rank[py]) {
            self.parent[px] = py;
        } else if (self.rank[px] > self.rank[py]) {
            self.parent[py] = px;
        } else {
            self.parent[py] = px;
            self.rank[px] += 1;
        }
        self.components -= 1;
        return true;
    }

    fn getTop3Sizes(self: *UnionFind) [3]usize {
        var counts = std.AutoHashMap(usize, usize).init(self.allocator);
        defer counts.deinit();

        for (0..self.parent.len) |i| {
            const root = self.find(i);
            const entry = counts.getOrPut(root) catch unreachable;
            if (entry.found_existing) {
                entry.value_ptr.* += 1;
            } else {
                entry.value_ptr.* = 1;
            }
        }

        var top3 = [3]usize{ 0, 0, 0 };
        var iter = counts.valueIterator();
        while (iter.next()) |v| {
            const size = v.*;
            if (size > top3[0]) {
                top3[2] = top3[1];
                top3[1] = top3[0];
                top3[0] = size;
            } else if (size > top3[1]) {
                top3[2] = top3[1];
                top3[1] = size;
            } else if (size > top3[2]) {
                top3[2] = size;
            }
        }

        return top3;
    }
};

fn distSq(a: Box, b: Box) i64 {
    const dx = b.x - a.x;
    const dy = b.y - a.y;
    const dz = b.z - a.z;
    return dx * dx + dy * dy + dz * dz;
}

fn comparePairs(_: void, a: Pair, b: Pair) bool {
    return a.dist_sq < b.dist_sq;
}

pub fn main() !void {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    defer _ = gpa.deinit();
    const allocator = gpa.allocator();

    const file = try std.fs.cwd().openFile("input.txt", .{});
    defer file.close();

    const content = try file.readToEndAlloc(allocator, 10 * 1024 * 1024);
    defer allocator.free(content);

    // Parse boxes
    var boxes = std.ArrayListUnmanaged(Box){};
    defer boxes.deinit(allocator);

    var lines = std.mem.splitScalar(u8, content, '\n');
    while (lines.next()) |line| {
        if (line.len == 0) continue;

        var parts = std.mem.splitScalar(u8, line, ',');
        const x = try std.fmt.parseInt(i64, parts.next().?, 10);
        const y = try std.fmt.parseInt(i64, parts.next().?, 10);
        const z = try std.fmt.parseInt(i64, parts.next().?, 10);
        try boxes.append(allocator, Box{ .x = x, .y = y, .z = z });
    }

    const n = boxes.items.len;

    // Generate all pairs
    var pairs = std.ArrayListUnmanaged(Pair){};
    defer pairs.deinit(allocator);

    for (0..n - 1) |i| {
        for (i + 1..n) |j| {
            try pairs.append(allocator, Pair{
                .dist_sq = distSq(boxes.items[i], boxes.items[j]),
                .i = i,
                .j = j,
            });
        }
    }

    // Sort by distance
    std.mem.sort(Pair, pairs.items, {}, comparePairs);

    // Part 1: Connect 1000 shortest pairs
    var uf1 = try UnionFind.init(allocator, n);
    defer uf1.deinit();

    for (0..1000) |idx| {
        _ = uf1.merge(pairs.items[idx].i, pairs.items[idx].j);
    }

    const top3 = uf1.getTop3Sizes();
    const part1 = @as(i64, @intCast(top3[0])) * @as(i64, @intCast(top3[1])) * @as(i64, @intCast(top3[2]));

    // Part 2: Find last connection that unifies all circuits
    var uf2 = try UnionFind.init(allocator, n);
    defer uf2.deinit();

    var last_i: usize = 0;
    var last_j: usize = 0;

    for (pairs.items) |p| {
        if (uf2.merge(p.i, p.j) and uf2.components == 1) {
            last_i = p.i;
            last_j = p.j;
            break;
        }
    }

    const part2 = boxes.items[last_i].x * boxes.items[last_j].x;

    std.debug.print("Part 1: {d}\n", .{part1});
    std.debug.print("Part 2: {d}\n", .{part2});
}
