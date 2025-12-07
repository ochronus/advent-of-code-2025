const std = @import("std");

const MemoKey = struct {
    row: usize,
    col: usize,
};

pub fn main() !void {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    defer _ = gpa.deinit();
    const allocator = gpa.allocator();

    const file = try std.fs.cwd().openFile("input.txt", .{});
    defer file.close();

    const content = try file.readToEndAlloc(allocator, 10 * 1024 * 1024);
    defer allocator.free(content);

    // Parse lines
    var line_list = std.ArrayListUnmanaged([]const u8){};
    defer line_list.deinit(allocator);

    var lines_iter = std.mem.splitScalar(u8, content, '\n');
    while (lines_iter.next()) |line| {
        if (line.len > 0) {
            try line_list.append(allocator, line);
        }
    }

    const lines = line_list.items;
    const height = lines.len;

    // Find start column
    var start_col: usize = 0;
    for (lines[0], 0..) |c, i| {
        if (c == 'S') {
            start_col = i;
            break;
        }
    }

    // Part 1: Count beam splits (beams merge at same position)
    const part1_result = blk: {
        var beams = std.AutoHashMap(usize, void).init(allocator);
        defer beams.deinit();
        try beams.put(start_col, {});

        var splits: u32 = 0;

        for (0..height - 1) |row| {
            const next_row = lines[row + 1];

            var new_beams = std.AutoHashMap(usize, void).init(allocator);

            var iter = beams.keyIterator();
            while (iter.next()) |col_ptr| {
                const col = col_ptr.*;
                if (next_row[col] == '^') {
                    splits += 1;
                    try new_beams.put(col - 1, {});
                    try new_beams.put(col + 1, {});
                } else {
                    try new_beams.put(col, {});
                }
            }

            beams.deinit();
            beams = new_beams;
        }

        break :blk splits;
    };

    // Part 2: Count timelines (many-worlds interpretation with memoization)
    var memo = std.AutoHashMap(MemoKey, i64).init(allocator);
    defer memo.deinit();

    const part2_result = try timelines(0, start_col, lines, height, &memo);

    std.debug.print("Part 1: {d}\n", .{part1_result});
    std.debug.print("Part 2: {d}\n", .{part2_result});
}

fn timelines(
    row: usize,
    col: usize,
    lines: []const []const u8,
    height: usize,
    memo: *std.AutoHashMap(MemoKey, i64),
) !i64 {
    if (row == height - 1) {
        return 1;
    }

    const key = MemoKey{ .row = row, .col = col };
    if (memo.get(key)) |val| {
        return val;
    }

    const result: i64 = if (lines[row + 1][col] == '^')
        try timelines(row + 1, col - 1, lines, height, memo) +
            try timelines(row + 1, col + 1, lines, height, memo)
    else
        try timelines(row + 1, col, lines, height, memo);

    try memo.put(key, result);
    return result;
}
