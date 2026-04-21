using RobotController.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace RobotController.CommandProviders
{
    /// <summary>
    /// <see cref="AdvancedHttpCommandProvider"/> extends the HTTP ACL with workflow orchestration
    /// and execution-log reporting:
    /// * when <c>ExecutionMode=AllOrNothing</c>, all mapped commands are wrapped in a single
    ///   <see cref="AtomicCommand"/> so they are validated via dry-run before being committed;
    /// * after the workflow yields, a <see cref="PostExecutionLogCommand"/> sentinel is yielded
    ///   so the robot's command history is POSTed back to <c>/command-executions</c> once the
    ///   real run completes. That POST is what the server needs to build a rollback later.
    /// </summary>
    internal class AdvancedHttpCommandProvider : IMetaCommandProvider
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

        public string MetaCommand => ":api";

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
            }
            else
            {
                foreach (var command in mapped)
                    yield return command;
            }

            if (commandSet.Id > 0)
            {
                var baseUrl = ExtractBaseUrl(url);
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    yield return new PostExecutionLogCommand(
                        httpClient,
                        baseUrl,
                        commandSet.Id,
                        commandSet.SchemaVersion);
                }
            }
        }

        /// <summary>
        /// Strips the path/query off a URL to leave scheme + host + port, e.g.
        /// <c>http://localhost:5233/command-sets/1</c> becomes <c>http://localhost:5233</c>.
        /// </summary>
        private static string ExtractBaseUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return null;
            return uri.GetLeftPart(UriPartial.Authority);
        }
    }
}
