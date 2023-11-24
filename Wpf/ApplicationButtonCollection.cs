using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Xml;
using System.Windows.Shell;
using System.Windows.Shapes;
using System.Windows.Markup;

namespace Utillities.Wpf
{
    public partial class WindowHandle
    {
        /// <summary>
        /// Represents a collection of application buttons within the WindowHandle.
        /// </summary>
        public class ApplicationButtonCollection : IWindowChromeElement
        {
            private Window window;
            private WindowHandle windowHandle;

            public static readonly string FULLSCRREN_1 = "⛶";

            public static readonly string EXIT_1 = "✕";
            public static readonly string EXIT_2 = "x";
            public static readonly string EXIT_3 = "🗙︎";

            public static readonly string MINIMIZE_1 = "🗕";
            public static readonly string MINIMIZE_2 = "-";
            public static readonly string MINIMIZE_3 = "_";

            public static readonly string MAXIMIZE_1 = "🗖︎";
            public static readonly string MAXIMIZE_2 = "🗗︎";

            public static readonly string SETTINGS_1 = "⚙";

            private const string ARROW_TOPLEFT = "🡬";
            private const string ARROW_TOPRIGHT = "🡭";
            private const string ARROW_BOTTOMLEFT = "🡯";
            private const string ARROW_BOTTOMRIGHT = "🡮";
            private bool isFullscreen = false;
            private _WindowState? prevWindowState;
            private bool exitSpriteActivated = false;
            private bool minimizeSpriteActivated = false;
            private bool maximizeSpriteActivated = false;

            private static double height = 30;
            private static double width = 40;
            /// <summary>Gets the fullscreen state of the window.</summary>
            public bool IsFullscreen => isFullscreen;

