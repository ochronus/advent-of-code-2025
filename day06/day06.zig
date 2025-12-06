const std = @import("std");

pub fn main() !void {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    defer _ = gpa.deinit();
    const allocator = gpa.allocator();

    const file = try std.fs.cwd().openFile("input.txt", .{});
    defer file.close();

    const content = try file.readToEndAlloc(allocator, 1024 * 1024);
    defer allocator.free(content);

    // Split into lines
    var lines_list = std.ArrayListUnmanaged([]const u8){};
    defer lines_list.deinit(allocator);

    var line_iter = std.mem.splitScalar(u8, content, '\n');
    while (line_iter.next()) |line| {
        if (line.len > 0) {
            try lines_list.append(allocator, line);
        }
    }

    const lines = lines_list.items;
    const h = lines.len;
    var w: usize = 0;
    for (lines) |line| {
        if (line.len > w) w = line.len;
    }

    // Find problem column groups
    var problems = std.ArrayListUnmanaged(std.ArrayListUnmanaged(usize)){};
    defer {
        for (problems.items) |*p| p.deinit(allocator);
        problems.deinit(allocator);
    }

    var current = std.ArrayListUnmanaged(usize){};

    for (0..w) |c| {
        if (isSep(lines, h, c)) {
            if (current.items.len > 0) {
                try problems.append(allocator, current);
                current = std.ArrayListUnmanaged(usize){};
            }
        } else {
            try current.append(allocator, c);
        }
    }
    if (current.items.len > 0) {
        try problems.append(allocator, current);
    } else {
        current.deinit(allocator);
    }

    // Part 1
    var part1: i64 = 0;
    for (problems.items) |p| {
        part1 += try solve(allocator, lines, h, p.items, false);
    }

    // Part 2 (reversed)
    var part2: i64 = 0;
    var i: usize = problems.items.len;
    while (i > 0) {
        i -= 1;
        part2 += try solve(allocator, lines, h, problems.items[i].items, true);
    }

    std.debug.print("Part 1: {d}\n", .{part1});
    std.debug.print("Part 2: {d}\n", .{part2});
}

fn getChar(ls: []const []const u8, row: usize, col: usize) u8 {
    if (col < ls[row].len) return ls[row][col];
    return ' ';
}

fn isSep(ls: []const []const u8, height: usize, col: usize) bool {
    for (0..height) |r| {
        if (getChar(ls, r, col) != ' ') return false;
    }
    return true;
}

fn solve(allocator: std.mem.Allocator, ls: []const []const u8, height: usize, cols: []const usize, vertical: bool) !i64 {
    // Find operator
    var op: enum { add, mul } = .add;
    for (cols) |c| {
        const ch = getChar(ls, height - 1, c);
        if (ch == '+') {
            op = .add;
            break;
        } else if (ch == '*') {
            op = .mul;
            break;
        }
    }

    // Collect numbers
    var nums = std.ArrayListUnmanaged(i64){};
    defer nums.deinit(allocator);

    if (vertical) {
        // Each column is a number
        for (cols) |c| {
            var num_buf: [32]u8 = undefined;
            var num_len: usize = 0;
            for (0..height - 1) |r| {
                const ch = getChar(ls, r, c);
                if (ch >= '0' and ch <= '9') {
                    num_buf[num_len] = ch;
                    num_len += 1;
                }
            }
            if (num_len > 0) {
                const n = try std.fmt.parseInt(i64, num_buf[0..num_len], 10);
                try nums.append(allocator, n);
            }
        }
    } else {
        // Each row is a number
        for (0..height - 1) |r| {
            var row_buf: [64]u8 = undefined;
            var row_len: usize = 0;
            for (cols) |c| {
                row_buf[row_len] = getChar(ls, r, c);
                row_len += 1;
            }
            // Trim spaces
            var start: usize = 0;
            var end: usize = row_len;
            while (start < end and row_buf[start] == ' ') start += 1;
            while (end > start and row_buf[end - 1] == ' ') end -= 1;
            if (start < end) {
                const n = try std.fmt.parseInt(i64, row_buf[start..end], 10);
                try nums.append(allocator, n);
            }
        }
    }

    // Apply operation
    if (nums.items.len == 0) return 0;
    var result = nums.items[0];
    for (nums.items[1..]) |n| {
        result = switch (op) {
            .add => result + n,
            .mul => result * n,
        };
    }
    return result;
}
