using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotController.Commands
{
    /// <summary>
    /// Robot command interface.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Command's name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Command's description.   
        /// </summary>
        string Description {  get; }

        /// <summary>
        /// Shows whether the command has been executed.
        /// </summary>
        bool Executed { get; set; }

        /// <summary>
        /// Shows whether the command has run successfully.
        /// </summary>
        bool Success { get; set; }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>True if the command has run successfully.</returns>
        bool Execute(IRobot robot);
    }
}
