using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;

namespace Utillities.Wpf
{
    /// <summary>
    /// Represents a drop-down menu in a WPF application. 
    /// </summary>
    public class DropDownMenu {
        private Window rootWindow;
        private bool isChildOfMenu = false;
        private DropDownMenu? parentMenu;
        private MenuOption? parentOption;
        private FrameworkElement? parentElement;
        private Point position = new(0, 0);
        private Border border = new();
        private StackPanel verticalPanel = new();
        private List<MenuOption> options = new();
        private string name;

        /// <summary>
        /// Gets the position of the drop-down menu.
        /// </summary>
        public Point Position => position;

        /// <summary>
        /// Gets the UI element of the drop-down menu.
        /// </summary>
        public FrameworkElement UIElement => border;

        /// <summary>
        /// Gets the list of menu options in the drop-down menu.
        /// </summary>
        public List<MenuOption> Options => options;

        /// <summary>
        /// Gets the name of the drop-down menu.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Initializes a new instance of the DropDownMenu class with the specified name and root window.
        /// </summary>
        /// <param name="name">The name of the drop-down menu.</param>
        /// <param name="rootWindow">The root window of the application.</param>
        public DropDownMenu(string name, Window rootWindow) {
            this.rootWindow = rootWindow;
            this.name = name;
            Init();
        }

        /// <summary>
        /// Sets the parent element for the drop-down menu.
        /// </summary>
        /// <param name="parent">The parent element.</param>
        public void Instanciate(FrameworkElement parent) {
            this.parentElement = parent;
        }

        /// <summary>
        /// Sets the parent menu and option for the drop-down menu.
        /// </summary>
        /// <param name="parentOption">The parent menu option.</param>
        /// <param name="parentMenu">The parent drop-down menu.</param>
        public void Instanciate(MenuOption parentOption, DropDownMenu parentMenu) {
            isChildOfMenu = true;
            this.parentMenu = parentMenu;
            this.parentOption = parentOption;
        }

        private void Init() {
            verticalPanel.Background = Helper.StringToSolidColorBrush("#2e2e2e");
            verticalPanel.Orientation = Orientation.Vertical;
            border.Child = verticalPanel;
            border.Style = System.Windows.Application.Current.Resources["ClientButtonUnfoldMenu_Style"] as Style;
        }

        /// <summary>
        /// Sets the canvas on which the drop-down menu is placed.
        /// </summary>
        /// <param name="canvas">The canvas on which the drop-down menu is placed.</param>
        public void SetCanvas(Canvas canvas) {
            canvas.Children.Add(UIElement);
        }

        /// <summary>
        /// Toggles the visibility of the drop-down menu.
        /// </summary>
        public void ToggleVisibility() {
            if (UIElement.Visibility == Visibility.Visible) {
                Hide();
            }
            else if (UIElement.Visibility == Visibility.Collapsed) {
                Show();
            }
        }

        /// <summary>
        /// Shows the drop-down menu.
        /// </summary>
        public void Show() => UIElement.Visibility = Visibility.Visible;

        /// <summary>
        /// Hides the drop-down menu.
        /// </summary>
        public void Hide() => UIElement.Visibility = Visibility.Collapsed;

        /// <summary>
        /// Hides the drop-down menu and its child menus.
        /// </summary>
        public void HideWidthChildrenMenus() {
            UIElement.Visibility = Visibility.Collapsed;
            foreach (var option in options) {
                if (option.HasMenu) {
                    option.HideChildrenMenu();
                }
            }
        }

        /// <summary>
        /// Hides the child menus of the drop-down menu.
        /// </summary>
        public void HideChildrenMenus() {
            foreach (var option in options) {
                if (option.HasMenu) {
                    option.HideChildrenMenu();
                }
            }
        }

        /// <summary>
        /// Changes the background color of the drop-down menu.
        /// </summary>
        /// <param name="color">The new background color.</param>
        public void ChangeBGColor(Brush color) {
            verticalPanel.Background = color;
        }

        /// <summary>
        /// Changes the background color of the drop-down menu and its child menus.
        /// </summary>
        /// <param name="color">The new background color.</param>
        public void ChangeBGColorWithChildren(Brush color) {
            ChangeBGColor(color);
            foreach (var option in options) {
                if (option.HasMenu) {
                    option.ChildMenu.ChangeBGColor(color);
                }
            }
        }

        /// <summary>
        /// Adds a new menu option with the specified name and height to the drop-down menu.
        /// </summary>
        /// <param name="name">The name of the menu option.</param>
        /// <param name="height">The height of the menu option.</param>
        /// <returns>The created menu option.</returns>
        public MenuOption AddOption(string name, double height = 22) {
            return AddOption(NewOption(name, height));
        }

