using System.Windows.Shapes;

namespace A_star_alg
{
    public enum Cell_type
    {
        Empty,
        Start,
        End,
        Wall,
        Path,
        Explored
    }
    public class Cell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Cell_type Type { get; set; }
        public Rectangle Rectangle { get; set; }
        public double F { get; set; }
        public double G { get; set; }
        public double H { get; set; }
        public Cell Parent { get; set; }
    }
}
