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
        /// <returns>If the old content of the Window was a Panel, it will be returned as is. 
        /// If not, a new Canvas will be created and the old Content of the window will be added to the newly created for further usage of the window.</returns>
        public static (Panel, Border) Wrap(Window window) {
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
                RadiusX = 25,
                RadiusY = 25,
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
            return (newPanel, border);
        }

    }
}
