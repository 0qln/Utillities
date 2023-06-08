using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows;

namespace Utillities.Wpf
{
    /// <summary>
    /// Provides helper methods for developing WPF projects.
    /// </summary>
    public static class Helper {
        /// <summary>
        /// Converts a color string representation into a WPF Brush object.
        /// The color string should be in the format: transparency, red, green, blue.
        /// Transparency and color values should be represented as two-digit hexadecimal numbers.
        /// </summary>
        /// <param name="colorString">The color string to convert.</param>
        /// <returns>A SolidColorBrush representing the color.</returns>
        /// <exception cref="ArgumentException">Thrown when the color string is invalid or has an incorrect format.</exception>
        public static Brush StringToBrush(string colorString) {
            if (colorString[0] == '#') colorString = colorString.Substring(1, 8);

            if (colorString.Length != 8) {
                throw new ArgumentException("Invalid color string. Expected format: transparency, r, g, b");
            }

            // Extract transparency, red, green, and blue values from the color string
            byte transparency = byte.Parse(colorString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte red = byte.Parse(colorString.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte green = byte.Parse(colorString.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte blue = byte.Parse(colorString.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

            // Create and return a SolidColorBrush using the extracted color values
            return new SolidColorBrush(Color.FromArgb(transparency, red, green, blue));
        }


        /// <summary>
        /// Updates the hover color of a button.
        /// </summary>
        /// <param name="button">The button to update.</param>
        /// <param name="colorWhenButtonhover">The color to apply when the button is hovered.</param>
        public static void UpdateButtonHoverColor(Button button, Brush colorWhenButtonhover) {
            // Get the button's style
            Style newStyle = new Style(typeof(Button), button.Style);

            // Create the new Trigger
            Trigger mouseOverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            mouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, colorWhenButtonhover));
            newStyle.Triggers.Add(mouseOverTrigger);

            // Apply the updated style to the button
            button.Style = newStyle;
        }

        /// <summary>
        /// Sets the specified element as active in the window's chrome.
        /// </summary>
        /// <param name="element">The element to set as active.</param>
        public static void SetWindowChromActive(IInputElement element) {
            WindowChrome.SetIsHitTestVisibleInChrome(element, true);
        }

        /// <summary>
        /// Sets the source of an image control to the specified path.
        /// </summary>
        /// <param name="image">The image control to update.</param>
        /// <param name="path">The path to the image source.</param>
        public static void SetImageSource(System.Windows.Controls.Image image, string path) {
            image.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Sets a child element within a grid at the specified row and column.
        /// </summary>
        /// <param name="grid">The parent grid.</param>
        /// <param name="child">The child element to add to the grid.</param>
        /// <param name="row">The row index to place the child element.</param>
        /// <param name="column">The column index to place the child element.</param>
        public static void SetChildInGrid(Grid grid, UIElement child, int row, int column) {
            grid.Children.Add(child);
            Grid.SetColumn(child, column);
            Grid.SetRow(child, row);
        }

        /// <summary>
        /// Retrieves the absolute position of an element relative to the specified root window.
        /// </summary>
        /// <param name="element">The element to get the position of.</param>
        /// <param name="rootWindow">The root window.</param>
        /// <returns>The absolute position of the element.</returns>
        public static Point GetAbsolutePosition(FrameworkElement? element, Window rootWindow) {
            if (element == null) return new Point(-1, -1);
            return element.TransformToAncestor(rootWindow).Transform(new Point(0, 0));
        }

        /// <summary>
        /// Retrieves the actual width of a column within a grid after layout calculation.
        /// </summary>
        /// <param name="grid">The grid containing the column.</param>
        /// <param name="columnIndex">The index of the column to measure.</param>
        /// <returns>The actual width of the column.</returns>
        public static double GetActualColumnWidth(Grid grid, int columnIndex) {
            grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            grid.Arrange(new Rect(0, 0, grid.DesiredSize.Width, grid.DesiredSize.Height));
            var columnWidth = grid.ColumnDefinitions[columnIndex].ActualWidth;
            return columnWidth;
        }

        /// <summary>
        /// Retrieves the actual height of a row within a grid after layout calculation.
        /// </summary>
        /// <param name="grid">The grid containing the row.</param>
        /// <param name="rowIndex">The index of the row to measure.</param>
        /// <returns>The actual height of the row.</returns>
        public static double GetActualRowHeight(Grid grid, int rowIndex) {
            grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            grid.Arrange(new Rect(0, 0, grid.DesiredSize.Width, grid.DesiredSize.Height));
            var rowHeight = grid.RowDefinitions[rowIndex].ActualHeight;
            return rowHeight;
        }

        /// <summary>
        /// Converts a string representation of a color to a SolidColorBrush with the specified opacity.
        /// </summary>
        /// <param name="colorString">The string representation of the color.</param>
        /// <param name="opacity">The opacity value to apply to the color (default is 1.0).</param>
        /// <returns>The SolidColorBrush representing the color.</returns>
        public static SolidColorBrush StringToSolidColorBrush(string colorString, double opacity = 1.0) {
            SolidColorBrush brush;

            try {
                Color color = (Color)ColorConverter.ConvertFromString(colorString);
                color.A = (byte)(255 * opacity);
                brush = new SolidColorBrush(color);
            }
            catch (FormatException) {
                // If the string cannot be converted to a color, return a transparent brush
                brush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }

            return brush;
        }

        /// <summary>
        /// Adds a new row with the specified height to a grid.
        /// </summary>
        /// <param name="grid">The grid to add the row to.</param>
        /// <param name="value">The height value of the row.</param>
        /// <param name="type">The type of height measurement for the row.</param>
        public static void AddRow(Grid grid, double value, GridUnitType type) {
            RowDefinition rowDefinition = new RowDefinition { Height = new GridLength(value, type) };
            grid.RowDefinitions.Add(rowDefinition);
        }

        /// <summary>
        /// Adds a new column with the specified width to a grid.
        /// </summary>
        /// <param name="grid">The grid to add the column to.</param>
        /// <param name="value">The width value of the column.</param>
        /// <param name="type">The type of width measurement for the column.</param>
        public static void AddColumn(Grid grid, double value, GridUnitType type) {
            ColumnDefinition columnDefinition = new ColumnDefinition { Width = new GridLength(value, type) };
            grid.ColumnDefinitions.Add(columnDefinition);
        }
    }
}
