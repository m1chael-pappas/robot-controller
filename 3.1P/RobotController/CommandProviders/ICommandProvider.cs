using RobotController.Commands;
using System.Collections.Generic;

namespace RobotController.CommandProviders
{
    /// <summary>
    /// Common commands provider interface  that is used to get robot commands input.
    /// </summary>
    public interface ICommandProvider
    {
        /// <summary>
        /// Gets <see cref="IEnumerable{ICommand}"/> from the input.
        /// </summary>
        /// <param name="args">String arguments passed from the input.</param>
        /// <returns><see cref="IEnumerable{ICommand}"/> for further processing.</returns>
        IEnumerable<ICommand> GetCommands(string[] args);
    }
}
