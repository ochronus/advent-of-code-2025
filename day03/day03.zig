const std = @import("std");

/// Part 1: Pick exactly 2 batteries to form the largest 2-digit number
fn findMaxJoltage2(bank: []const u8) u32 {
    var max_joltage: u32 = 0;

    for (0..bank.len - 1) |i| {
        for (i + 1..bank.len) |j| {
            const digit_i = bank[i] - '0';
            const digit_j = bank[j] - '0';
            const joltage = @as(u32, digit_i) * 10 + @as(u32, digit_j);
            max_joltage = @max(max_joltage, joltage);
        }
    }

    return max_joltage;
}

/// Part 2: Pick exactly 12 batteries to form the largest 12-digit number
/// Uses a greedy approach: at each step, pick the largest digit possible
/// while ensuring enough digits remain for the rest of the selection
fn findMaxJoltage12(bank: []const u8) u64 {
    const num_to_select: usize = 12;
    const n = bank.len;

    var result: u64 = 0;
    var start_idx: usize = 0;

    var remaining: usize = num_to_select;
    while (remaining > 0) : (remaining -= 1) {
        // We can only look up to index (n - remaining) to leave enough for the rest
        const end_idx = n - remaining;

        // Find position of maximum digit in valid range
        var max_digit: u8 = 0;
        var max_pos: usize = start_idx;
        for (start_idx..end_idx + 1) |i| {
            const digit = bank[i] - '0';
            if (digit > max_digit) {
                max_digit = digit;
                max_pos = i;
            }
        }

        result = result * 10 + @as(u64, max_digit);
        start_idx = max_pos + 1;
    }

    return result;
}

pub fn main() !void {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    defer _ = gpa.deinit();
    const allocator = gpa.allocator();

    const file = try std.fs.cwd().openFile("input.txt", .{});
    defer file.close();

    const content = try file.readToEndAlloc(allocator, 10 * 1024 * 1024);
    defer allocator.free(content);

    var total_part1: u32 = 0;
    var total_part2: u64 = 0;

    var lines = std.mem.splitScalar(u8, content, '\n');
    while (lines.next()) |line| {
        if (line.len == 0) continue;

        total_part1 += findMaxJoltage2(line);
        total_part2 += findMaxJoltage12(line);
    }

    const stdout = std.fs.File.stdout();
    var buf: [128]u8 = undefined;

    var out = std.fmt.bufPrint(&buf, "Part 1 - Total output joltage: {d}\n", .{total_part1}) catch unreachable;
    try stdout.writeAll(out);

    out = std.fmt.bufPrint(&buf, "Part 2 - Total output joltage: {d}\n", .{total_part2}) catch unreachable;
    try stdout.writeAll(out);
}
