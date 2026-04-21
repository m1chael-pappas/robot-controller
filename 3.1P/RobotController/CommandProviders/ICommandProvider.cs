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

    /// <summary>
    /// A provider that can be selected at runtime by <see cref="DynamicCommandProvider"/> through
    /// its <see cref="MetaCommand"/> (e.g. ":console", ":file", ":api").
    /// </summary>
    public interface IMetaCommandProvider : ICommandProvider
    {
        /// <summary>
        /// The sentinel string (starting with ':') that selects this provider in dynamic mode.
        /// </summary>
        string MetaCommand { get; }
    }
}
