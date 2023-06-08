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
    /// Represents a UIElement used in WPFs that provides a custom window title bar for windows that use window chrome and have no window style.
    /// </summary>
    public class WindowHandle {
        private WindowChrome windowChrome = new WindowChrome();
        private Window window;
        private ApplicationButtonCollection applicationButtons;
        private static double clientButtonHeight = 20;
        private double height = 30;
        private Image icon = new Image();
        private List<(Button, DropDownMenu)> clientButtons = new List<(Button, DropDownMenu)>();
        private bool isUsingClientButtons = false;
        private StackPanel clientButtonStackPanel = new StackPanel();
        private Grid mainGrid = new Grid();
        private static Brush colorWhenButtonHover = Helper.StringToSolidColorBrush("#3d3d3d");
        private Brush bgColor = Helper.StringToSolidColorBrush("#1f1f1f");
        private Canvas? parentCanvas;

        /// <summary>
        /// Gets the hight of this WindowHandle.
        /// </summary>
        public double Height => height;

        /// <summary>
        /// Gets the application buttons. 
        /// </summary>
        public ApplicationButtonCollection ApplicationButtons => applicationButtons;

        /// <summary>
        /// Gets the FrameworkElement of the WindowHandle.
        /// </summary>
        public FrameworkElement FrameworkElement => mainGrid;

        /// <summary>
        /// Gets the background color of the WindowHandle.
        /// </summary>
        public Brush BGColor => bgColor;

        // Handle Bar Init
        /// <summary>
        /// Initializes a new instance of the WindowHandle class.
        /// </summary>
        /// <param name="window">The window associated with the WindowHandle.</param>
        public WindowHandle(Window window) {
            this.window = window;

            // var
            WindowChrome.SetWindowChrome(window, windowChrome);
            applicationButtons = new(this, window);
            window.SourceInitialized += (s, e) => {
                IntPtr handle = (new WindowInteropHelper(window)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };

            // Set up Main Grid
            mainGrid.Background = bgColor;
            mainGrid.VerticalAlignment = VerticalAlignment.Top;
            mainGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            mainGrid.Width = window.Width;
            mainGrid.Height = height;

            window.SizeChanged += (s, e) => {
                mainGrid.Width = window.ActualWidth;
            };

            var mainRow = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
            var clientButtonColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var applicationButtonColumn = new ColumnDefinition { Width = new GridLength(applicationButtons.Width * 3) };

            mainGrid.RowDefinitions.Add(mainRow);
            mainGrid.ColumnDefinitions.Add(clientButtonColumn);
            mainGrid.ColumnDefinitions.Add(applicationButtonColumn);

            Helper.SetChildInGrid(mainGrid, clientButtonStackPanel, 0, 0);
            Helper.SetChildInGrid(mainGrid, applicationButtons.FrameworkElement, 0, 1);

            // Set up Client Button Stack Panel
            clientButtonStackPanel.Background = Brushes.Transparent;
            clientButtonStackPanel.Orientation = Orientation.Horizontal;
        }
        /// <summary>
        /// Sets the parent Canvas for the WindowHandle.
        /// </summary>
        /// <param name="parentCanvas">The parent Canvas to which the WindowHandle will be added.</param>
        /// <returns>The WindowHandle instance.</returns>
        public WindowHandle SetParentWindow(Canvas parentCanvas) {
            parentCanvas.Children.Add(mainGrid);
            this.parentCanvas = parentCanvas;
            return this;
        }

        /// <summary>
        /// Adds an icon to the WindowHandle.
        /// </summary>
        /// <param name="path">The path to the icon image.</param>
        /// <returns>The WindowHandle instance.</returns>
        public WindowHandle AddIcon(string path) {
            Helper.SetImageSource(icon, path);
            icon.Margin = new Thickness(5);
            clientButtonStackPanel.Children.Insert(0, icon);
            return this;
        }
        /// <summary>
        /// Sets the height of the WindowHandle.
        /// </summary>
        /// <param name="height">The desired height.</param>
        /// <returns>The WindowHandle instance.</returns>
        public WindowHandle SetHeight(double height) {
            this.height = height;
            mainGrid.Height = height;
            windowChrome.CaptionHeight = height;

            if (applicationButtons.Height < height) {
                applicationButtons.Height = height;
            }

            return this;
        }
        /// <summary>
        /// Sets the background color of the WindowHandle.
        /// </summary>
        /// <param name="color">The background color to set.</param>
        /// <returns>The WindowHandle instance.</returns>
        public WindowHandle SetBGColor(Brush color) {
            mainGrid.Background = color;
            return this;
        }

        /// <summary>
        /// Sets the color when a button is hovered over in the WindowHandle.
        /// </summary>
        /// <param name="color">The hover color to set.</param>
        /// <returns>The WindowHandle instance.</returns>
        public WindowHandle SetColorWhenHover(Brush color) {
            colorWhenButtonHover = color;
            foreach ((Button, DropDownMenu) button in clientButtons) {
                UpdateButtonHoverColor(button.Item1);
                button.Item2.ChangeBGColorWithChildren(color);
            }
            return this;
        }

        /// <summary>
        /// Creates a client button in the WindowHandle.
        /// </summary>
        /// <param name="dropDownMenu">The DropDownMenu associated with the client button.</param>
        /// <returns>The WindowHandle instance.</returns>
        public WindowHandle CreateClientButton(DropDownMenu dropDownMenu) {
            Button newClientButton = new() {
                Content = dropDownMenu.Name,
                Style = ClientButtonStyle()
            };
            Helper.SetWindowChromActive(newClientButton);
            parentCanvas?.Children.Add(dropDownMenu.UIElement);
            clientButtons.Add((newClientButton, dropDownMenu));
            clientButtonStackPanel.Children.Add(newClientButton);

            dropDownMenu.Instanciate(newClientButton);

            return this;
        }

        public void AddElement(FrameworkElement element) {
            clientButtonStackPanel.Children.Add(element);
        }

        // Client Button Init
        private void ActivateClientButton((Button, DropDownMenu) button) {
            button.Item1.Loaded += (object sender, RoutedEventArgs e) => button.Item2.UpdateOptionLayout();
            button.Item1.Loaded += (object sender, RoutedEventArgs e) => button.Item2.UpdateMenuPositionWithChildren();
            button.Item1.Click += (object sender, RoutedEventArgs e) => ToggleMenu(button.Item2);
            button.Item1.MouseEnter += (object sender, MouseEventArgs e) => {
                if (isUsingClientButtons) {
                    HideAllMenus();
                    button.Item2.Show();
                }
            };
        }
        private void ActivateClientButton((Button, DropDownMenu) button, Action action) {
            ActivateClientButton(button);
            button.Item1.Click += (object sender, RoutedEventArgs e) => action();
        }

        /// <summary>
        /// Activates all client buttons in the WindowHandle.
        /// </summary>
        public void ActivateAllClientButtons() {
            foreach (var button in clientButtons) {
                ActivateClientButton(button);
                button.Item2.HideWidthChildrenMenus();
                button.Item2.UpdateOptionLayout();
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
        public (Button, DropDownMenu) GetClientButton(string name) => clientButtons.Find(x => x.Item1.Content.ToString() == name);

        /// <summary>
        /// Gets the client button (Button) based on the name.
        /// </summary>
        /// <param name="name">The name of the client button.</param>
        /// <returns>The client button (Button).</returns>
        public Button? GetClientButtonButton(string name) => clientButtons.Find(x => x.Item1.Content.ToString() == name).Item1;

        /// <summary>
        /// Hides all the menus associated with the client buttons in the WindowHandle.
        /// </summary>
        public void HideAllMenus() => clientButtons.ForEach(x => x.Item2.HideWidthChildrenMenus());
        /// <summary>
        /// Toggles the visibility of a specific DropDownMenu associated with a client button in the WindowHandle.
        /// </summary>
        /// <param name="element">The DropDownMenu to toggle.</param>
        public void ToggleMenu(DropDownMenu element) {
            if (isUsingClientButtons) {
                HideAllMenus();
                isUsingClientButtons = false;
            }
            else {
                isUsingClientButtons = true;
                element.Show();
            }
        }

        // Window Chrome
        /// <summary>
        /// Sets the WindowChrome properties as active for all elements in the WindowHandle.
        /// </summary>
        public void SetWindowChromeActiveAll() {
            applicationButtons.SetWindowChromeActive();
            foreach ((Button, DropDownMenu) button in clientButtons) {
                Helper.SetWindowChromActive(button.Item1);
            }
        }

        // Visual
        private void UpdateButtonHoverColor(Button button) {
            // Get the button's style
            Style newStyle = new Style(typeof(Button), button.Style);

            // Create the new Trigger
            Trigger mouseOverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            mouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, colorWhenButtonHover));
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
            clientButtonsStyle.Setters.Add(new Setter(Button.HeightProperty, clientButtonHeight));

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
            mouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, colorWhenButtonHover));
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

        /// <summary>
        /// Represents a collection of application buttons within the WindowHandle.
        /// </summary>
        public class ApplicationButtonCollection {
            private Window window;

            private static double height = 30;
            private static double width = 40;

            /// <summary> Gets or sets the height of the application buttons. </summary>
            public double Height {
                get { return height; }
                set {
                    if (value <= window.Height) {
                        height = value;
                        exitButton.Height = height;
                        minimizeButton.Height = height;
                        maximizeButton.Height = height;

                        if (settingsButton != null)
                            settingsButton.Height = height;
                    }
                }
            }
            /// <summary>Gets or sets the width of the application buttons.</summary>
            public double Width {
                get { return width; }
                set {
                    if (value <= window.Width / 3) {
                        width = value;
                        exitButton.Width = value;
                        minimizeButton.Width = value;
                        maximizeButton.Width = value;
                        if (settingsButton != null)
                            settingsButton.Width = value;
                    }
                }
            }
            /// <summary>Gets or sets the image source for the settings button.</summary>
            public string? SettingsButtonImageSource {
                get {
                    if (settingsButton == null) return null;
                    if ((settingsButton.Content as Border)!.Child == null) return null;

                    return ((settingsButton.Content as Border)!.Child as System.Windows.Controls.Image)!.Source.ToString();
                }
                set {
                    if (settingsButton == null) return;

                    var imageContent = ((settingsButton.Content as Border)!.Child as System.Windows.Controls.Image)!;
                    imageContent.Source = new BitmapImage(new Uri(value!));
                }
            }
            /// <summary>Gets or sets the padding for the settings button image.</summary>
            public Thickness SettingsButtonImagePadding {
                get {
                    if (settingsButton == null) return new Thickness();

                    return (settingsButton.Content as Border)!.Padding;
                }
                set {
                    if (settingsButton == null) return;

                    (settingsButton.Content as Border)!.Padding = value;
                }
            }


            private Button settingsButton = new();
            private Button exitButton = new();
            private Button minimizeButton = new();
            private Button maximizeButton = new();
            /// <summary>Gets the exit button.</summary>
            public Button ExitButton => exitButton;

            /// <summary>Gets the minimize button.</summary>
            public Button MinimizeButton => minimizeButton;

            /// <summary>Gets the maximize button.</summary>
            public Button MaximizeButton => maximizeButton;

            /// <summary>Gets the framework element containing the application buttons.</summary>
            public FrameworkElement FrameworkElement => stackPanel;
            private StackPanel stackPanel = new();

            private Brush colorWhenButtonHover = Helper.StringToSolidColorBrush("#3d3d3d");
            private static Brush color = Brushes.Transparent;
            private static Brush symbolColor = Brushes.White;
            /// <summary>Gets or sets the color of the application buttons when hovered.</summary>
            public Brush ColorWhenButtonHover {
                get => colorWhenButtonHover;
                set {
                    colorWhenButtonHover = value;
                    UpdateColors();
                }
            }
            /// <summary>Gets or sets the color of the application buttons.</summary>
            public Brush Color {
                get => color;
                set {
                    color = value;
                    UpdateColors();
                }
            }
            /// <summary>Gets or sets the color of the symbol within the application buttons.</summary>
            public Brush SymbolColor {
                get => symbolColor;
                set {
                    symbolColor = value;
                    UpdateColors();
                }
            }


            private WindowHandle windowHandle;

            /// <summary>
            /// Initializes a new instance of the <see cref="ApplicationButtonCollection"/> class.
            /// </summary>
            /// <param name="windowHandle">The WindowHandle.</param>
            /// <param name="window">The associated window.</param>
            public ApplicationButtonCollection(WindowHandle windowHandle, Window window) {
                this.window = window;
                this.windowHandle = windowHandle;


                exitButton.Style = ButtonStyle();
                exitButton.Content = "x";
                exitButton.Click += Shutdown;
                exitButton.MouseEnter += (s, e) => { exitButton.Background = colorWhenButtonHover; };
                exitButton.MouseLeave += (s, e) => { exitButton.Background = color; };
                Helper.SetWindowChromActive(exitButton);

                minimizeButton.Style = ButtonStyle();
                minimizeButton.Content = "-";
                minimizeButton.Click += Minimize;
                minimizeButton.MouseEnter += (s, e) => { minimizeButton.Background = colorWhenButtonHover; };
                minimizeButton.MouseLeave += (s, e) => { minimizeButton.Background = color; };
                Helper.SetWindowChromActive(minimizeButton);

                maximizeButton.Style = ButtonStyle();
                maximizeButton.Content = "□";
                maximizeButton.Click += Maximize;
                maximizeButton.MouseEnter += (s, e) => { maximizeButton.Background = colorWhenButtonHover; };
                maximizeButton.MouseLeave += (s, e) => { maximizeButton.Background = color; };
                Helper.SetWindowChromActive(maximizeButton);


                stackPanel.VerticalAlignment = VerticalAlignment.Center;
                stackPanel.HorizontalAlignment = HorizontalAlignment.Right;
                stackPanel.Orientation = Orientation.Horizontal;

                stackPanel.Children.Add(minimizeButton);
                stackPanel.Children.Add(maximizeButton);
                stackPanel.Children.Add(exitButton);

                UpdateColors();
                UpdateSize();
            }

            /// <summary>
            /// Adds the settings button to the collection.
            /// </summary>
            public void AddSettingsButton() {
                windowHandle.mainGrid.ColumnDefinitions[1].Width = new GridLength(width * 4);

                settingsButton.Style = ButtonStyle();
                settingsButton.Click += Settings;
                settingsButton.MouseEnter += (s, e) => { settingsButton.Background = colorWhenButtonHover; };
                settingsButton.MouseLeave += (s, e) => { settingsButton.Background = color; };

                var container = new Border {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };
                settingsButton.Content = container;

                var imageContent = new System.Windows.Controls.Image {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                container.Child = imageContent;

                Helper.SetWindowChromActive(settingsButton);
                stackPanel.Children.Insert(0, settingsButton);

                UpdateColors();
                UpdateSize();
            }

            /// <summary>
            /// Creates the button style for the application buttons.
            /// </summary>
            /// <returns>The button style.</returns>
            public static Style ButtonStyle() {
                // Create a new style for the button
                Style style = new Style(typeof(Button));
                style.Setters.Add(new Setter(Button.BackgroundProperty, color));
                style.Setters.Add(new Setter(Button.ForegroundProperty, symbolColor));
                style.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Transparent));
                style.Setters.Add(new Setter(Button.HorizontalAlignmentProperty, HorizontalAlignment.Right));
                style.Setters.Add(new Setter(Button.VerticalAlignmentProperty, VerticalAlignment.Top));
                style.Setters.Add(new Setter(Button.WidthProperty, width));
                style.Setters.Add(new Setter(Button.HeightProperty, height));

                // Set the control template of the button
                ControlTemplate template = new ControlTemplate(typeof(Button));
                FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
                border.SetBinding(Button.BackgroundProperty, new Binding("Background") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
                border.SetBinding(Button.BorderBrushProperty, new Binding("BorderBrush") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
                border.SetBinding(Button.BorderThicknessProperty, new Binding("BorderThickness") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
                FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
                contentPresenter.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                contentPresenter.SetValue(Button.VerticalAlignmentProperty, VerticalAlignment.Center);
                border.AppendChild(contentPresenter);
                template.VisualTree = border;
                style.Setters.Add(new Setter(Button.TemplateProperty, template));

                return style;
            }

            /// <summary>
            /// Sets the WindowChrome properties for the application buttons.
            /// </summary>
            public void SetWindowChromeActive() {
                Helper.SetWindowChromActive(exitButton);
                Helper.SetWindowChromActive(minimizeButton);
                Helper.SetWindowChromActive(maximizeButton);
                if (settingsButton != null) {
                    Helper.SetWindowChromActive(settingsButton);
                }
            }

            /// <summary>
            /// Overrides the default shutdown behavior of the exit button with a custom action.
            /// </summary>
            /// <param name="action">The custom action to be executed on exit button click.</param>
            public void OverrideShutdown(Action action) {
                exitButton.Click -= Shutdown;
                exitButton.Click += (object sender, RoutedEventArgs e) => action();
            }
            /// <summary>
            /// Overrides the default minimize behavior of the minimize button with a custom action.
            /// </summary>
            /// <param name="action">The custom action to be executed on minimize button click.</param>
            public void OverrideMinimize(Action action) {
                exitButton.Click -= Minimize;
                exitButton.Click += (object sender, RoutedEventArgs e) => action();
            }
            /// <summary>
            /// Overrides the default maximize behavior of the maximize button with a custom action.
            /// </summary>
            /// <param name="action">The custom action to be executed on maximize button click.</param>
            public void OverrideMaximize(Action action) {
                exitButton.Click -= Maximize;
                exitButton.Click += (object sender, RoutedEventArgs e) => action();
            }
            /// <summary>
            /// Overrides the default settings button behavior with a custom action.
            /// </summary>
            /// <param name="action">The custom action to be executed on settings button click.</param>
            public void OverrideSettings(Action action) {
                settingsButton.Click -= Settings;
                settingsButton.Click += (object sender, RoutedEventArgs e) => action();
            }

            private void Shutdown(object sender, RoutedEventArgs e) {
                window.Close();
            }
            private void Minimize(object sender, RoutedEventArgs e) {
                window.WindowState = WindowState.Minimized;
            }
            private void Maximize(object sender, RoutedEventArgs e) {
                if (window.WindowState == WindowState.Maximized) {
                    // Go into windowed
                    window.WindowState = WindowState.Normal;
                }
                else {
                    // Go into maximized
                    window.WindowState = WindowState.Maximized;
                }
                // Update Layout
                window.UpdateLayout();
            }
            private void Settings(object sender, RoutedEventArgs e) {
                // Acting as a dummy method
            }

            /// <summary>
            /// 
            /// </summary>
            public void UpdateSize() {
                exitButton.Width = Width;
                exitButton.Height = Height;


                minimizeButton.Width = Width;
                minimizeButton.Height = Height;


                maximizeButton.Width = Width;
                maximizeButton.Height = Height;

                if (settingsButton != null) {
                    settingsButton.Width = Width;
                    settingsButton.Height = Height;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            public void UpdateColors() {
                exitButton.Background = color;
                exitButton.Foreground = symbolColor;
                Helper.UpdateButtonHoverColor(ExitButton, colorWhenButtonHover);


                minimizeButton.Background = color;
                minimizeButton.Foreground = symbolColor;
                Helper.UpdateButtonHoverColor(minimizeButton, colorWhenButtonHover);


                maximizeButton.Background = color;
                maximizeButton.Foreground = symbolColor;
                Helper.UpdateButtonHoverColor(maximizeButton, colorWhenButtonHover);

                if (settingsButton != null) {
                    settingsButton.Background = color;
                    settingsButton.Foreground = symbolColor;
                    Helper.UpdateButtonHoverColor(settingsButton, colorWhenButtonHover);
                }
            }
        }
    }
}
