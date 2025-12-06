const std = @import("std");

const Range = struct {
    start: i64,
    stop: i64,
};

fn parseInput(allocator: std.mem.Allocator, input: []const u8) !std.ArrayList(Range) {
    var ranges: std.ArrayList(Range) = .empty;

    const trimmed = std.mem.trim(u8, input, " \n\r");
    const without_trailing_comma = if (trimmed.len > 0 and trimmed[trimmed.len - 1] == ',')
        trimmed[0 .. trimmed.len - 1]
    else
        trimmed;

    var it = std.mem.splitScalar(u8, without_trailing_comma, ',');
    while (it.next()) |range_str| {
        var range_it = std.mem.splitScalar(u8, range_str, '-');
        const start_str = range_it.next() orelse continue;
        const stop_str = range_it.next() orelse continue;

        const start = std.fmt.parseInt(i64, start_str, 10) catch continue;
        const stop = std.fmt.parseInt(i64, stop_str, 10) catch continue;

        try ranges.append(allocator, .{ .start = start, .stop = stop });
    }

    return ranges;
}

fn countDigits(n: i64) usize {
    if (n == 0) return 1;
    var num = if (n < 0) -n else n;
    var count: usize = 0;
    while (num > 0) {
        num = @divTrunc(num, 10);
        count += 1;
    }
    return count;
}

fn pow10(exp: usize) i64 {
    var result: i64 = 1;
    for (0..exp) |_| {
        result *= 10;
    }
    return result;
}

fn findInvalidIds(allocator: std.mem.Allocator, start: i64, stop: i64, exactly_two_reps: bool) !std.ArrayList(i64) {
    var result: std.ArrayList(i64) = .empty;

    const start_digits = countDigits(start);
    const stop_digits = countDigits(stop);

    for (start_digits..stop_digits + 1) |digit_count| {
        for (1..digit_count / 2 + 1) |pattern_len| {
            if (digit_count % pattern_len != 0) continue;

            const reps = digit_count / pattern_len;
            if (exactly_two_reps and reps != 2) continue;
            if (!exactly_two_reps and reps < 2) continue;

            const min_pattern: i64 = if (pattern_len == 1) 1 else pow10(pattern_len - 1);
            const max_pattern: i64 = pow10(pattern_len) - 1;

            var pattern = min_pattern;
            while (pattern <= max_pattern) : (pattern += 1) {
                // Build the full number by repeating the pattern
                var full_num: i64 = 0;
                for (0..reps) |_| {
                    full_num = full_num * pow10(pattern_len) + pattern;
                }

                if (full_num >= start and full_num <= stop) {
                    try result.append(allocator, full_num);
                }
            }
        }
    }

    return result;
}

fn solve(allocator: std.mem.Allocator, ranges: []const Range, exactly_two_reps: bool) !i64 {
    var seen = std.AutoHashMap(i64, void).init(allocator);
    defer seen.deinit();

    for (ranges) |range| {
        var invalid_ids = try findInvalidIds(allocator, range.start, range.stop, exactly_two_reps);
        defer invalid_ids.deinit(allocator);

        for (invalid_ids.items) |id| {
            try seen.put(id, {});
        }
    }

    var sum: i64 = 0;
    var iter = seen.keyIterator();
    while (iter.next()) |key| {
        sum += key.*;
    }

    return sum;
}

pub fn main() !void {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    defer _ = gpa.deinit();
    const allocator = gpa.allocator();

    const file = try std.fs.cwd().openFile("input.txt", .{});
    defer file.close();

    const content = try file.readToEndAlloc(allocator, 1024 * 1024);
    defer allocator.free(content);

    var ranges = try parseInput(allocator, content);
    defer ranges.deinit(allocator);

    const part1 = try solve(allocator, ranges.items, true);
    const part2 = try solve(allocator, ranges.items, false);

    const stdout = std.fs.File.stdout();
    var buf: [64]u8 = undefined;

    var out = std.fmt.bufPrint(&buf, "Part 1: {d}\n", .{part1}) catch unreachable;
    try stdout.writeAll(out);

    out = std.fmt.bufPrint(&buf, "Part 2: {d}\n", .{part2}) catch unreachable;
    try stdout.writeAll(out);
}
