var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// --- In-memory state ---

var commandSets = new List<CommandSet>
{
    new CommandSet(
        comment: "Basic command set",
        schemaVersion: "1.0",
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
        schemaVersion: "1.0",
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
        schemaVersion: "1.0",
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
        schemaVersion: "1.0",
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
        schemaVersion: "1.0",
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
        schemaVersion: "1.0",
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
        schemaVersion: "1.0",
        executionMode: "AllOrNothing",
        commands: new List<RobotCommandRecord>
        {
            new RobotCommandRecord { Name = "PLACE", X = 2, Y = 2, Direction = "North" },
            new RobotCommandRecord { Name = "MOVE" },
            new RobotCommandRecord { Name = "RIGHT" },
            new RobotCommandRecord { Name = "MOVE" },
            new RobotCommandRecord { Name = "REPORT" }
        })
    { Id = 7 },

    new CommandSet(
        comment: "HD demo — rollback-friendly AllOrNothing workflow: PLACE 2,2,N → MOVE → RIGHT → MOVE",
        schemaVersion: "2.0",
        executionMode: "AllOrNothing",
        commands: new List<RobotCommandRecord>
        {
            new RobotCommandRecord { Name = "PLACE", X = 2, Y = 2, Direction = "North" },
            new RobotCommandRecord { Name = "MOVE" },
            new RobotCommandRecord { Name = "RIGHT" },
            new RobotCommandRecord { Name = "MOVE" },
            new RobotCommandRecord { Name = "REPORT" }
        })
    { Id = 8 }
};

var nextId = 9;

// Execution logs keyed by workflow id (= CommandSet.Id the client executed).
var executionLogs = new Dictionary<int, CommandExecutionLog>();

// Rollback sets keyed by source workflow id. Persisted on first POST; subsequent GETs return the same set.
var rollbackSets = new Dictionary<int, CommandSet>();

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

// --- Execution log (HD) ---

app.MapPost("/command-executions", (CommandExecutionLog log) =>
{
    executionLogs[log.WorkflowId] = log;
    return Results.Created($"/command-executions/{log.WorkflowId}", log);
});

app.MapGet("/command-executions/{workflowId:int}", (int workflowId) =>
{
    return executionLogs.TryGetValue(workflowId, out var log)
        ? Results.Ok(log)
        : Results.NotFound();
});

// --- Rollback generation (HD) ---
// Algorithm:
//   1. Fetch the execution log for the source workflow id.
//   2. Keep only commands whose Success == true (you can't undo something that never ran).
//   3. Reverse the list — rollback executes in reverse time order.
//   4. Trim at the first PLACE encountered in the reversed list (exclusive). The PLACE is
//      where the workflow started; rollback rewinds *to* that point, not past it.
//   5. Map each remaining command to its antipode:
//          MOVE  → STEP_BACK
//          STEP_BACK → MOVE
//          LEFT  → RIGHT
//          RIGHT → LEFT
//          JUMP_FORWARD n → JUMP_BACKWARD n
//          JUMP_BACKWARD n → JUMP_FORWARD n
//          REPORT → dropped
//   6. Wrap as new CommandSet with SchemaVersion "2.0" + ExecutionMode "AllOrNothing".

app.MapPost("/command-sets/{id:int}/rollback", (int id) =>
{
    if (!executionLogs.TryGetValue(id, out var log))
        return Results.NotFound();

    var rollback = BuildRollbackCommands(log);
    var set = new CommandSet(
        comment: $"Rollback for workflow {id}",
        schemaVersion: "2.0",
        executionMode: "AllOrNothing",
        commands: rollback)
    { Id = nextId++ };

    rollbackSets[id] = set;
    return Results.Created($"/command-sets/{id}/rollback", set);
});

app.MapGet("/command-sets/{id:int}/rollback", (int id) =>
{
    return rollbackSets.TryGetValue(id, out var set)
        ? Results.Ok(set)
        : Results.NotFound();
});

app.Run();

static List<RobotCommandRecord> BuildRollbackCommands(CommandExecutionLog log)
{
    var result = new List<RobotCommandRecord>();
    var successes = log.Commands.Where(c => c.Success).Reverse().ToList();

    foreach (var entry in successes)
    {
        var name = entry.Name?.ToUpperInvariant();
        if (name == "PLACE") break;

        switch (name)
        {
            case "MOVE":
                result.Add(new RobotCommandRecord { Name = "STEP_BACK" });
                break;
            case "STEP_BACK":
                result.Add(new RobotCommandRecord { Name = "MOVE" });
                break;
            case "LEFT":
                result.Add(new RobotCommandRecord { Name = "RIGHT" });
                break;
            case "RIGHT":
                result.Add(new RobotCommandRecord { Name = "LEFT" });
                break;
            case "JUMP_FORWARD":
                result.Add(new RobotCommandRecord { Name = "JUMP_BACKWARD", NumberOfSteps = entry.NumberOfSteps });
                break;
            case "JUMP_BACKWARD":
                result.Add(new RobotCommandRecord { Name = "JUMP_FORWARD", NumberOfSteps = entry.NumberOfSteps });
                break;
            case "REPORT":
                break;
        }
    }

    return result;
}

// --- Data contracts ---

public record CommandSet
{
    public int Id { get; set; }
    public string Comment { get; set; }
    public string? SchemaVersion { get; set; }
    public string? ExecutionMode { get; set; }
    public List<RobotCommandRecord> Commands { get; set; }

    public CommandSet(string comment, string? schemaVersion, string? executionMode, List<RobotCommandRecord> commands)
    {
        Id = 0;
        Comment = comment;
        SchemaVersion = schemaVersion;
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

public record ExecutedCommandRecord
{
    public string Name { get; set; } = string.Empty;
    public bool Executed { get; set; }
    public bool Success { get; set; }
    public int? NumberOfSteps { get; set; }
    public int? X { get; set; }
    public int? Y { get; set; }
    public string? Direction { get; set; }
    public string? Comment { get; set; }
}

public record CommandExecutionLog
{
    public int WorkflowId { get; set; }
    public string? SchemaVersion { get; set; }
    public List<ExecutedCommandRecord> Commands { get; set; } = new();
}
