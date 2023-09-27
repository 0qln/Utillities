using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Utillities.Wpf
{
    /// <summary>
    /// A container for a collection of <see cref="PopupMenuOption"/> objects. <br />
    /// An improvement to the <see cref="DropDownMenu"/> class.
    /// </summary>
    public class PopupMenu : IPopupMenu
    {
        /// <summary>
        /// Acting as a dummy. This implementation of the PopupMenu 
        /// does not need an Instanciate method.
        /// </summary>
        /// <param name="parent"></param>
        public void Instanciate(FrameworkElement parent) { }

        private Border _border = new Border { BorderBrush = Brushes.White, BorderThickness = new Thickness(1) };
        private StackPanel _stackPanel = new();
        private List<IPopupMenu.IPopupMenuOption> _options = new();
        private IPopupMenu.IPopupMenuOption? _openOption;

        /// <summary>
        /// Whether this Menu is the top menu in the hierachy or not.
        /// Used for internals.
        /// </summary>
        bool IPopupMenu.IsTopMenu { get; set; } = false;

        /// <summary>
        /// Whether this menu is expanded or not.
        /// </summary>
        public bool IsExpanded => FrameworkElement.Visibility == Visibility.Visible;

        /// <summary>
        /// All options.
        /// </summary>
        public List<IPopupMenu.IPopupMenuOption> Options => _options;

        /// <summary>
        /// The System.Windows.FrameworkElement to handle this object with the System.Windows API.
        /// </summary>
        public FrameworkElement FrameworkElement => _border;

        /// <summary>
        /// Border to add background and border brush.
        /// </summary>
        public Border Border => _border;

        /// <summary>
        /// Gets the stackpanel that contains the options.
        /// </summary>
        public StackPanel StackPanel => _stackPanel;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        public PopupMenu(params IPopupMenu.IPopupMenuOption[] options)
        {
            if (options is not null)
            {
                foreach (var option in options)
                {
                    AddOption(option);
                }
            }

            _border.Child = _stackPanel;
        }


        /// <summary>
        /// Adds an option.
        /// </summary>
        /// <param name="option"></param>
        public void AddOption(IPopupMenu.IPopupMenuOption option)
        {
            _options.Add(option);
            _stackPanel.Children.Add(option.FrameworkElement);

            if (option.CanBeMeasured) MeasureAndArrange();
            else option.OnMeasurable += MeasureAndArrange;

            option.OnClick += delegate
            {
                _openOption = option;
                foreach (var otherOption in  _options)
                {
                    if (otherOption != _openOption) otherOption.Collapse();
                }

                // Disabled when testing
                if (!option.HasInnerMenu) Collapse();
            };
        }

        private bool HasArrow => _options.Any(x => x.HasInnerMenu);
        private bool HasIcon => _options.Any(x => x.Icon is not null);
        private double MaxIconWidth => HasIcon ? _options.Max(x => x.IconWidth) : 0;
        private double MaxSpacingLeft => _options.Max(x => x.SpacingLeft);
        private double MaxNameWidth => _options.Max(x => x.NameWidth);
        private double MaxSpacingRight => _options.Max(x => x.SpacingRight);
        private double MaxArrowWidth => HasArrow ? _options.Max(x => x.ArrowWidth) : 0;
        private double MaxWidth => _options.Max(x => x.Width);


        /// <summary>
        /// Correct the spacing of all options of this menu and the menus position, 
        /// if it is marked as the top menu.
        /// </summary>
        public void MeasureAndArrange()
        {
            foreach (var option in _options)
            {
                option.IconWidth = MaxIconWidth;
                option.SpacingLeft = MaxSpacingLeft;
                option.NameWidth = MaxNameWidth;
                option.SpacingRight = MaxSpacingRight;
                option.ArrowWidth = MaxArrowWidth;
                option.DesiredWidth = MaxWidth;
            }

            _border.Width = MaxWidth;
        }

        /// <summary>
        /// Toggles between not visible and visible.
        /// </summary>
        public void Toggle()
        {
            if (IsExpanded) Collapse();
            else            Expand();
        }

        /// <summary>
        /// Collapses all options and their children menus.
        /// </summary>
        public void Collapse()
        {
            foreach (var option in _options)
            {
                option.Collapse();
            }
            FrameworkElement.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// Expand this menu.
        /// </summary>
        public void Expand()
        {
            FrameworkElement.Visibility = Visibility.Visible;
        }
    }



    /// <summary>
    /// Representing an option in the <see cref="PopupMenu"/> class.
    /// </summary>
    public class PopupMenuOption : IPopupMenu.IPopupMenuOption 
    {
        /// <summary>
        /// The height that every object of this class is instanciated with.
        /// </summary>
        public static readonly double DefaultHeight = 22;
        /// <summary>
        /// The space between the icon and the name that every object of this class is instanciated with.
        /// </summary>
        public static readonly double DefaultSpacingLeft = 10;
        /// <summary>
        /// The space between the name and the arrow that every object of this class is instanciated with.
        /// </summary>
        public static readonly double DefaultSpacingRight = 10;

        /// <summary> | _stackPanel | _childMenuContainer </summary>
        private Canvas _parentContainer = new Canvas { };

        /// <summary> | Icon | SpacingLeft | Name | SpacingRight | Arrow | </summary>
        private StackPanel _stackPanel = new StackPanel { Orientation = Orientation.Horizontal };


        private Image? _icon = null;
        private Border _iconBorder = new Border { };
        private TextBlock _spacingLeft = new TextBlock { };
        private TextBlock _name = new TextBlock { Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center };
        private TextBlock _spacingRight = new TextBlock { };
        private TextBlock _arrow = new TextBlock { Foreground = Brushes.White, TextAlignment = TextAlignment.Center };
        private Border _border = new Border { BorderThickness = new Thickness(0) };
        private Border _innerMenuContainer = new Border { BorderThickness = new Thickness(0) };
        private double _height = 0;
        private double _cachedNameWidth = 0;
        private PopupMenu? _innerMenu;
        private bool _canBeMeasured = false;


        /// <summary>
        /// Desired width, let's see what we can do for you.
        /// </summary>
        public double? DesiredWidth { get; set; } = null;

        /// <summary>
        /// Background color
        /// </summary>
        public Brush Background
        {
            set
            {
                _border.Background = value;
            }
            get => _border.Background;
        }

        /// <summary>
        /// Whtether the inner menu is expanded or not.
        /// </summary>
        public bool IsExpanded => _innerMenu?.IsExpanded ?? false;

        /// <summary>
        /// The space between the icon and the name.
        /// </summary>
        public double SpacingLeft
        {
            get => _spacingLeft.MinWidth;
            set => _spacingLeft.MinWidth = value;
        }
        /// <summary>
        /// The space between the name and the arrow
        /// </summary>
        public double SpacingRight
        {
            get => _spacingRight.MinWidth;
            set => _spacingRight.MinWidth = value;
        }

        /// <summary>
        /// Wether this option has an inner menu or not.
        /// </summary>
        public bool HasInnerMenu => _innerMenu is not null;

        /// <summary>
        /// Get's invoked when this option is clicked.
        /// </summary>
        public event Action? OnClick;

        /// <summary>
        /// Get's invoked when the size changed.
        /// </summary>
        public event Action? OnSizeChanged;

        /// <summary>
        /// Get's called when this ojbect is loaded.
        /// </summary>
        public event Action? OnMeasurable;

        /// <summary>
        /// Get's invoked when an icon is added to this option.
        /// </summary>
        public event Action? OnIconAdded;

        /// <summary>
        /// Get's invoked when an icon is removed from this option.
        /// </summary>
        public event Action? OnIconRemoved;
        
        /// <summary>
        /// Get's invoked when the icon get's accessed or written to.
        /// </summary>
        public event Action? OnIconChanged;

        /// <summary>
        /// Wether this object can be measured or not.
        /// </summary>
        public bool CanBeMeasured
        {
            get => _canBeMeasured;
            set
            {
                _canBeMeasured = value;
                if (_canBeMeasured) OnMeasurable?.Invoke();
            }
        }

        /// <summary>
        /// Width
        /// </summary>
        public double Width
        {
            get
            {
                return IconWidth + SpacingLeft + NameWidth + SpacingRight + ArrowWidth;
            }
        }

        /// <summary>
        /// Height
        /// </summary>
        public double Height
        {
            get => _height;
            internal set
            {
                _height = value;

                _arrow.Height = _height;
                _name.Height = _height;
                _border.Height = _height;
                _iconBorder.Height = _height;
                _spacingLeft.MinHeight = _height;
                _spacingRight.MinHeight = _height;
                if (_icon is not null) _icon.Height = _height;

                OnSizeChanged?.Invoke();
            }
        }

        /// <summary>
        /// Icon Width
        /// </summary>
        public double IconWidth
        {
            get => _iconBorder.Width;
            set
            {
                _iconBorder.Width = value;
                if (_icon is not null) _icon.Width = value;

                AdjustInnerMenuPos();

                OnSizeChanged?.Invoke();
            }
        }

        /// <summary>
        /// Arrow Width
        /// </summary>
        public double ArrowWidth
        {
            get => _arrow.Width;
            set
            {
                _arrow.Width = value;

                AdjustInnerMenuPos();

                OnSizeChanged?.Invoke();
            }
        }

        /// <summary>
        /// Name Width
        /// </summary>
        public double NameWidth
        {
            get => CanBeMeasured && _name.Visibility == Visibility.Visible ? _name.ActualWidth : _cachedNameWidth;
            set
            {
                _name.Width = value;

                AdjustInnerMenuPos();

                OnSizeChanged?.Invoke();
            }
        }

        /// <summary>
        /// The System.Windows.FrameworkElement to handle this object with the System.Windows API.
        /// </summary>
        public FrameworkElement FrameworkElement => _border;

        /// <summary>
        /// The icon next to this option.
        /// </summary>
        public Image? Icon
        {
            get
            {
                OnIconChanged?.Invoke();
                
                return _icon;
            }
            set
            {
                _icon = value;
                
                OnIconChanged?.Invoke();

                if (value is not null)
                {
                    OnIconAdded?.Invoke();
                }
                else
                {
                    OnIconRemoved?.Invoke();
                }
            }
        }

        /// <summary>
        /// The name of the option
        /// </summary>
        public string Name
        {
            get => _name.Text;
            set => _name.Text = value;
        }


        /// <summary>
        /// Contructor
        /// </summary>
        public PopupMenuOption(string name)
        {
            _name.Text = name;

            Init();
        }

        /// <summary>
        /// Contructor
        /// </summary>
        public PopupMenuOption(string name, PopupMenu innerMenu)
        {
            _name.Text = name;
            _innerMenu = innerMenu;

            Init();
        }

        private void Init()
        {
            InitEvents();

            InitHierachy();

            InitVariables();
        }

        private void InitEvents()
        {
            _stackPanel.PreviewMouseDown += (_, _) => OnClick?.Invoke();

            OnMeasurable += () => _cachedNameWidth = _name.ActualWidth;

            if (_innerMenu is not null)
            {
                OnClick += Toggle;
                OnMeasurable += Collapse;
                OnMeasurable += AdjustInnerMenuPos;
                OnSizeChanged += AdjustInnerMenuPos;
            }
        }

        private void InitVariables()
        {
            Background = IPopupMenuOptionDefaults.Background;


            // Init CanBeMeasured
            if (_name.IsLoaded == false)
                _name.Loaded += delegate { CanBeMeasured = true; };
            else CanBeMeasured = true;

            // Spacing
            Height = DefaultHeight;

            SpacingLeft = DefaultSpacingLeft;
            SpacingRight = DefaultSpacingRight;

            IconWidth = Height;
            ArrowWidth = Height;
        }

        private void InitHierachy()
        {
            _stackPanel.Children.Add(_iconBorder);
            _stackPanel.Children.Add(_spacingLeft);
            _stackPanel.Children.Add(_name);
            _stackPanel.Children.Add(_spacingRight);
            _stackPanel.Children.Add(_arrow);

            _border.Child = _parentContainer;

            _parentContainer.Children.Add(_stackPanel);

            if (_icon is not null) _iconBorder.Child = _icon;

            if (_innerMenu is not null)
            {
                _innerMenuContainer.Child = _innerMenu.FrameworkElement;
                _parentContainer.Children.Add(_innerMenuContainer);
            }
        }

        /// <summary>
        /// Adds the action to `OnClick` and returns this instance. 
        /// Useful for Nested initializations.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public PopupMenuOption SetAction(Action action)
        {
            OnClick += action;
            return this;
        }


        internal void AdjustInnerMenuPos()
        {
            Canvas.SetLeft(_innerMenuContainer, DesiredWidth ?? Width);
            Canvas.SetTop(_innerMenuContainer, -1);
        }

        /// <summary>
        /// Toggle the visibility of the inner menu, if existend.
        /// </summary>
        public void Toggle()
        {
            if (IsExpanded) Collapse();
            else            Expand();
        }

        /// <summary>
        /// Collapse the inner menu, if existend.
        /// </summary>
        public void Collapse()
        {
            _innerMenu?.Collapse();
            if (HasInnerMenu) _arrow.Text = "🡢";
        }

        /// <summary>
        /// Expand the inner menu, if existend.
        /// </summary>
        public void Expand()
        {
            _innerMenu?.Expand();
            if (HasInnerMenu) _arrow.Text = "🡦";
        }
    }
}
