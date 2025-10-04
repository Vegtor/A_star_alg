using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using A_star_alg;

namespace A_star_alg
{
    public partial class MainWindow : Window
    {
        private enum Mode
        {
            None,
            PlaceStart,
            PlaceEnd,
            PlaceWall
        }

        private Cell[,] grid;
        private int grid_width;
        private int grid_height;
        private double cell_size = 20;
        private Mode current_mode = Mode.None;
        private bool is_dragging = false;
        private Cell start_cell = null;
        private Cell end_cell = null;

        public MainWindow()
        {
            InitializeComponent();
            setup_textbox_validation();
            initialize_grid();
        }


        // Input check for numbers ************************************************************************
        private void setup_textbox_validation()
        {
            x_axes.AddHandler(CommandManager.PreviewExecutedEvent, new ExecutedRoutedEventHandler(preview_executed_handler), true);
            y_axes.AddHandler(CommandManager.PreviewExecutedEvent, new ExecutedRoutedEventHandler(preview_executed_handler), true);
        }

        private void numeric_textbox_preview_text_input(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void preview_executed_handler(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                string clipboardText = Clipboard.GetText();
                if (!is_numeric(clipboardText))
                {
                    e.Handled = true;
                }
            }
        }

        private void textbox_text_changed(object sender, System.EventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                if (int.TryParse(textBox.Text, out int value))
                {
                    if (value < 5)
                    {
                        textBox.Text = "5";
                        textBox.CaretIndex = textBox.Text.Length;
                    }
                }
                else if (string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = "5";
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
        }

        private bool is_numeric(string text)
        {
            return int.TryParse(text, out _);
        }
        // ***********************************************************************************************************

        private void update_grid_dimensions()
        {
            if (!int.TryParse(x_axes.Text, out grid_width) || grid_width < 5)
            {
                grid_width = 30;
                x_axes.Text = "30";
            }
                

            if (!int.TryParse(y_axes.Text, out grid_height) || grid_height < 5)
            {
                grid_width = 30;
                y_axes.Text = "30";
            }
        }

        private void initialize_grid()
        {
            update_grid_dimensions();

            grid_canvas.Children.Clear();
            grid = new Cell[grid_width, grid_height];

            grid_canvas.SizeChanged += (s, e) => {
                if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
                {
                    cell_size = Math.Min(e.NewSize.Width / grid_width, e.NewSize.Height / grid_height);
                    draw_grid();
                }
            };

            for (int x = 0; x < grid_width; x++)
            {
                for (int y = 0; y < grid_height; y++)
                {
                    grid[x, y] = new Cell
                    {
                        X = x,
                        Y = y,
                        Type = Cell_type.Empty
                    };
                }
            }

            draw_grid();
        }

        private void draw_grid()
        {
            grid_canvas.Children.Clear();

            for (int x = 0; x < grid_width; x++)
            {
                for (int y = 0; y < grid_height; y++)
                {
                    var cell = grid[x, y];
                    var rect = new Rectangle
                    {
                        Width = cell_size - 1,
                        Height = cell_size - 1,
                        Stroke = Brushes.Gray,
                        StrokeThickness = 0.5
                    };

                    Canvas.SetLeft(rect, x * cell_size);
                    Canvas.SetTop(rect, y * cell_size);

                    cell.Rectangle = rect;
                    update_cell_appearance(cell);
                    grid_canvas.Children.Add(rect);
                }
            }
        }

        private void update_cell_appearance(Cell cell)
        {
            switch (cell.Type)
            {
                case Cell_type.Empty:
                    cell.Rectangle.Fill = Brushes.White;
                    break;
                case Cell_type.Start:
                    cell.Rectangle.Fill = new SolidColorBrush(Color.FromRgb(52, 152, 219));
                    break;
                case Cell_type.End:
                    cell.Rectangle.Fill = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                    break;
                case Cell_type.Wall:
                    cell.Rectangle.Fill = new SolidColorBrush(Color.FromRgb(52, 73, 94));
                    break;
                case Cell_type.Path:
                    cell.Rectangle.Fill = new SolidColorBrush(Color.FromRgb(39, 174, 96));
                    break;
                case Cell_type.Explored:
                    cell.Rectangle.Fill = new SolidColorBrush(Color.FromRgb(255, 193, 7));
                    break;
            }
        }

        private Cell get_cell_from_position(Point position)
        {
            int x = (int)(position.X / cell_size);
            int y = (int)(position.Y / cell_size);

            if (x >= 0 && x < grid_width && y >= 0 && y < grid_height)
                return grid[x, y];

            return null;
        }

        private void set_button_styles()
        {
            btn_start.Background = new SolidColorBrush(Color.FromRgb(52, 152, 219));
            btn_end.Background = new SolidColorBrush(Color.FromRgb(231, 76, 60));
            btn_wall.Background = new SolidColorBrush(Color.FromRgb(52, 73, 94));
            btn_clear.Background = new SolidColorBrush(Color.FromRgb(149, 165, 166));
            btn_find_path.Background = new SolidColorBrush(Color.FromRgb(39, 174, 96));
            btn_set_grid.Background = new SolidColorBrush(Color.FromRgb(147, 112, 219)); // MediumPurple
        }

        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            current_mode = Mode.PlaceStart;
            set_button_styles();
            ((Button)sender).Background = new SolidColorBrush(Color.FromRgb(41, 128, 185));
        }

