using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shell;

namespace Utillities.Wpf
{
    /// <summary>
    /// An element that can be window chrome activated.
    /// </summary>
    public interface IWindowChromeElement : IFrameworkElement
    {
        /// <summary>
        /// Set the window chrome activation
        /// </summary>
        /// <param name="active"></param>
        public void SetWindowChromeActive(bool active);
    }
}
