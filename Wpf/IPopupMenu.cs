using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Utillities.Wpf
{
    public static class IPopupMenuOptionDefaults
    {
        /// <summary>
        /// Default bg color each popupmenuoption is initialized with.
        /// </summary>
        public static Brush Background = Brushes.Black;
    }

    /// <summary>
    /// Popup menu type framework elements
    /// </summary>
    public interface IPopupMenu : IFrameworkElement
    {
        /// <summary>
        /// Support for the old <see cref="DropDownMenu"/> class.
        /// </summary>
        /// <param name="parent"></param>
        public void Instanciate(FrameworkElement parent);

        /// <summary>
        /// Collapses all options and their children menus.
        /// </summary>
        public void Collapse();

        /// <summary>
        /// Shows the menu with all it's options.
        /// </summary>
        public void Expand();

        /// <summary>
        /// Toggle between expanded and collapsed.
        /// </summary>
        public void Toggle();

        /// <summary>
        /// Correct the spacing of all options of this menu.
        /// </summary>
        public void MeasureAndArrange();


        internal bool IsTopMenu { get; set; }


        /// <summary>
        /// An option for the IPopupMenu class
        /// </summary>
        public interface IPopupMenuOption : IFrameworkElement
        {
            /// <summary>
            /// 
            /// </summary>
            public bool HasInnerMenu { get; }

            /// <summary>
            /// 
            /// </summary>
            public Image Icon { get; }

            public double IconWidth { get; set; }
            public double ArrowWidth { get; set; }
            public double NameWidth { get; set; }

            public double? DesiredWidth { set; }

            public double Width { get; }

            public double SpacingRight { get; set; }
            public double SpacingLeft { get; set; }

            public bool CanBeMeasured { get; }
            public event Action? OnMeasurable;

            public event Action? OnClick;

            public void Collapse();
        }
    }
}
