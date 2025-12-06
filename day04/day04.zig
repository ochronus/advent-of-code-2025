const std = @import("std");

const Direction = struct {
    dr: i32,
    dc: i32,
};

const DIRECTIONS = [_]Direction{
    .{ .dr = -1, .dc = 0 }, // N
    .{ .dr = -1, .dc = 1 }, // NE
    .{ .dr = 0, .dc = 1 }, // E
    .{ .dr = 1, .dc = 1 }, // SE
    .{ .dr = 1, .dc = 0 }, // S
    .{ .dr = 1, .dc = -1 }, // SW
    .{ .dr = 0, .dc = -1 }, // W
    .{ .dr = -1, .dc = -1 }, // NW
};

fn parseGrid(allocator: std.mem.Allocator, input: []const u8) !std.ArrayList(std.ArrayList(u8)) {
    var grid: std.ArrayList(std.ArrayList(u8)) = .empty;

    var lines = std.mem.splitScalar(u8, std.mem.trim(u8, input, "\n\r "), '\n');
    while (lines.next()) |line| {
        if (line.len == 0) continue;

        var row: std.ArrayList(u8) = .empty;
        for (line) |c| {
            try row.append(allocator, c);
        }
        try grid.append(allocator, row);
    }

    return grid;
}

fn freeGrid(allocator: std.mem.Allocator, grid: *std.ArrayList(std.ArrayList(u8))) void {
    for (grid.items) |*row| {
        row.deinit(allocator);
    }
    grid.deinit(allocator);
}

fn countAdjacentRolls(grid: []const std.ArrayList(u8), row: usize, col: usize) usize {
    const rows = grid.len;
    const cols = grid[0].items.len;

    var count: usize = 0;
    for (DIRECTIONS) |dir| {
        const new_row_signed = @as(i32, @intCast(row)) + dir.dr;
        const new_col_signed = @as(i32, @intCast(col)) + dir.dc;

        if (new_row_signed < 0 or new_col_signed < 0) continue;

        const new_row: usize = @intCast(new_row_signed);
        const new_col: usize = @intCast(new_col_signed);

        if (new_row >= rows or new_col >= cols) continue;

        if (grid[new_row].items[new_col] == '@') {
            count += 1;
        }
    }

    return count;
}

const Position = struct {
    row: usize,
    col: usize,
};

fn findAccessibleRolls(allocator: std.mem.Allocator, grid: []const std.ArrayList(u8)) !std.ArrayList(Position) {
    var accessible: std.ArrayList(Position) = .empty;

    const rows = grid.len;
    const cols = grid[0].items.len;

    for (0..rows) |row| {
        for (0..cols) |col| {
            if (grid[row].items[col] == '@' and countAdjacentRolls(grid, row, col) < 4) {
                try accessible.append(allocator, .{ .row = row, .col = col });
            }
        }
    }

    return accessible;
}

fn solvePart1(allocator: std.mem.Allocator, grid: []const std.ArrayList(u8)) !usize {
    var accessible = try findAccessibleRolls(allocator, grid);
    defer accessible.deinit(allocator);
    return accessible.items.len;
}

fn solvePart2(allocator: std.mem.Allocator, grid: *std.ArrayList(std.ArrayList(u8))) !usize {
    var total_removed: usize = 0;

    while (true) {
        var accessible = try findAccessibleRolls(allocator, grid.items);
        defer accessible.deinit(allocator);

        if (accessible.items.len == 0) break;

        for (accessible.items) |pos| {
            grid.items[pos.row].items[pos.col] = '.';
        }

        total_removed += accessible.items.len;
    }

    return total_removed;
}

pub fn main() !void {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    defer _ = gpa.deinit();
    const allocator = gpa.allocator();

    const file = try std.fs.cwd().openFile("input.txt", .{});
    defer file.close();

    const content = try file.readToEndAlloc(allocator, 10 * 1024 * 1024);
    defer allocator.free(content);

    var grid = try parseGrid(allocator, content);
    defer freeGrid(allocator, &grid);

    // Part 1: we need to run it on the original grid first
    const part1 = try solvePart1(allocator, grid.items);

    // Part 2: modifies the grid in place
    const part2 = try solvePart2(allocator, &grid);

    const stdout = std.fs.File.stdout();
    var buf: [64]u8 = undefined;

    var out = std.fmt.bufPrint(&buf, "Part 1: {d}\n", .{part1}) catch unreachable;
    try stdout.writeAll(out);

    out = std.fmt.bufPrint(&buf, "Part 2: {d}\n", .{part2}) catch unreachable;
    try stdout.writeAll(out);
}
