using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Utillities.Wpf {
    /// <summary>
    /// Provides a quick way to wrap a window in a border for depper customization.
    /// </summary>
    public static class WindowWrapper {
        /// <summary>
        /// Will wrap the window in a border which allows for better customization.
        /// </summary>
        /// <param name="window">The window to be wrapped</param>
        /// <param name="pPanel">Output parameter that receives the panel containing the window's content.</param>
        /// <param name="pBorder">Output parameter that receives the border wrapping the window.</param>
        /// <param name="pRectangleGeometry">Output parameter that receives the rectangle geometry used for clipping.</param>
        /// <remarks>
        /// If the old content of the window was not a Panel, a new Canvas is created, and the old content of the window is added to the newly created panel for further usage of the window.
        /// The original content of the window is set to null before wrapping it in the border.
        /// The rectangle geometry is used to clip the wrapped content to the size of the window.
        /// </remarks>
        public static void Wrap(Window window, out Panel pPanel, out Border pBorder, out RectangleGeometry pRectangleGeometry) {
            Panel newPanel;
            if (window.Content is Panel)
                newPanel = (window.Content as Panel)!;
            else {
                newPanel = new Canvas { Background = Brushes.Blue };
                newPanel.Children.Add(window.Content as UIElement);
            }
            window.Content = null;

            var rectangleGeometry = new RectangleGeometry {
                Rect = new Rect(0, 0, window.Width, window.Width),
                RadiusX = 10,
                RadiusY = 10,
            };
            newPanel.Clip = rectangleGeometry;

            Border border = new Border {
                CornerRadius = new CornerRadius(0),
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Transparent,
                Child = newPanel
            };
            border.SizeChanged += (_, e) => {
                rectangleGeometry.Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height);
            };

            window.Content = border;

            pPanel = newPanel;
            pBorder = border;
            pRectangleGeometry = rectangleGeometry;
        }

    }
}
