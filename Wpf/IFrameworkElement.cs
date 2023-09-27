using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Utillities.Wpf
{
    /// <summary>
    /// A contract that the class inheriting this interface is compatible with the System.Windows namespace.
    /// </summary>
    public interface IFrameworkElement
    {
        /// <summary>
        /// The System.Windows.FrameworkElement to handle this object with the System.Windows API.
        /// </summary>
        public FrameworkElement FrameworkElement { get; }
    }
}