        private void btn_end_Click(object sender, RoutedEventArgs e)
        {
            current_mode = Mode.PlaceEnd;
            set_button_styles();
            ((Button)sender).Background = new SolidColorBrush(Color.FromRgb(192, 57, 43));
        }

        private void btn_wall_Click(object sender, RoutedEventArgs e)
        {
            current_mode = Mode.PlaceWall;
            set_button_styles();
            ((Button)sender).Background = new SolidColorBrush(Color.FromRgb(44, 62, 80));
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            clear_grid();
            current_mode = Mode.None;
            set_button_styles();
        }

        private void btn_set_grid_Click(object sender, RoutedEventArgs e)
        {
            start_cell = null;
            end_cell = null;
            current_mode = Mode.None;
            set_button_styles();

            initialize_grid();

            path_length_text.Text = "0";
            nodes_explored_text.Text = "0";
        }

        private void clear_grid()
        {
            for (int x = 0; x < grid_width; x++)
            {
                for (int y = 0; y < grid_height; y++)
                {
                    var cell = grid[x, y];
                    cell.Type = Cell_type.Empty;
                    cell.G = 0;
                    cell.H = 0;
                    cell.F = 0;
                    cell.Parent = null;
                    update_cell_appearance(cell);
                }
            }
            start_cell = null;
            end_cell = null;
            path_length_text.Text = "0";
            nodes_explored_text.Text = "0";
        }

        private void grid_canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            is_dragging = true;
            grid_canvas.CaptureMouse();
            handle_cell_click(e.GetPosition(grid_canvas));
        }

        private void grid_canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(!is_dragging)
            {
                var selected_cell = get_cell_from_position(e.GetPosition(grid_canvas));
                if (selected_cell.Type != Cell_type.Empty)
                {
                    switch (selected_cell.Type)
                    {
                        case Cell_type.Start:
                            start_cell = null;
                            break;
                        case Cell_type.End:
                            end_cell = null;
                            break;
                    }
                    selected_cell.Type = Cell_type.Empty;
                    update_cell_appearance(selected_cell);
                }
            }
        }
        private void grid_canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (is_dragging && current_mode == Mode.PlaceWall)
            {
                handle_cell_click(e.GetPosition(grid_canvas));
            }
        }

        private void grid_canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            is_dragging = false;
            grid_canvas.ReleaseMouseCapture();
        }

        private void handle_cell_click(Point position)
        {
            var cell = get_cell_from_position(position);
            if (cell == null) return;

            switch (current_mode)
            {
                case Mode.PlaceStart:
                    if (start_cell != null)
                    {
                        start_cell.Type = Cell_type.Empty;
                        update_cell_appearance(start_cell);
                    }
                    if (cell.Type != Cell_type.End && cell.Type != Cell_type.Wall)
                    {
                        start_cell = cell;
                        cell.Type = Cell_type.Start;
                        update_cell_appearance(cell);
                    }
                    break;

                case Mode.PlaceEnd:
                    if (end_cell != null)
                    {
                        end_cell.Type = Cell_type.Empty;
                        update_cell_appearance(end_cell);
                    }
                    if (cell.Type != Cell_type.Start && cell.Type != Cell_type.Wall)
                    {
                        end_cell = cell;
                        cell.Type = Cell_type.End;
                        update_cell_appearance(cell);
                    }
                    break;

                case Mode.PlaceWall:
                    if (cell.Type == Cell_type.Empty)
                    {
                        cell.Type = Cell_type.Wall;
                        update_cell_appearance(cell);
                    }
                    break;
            }
        }

        private void clear_pathfinding_marks()
        {
            foreach (var cell in grid)
            {
                if (cell.Type == Cell_type.Path || cell.Type == Cell_type.Explored)
                {
                    cell.Type = Cell_type.Empty;
                    update_cell_appearance(cell);
                }
                cell.G = cell.H = cell.F = 0;
                cell.Parent = null;
            }
        }

        private async void run_a_star()
        {
            bool animate = checkbox_animate.IsChecked == true;
            var Path_finder = new A_Star_Path_Finder(grid, start_cell, end_cell, update_cell_appearance);
            if (animate) 
            {
                var path = await Path_finder.find_path_animated();
                finish_path_display(path);
            }
            else 
            {
                var path = Path_finder.find_path();
                finish_path_display(path);
            }
        }

        private void finish_path_display(List<Cell> path)
        {
            if (path != null)
            {
                path_length_text.Text = path.Count.ToString();
                nodes_explored_text.Text = grid.Cast<Cell>().Count(c => c.Type == Cell_type.Explored || c.Type == Cell_type.Path).ToString();
            }
            else
            {
                MessageBox.Show("No path found!", "A* Result", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void btn_find_path_Click(object sender, RoutedEventArgs e)
        {
            if (start_cell != null && end_cell != null)
            {
                clear_pathfinding_marks();
                run_a_star();
            }
            else
            {
                MessageBox.Show("Please set both start and end points!", "Missing Points",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}