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
    /// A wrapping for a WPF window.
    /// </summary>
    public record class WindowWrapping
    {
        /// <summary>
        /// The new parent container, that is supposed to hold all 
        /// the windows content.
        /// </summary>
        public Panel Panel;

        /// <summary>
        /// The new Border that will surround the window.
        /// </summary>
        public Border Border;

        /// <summary>
        /// A `RectangleGeometry` object to help the with border visuals, such as clipping.
        /// </summary>
        public RectangleGeometry RectangleGeometry;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="border"></param>
        /// <param name="rectangleGeometry"></param>
        public WindowWrapping(Panel panel, Border border, RectangleGeometry rectangleGeometry)
        {
            Panel = panel;
            Border = border;
            RectangleGeometry = rectangleGeometry;
        }
    }
    

    /// <summary>
    /// Provides a quick way to wrap a window in a border for depper customization.
    /// </summary>
    public static class WindowWrapper {
        /// <summary>
        /// Will wrap the window in a border which allows for better customization.
        /// </summary>
        /// <param name="window">The window to be wrapped</param>
        /// <remarks>
        /// If the old content of the window was not a Panel, a new Canvas is created, and the old content of the window is added to the newly created panel for further usage of the window.
        /// The original content of the window is set to null before wrapping it in the border.
        /// The rectangle geometry is used to clip the wrapped content to the size of the window.
        /// </remarks>
        public static WindowWrapping Wrap(Window window) {
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

            return new WindowWrapping(newPanel, border, rectangleGeometry);
        }

    }
}
