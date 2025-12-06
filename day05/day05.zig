const std = @import("std");

const Range = struct {
    start: i64,
    stop: i64,
};

fn parseInput(allocator: std.mem.Allocator, input: []const u8) !struct { ranges: std.ArrayList(Range), ingredients: std.ArrayList(i64) } {
    var ranges: std.ArrayList(Range) = .empty;
    var ingredients: std.ArrayList(i64) = .empty;

    var parts = std.mem.splitSequence(u8, std.mem.trim(u8, input, "\n\r "), "\n\n");

    // Parse ranges
    if (parts.next()) |ranges_section| {
        var lines = std.mem.splitScalar(u8, ranges_section, '\n');
        while (lines.next()) |line| {
            if (line.len == 0) continue;

            var range_it = std.mem.splitScalar(u8, line, '-');
            const start_str = range_it.next() orelse continue;
            const stop_str = range_it.next() orelse continue;

            const start = std.fmt.parseInt(i64, start_str, 10) catch continue;
            const stop = std.fmt.parseInt(i64, stop_str, 10) catch continue;

            try ranges.append(allocator, .{ .start = start, .stop = stop });
        }
    }

    // Parse ingredients
    if (parts.next()) |ingredients_section| {
        var lines = std.mem.splitScalar(u8, ingredients_section, '\n');
        while (lines.next()) |line| {
            if (line.len == 0) continue;
            const id = std.fmt.parseInt(i64, line, 10) catch continue;
            try ingredients.append(allocator, id);
        }
    }

    return .{ .ranges = ranges, .ingredients = ingredients };
}

fn isFresh(ranges: []const Range, id: i64) bool {
    for (ranges) |range| {
        if (id >= range.start and id <= range.stop) {
            return true;
        }
    }
    return false;
}

fn solvePart1(ranges: []const Range, ingredients: []const i64) usize {
    var count: usize = 0;
    for (ingredients) |id| {
        if (isFresh(ranges, id)) {
            count += 1;
        }
    }
    return count;
}

fn compareRanges(_: void, a: Range, b: Range) bool {
    return a.start < b.start;
}

fn mergeRanges(allocator: std.mem.Allocator, ranges: []const Range) !std.ArrayList(Range) {
    if (ranges.len == 0) {
        return .empty;
    }

    // Create a copy and sort
    var sorted: std.ArrayList(Range) = .empty;
    for (ranges) |r| {
        try sorted.append(allocator, r);
    }

    std.mem.sort(Range, sorted.items, {}, compareRanges);

    var merged: std.ArrayList(Range) = .empty;

    for (sorted.items) |range| {
        if (merged.items.len == 0) {
            try merged.append(allocator, range);
        } else {
            const last_idx = merged.items.len - 1;
            if (range.start <= merged.items[last_idx].stop + 1) {
                // Overlapping or adjacent, merge them
                merged.items[last_idx].stop = @max(merged.items[last_idx].stop, range.stop);
            } else {
                // No overlap, add new range
                try merged.append(allocator, range);
            }
        }
    }

    sorted.deinit(allocator);
    return merged;
}

fn solvePart2(allocator: std.mem.Allocator, ranges: []const Range) !i64 {
    var merged = try mergeRanges(allocator, ranges);
    defer merged.deinit(allocator);

    var total: i64 = 0;
    for (merged.items) |range| {
        total += range.stop - range.start + 1;
    }

    return total;
}

pub fn main() !void {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    defer _ = gpa.deinit();
    const allocator = gpa.allocator();

    const file = try std.fs.cwd().openFile("input.txt", .{});
    defer file.close();

    const content = try file.readToEndAlloc(allocator, 10 * 1024 * 1024);
    defer allocator.free(content);

    var parsed = try parseInput(allocator, content);
    defer parsed.ranges.deinit(allocator);
    defer parsed.ingredients.deinit(allocator);

    const part1 = solvePart1(parsed.ranges.items, parsed.ingredients.items);
    const part2 = try solvePart2(allocator, parsed.ranges.items);

    const stdout = std.fs.File.stdout();
    var buf: [64]u8 = undefined;

    var out = std.fmt.bufPrint(&buf, "Part 1: {d}\n", .{part1}) catch unreachable;
    try stdout.writeAll(out);

    out = std.fmt.bufPrint(&buf, "Part 2: {d}\n", .{part2}) catch unreachable;
    try stdout.writeAll(out);
}
