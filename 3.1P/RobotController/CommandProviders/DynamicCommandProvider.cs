using RobotController.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RobotController.CommandProviders
{
    /// <summary>
    /// Strategy dispatcher over a dictionary of <see cref="IMetaCommandProvider"/>s keyed by
    /// their <c>MetaCommand</c> (":console", ":file", ":api"). Starts in the console provider
    /// and switches whenever the active provider emits an <see cref="UnknownCommand"/> whose
    /// raw input begins with ':'. The sentinel <c>:quit</c> ends the session. After a
    /// non-console provider exhausts, control returns to the console provider so the user
    /// can continue interacting.
    /// </summary>
    internal class DynamicCommandProvider : ICommandProvider
    {
        private readonly Dictionary<string, IMetaCommandProvider> providersByMetaCommand;
        private readonly IMetaCommandProvider consoleProvider;
        private string[] currentArgs = Array.Empty<string>();
        private bool shouldExit;

        public DynamicCommandProvider(IReadOnlyCollection<IMetaCommandProvider> commandProviders)
        {
            if (commandProviders == null)
                throw new ArgumentNullException(nameof(commandProviders));
            if (commandProviders.Count == 0)
                throw new ArgumentException(
                    "At least one command provider must be supplied.",
                    nameof(commandProviders));

            providersByMetaCommand = commandProviders.ToDictionary(
                p => p.MetaCommand,
                p => p,
                StringComparer.OrdinalIgnoreCase);

            if (!providersByMetaCommand.TryGetValue(":console", out var foundConsoleProvider))
                throw new InvalidOperationException(
                    "A command provider with MetaCommand ':console' must be registered because it is the default provider.");

            consoleProvider = foundConsoleProvider;
            CurrentProvider = consoleProvider;
        }

        public IMetaCommandProvider CurrentProvider { get; private set; }

        public void SwitchTo(IMetaCommandProvider provider, string[] args)
        {
            CurrentProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            currentArgs = args ?? Array.Empty<string>();
        }

        public IEnumerable<ICommand> GetCommands(string[] args)
        {
            while (!shouldExit)
            {
                var switchedProvider = false;

                foreach (var command in CurrentProvider.GetCommands(currentArgs))
                {
                    if (command is UnknownCommand unknownCommand &&
                        !string.IsNullOrWhiteSpace(unknownCommand.Input))
                    {
                        var parts = unknownCommand.Input
                            .Trim()
                            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        var metaCommand = parts[0];

                        if (!metaCommand.StartsWith(":", StringComparison.Ordinal))
                        {
                            Console.WriteLine("Unsupported command: " + unknownCommand.Input);
                            continue;
                        }

                        if (metaCommand.Equals(":quit", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("Exiting dynamic command provider.");
                            shouldExit = true;
                            yield break;
                        }

                        if (providersByMetaCommand.TryGetValue(metaCommand, out var provider))
                        {
                            SwitchTo(provider, parts.Skip(1).ToArray());
                            Console.WriteLine("Switched to provider " + metaCommand + ".");
                            switchedProvider = true;
                            break;
                        }

                        Console.WriteLine("Unsupported command: " + unknownCommand.Input);
                        continue;
                    }

                    yield return command;
                }

                if (shouldExit) yield break;

                if (switchedProvider) continue;

                // Active provider exhausted naturally. If we're already on console, stdin
                // reached EOF so end the session; otherwise return to console so the user
                // can continue interacting.
                if (ReferenceEquals(CurrentProvider, consoleProvider))
                    yield break;

                SwitchTo(consoleProvider, Array.Empty<string>());
                Console.WriteLine("Switched back to console input.");
            }
        }
    }
}
