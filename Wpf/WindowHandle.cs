using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using System.Xml;

namespace Utillities.Wpf
{
    /*
    /// <summary>
    /// Represents a UIElement used in WPFs that provides a custom window title bar for windows that use window chrome and have no window style.
    /// </summary>
    public class WindowHandle_OLD : IFrameworkElement {
        private WindowChrome _windowChrome = new WindowChrome();
        private Window _window;
        private Panel _parentContainer;
        private RectangleGeometry _rectangleGeometry;
        private Border _windowBorder;
        private ApplicationButtonCollection _applicationButtons;
        private static double _clientButtonHeight = 20;
        private double _height = 30;
        private Image _icon = new Image();
        private List<(Button, IPopupMenu)> _clientButtons = new();
        private bool _isUsingClientButtons = false;
        private StackPanel _clientButtonStackPanel = new();
        private Grid _mainGrid = new();
        private static Brush _colorWhenButtonHover = Helper.StringToSolidColorBrush("#3d3d3d");
        private Brush _bgColor = Helper.StringToSolidColorBrush("#1f1f1f");

        /// <summary>
        /// Gets the hight of this WindowHandle.
        /// </summary>
        public double Height => _height;

        /// <summary>
        /// Gets the application buttons. 
        /// </summary>
        public ApplicationButtonCollection ApplicationButtons => _applicationButtons;

        /// <summary>
        /// Gets the FrameworkElement of the WindowHandle.
        /// </summary>
        public FrameworkElement FrameworkElement => _mainGrid;

        /// <summary>
        /// Gets the background color of the WindowHandle.
        /// </summary>
        public Brush BGColor => _bgColor;

        // Handle Bar Init
        /// <summary>
        /// Initializes a new instance of the WindowHandle class.
        /// </summary>
        /// <param name="window">The window associated with the WindowHandle.</param>
        /// <param name="cornerRadius">The corner radius of the TopLeft and TopRight corners of the Window. If the window has no rounded corners, just pass in 0.</param>
        public WindowHandle_OLD(Window window) {
            this._window = window;
            window.WindowStyle = WindowStyle.None;
            window.AllowsTransparency = true;
            window.Background = Brushes.Transparent;

            // var
            WindowChrome.SetWindowChrome(window, _windowChrome);
            _applicationButtons = new(this, window);
            window.SourceInitialized += (s, e) => {
                IntPtr handle = (new WindowInteropHelper(window)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };

            // Set up Main Grid
            _mainGrid.Background = _bgColor;
            _mainGrid.VerticalAlignment = VerticalAlignment.Top;
            _mainGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            _mainGrid.Width = window.Width;
            _mainGrid.Height = _height;
            
            window.SizeChanged += (s, e) => {
                _mainGrid.Width = window.ActualWidth;
            };

            var mainRow = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
            var clientButtonColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var applicationButtonColumn = new ColumnDefinition { Width = new GridLength(_applicationButtons.Width * 3) };

            _mainGrid.RowDefinitions.Add(mainRow);
            _mainGrid.ColumnDefinitions.Add(clientButtonColumn);
            _mainGrid.ColumnDefinitions.Add(applicationButtonColumn);

            Helper.SetChildInGrid(_mainGrid, _clientButtonStackPanel, 0, 0);
            Helper.SetChildInGrid(_mainGrid, _applicationButtons.FrameworkElement, 0, 1);

            // Set up Client Button Stack Panel
            _clientButtonStackPanel.Background = Brushes.Transparent;
            _clientButtonStackPanel.Orientation = Orientation.Horizontal;

            WindowWrapper.Wrap(window, out _parentContainer, out _windowBorder, out _rectangleGeometry);
            _parentContainer.Children.Add(FrameworkElement);
        }

        /// <summary>
        /// Adds an icon to the WindowHandle.
        /// </summary>
        /// <param name="path">The path to the icon image.</param>
        /// <returns>The WindowHandle instance.</returns>
        public WindowHandle AddIcon(string path) {
            Helper.SetImageSource(_icon, path);
            _icon.Margin = new Thickness(5);
            _clientButtonStackPanel.Children.Insert(0, _icon);
            return this;
        }
        /// <summary>
        /// Sets the height of the WindowHandle.
        /// </summary>
        /// <param name="height">The desired height.</param>
        /// <returns>The WindowHandle instance.</returns>
        public WindowHandle SetHeight(double height) {
            this._height = height;
            _mainGrid.Height = height;
            _windowChrome.CaptionHeight = height;

            if (_applicationButtons.Height < height) {
                _applicationButtons.Height = height;
            }

            return this;
        }
        /// <summary>
        /// Sets the background color of the WindowHandle.
        /// </summary>
        /// <param name="color">The background color to set.</param>
        /// <returns>The WindowHandle instance.</returns>
        public WindowHandle SetBGColor(Brush color) {
            _mainGrid.Background = color;
            return this;
        }

        /// <summary>
        /// Sets the color when a button is hovered over in the WindowHandle.
        /// </summary>
        /// <param name="color">The hover color to set.</param>
        /// <returns>The WindowHandle instance.</returns>
        public WindowHandle SetColorWhenHover(Brush color) {
            _colorWhenButtonHover = color;
            foreach ((Button, DropDownMenu) button in _clientButtons) {
                UpdateButtonHoverColor(button.Item1);
                button.Item2.ChangeBGColorWithChildren(color);
            }
            return this;
        }

        /// <summary>
        /// Creates a client button in the WindowHandle.
        /// </summary>
        /// <param name="dropDownMenu">The DropDownMenu associated with the client button.</param>
        /// <param name="name"></param>
        /// <returns>The WindowHandle instance.</returns>
        public WindowHandle CreateClientButton(IPopupMenu dropDownMenu, string name) {

            Button newClientButton = new() {
                Content = name,
                Style = ClientButtonStyle()
            };
            Helper.SetWindowChromActive(newClientButton);
            _parentContainer?.Children.Add(dropDownMenu.FrameworkElement);
            _clientButtons.Add((newClientButton, dropDownMenu));
            _clientButtonStackPanel.Children.Add(newClientButton);

            dropDownMenu.IsTopMenu = true;
            dropDownMenu.ActivationButton = newClientButton;
            dropDownMenu.Instanciate(newClientButton);

            return this;
        }

        /// <summary>
        /// Add a framework element to the client button stack panel.
        /// </summary>
        /// <param name="element"></param>
        public void AddElement(FrameworkElement element) {
            _clientButtonStackPanel.Children.Add(element);
        }

        // Client Button Init
        private void ActivateClientButton((Button, IPopupMenu) button) {
            button.Item1.Loaded += (object sender, RoutedEventArgs e) => button.Item2.MeasureAndArrange();
            button.Item1.Loaded += (object sender, RoutedEventArgs e) => button.Item2.Collapse();
            button.Item1.Click += (object sender, RoutedEventArgs e) => ToggleMenu(button.Item2);
            button.Item1.MouseEnter += (object sender, MouseEventArgs e) => {
                if (_isUsingClientButtons) {
                    HideAllMenus();
                    button.Item2.Expand();
                }
            };
        }
        private void ActivateClientButton((Button, IPopupMenu) button, Action action) {
            ActivateClientButton(button);
            button.Item1.Click += (object sender, RoutedEventArgs e) => action();
        }

        /// <summary>
        /// Activates all client buttons in the WindowHandle.
        /// </summary>
        public void ActivateAllClientButtons() {
            foreach (var button in _clientButtons) {
                ActivateClientButton(button);
                button.Item2.Collapse();
                button.Item2.MeasureAndArrange();
            }
        }
        /// <summary>
        /// Activates the function of the specified client button in the WindowHandle.
        /// </summary>
        /// <param name="name">The name of the client button.</param>
        public void ActivateButtonFunction(string name) {
            var button = GetClientButton(name);
            if (button.Item1 == null || button.Item2 == null) {
                return;
            }
            ActivateClientButton(button);
        }

        /// <summary>
        /// Sets the function of the specified client button in the WindowHandle.
        /// </summary>
        /// <param name="name">The name of the client button.</param>
        /// <param name="action">The action to be performed when the client button is clicked.</param>
        public void SetButtonFunction(string name, Action action) {
            var button = GetClientButton(name);
            if (button.Item1 == null || button.Item2 == null) {
                return;
            }
            ActivateClientButton(button, action);
        }

        // Client Button Management
        /// <summary>
        /// Gets the client button and its associated DropDownMenu based on the name.
        /// </summary>
        /// <param name="name">The name of the client button.</param>
        /// <returns>A tuple containing the client button and its associated DropDownMenu.</returns>
        public (Button, IPopupMenu) GetClientButton(string name) => _clientButtons.Find(x => x.Item1.Content.ToString() == name);

        /// <summary>
        /// Gets the client button (Button) based on the name.
        /// </summary>
        /// <param name="name">The name of the client button.</param>
        /// <returns>The client button (Button).</returns>
        public Button? GetClientButtonButton(string name) => _clientButtons.Find(x => x.Item1.Content.ToString() == name).Item1;

        /// <summary>
        /// Hides all the menus associated with the client buttons in the WindowHandle.
        /// </summary>
        public void HideAllMenus() => _clientButtons.ForEach(x => x.Item2.Collapse());
        /// <summary>
        /// Toggles the visibility of a specific DropDownMenu associated with a client button in the WindowHandle.
        /// </summary>
        /// <param name="element">The DropDownMenu to toggle.</param>
        public void ToggleMenu(IPopupMenu element) {
            if (_isUsingClientButtons) {
                HideAllMenus();
                _isUsingClientButtons = false;
            }
            else {
                _isUsingClientButtons = true;
                element.Expand();
            }
        }

        // Window Chrome
        /// <summary>
        /// Sets the WindowChrome properties as active for all elements in the WindowHandle.
        /// </summary>
        public void SetWindowChromeActiveAll() {
            _applicationButtons.SetWindowChromeActive();
            foreach ((Button, DropDownMenu) button in _clientButtons) {
                Helper.SetWindowChromActive(button.Item1);
            }
        }

        // Visual
        private void UpdateButtonHoverColor(Button button) {
            // Get the button's style
            Style newStyle = new Style(typeof(Button), button.Style);

            // Create the new Trigger
            Trigger mouseOverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            mouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, _colorWhenButtonHover));
            newStyle.Triggers.Add(mouseOverTrigger);

            // Apply the updated style to the button
            button.Style = newStyle;
        }
        /// <summary>
        /// Gets the style for the client buttons in the WindowHandle.
        /// </summary>
        /// <returns>The style for the client buttons.</returns>
        public static Style ClientButtonStyle() {
            Style clientButtonsStyle = new Style(typeof(Button));

            clientButtonsStyle.Setters.Add(new Setter(Button.MarginProperty, new Thickness(10, 0, 0, 0)));
            clientButtonsStyle.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.Transparent));
            clientButtonsStyle.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            clientButtonsStyle.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Transparent));
            clientButtonsStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(1)));
            clientButtonsStyle.Setters.Add(new Setter(Button.HorizontalAlignmentProperty, HorizontalAlignment.Left));
            clientButtonsStyle.Setters.Add(new Setter(Button.VerticalAlignmentProperty, VerticalAlignment.Center));
            clientButtonsStyle.Setters.Add(new Setter(Button.HeightProperty, _clientButtonHeight));

            ControlTemplate userButtonTemplate = new ControlTemplate(typeof(Button));
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            borderFactory.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            borderFactory.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));

            FrameworkElementFactory contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            borderFactory.AppendChild(contentPresenterFactory);

            userButtonTemplate.VisualTree = borderFactory;

            Trigger mouseOverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            mouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, _colorWhenButtonHover));
            mouseOverTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Gray));

            userButtonTemplate.Triggers.Add(mouseOverTrigger);

            clientButtonsStyle.Setters.Add(new Setter(Button.TemplateProperty, userButtonTemplate));

            clientButtonsStyle.Seal();

            return clientButtonsStyle;
        }

        #region Fix for the Winodw maximizing glitch
        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (msg) {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }
            return (IntPtr)0;
        }
        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam) {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO))!;
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero) {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = System.Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = System.Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = System.Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = System.Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT {
            /// <summary>x coordinate of point.</summary>
            public int x;
            /// <summary>y coordinate of point.</summary>
            public int y;
            /// <summary>Construct a point of coordinates (x,y).</summary>
            public POINT(int x, int y) {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MONITORINFO {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        private struct RECT {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public static readonly RECT Empty = new RECT();
            public int Width { get { return System.Math.Abs(right - left); } }
            public int Height { get { return bottom - top; } }
            public RECT(int left, int top, int right, int bottom) {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
            public RECT(RECT rcSrc) {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }
            public bool IsEmpty { get { return left >= right || top >= bottom; } }
            public override string ToString() {
                if (this == Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }
            public override bool Equals(object? obj) {
                if (obj is not Rect) return false;
                return this == (RECT)obj;
            }
            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode() => left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2) { return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom); }
            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2) { return !(rect1 == rect2); }
        }

        [DllImport("user32")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
        #endregion
    }
    */
}

