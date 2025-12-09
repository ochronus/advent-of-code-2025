const std = @import("std");

const Point = struct { x: i64, y: i64 };

fn rectangleArea(p1: Point, p2: Point) i64 {
    const dx: i64 = @intCast(@abs(p2.x - p1.x));
    const dy: i64 = @intCast(@abs(p2.y - p1.y));
    return (dx + 1) * (dy + 1);
}

fn part1(tiles: []Point) i64 {
    var max_area: i64 = 0;
    var i: usize = 0;
    while (i < tiles.len - 1) : (i += 1) {
        var j = i + 1;
        while (j < tiles.len) : (j += 1) {
            const area = rectangleArea(tiles[i], tiles[j]);
            max_area = @max(max_area, area);
        }
    }
    return max_area;
}

fn isInsidePolygon(p: Point, polygon: []Point) bool {
    var inside = false;
    var j: usize = polygon.len - 1;
    var i: usize = 0;
    while (i < polygon.len) : (i += 1) {
        const xi = polygon[i].x;
        const yi = polygon[i].y;
        const xj = polygon[j].x;
        const yj = polygon[j].y;
        if (((yi > p.y) != (yj > p.y)) and (p.x < @divFloor((xj - xi) * (p.y - yi), (yj - yi)) + xi)) {
            inside = !inside;
        }
        j = i;
    }
    return inside;
}

fn isOnSegment(p: Point, p1: Point, p2: Point) bool {
    const min_x = @min(p1.x, p2.x);
    const max_x = @max(p1.x, p2.x);
    const min_y = @min(p1.y, p2.y);
    const max_y = @max(p1.y, p2.y);
    if (p.x < min_x or p.x > max_x or p.y < min_y or p.y > max_y) return false;
    if (p1.x == p2.x) return p.x == p1.x and p.y >= min_y and p.y <= max_y;
    if (p1.y == p2.y) return p.y == p1.y and p.x >= min_x and p.x <= max_x;
    return false;
}

fn isOnPolygonBoundary(p: Point, polygon: []Point) bool {
    var i: usize = 0;
    while (i < polygon.len) : (i += 1) {
        if (isOnSegment(p, polygon[i], polygon[(i + 1) % polygon.len])) return true;
    }
    return false;
}

fn isInsideOrOnPolygon(p: Point, polygon: []Point) bool {
    return isInsidePolygon(p, polygon) or isOnPolygonBoundary(p, polygon);
}

pub fn main() !void {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    defer _ = gpa.deinit();
    const allocator = gpa.allocator();

    const file = try std.fs.cwd().openFile("input.txt", .{});
    defer file.close();
    const content = try file.readToEndAlloc(allocator, 10 * 1024 * 1024);
    defer allocator.free(content);

    var tiles_list = std.ArrayListUnmanaged(Point){};
    defer tiles_list.deinit(allocator);

    var lines = std.mem.splitScalar(u8, content, '\n');
    while (lines.next()) |line| {
        if (line.len == 0) continue;
        var it = std.mem.splitScalar(u8, line, ',');
        const x = try std.fmt.parseInt(i64, it.next().?, 10);
        const y = try std.fmt.parseInt(i64, it.next().?, 10);
        try tiles_list.append(allocator, Point{ .x = x, .y = y });
    }

    const tiles = tiles_list.items;
    std.debug.print("Part 1: {d}\n", .{part1(tiles)});

    // Part 2
    var max_area: i64 = 0;
    var x_set = std.AutoHashMap(i64, void).init(allocator);
    defer x_set.deinit();
    var y_set = std.AutoHashMap(i64, void).init(allocator);
    defer y_set.deinit();

    for (tiles) |t| {
        try x_set.put(t.x, {});
        try y_set.put(t.y, {});
    }

    var all_x = std.ArrayListUnmanaged(i64){};
    defer all_x.deinit(allocator);
    var all_y = std.ArrayListUnmanaged(i64){};
    defer all_y.deinit(allocator);

    var x_it = x_set.keyIterator();
    while (x_it.next()) |x| try all_x.append(allocator, x.*);
    var y_it = y_set.keyIterator();
    while (y_it.next()) |y| try all_y.append(allocator, y.*);

    var i: usize = 0;
    while (i < tiles.len - 1) : (i += 1) {
        var j = i + 1;
        while (j < tiles.len) : (j += 1) {
            const min_x = @min(tiles[i].x, tiles[j].x);
            const max_x = @max(tiles[i].x, tiles[j].x);
            const min_y = @min(tiles[i].y, tiles[j].y);
            const max_y = @max(tiles[i].y, tiles[j].y);

            var all_valid = true;
            for (all_x.items) |x| {
                if (x < min_x or x > max_x) continue;
                for (all_y.items) |y| {
                    if (y < min_y or y > max_y) continue;
                    if (!isInsideOrOnPolygon(Point{ .x = x, .y = y }, tiles)) {
                        all_valid = false;
                        break;
                    }
                }
                if (!all_valid) break;
            }

            if (all_valid) max_area = @max(max_area, rectangleArea(tiles[i], tiles[j]));
        }
    }

    std.debug.print("Part 2: {d}\n", .{max_area});
}