        /// <summary>
        /// Adds the specified menu option to the drop-down menu.
        /// </summary>
        /// <param name="option">The menu option to add.</param>
        /// <returns>The added menu option.</returns>
        public MenuOption AddOption(MenuOption option) {
            options.Add(option);
            verticalPanel.Children.Add(option.UIElement);
            UpdateOptionLayout();
            return option;
        }

        /// <summary>
        /// Creates a new menu option with the specified name and height.
        /// </summary>
        /// <param name="name">The name of the menu option.</param>
        /// <param name="height">The height of the menu option.</param>
        /// <returns>The created menu option.</returns>
        public MenuOption NewOption(string name, double height = 22) {
            return new MenuOption(height, name, this);
        }

        /// <summary>
        /// Gets the menu option at the specified index.
        /// </summary>
        /// <param name="index">The index of the menu option.</param>
        /// <returns>The menu option at the specified index.</returns>
        public MenuOption GetOption(int index) {
            return options[index];
        }

        /// <summary>
        /// Gets the menu option with the specified name.
        /// </summary>
        /// <param name="name">The name of the menu option.</param>
        /// <returns>The menu option with the specified name, or null if not found.</returns>
        public MenuOption? GetOption(string name) {
            return options.Find(option => name == option.GetName);
        }

        /// <summary>
        /// Updates the layout of the menu options in the drop-down menu.
        /// </summary>
        public void UpdateOptionLayout() {
            double maxWidth = 0;
            var grids = verticalPanel.Children.OfType<Grid>();

            // Adjust column width for name
            foreach (var grid in grids) {
                grid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Auto);
            }
            maxWidth = grids.Max(g => Helper.GetActualColumnWidth(g, 1));
            foreach (var grid in grids) {
                grid.ColumnDefinitions[1].Width = new GridLength(maxWidth);
            }