            /// <summary>Gets or sets the height of the application buttons.</summary>
            public double Height
            {
                get { return height; }
                set
                {
                    if (value <= window.Height)
                    {
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
            public double Width
            {
                get { return width; }
                set
                {
                    if (value <= window.Width / 3)
                    {
                        width = value;
                        exitButton.Width = value;
                        minimizeButton.Width = value;
                        maximizeButton.Width = value;
                        if (settingsButton != null)
                            settingsButton.Width = value;
                    }
                }
            }


            private StackPanel stackPanel = new();
            private Button? fullscreenButton;
            private Button? settingsButton;
            private Button exitButton = new();
            private Button minimizeButton = new();
            private Button maximizeButton = new();
            private string? windowedButtonSource;
            private string? maximizedButtonSource;

            /// <summary>Gets the exit button.</summary>
            public Button ExitButton => exitButton;
            /// <summary>Gets or sets the image source for the exit button.</summary>
            public string? ExitButtonImageSource
            {
                get
                {
                    if (!exitSpriteActivated) throw new NotActivatedException("Exit");

                    return ((exitButton.Content as Border)!.Child as System.Windows.Controls.Image)!.Source.ToString();
                }
                set
                {
                    if (!exitSpriteActivated) throw new NotActivatedException("Exit");

                    var imageContent = ((exitButton.Content as Border)!.Child as System.Windows.Controls.Image)!;
                    imageContent.Source = new BitmapImage(new Uri(value!));
                }
            }
            /// <summary>Gets or sets the padding for the exit button image.</summary>
            public Thickness ExitButtonImagePadding
            {
                get
                {
                    if (!exitSpriteActivated) throw new NotActivatedException("Exit");

                    return (exitButton.Content as Border)!.Padding;
                }
                set
                {
                    if (!exitSpriteActivated) throw new NotActivatedException("Exit");

                    (exitButton.Content as Border)!.Padding = value;
                }
            }


            /// <summary>Gets the minimize button.</summary>
            public Button MinimizeButton => minimizeButton;
            /// <summary>Gets or sets the image source for the minimize button.</summary>
            public string? MinimizeButtonImageSource
            {
                get
                {
                    if (!minimizeSpriteActivated) throw new NotActivatedException("Minimize");

                    return ((minimizeButton.Content as Border)!.Child as System.Windows.Controls.Image)!.Source.ToString();
                }
                set
                {
                    if (!minimizeSpriteActivated) throw new NotActivatedException("Minimize");

                    var imageContent = ((minimizeButton.Content as Border)!.Child as System.Windows.Controls.Image)!;
                    imageContent.Source = new BitmapImage(new Uri(value!));
                }
            }
            /// <summary>Gets or sets the padding for the minimize button image.</summary>
            public Thickness MinimizeButtonImagePadding
            {
                get
                {
                    if (!minimizeSpriteActivated) throw new NotActivatedException("Minimize");

                    return (minimizeButton.Content as Border)!.Padding;
                }
                set
                {
                    if (!minimizeSpriteActivated) throw new NotActivatedException("Minimize");

                    (minimizeButton.Content as Border)!.Padding = value;
                }
            }


            /// <summary>Gets the maximize button.</summary>
            public Button MaximizeButton => maximizeButton;
            /// <summary>Gets or sets the image source for the maximize button.</summary>
            public string? MaximizeButtonImageSource
            {
                get
                {
                    if (!maximizeSpriteActivated) throw new NotActivatedException("Maximize");
                    return ((maximizeButton.Content as Border)!.Child as System.Windows.Controls.Image)!.Source.ToString();
                }
                set
                {
                    if (!maximizeSpriteActivated) throw new NotActivatedException("Maximize");
                    var imageContent = ((maximizeButton.Content as Border)!.Child as System.Windows.Controls.Image)!;
                    imageContent.Source = new BitmapImage(new Uri(value!));
                }
            }
            /// <summary>Gets or sets the image source for the maximize button when it is maximized.</summary>
            public string? MaximizeButtonImageSourceWhenMaximized
            {
                get
                {
                    if (!maximizeSpriteActivated) throw new NotActivatedException("Maximize");

                    return maximizedButtonSource;
                }
                set
                {
                    if (!maximizeSpriteActivated) throw new NotActivatedException("Maximize");

                    maximizedButtonSource = value;

                    if (window.WindowState == WindowState.Maximized)
                    {
                        var imageContent = ((maximizeButton.Content as Border)!.Child as System.Windows.Controls.Image)!;
                        imageContent.Source = new BitmapImage(new Uri(value!));
                    }
                }
            }
            /// <summary>Gets or sets the padding for the maximize button image.</summary>
            public Thickness MaximizeButtonImagePadding
            {
                get
                {
                    if (!maximizeSpriteActivated) throw new NotActivatedException("Maximize");

                    return (maximizeButton.Content as Border)!.Padding;
                }
                set
                {
                    if (!maximizeSpriteActivated) throw new NotActivatedException("Maximize");

                    (maximizeButton.Content as Border)!.Padding = value;
                }
            }
            /// <summary>Gets or sets the image source for the maximize button when it is windowed</summary>
            public string? MaximizeButtonImageSourceWhenWindowed
            {
                get
                {
                    if (!maximizeSpriteActivated) throw new NotActivatedException("Maximize");

                    return windowedButtonSource;
                }
                set
                {
                    if (!maximizeSpriteActivated) throw new NotActivatedException("Maximize");

                    windowedButtonSource = value;

                    if (window.WindowState != WindowState.Normal)
                    {
                        var imageContent = ((maximizeButton.Content as Border)!.Child as System.Windows.Controls.Image)!;
                        imageContent.Source = new BitmapImage(new Uri(value!));
                    }
                }
            }


            /// <summary>Gets the settings button.</summary>
            public Button? SettingsButton => settingsButton;
            /// <summary>Gets or sets the image source for the settings button. Expects an image file, like `png`.</summary>
            public string? SettingsButtonImageSource
            {
                get
                {
                    if (settingsButton == null) return null;
                    if ((settingsButton.Content as Border)!.Child == null) return null;

                    return ((settingsButton.Content as Border)!.Child as System.Windows.Controls.Image)!.Source.ToString();
                }
                set
                {
                    if (settingsButton == null) return;

                    var imageContent = ((settingsButton.Content as Border)!.Child as System.Windows.Controls.Image)!;
                    imageContent.Source = new BitmapImage(new Uri(value!));
                }
            }
            public string? SettingsButtonXmlSource
            {
                set
                {
                    if (!File.Exists(value))
                    {
                        return;
                    }
                    using StreamReader sr = new StreamReader(value!);
                    using XmlReader xmlReader = XmlReader.Create(sr);

                    Rectangle rect = new Rectangle
                    {
                        Width = width,
                        Height = height
                    };
                    DrawingBrush brush = new();
                    rect.Fill = brush;
                    brush.Drawing = (Drawing)XamlReader.Load(xmlReader);

                    (settingsButton.Content as Border)!.Child = rect;
                }
            }
            /// <summary>Set the content of the settings button directly.</summary>
            public object SettingsButtonContent
            {
                set
                {
                    settingsButton.Content = value;
                }
            }
            /// <summary>Gets or sets the padding for the settings button image.</summary>
            public Thickness SettingsButtonContentPadding
            {
                get
                {
                    if (settingsButton == null) return new Thickness();

                    return (settingsButton.Content as Border)!.Padding;
                }
                set
                {
                    if (settingsButton == null) return;

                    (settingsButton.Content as Border)!.Padding = value;
                }
            }


            /// <summary>Gets the fullscreen button.</summary>
            public Button? FullscreenButton => fullscreenButton;
            /// <summary>Gets or sets the image source for the fullscreen button.</summary>
            public string? FullscreenButtonImageSource
            {
                get
                {
                    if (fullscreenButton == null) return null;
                    if ((fullscreenButton.Content as Border)!.Child == null) return null;

                    return ((fullscreenButton.Content as Border)!.Child as System.Windows.Controls.Image)!.Source.ToString();
                }
                set
                {
                    if (fullscreenButton == null) return;

                    var imageContent = ((fullscreenButton.Content as Border)!.Child as System.Windows.Controls.Image)!;
                    imageContent.Source = new BitmapImage(new Uri(value!));
                }
            }
            /// <summary>Gets or sets the padding for the settings button image.</summary>
            public Thickness FullscreenButtonImagePadding
            {
                get
                {
                    if (fullscreenButton == null) return new Thickness();

                    return (fullscreenButton.Content as Border)!.Padding;
                }
                set
                {
                    if (fullscreenButton == null) return;

                    (fullscreenButton.Content as Border)!.Padding = value;
                }
            }


            /// <summary>Gets the framework element which contains the application buttons.</summary>
            public FrameworkElement FrameworkElement => stackPanel;


            private Brush BGcolorOnHover = Helper.StringToSolidColorBrush("#3d3d3d");
            private static Brush BGcolor = Brushes.Transparent;
            private static Brush symbolColor = Brushes.White;
            /// <summary>Gets or sets the color of the application buttons when hovered.</summary>
            public Brush ColorWhenButtonHover
            {
                get => BGcolorOnHover;
                set
                {
                    BGcolorOnHover = value;
                    UpdateButtonColors();
                }
            }

            /// <summary>Gets or sets the color of the application buttons.</summary>
            public Brush Color
            {
                get => BGcolor;
                set
                {
                    BGcolor = value;
                    UpdateButtonColors();
                }
            }

            /// <summary>Gets or sets the color of the symbol within the application buttons.</summary>
            public Brush SymbolColor
            {
                get => symbolColor;
                set
                {
                    symbolColor = value;
                    UpdateButtonColors();
                }
            }



            /// <summary>
            /// Initializes a new instance of the <see cref="ApplicationButtonCollection"/> class.
            /// </summary>
            /// <param name="windowHandle">The WindowHandle.</param>
            /// <param name="window">The associated window.</param>
            public ApplicationButtonCollection(WindowHandle windowHandle, Window window)
            {
                this.window = window;
                this.windowHandle = windowHandle;



                exitButton.Style = ButtonStyle();
                exitButton.Content = EXIT_3;
                exitButton.Click += Shutdown;
                exitButton.MouseEnter += (s, e) => { exitButton.Background = BGcolorOnHover; };
                exitButton.MouseLeave += (s, e) => { exitButton.Background = BGcolor; };
                Helper.SetWindowChromActive(exitButton);


                minimizeButton.Style = ButtonStyle();
                minimizeButton.Content = MINIMIZE_1;
                minimizeButton.Click += Minimize;
                minimizeButton.MouseEnter += (s, e) => { minimizeButton.Background = BGcolorOnHover; };
                minimizeButton.MouseLeave += (s, e) => { minimizeButton.Background = BGcolor; };
                Helper.SetWindowChromActive(minimizeButton);


                maximizeButton.Style = ButtonStyle();
                maximizeButton.Content = MAXIMIZE_2;
                maximizeButton.Click += Maximize;
                maximizeButton.MouseEnter += (s, e) => { maximizeButton.Background = BGcolorOnHover; };
                maximizeButton.MouseLeave += (s, e) => { maximizeButton.Background = BGcolor; };
                Helper.SetWindowChromActive(maximizeButton);


                stackPanel.VerticalAlignment = VerticalAlignment.Center;
                stackPanel.HorizontalAlignment = HorizontalAlignment.Right;
                stackPanel.Orientation = Orientation.Horizontal;

                stackPanel.Children.Add(minimizeButton);
                stackPanel.Children.Add(maximizeButton);
                stackPanel.Children.Add(exitButton);

                UpdateButtonColors();
                UpdateButtonSize();
            }

            private class NotActivatedException : Exception
            {
                public NotActivatedException(string button)
                    : base($"The sprite for the {button} button has not been activated yet. Consider calling `Activate{button}ButtonSprite()` before this.")
                {
                }
            }

            /// <summary>
            /// Will prepare the exit button to have an image as content.
            /// </summary>
            public void ActivateExitButtonSprite()
            {
                ActivateButton(exitButton);
                exitSpriteActivated = true;
            }

            /// <summary>
            /// Will prepare the exit button to have an image as content.
            /// </summary>
            public void ActivateMaximizeButtonSprite()
            {
                ActivateButton(maximizeButton);
                maximizeSpriteActivated = true;
            }

            /// <summary>
            /// Will prepare the exit button to have an image as content.
            /// </summary>
            public void ActivateMinimizeButtonSprite()
            {
                ActivateButton(minimizeButton);
                minimizeSpriteActivated = true;
            }

            private void ActivateButton(Button button)
            {
                var container = new Border
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };
                button.Content = container;

                var imageContent = new System.Windows.Controls.Image
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                container.Child = imageContent;
            }

            /// <summary>
            /// Adds the settings button to the collection.
            /// </summary>
            public void AddSettingsButton()
            {
                settingsButton = new();

                windowHandle._mainGrid.ColumnDefinitions[1].Width = new GridLength((stackPanel.Children.Count + 1) * width);

                settingsButton.Style = ButtonStyle();
                settingsButton.Click += Settings;
                settingsButton.MouseEnter += (s, e) => settingsButton.Background = BGcolorOnHover;
                settingsButton.MouseLeave += (s, e) => settingsButton.Background = BGcolor;

                ActivateButton(settingsButton);

                Helper.SetWindowChromActive(settingsButton);
                stackPanel.Children.Insert(0, settingsButton);

                UpdateButtonColors();
                UpdateButtonSize();
            }

            /// <summary>
            /// Adds the fullscreen button to the collection.
            /// </summary>
            public void AddFullcreenButton()
            {
                fullscreenButton = new();

                windowHandle._mainGrid.ColumnDefinitions[1].Width = new GridLength((stackPanel.Children.Count + 1) * width);

                fullscreenButton.Style = ButtonStyle();
                fullscreenButton.Click += Fullscreen;
                fullscreenButton.MouseEnter += (s, e) => { fullscreenButton.Background = BGcolorOnHover; };
                fullscreenButton.MouseLeave += (s, e) => { fullscreenButton.Background = BGcolor; };
                Helper.SetWindowChromActive(fullscreenButton);

                ActivateButton(fullscreenButton);

                stackPanel.Children.Insert(0, fullscreenButton);

                UpdateButtonColors();
                UpdateButtonSize();
            }

            /// <summary>
            /// Creates the button style for the application buttons.
            /// </summary>
            /// <returns>The button style.</returns>
            public static Style ButtonStyle()
            {
                // Create a new style for the button
                Style style = new Style(typeof(Button));
                style.Setters.Add(new Setter(Button.BackgroundProperty, BGcolor));
                style.Setters.Add(new Setter(Button.ForegroundProperty, symbolColor));
                style.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Transparent));
                style.Setters.Add(new Setter(Button.HorizontalAlignmentProperty, HorizontalAlignment.Right));
                style.Setters.Add(new Setter(Button.VerticalAlignmentProperty, VerticalAlignment.Top));
                style.Setters.Add(new Setter(Button.FontSizeProperty, 13.0));
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
            public void SetWindowChromeActive()
            {
                Helper.SetWindowChromActive(exitButton);
                Helper.SetWindowChromActive(minimizeButton);
                Helper.SetWindowChromActive(maximizeButton);
                if (settingsButton != null) Helper.SetWindowChromActive(settingsButton);
                if (fullscreenButton != null) Helper.SetWindowChromActive(fullscreenButton);
            }

            /// <summary>
            /// Overrides the default shutdown behavior of the exit button with a custom action.
            /// </summary>
            /// <param name="action">The custom action to be executed on exit button click.</param>
            public void OverrideShutdown(Action action)
            {
                exitButton.Click -= Shutdown;
                exitButton.Click += (object sender, RoutedEventArgs e) => action();
            }
            /// <summary>
            /// Overrides the default minimize behavior of the minimize button with a custom action.
            /// </summary>
            /// <param name="action">The custom action to be executed on minimize button click.</param>
            public void OverrideMinimize(Action action)
            {
                exitButton.Click -= Minimize;
                exitButton.Click += (object sender, RoutedEventArgs e) => action();
            }
            /// <summary>
            /// Overrides the default maximize behavior of the maximize button with a custom action.
            /// </summary>
            /// <param name="action">The custom action to be executed on maximize button click.</param>
            public void OverrideMaximize(Action action)
            {
                exitButton.Click -= Maximize;
                exitButton.Click += (object sender, RoutedEventArgs e) => action();
            }
            /// <summary>
            /// Overrides the default settings button behavior with a custom action.
            /// </summary>
            /// <param name="action">The custom action to be executed on settings button click.</param>
            public void OverrideSettings(Action action)
            {
                if (settingsButton is null) return;

                settingsButton.Click -= Settings;
                settingsButton.Click += (object sender, RoutedEventArgs e) => action();
            }
            /// <summary>
            /// Overrides the default fullscreen button behavior with a custom action.
            /// </summary>
            /// <param name="action"></param>
            public void OverrideFullscreen(Action action)
            {
                if (fullscreenButton is null) return;

                fullscreenButton.Click -= Fullscreen;
                fullscreenButton.Click += (s, e) => action();
            }

            public delegate void EventHandler();
            public event EventHandler? OnFullscreen;

            private void Shutdown(object sender, RoutedEventArgs e)
            {
                window.Close();
            }
            private void Minimize(object sender, RoutedEventArgs e)
            {
                window.WindowState = WindowState.Minimized;
            }
            private void Maximize(object sender, RoutedEventArgs e)
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    // Go into windowed
                    window.WindowState = WindowState.Normal;

                    if (maximizeSpriteActivated)
                    {
                        MaximizeButtonImageSource = maximizedButtonSource;
                    }
                }
                else
                {
                    // Go into maximized
                    window.WindowState = WindowState.Maximized;

                    if (maximizeSpriteActivated)
                    {
                        MaximizeButtonImageSource = windowedButtonSource;
                    }
                }
                // Update Layout
                window.UpdateLayout();
            }
            private void Settings(object sender, RoutedEventArgs e)
            {
                // Acting as a dummy method
            }
            private void Fullscreen(object sender, RoutedEventArgs e)
            {
                if (!isFullscreen)
                {
                    isFullscreen = true;

                    prevWindowState = new _WindowState(window.WindowState, window.Top, window.Left, window.Width, window.Height);
                    
                    window.WindowState = WindowState.Normal;
                    window.Left = 0;
                    window.Top = 0;
                    window.Width = SystemParameters.PrimaryScreenWidth;
                    window.Height = SystemParameters.PrimaryScreenHeight;

                    windowHandle.Wrap.RoundedCorners = true;

                    OnFullscreen?.Invoke();
                }
                else
                {
                    isFullscreen = false;
                    window.WindowState = prevWindowState!.Value.windowState;
                    window.Left = prevWindowState.Value.Left;
                    window.Top = prevWindowState.Value.Top;
                    window.Width = prevWindowState.Value.Width;
                    window.Height = prevWindowState.Value.Height;

                    windowHandle.Wrap.RoundedCorners = false;

                    prevWindowState = null;
                }
            }

