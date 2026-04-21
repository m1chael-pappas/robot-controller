using RobotController.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace RobotController.CommandProviders
{
    /// <summary>
    /// <see cref="AdvancedHttpCommandProvider"/> extends the HTTP ACL with workflow orchestration:
    /// when <c>ExecutionMode=AllOrNothing</c>, all mapped commands are wrapped in a single
    /// <see cref="AtomicCommand"/> so they are validated via dry-run before being committed.
    /// </summary>
    internal class AdvancedHttpCommandProvider : ICommandProvider
    {
        private const string NotSupportedComment = "not supported - MS - show flexibility and additional API responsibility.";

        private readonly HttpClient httpClient;

        public AdvancedHttpCommandProvider() : this(new HttpClient())
        {
        }

        public AdvancedHttpCommandProvider(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public IEnumerable<ICommand> GetCommands(string[] args)
        {
            if (args == null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
                yield break;

            var url = args[0];
            var response = httpClient.GetStringAsync(url).Result;

            if (string.IsNullOrWhiteSpace(response))
                yield break;

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };

            var commandSet = JsonSerializer.Deserialize<CommandSetDto>(response, options);
            if (commandSet == null)
                yield break;

            var mapped = commandSet.Commands
                .Where(x => x.Comment != NotSupportedComment)
                .SelectMany(HttpCommandProvider.Map)
                .ToList();

            var executionMode = string.IsNullOrWhiteSpace(commandSet.ExecutionMode)
                ? "BestEffort"
                : commandSet.ExecutionMode;

            if (executionMode.Equals("AllOrNothing", StringComparison.OrdinalIgnoreCase))
            {
                yield return new AtomicCommand(mapped);
                yield break;
            }

            foreach (var command in mapped)
                yield return command;
        }
    }
}