            // Adjust column width for shortcut
            foreach (var grid in grids) {
                grid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Auto);
            }
            maxWidth = grids.Max(g => Helper.GetActualColumnWidth(g, 2));
            foreach (var grid in grids) {
                grid.ColumnDefinitions[2].Width = new GridLength(maxWidth);
            }
        }

        /// <summary>
        /// Updates the position of the drop-down menu.
        /// </summary>
        public void UpdateMenuPosition() {
            if (!isChildOfMenu) {
                if (parentElement == null) {
                    return;
                }
                position = Helper.GetAbsolutePosition(parentElement, rootWindow);
                position.Y += parentElement.ActualHeight;
                border.RenderTransform = new TranslateTransform(position.X, position.Y);
            }
            else if (parentMenu != null && parentOption != null) {
                position = parentMenu!.Position;
                position.X += parentOption!.UIElement.ActualWidth;
                position.Y += parentMenu!.options.IndexOf(parentOption) * parentOption!.UIElement.ActualHeight;
                border.RenderTransform = new TranslateTransform(position.X, position.Y);
            }
        }

        /// <summary>
        /// Updates the position of the drop-down menu and its child menus.
        /// </summary>
        public void UpdateMenuPositionWithChildren() {
            UpdateMenuPosition();
            foreach (var option in options) {
                if (option.HasMenu) {
                    option.UpdateMenuPosition();
                }
            }
        }

        /// <summary>
        /// Represents a menu option in the drop-down menu.
        /// </summary>
        public class MenuOption {
            private string? name;
            /// <summary> Gets the name of the menu option. </summary>
            public string? GetName => name;

            private Point position;
            /// <summary> Gets the position of the menu option. </summary>
            public Point Position => position;

            private Grid grid = new Grid();
            /// <summary> Gets the UI element of the menu option. </summary>
            public FrameworkElement UIElement => grid;

            internal Image icon = new();
            internal TextBlock title = new TextBlock();
            internal TextBlock keyboardShortcut = new TextBlock();
            internal TextBlock arrow = new TextBlock();

            private double height;
            /// <summary> Gets the height of the menu option. </summary>
            public double Height => height;

            private DropDownMenu childMenu;
            /// <summary> Gets the child menu of the menu option. </summary>
            public DropDownMenu ChildMenu => childMenu;

            private DropDownMenu parentMenu;
            /// <summary> Gets the parent menu of the menu option. </summary>
            public DropDownMenu ParentMenu => parentMenu;

            private bool hasMenu = false;
            /// <summary> Gets a value indicating whether the menu option has a child menu. </summary>
            public bool HasMenu => hasMenu;

            /// <summary>
            /// Initializes a new instance of the <see cref="MenuOption"/> class with the specified height, option name, and parent menu.
            /// </summary>
            /// <param name="height">The height of the menu option.</param>
            /// <param name="optionName">The name of the menu option.</param>
            /// <param name="parentMenu">The parent menu of the menu option.</param>
            public MenuOption(double height, string optionName, DropDownMenu parentMenu) {
                this.parentMenu = parentMenu;
                this.name = optionName;
                this.height = height;
                childMenu = new DropDownMenu(optionName, parentMenu.rootWindow);
                arrow.Text = " ";
                icon.RenderSize = new Size(height, height);

                title.Margin = new Thickness(15, 0, 0, 0);
                title.Text = optionName;
                title.Foreground = Brushes.White;
                title.VerticalAlignment = VerticalAlignment.Top;
                title.HorizontalAlignment = HorizontalAlignment.Left;

                ColumnDefinition symbolCol = new ColumnDefinition();
                symbolCol.Width = new GridLength(height, GridUnitType.Pixel);
                grid.ColumnDefinitions.Add(symbolCol);

                ColumnDefinition nameCol = new ColumnDefinition();
                nameCol.Width = new GridLength(1, GridUnitType.Auto);
                grid.ColumnDefinitions.Add(nameCol);

                ColumnDefinition shortcutCol = new ColumnDefinition();
                shortcutCol.Width = new GridLength(1, GridUnitType.Auto);
                grid.ColumnDefinitions.Add(shortcutCol);

                ColumnDefinition arrowCol = new ColumnDefinition();
                arrowCol.Width = new GridLength(height, GridUnitType.Pixel);
                grid.ColumnDefinitions.Add(arrowCol);

                Helper.SetChildInGrid(grid, icon, 0, 0);
                Helper.SetChildInGrid(grid, title, 0, 1);
                Helper.SetChildInGrid(grid, keyboardShortcut, 0, 2);
                Helper.SetChildInGrid(grid, arrow, 0, 3);

                grid.MouseEnter += (s, e) => grid.Background = Helper.StringToSolidColorBrush("#3d3d3d");
                grid.MouseLeave += (s, e) => grid.Background = Brushes.Transparent;
            }

            /// <summary>
            /// Adds a symbol to the menu option using the specified image path.
            /// </summary>
            /// <param name="path">The path of the image.</param>
            /// <returns>The current instance of the <see cref="MenuOption"/> class.</returns>
            public MenuOption AddSymbol(string path) {
                Helper.SetImageSource(icon, path);
                return this;
            }

            /// <summary>
            /// Sets the keyboard shortcut for the menu option.
            /// </summary>
            /// <param name="kShortcut">The keyboard shortcut to set.</param>
            /// <returns>The current instance of the <see cref="MenuOption"/> class.</returns>
            public MenuOption SetKeyboardShortcut(string kShortcut) {
                keyboardShortcut.Margin = new Thickness(15, 0, 0, 0);
                keyboardShortcut.Text = kShortcut;
                keyboardShortcut.Foreground = Brushes.White;
                keyboardShortcut.VerticalAlignment = VerticalAlignment.Top;
                keyboardShortcut.HorizontalAlignment = HorizontalAlignment.Left;
                return this;
            }

            /// <summary>
            /// Adds a drop-down menu as a child menu to the menu option.
            /// </summary>
            /// <param name="menu">The drop-down menu to add as a child menu.</param>
            /// <returns>The current instance of the <see cref="MenuOption"/> class.</returns>
            public MenuOption AddDropdownMenu(DropDownMenu menu) {
                arrow.Margin = new Thickness(15, 0, 0, 0);
                arrow.Text = ">";
                arrow.Foreground = Brushes.White;

                this.childMenu = menu;
                hasMenu = true;

                AddCommand(parentMenu.HideChildrenMenus);
                AddCommand(menu.ToggleVisibility);
                return this;
            }

            /// <summary>
            /// Adds a command to the menu option that is executed when clicked.
            /// </summary>
            /// <param name="command">The action to execute as a command.</param>
            /// <returns>The current instance of the <see cref="MenuOption"/> class.</returns>
            public MenuOption AddCommand(Action command) {
                grid.MouseLeftButtonUp += (s, e) => command();
                return this;
            }

            /// <summary>
            /// Hides the child menu of the menu option.
            /// </summary>
            public void HideChildrenMenu() {
                childMenu.HideWidthChildrenMenus();
            }

            /// <summary>
            /// Updates the position of the child menu associated with the menu option.
            /// </summary>
            public void UpdateMenuPosition() {
                childMenu.UpdateMenuPositionWithChildren();
            }
        }
    }
}
