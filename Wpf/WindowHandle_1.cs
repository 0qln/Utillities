using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
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
    /// <summary>
    /// 
    /// </summary>
    public class ClientButtonCollection : IWindowChromeElement, ICollection<ClientButton>, IList<ClientButton>
    {
        private StackPanel _controls = new();
        private List<ClientButton> _objects = new();

        /// <summary>
        /// FrameworkElement
        /// </summary>
        public FrameworkElement FrameworkElement => _controls;

        /// <summary>
        /// The total count of `ClientButton` objects in this collection
        /// </summary>
        public int Count => ((ICollection<ClientButton>)_objects).Count;

        /// <summary>
        /// Wether this is readonly or not.
        /// </summary>
        public bool IsReadOnly => ((ICollection<ClientButton>)_objects).IsReadOnly;

        /// <summary>
        /// Get or Set the `ClientButton` at a specific index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ClientButton this[int index]
        {
            get => _objects[index];
            set
            {
                _objects[index] = value;
                _controls.Children[index] = value.FrameworkElement;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ClientButtonCollection()
        {
            _controls.Orientation = Orientation.Horizontal;
            _controls.Background = Brushes.Transparent;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void SetWindowChromActiveAll(bool active)
        {
            foreach (var b in _objects)
            {
                WindowChrome.SetIsHitTestVisibleInChrome(b.FrameworkElement, active);
            }
        }


        void IWindowChromeElement.SetWindowChromeActive(bool active)
        {
            WindowChrome.SetIsHitTestVisibleInChrome(FrameworkElement, active);
        }

        public void Add(ClientButton item)
        {
            ((ICollection<ClientButton>)_objects).Add(item);
            _controls.Children.Add(item.FrameworkElement);
        }

        public void Clear()
        {
            ((ICollection<ClientButton>)_objects).Clear();
            _controls.Children.Clear();
        }

        public bool Contains(ClientButton item)
        {
            return ((ICollection<ClientButton>)_objects).Contains(item)
                && _controls.Children.Contains(item.FrameworkElement);
        }

        public void CopyTo(ClientButton[] array, int arrayIndex)
        {
            ((ICollection<ClientButton>)_objects).CopyTo(array, arrayIndex);
        }

        public bool Remove(ClientButton item)
        {
            bool ret = true;
            _controls.Children.Remove(item.FrameworkElement);
            ret = ((ICollection<ClientButton>)_objects).Remove(item);
            return ret;
        }

        public IEnumerator<ClientButton> GetEnumerator()
        {
            return ((IEnumerable<ClientButton>)_objects).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_objects).GetEnumerator();
        }

        public int IndexOf(ClientButton item)
        {
            return ((IList<ClientButton>)_objects).IndexOf(item);
        }

        public void Insert(int index, ClientButton item)
        {
            ((IList<ClientButton>)_objects).Insert(index, item);
            _controls.Children.Insert(index, item.FrameworkElement);
        }

        public void RemoveAt(int index)
        {
            ((IList<ClientButton>)_objects).RemoveAt(index);
            _controls.Children.RemoveAt(index);
        }
    }

    public partial class ClientButton
    {
        /// <summary>
        /// The default height every button will be initialized with.
        /// </summary>
        public static double DefaultHeight = 20;

        /// <summary>
        /// NOT The default style every button will be initialized with.
        /// </summary>
        /// <returns></returns>
        public static Style Style()
        {

            Style clientButtonsStyle = new Style(typeof(Button));

            clientButtonsStyle.Setters.Add(new Setter(Button.MarginProperty, new Thickness(10, 0, 0, 0)));
            clientButtonsStyle.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.Transparent));
            clientButtonsStyle.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            clientButtonsStyle.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Transparent));
            clientButtonsStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(1)));
            clientButtonsStyle.Setters.Add(new Setter(Button.HorizontalAlignmentProperty, HorizontalAlignment.Left));
            clientButtonsStyle.Setters.Add(new Setter(Button.VerticalAlignmentProperty, VerticalAlignment.Center));
            clientButtonsStyle.Setters.Add(new Setter(Button.HeightProperty, 20.0));

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
            mouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.LightGray));
            mouseOverTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Gray));

            userButtonTemplate.Triggers.Add(mouseOverTrigger);

            clientButtonsStyle.Setters.Add(new Setter(Button.TemplateProperty, userButtonTemplate));

            clientButtonsStyle.Seal();

            return clientButtonsStyle;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ClientButton : IWindowChromeElement
    {


        internal IPopupMenu Menu { get; set; }
        internal StackPanel StackPanel { get; set; } = new();
        internal Canvas Canvas { get; set; } = new();

        /// <summary>
        /// Button 
        /// </summary>
        public Button Button { get; set; } = new();

        /// <summary>
        /// FrameworkElement
        /// </summary>
        public FrameworkElement FrameworkElement => StackPanel;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="name"></param>
        public ClientButton(string name, IPopupMenu menu)
        {
            this.Menu = menu;
            this.Button.Content = name;
            InitHierachy();
            if (FrameworkElement.IsLoaded) InitSpacing();
            else FrameworkElement.Loaded += (_, _) => InitSpacing();
            FrameworkElement.VerticalAlignment = VerticalAlignment.Center;
            FrameworkElement.Margin = new Thickness(10, 0, 0, 0);
            Button.Click += delegate { menu.Toggle(); };
            menu.FrameworkElement.Loaded += delegate { menu.Toggle(); };
        }

        internal void InitHierachy()
        {
            StackPanel.Children.Add(Button);
            StackPanel.Children.Add(Canvas);
            Canvas.Children.Add(Menu.FrameworkElement);
        }

        internal void InitSpacing()
        {
            Canvas.SetLeft(Menu.FrameworkElement, Canvas.GetLeft(FrameworkElement));
            Canvas.SetTop(Menu.FrameworkElement, Canvas.GetTop(FrameworkElement) + FrameworkElement.ActualHeight);
        }

        void IWindowChromeElement.SetWindowChromeActive(bool active)
        {
            WindowChrome.SetIsHitTestVisibleInChrome(FrameworkElement, active);
        }
    }


    /// <summary>
    /// Represents a UIElement used in WPFs that provides a custom window title bar for windows that use window chrome and have no window style.
    /// </summary>
    public partial class WindowHandle : IFrameworkElement
    {
        private WindowChrome _windowChrome = new WindowChrome();
        private Window _window;

        // the container managine all the content of this window
        // handle, such as the client buttons.
        private Grid _mainGrid = new();

        private StackPanel _miscContainer = new();

        // the container, which this object lives in.
        private Panel _parentContainer;
        
        private ApplicationButtonCollection _applicationButtons;
        private ClientButtonCollection _clientButtons;

        private double _clientButtonHeight = 20;
        private double _height = 30;
        private Image _icon = new();


        /// <summary>
        /// Window title
        /// </summary>
        public TextBlock Title = new();

        /// <summary>
        /// The window wrapping for the parent window of the handle.
        /// </summary>
        public WindowWrapping Wrap { get; set; }

        /// <summary>
        /// Gets the hight of this WindowHandle.
        /// </summary>
        public double Height => _height;

        /// <summary>
        /// Gets the application buttons. 
        /// </summary>
        public ApplicationButtonCollection ApplicationButtons => _applicationButtons;

        /// <summary>
        /// Gets the client button collection for this window handle.
        /// </summary>
        public ClientButtonCollection ClientButtons => _clientButtons;

        /// <summary>
        /// Gets the FrameworkElement of the WindowHandle.
        /// </summary>
        public FrameworkElement FrameworkElement => _mainGrid;

        /// <summary>
        /// Gets or Sets the background color for the window handle.
        /// </summary>
        public Brush BackgroundColor
        {
            get => _mainGrid.Background;
            set => _mainGrid.Background = value;
        }


        /// <summary>
        /// Initializes a new instance of the WindowHandle class.
        /// </summary>
        /// <param name="window">The window associated with the WindowHandle.</param>
        public WindowHandle(Window window)
        {
            this._window = window;
            window.WindowStyle = WindowStyle.None;
            window.AllowsTransparency = true;
            window.Background = Brushes.Transparent;

            WindowChrome.SetWindowChrome(window, _windowChrome);
            _applicationButtons = new(this, window);
            _clientButtons = new();
            window.SourceInitialized += (s, e) => {
                IntPtr handle = (new WindowInteropHelper(window)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };


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

            Helper.SetChildInGrid(_mainGrid, _miscContainer, 0, 0);
            Helper.SetChildInGrid(_mainGrid, _applicationButtons.FrameworkElement, 0, 1);
            _miscContainer.Orientation = Orientation.Horizontal;
            _miscContainer.Children.Add(_icon);
            _miscContainer.Children.Add(Title);
            _miscContainer.Children.Add(_clientButtons.FrameworkElement);

            Wrap = WindowWrapper.Wrap(window);
            _parentContainer = Wrap.Panel;
            _parentContainer.Children.Add(FrameworkElement);
        }

        /// <summary>
        /// Adds an icon to the WindowHandle.
        /// </summary>
        /// <param name="path">The path to the icon image.</param>
        /// <returns>The WindowHandle instance.</returns>
        public WindowHandle AddIcon(string path)
        {
            Helper.SetImageSource(_icon, path);
            _icon.Margin = new Thickness(5);
            return this;
        }
        /// <summary>
        /// Sets the height of the WindowHandle.
        /// </summary>
        /// <param name="height">The desired height.</param>
        /// <returns>The WindowHandle instance.</returns>
        public WindowHandle SetHeight(double height)
        {
            this._height = height;
            _mainGrid.Height = height;
            _windowChrome.CaptionHeight = height;

            if (_applicationButtons.Height < height)
            {
                _applicationButtons.Height = height;
            }

            return this;
        }

        #region Fix for the Winodw maximizing glitch
        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }
            return (IntPtr)0;
        }
        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO))!;
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
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
        private struct POINT
        {
            /// <summary>x coordinate of point.</summary>
            public int x;
            /// <summary>y coordinate of point.</summary>
            public int y;
            /// <summary>Construct a point of coordinates (x,y).</summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public static readonly RECT Empty = new RECT();
            public int Width { get { return System.Math.Abs(right - left); } }
            public int Height { get { return bottom - top; } }
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
            public RECT(RECT rcSrc)
            {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }
            public bool IsEmpty { get { return left >= right || top >= bottom; } }
            public override string ToString()
            {
                if (this == Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }
            public override bool Equals(object? obj)
            {
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
}
