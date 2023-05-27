using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utillities.ConsoleApplication
{
    /// <summary>
    /// Defines the program run type of a ConsoleWindow application. 
    /// </summary>
    public enum ProgramRunType {
        /// <summary>
        /// The ConsoleWindow application was started via the DebugLibrary. A StreamPipe between them will be created, can cause crashes if not handled properly.
        /// </summary>
        DebugLibrary,
        /// <summary>
        /// The ConsoleWindow application was started as am application externally, outside the intended DebugLibrary Framework.
        /// </summary>
        StandAlone
    }
}
