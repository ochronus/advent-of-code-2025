const std = @import("std");

fn parseLine(line: []const u8) i32 {
    if (line.len == 0) return 0;

    const direction = line[0];
    const value = std.fmt.parseInt(i32, line[1..], 10) catch return 0;

    return if (direction == 'L') -value else value;
}

fn countCrossings(dial: i32, movement: i32) i32 {
    const full_rotations = @divTrunc(@abs(movement), 100);
    const sign: i32 = if (movement >= 0) 1 else -1;
    const remainder = movement - sign * @as(i32, @intCast(full_rotations)) * 100;
    const new_pos = dial + remainder;

    const boundary_crossing: i32 = if (remainder < 0 and dial > 0 and new_pos <= 0)
        1
    else if (remainder > 0 and new_pos > 99)
        1
    else
        0;

    return @as(i32, @intCast(full_rotations)) + boundary_crossing;
}

fn normalizeDial(dial: i32) i32 {
    return @mod(@mod(dial, 100) + 100, 100);
}

pub fn main() !void {
    const allocator = std.heap.page_allocator;

    const file = try std.fs.cwd().openFile("input.txt", .{});
    defer file.close();

    const content = try file.readToEndAlloc(allocator, 1024 * 1024);
    defer allocator.free(content);

    var dial: i32 = 50;
    var zeros: i32 = 0;
    var crossings: i32 = 0;

    var lines = std.mem.splitScalar(u8, content, '\n');
    while (lines.next()) |line| {
        if (line.len == 0) continue;

        const movement = parseLine(line);
        crossings += countCrossings(dial, movement);
        dial = normalizeDial(dial + movement);
        if (dial == 0) zeros += 1;
    }

    const stdout = std.fs.File.stdout();
    var buf: [64]u8 = undefined;

    var out = std.fmt.bufPrint(&buf, "{d}\n", .{zeros}) catch unreachable;
    try stdout.writeAll(out);

    out = std.fmt.bufPrint(&buf, "{d}\n", .{crossings}) catch unreachable;
    try stdout.writeAll(out);
}