            /// <summary>
            /// Forces the Sizes of the buttons to update.
            /// </summary>
            public void UpdateButtonSize()
            {
                exitButton.Width = Width;
                exitButton.Height = Height;

                minimizeButton.Width = Width;
                minimizeButton.Height = Height;

                maximizeButton.Width = Width;
                maximizeButton.Height = Height;

                if (settingsButton != null)
                {
                    settingsButton.Width = Width;
                    settingsButton.Height = Height;
                }

                if (fullscreenButton != null)
                {
                    fullscreenButton.Width = Width;
                    fullscreenButton.Height = Height;
                }
            }
            /// <summary>
            /// Forces the colors of the buttons to update.
            /// </summary>
            public void UpdateButtonColors()
            {
                exitButton.Background = BGcolor;
                exitButton.Foreground = symbolColor;

                minimizeButton.Background = BGcolor;
                minimizeButton.Foreground = symbolColor;


                maximizeButton.Background = BGcolor;
                maximizeButton.Foreground = symbolColor;

                if (settingsButton != null)
                {
                    settingsButton.Background = BGcolor;
                }

                if (fullscreenButton != null)
                {
                    fullscreenButton.Background = BGcolor;
                }
            }

            /// <summary>
            /// Set `WindowChrome.SetIsHitTestVisibleInChrome` for this element.
            /// </summary>
            /// <param name="active"></param>
            public void SetWindowChromeActive(bool active)
            {
                WindowChrome.SetIsHitTestVisibleInChrome(FrameworkElement, active);
            }

            private struct _WindowState
            {
                public WindowState windowState;
                public double Top;
                public double Left;
                public double Width;
                public double Height;

                public _WindowState(WindowState windowState,
                                    double Top,
                                    double Left,
                                    double Width,
                                    double Height)
                {
                    this.windowState = windowState;
                    this.Top = Top;
                    this.Left = Left;
                    this.Width = Width;
                    this.Height = Height;
                }
            }
        }
    }
}
