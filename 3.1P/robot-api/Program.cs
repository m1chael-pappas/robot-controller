var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// --- In-memory state ---

var commandSets = new List<CommandSet>
{
    new CommandSet(
        comment: "Basic command set",
        executionMode: "BestEffort",
        commands: new List<RobotCommandRecord>
        {
            new RobotCommandRecord { Name = "PLACE", X = 0, Y = 0, Direction = "North" },
            new RobotCommandRecord { Name = "MOVE" },
            new RobotCommandRecord { Name = "RIGHT" },
            new RobotCommandRecord { Name = "MOVE" },
            new RobotCommandRecord { Name = "REPORT" }
        })
    { Id = 1 },

    new CommandSet(
        comment: "MOVE with NumberOfSteps=2 (expands into 2 MoveCommands — BestEffort)",
        executionMode: "BestEffort",
        commands: new List<RobotCommandRecord>
        {
            new RobotCommandRecord { Name = "PLACE", X = 0, Y = 0, Direction = "North" },
            new RobotCommandRecord { Name = "MOVE", NumberOfSteps = 2 },
            new RobotCommandRecord { Name = "REPORT" }
        })
    { Id = 2 },

    new CommandSet(
        comment: "MOVE NumberOfSteps=5 from Y=8 (second step should fail, BestEffort leaves robot partway)",
        executionMode: "BestEffort",
        commands: new List<RobotCommandRecord>
        {
            new RobotCommandRecord { Name = "PLACE", X = 0, Y = 8, Direction = "North" },
            new RobotCommandRecord { Name = "MOVE", NumberOfSteps = 5 },
            new RobotCommandRecord { Name = "REPORT" }
        })
    { Id = 3 },

    new CommandSet(
        comment: "JUMP_FORWARD 2 — atomic single-command semantics",
        executionMode: "BestEffort",
        commands: new List<RobotCommandRecord>
        {
            new RobotCommandRecord { Name = "PLACE", X = 0, Y = 0, Direction = "North" },
            new RobotCommandRecord { Name = "JUMP_FORWARD", NumberOfSteps = 2 },
            new RobotCommandRecord { Name = "REPORT" }
        })
    { Id = 4 },

    new CommandSet(
        comment: "JUMP_FORWARD 5 from Y=8 — off-map, should fail atomically, robot stays at 0,8",
        executionMode: "BestEffort",
        commands: new List<RobotCommandRecord>
        {
            new RobotCommandRecord { Name = "PLACE", X = 0, Y = 8, Direction = "North" },
            new RobotCommandRecord { Name = "JUMP_FORWARD", NumberOfSteps = 5 },
            new RobotCommandRecord { Name = "REPORT" }
        })
    { Id = 5 },

    new CommandSet(
        comment: "AllOrNothing from Y=8 — dry-run detects 3rd MOVE fails, real robot stays at 0,8",
        executionMode: "AllOrNothing",
        commands: new List<RobotCommandRecord>
        {
            new RobotCommandRecord { Name = "PLACE", X = 0, Y = 8, Direction = "North" },
            new RobotCommandRecord { Name = "MOVE" },
            new RobotCommandRecord { Name = "MOVE" },
            new RobotCommandRecord { Name = "MOVE" },
            new RobotCommandRecord { Name = "REPORT" }
        })
    { Id = 6 },

    new CommandSet(
        comment: "AllOrNothing that succeeds — committed as one unit",
        executionMode: "AllOrNothing",
        commands: new List<RobotCommandRecord>
        {
            new RobotCommandRecord { Name = "PLACE", X = 2, Y = 2, Direction = "North" },
            new RobotCommandRecord { Name = "MOVE" },
            new RobotCommandRecord { Name = "RIGHT" },
            new RobotCommandRecord { Name = "MOVE" },
            new RobotCommandRecord { Name = "REPORT" }
        })
    { Id = 7 }
};

var nextId = 8;

// --- Command-set CRUD ---

app.MapGet("/", () => "Hello, Robot API!");

app.MapGet("/command-sets", () => Results.Ok(commandSets));

app.MapGet("/command-sets/{id:int}", (int id) =>
{
    var set = commandSets.FirstOrDefault(c => c.Id == id);
    return set is not null ? Results.Ok(set) : Results.NotFound();
});

app.MapPost("/command-sets", (CommandSet incoming) =>
{
    var created = incoming with { Id = nextId++ };
    commandSets.Add(created);
    return Results.Created($"/command-sets/{created.Id}", created);
});

app.MapPut("/command-sets/{id:int}", (int id, CommandSet updated) =>
{
    var index = commandSets.FindIndex(c => c.Id == id);
    if (index == -1) return Results.NotFound();
    commandSets[index] = updated with { Id = id };
    return Results.NoContent();
});

app.MapDelete("/command-sets/{id:int}", (int id) =>
{
    var index = commandSets.FindIndex(c => c.Id == id);
    if (index == -1) return Results.NotFound();
    commandSets.RemoveAt(index);
    return Results.NoContent();
});

app.Run();

// --- Data contracts ---

public record CommandSet
{
    public int Id { get; set; }
    public string Comment { get; set; }
    public string? ExecutionMode { get; set; }
    public List<RobotCommandRecord> Commands { get; set; }

    public CommandSet(string comment, string? executionMode, List<RobotCommandRecord> commands)
    {
        Id = 0;
        Comment = comment;
        ExecutionMode = executionMode;
        Commands = commands ?? new List<RobotCommandRecord>();
    }
}

public record RobotCommandRecord
{
    public string Name { get; set; } = string.Empty;
    public int? NumberOfSteps { get; set; }
    public int? X { get; set; }
    public int? Y { get; set; }
    public string? Direction { get; set; }
    public string? Comment { get; set; }
}
