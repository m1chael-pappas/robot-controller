using RobotController.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace RobotController.CommandProviders
{
    /// <summary>
    /// <see cref="HttpCommandProvider"/> acts as an Anti-Corruption Layer between the
    /// Minimal API (Open Host Service) and the legacy robot command model.
    /// </summary>
    internal class HttpCommandProvider : ICommandProvider
    {
        private const string NotSupportedComment = "not supported - MS - show flexibility and additional API responsibility.";

        private readonly HttpClient httpClient;

        public HttpCommandProvider() : this(new HttpClient())
        {
        }

        public HttpCommandProvider(HttpClient httpClient)
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

            foreach (var dto in commandSet.Commands.Where(x => x.Comment != NotSupportedComment))
            {
                foreach (var cmd in Map(dto))
                    yield return cmd;
            }
        }

        internal static IEnumerable<ICommand> Map(RobotCommandDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                yield break;

            switch (dto.Name.ToUpperInvariant())
            {
                case "MOVE":
                    var steps = dto.NumberOfSteps.GetValueOrDefault(1);
                    if (steps < 1) steps = 1;
                    for (var i = 0; i < steps; i++)
                        yield return new MoveCommand();
                    yield break;

                case "LEFT":
                    yield return new LeftCommand();
                    yield break;

                case "RIGHT":
                    yield return new RightCommand();
                    yield break;

                case "REPORT":
                    yield return new ReportCommand();
                    yield break;

                case "PLACE":
                    var place = DeserializePlace(dto);
                    if (place != null)
                        yield return place;
                    yield break;

                case "STEP_BACK":
                    yield return new StepBackCommand();
                    yield break;

                case "JUMP_FORWARD":
                    var jump = dto.NumberOfSteps.GetValueOrDefault(2);
                    if (jump < 1) jump = 1;
                    yield return new JumpForwardCommand(jump);
                    yield break;

                case "JUMP_BACKWARD":
                    var jumpBack = dto.NumberOfSteps.GetValueOrDefault(2);
                    if (jumpBack < 1) jumpBack = 1;
                    yield return new JumpBackwardCommand(jumpBack);
                    yield break;
            }
        }

        private static PlaceCommand DeserializePlace(RobotCommandDto dto)
        {
            if (!dto.X.HasValue || !dto.Y.HasValue || string.IsNullOrWhiteSpace(dto.Direction))
                return null;

            Direction facing;
            switch (dto.Direction.ToUpperInvariant())
            {
                case "NORTH": facing = Direction.North; break;
                case "EAST": facing = Direction.East; break;
                case "SOUTH": facing = Direction.South; break;
                case "WEST": facing = Direction.West; break;
                default: return null;
            }

            return new PlaceCommand(dto.X.Value, dto.Y.Value, facing);
        }
    }

    internal class RobotCommandDto
    {
        public string Name { get; set; }
        public bool? IsMoveCommand { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public string Direction { get; set; }
        public string Comment { get; set; }
        public int? NumberOfSteps { get; set; }
    }

    internal class CommandSetDto
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public string SchemaVersion { get; set; }
        public string ExecutionMode { get; set; }
        public List<RobotCommandDto> Commands { get; set; } = new List<RobotCommandDto>();
    }
}
