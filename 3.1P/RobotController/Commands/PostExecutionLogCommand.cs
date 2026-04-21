using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace RobotController.Commands
{
    /// <summary>
    /// Sentinel command yielded by <see cref="CommandProviders.AdvancedHttpCommandProvider"/>
    /// after a fetched workflow completes. On <see cref="Execute"/>, it flattens the
    /// <see cref="AdvancedRobot.CommandHistory"/> (skipping workflow wrappers, other sentinels,
    /// and unrecognised input) and POSTs it to <c>{baseUrl}/command-executions</c>. This keeps
    /// the transport concern inside the provider without adding orchestration plumbing to
    /// Program.cs.
    /// </summary>
    internal class PostExecutionLogCommand : ICommand
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;
        private readonly int workflowId;
        private readonly string schemaVersion;

        public PostExecutionLogCommand(HttpClient httpClient, string baseUrl, int workflowId, string schemaVersion)
        {
            this.httpClient = httpClient;
            this.baseUrl = baseUrl;
            this.workflowId = workflowId;
            this.schemaVersion = schemaVersion;
        }

        public string Name => "POST_EXECUTION_LOG";

        public string Description => "Posts robot command history back to /command-executions.";

        public bool Executed { get; set; }

        public bool Success { get; set; }

        public bool Execute(IRobot robot)
        {
            Executed = true;

            var advanced = robot as AdvancedRobot;
            if (advanced == null)
                return Success = false;

            var entries = new List<ExecutedCommandDto>();
            foreach (var command in advanced.CommandHistory)
            {
                if (command is AtomicCommand) continue;
                if (command is UnknownCommand) continue;
                if (command is PostExecutionLogCommand) continue;

                var dto = new ExecutedCommandDto
                {
                    Name = command.Name,
                    Executed = command.Executed,
                    Success = command.Success
                };

                switch (command)
                {
                    case PlaceCommand place:
                        dto.X = place.X;
                        dto.Y = place.Y;
                        dto.Direction = place.Facing.ToString().ToUpperInvariant();
                        break;
                    case JumpForwardCommand jf:
                        dto.NumberOfSteps = jf.Steps;
                        break;
                    case JumpBackwardCommand jb:
                        dto.NumberOfSteps = jb.Steps;
                        break;
                }

                entries.Add(dto);
            }

            var log = new CommandExecutionLogDto
            {
                WorkflowId = workflowId,
                SchemaVersion = schemaVersion,
                Commands = entries
            };

            try
            {
                var json = JsonSerializer.Serialize(log, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = httpClient.PostAsync(baseUrl + "/command-executions", content).Result;
                return Success = response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return Success = false;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    internal class ExecutedCommandDto
    {
        public string Name { get; set; }
        public bool Executed { get; set; }
        public bool Success { get; set; }
        public int? NumberOfSteps { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public string Direction { get; set; }
        public string Comment { get; set; }
    }

    internal class CommandExecutionLogDto
    {
        public int WorkflowId { get; set; }
        public string SchemaVersion { get; set; }
        public List<ExecutedCommandDto> Commands { get; set; } = new List<ExecutedCommandDto>();
    }
}
