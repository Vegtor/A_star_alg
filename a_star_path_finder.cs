using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Shapes;
using A_star_alg;

namespace A_star_alg
{
    public class A_Star_Path_Finder
    {
        private Cell[,] grid;
        private Cell start;
        private Cell end;
        private Action<Cell> update_visual;

        public A_Star_Path_Finder(Cell[,] grid, Cell start, Cell end, Action<Cell> update_visual)
        {
            this.grid = grid;
            this.start = start;
            this.end = end;
            this.update_visual = update_visual;
        }

        private double heuristic(Cell a, Cell b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private IEnumerable<Cell> get_neighbors(Cell cell)
        {
            var neighbors = new List<Cell>();

            var directions = new (int dx, int dy)[]
            {
                (-1, 0),
                (1, 0),
                (0, -1),
                (0, 1),
                (-1, -1),
                (-1, 1),
                (1, -1),
                (1, 1)
            };

            foreach (var (dx, dy) in directions)
            {
                int newX = cell.X + dx;
                int newY = cell.Y + dy;

                if (newX >= 0 && newX < grid.GetLength(0) &&
                    newY >= 0 && newY < grid.GetLength(1))
                {
                    neighbors.Add(grid[newX, newY]);
                }
            }

            return neighbors;
        }

        public List<Cell> find_path()
        {
            var open_set = new List<Cell> { start };
            var closed_set = new HashSet<Cell>();

            foreach (var cell in grid)
            {
                cell.G = cell.H = cell.F = 0;
                cell.Parent = null;
            }

            while (open_set.Any())
            {
                var current = open_set.OrderBy(c => c.F).First();
                if (current == end)
                    return reconstruct_path(current);

                open_set.Remove(current);
                closed_set.Add(current);

                foreach (var neighbor in get_neighbors(current))
                {
                    if (closed_set.Contains(neighbor) || neighbor.Type == Cell_type.Wall)
                        continue;

                    double tentative_G = current.G + 1;

                    if (!open_set.Contains(neighbor))
                        open_set.Add(neighbor);
                    else if (tentative_G >= neighbor.G)
                        continue;

                    neighbor.Parent = current;
                    neighbor.G = tentative_G;
                    neighbor.H = heuristic(neighbor, end);
                    neighbor.F = neighbor.G + neighbor.H;

                    if (neighbor != start && neighbor != end)
                    {
                        neighbor.Type = Cell_type.Explored;
                        update_visual(neighbor);
                    }
                }
            }
            return null;
        }

        private List<Cell> reconstruct_path(Cell current)
        {
            var path = new List<Cell>();
            while (current.Parent != null)
            {
                if (current != start && current != end)
                {
                    current.Type = Cell_type.Path;
                    update_visual(current);
                }
                path.Add(current);
                current = current.Parent;
            }
            path.Reverse();
            return path;
        }

        public async Task<List<Cell>> find_path_animated()
        {
            var open_set = new List<Cell> { start };
            var closed_set = new HashSet<Cell>();

            foreach (var cell in grid)
            {
                cell.G = cell.H = cell.F = 0;
                cell.Parent = null;
            }

            while (open_set.Any())
            {
                var current = open_set.OrderBy(c => c.F).First();
                if (current == end)
                    return await reconstruct_path_animated(current);

                open_set.Remove(current);
                closed_set.Add(current);

                foreach (var neighbor in get_neighbors(current))
                {
                    if (closed_set.Contains(neighbor) || neighbor.Type == Cell_type.Wall)
                        continue;

                    double tentative_G = current.G + 1;

                    if (!open_set.Contains(neighbor))
                        open_set.Add(neighbor);
                    else if (tentative_G >= neighbor.G)
                        continue;

                    neighbor.Parent = current;
                    neighbor.G = tentative_G;
                    neighbor.H = heuristic(neighbor, end);
                    neighbor.F = neighbor.G + neighbor.H;

                    if (neighbor != start && neighbor != end)
                    {
                        neighbor.Type = Cell_type.Explored;
                        update_visual(neighbor);
                        await Task.Delay(10);
                    }
                }
            }
            return null;
        }

        private async Task<List<Cell>> reconstruct_path_animated(Cell current)
        {
            var path = new List<Cell>();
            while (current.Parent != null)
            {
                if (current != start && current != end)
                {
                    current.Type = Cell_type.Path;
                    update_visual(current);
                    await Task.Delay(20); // Slower animation for path
                }
                path.Add(current);
                current = current.Parent;
            }
            path.Reverse();
            return path;
        }


    }
}
