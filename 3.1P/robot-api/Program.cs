var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// --- In-memory state ---

var commandSets = new List<CommandSet>
{
    new CommandSet(
        comment: "Basic command set",
        commands: new List<RobotCommandRecord>
        {
            new RobotCommandRecord { Name = "PLACE", X = 0, Y = 0, Direction = "North" },
            new RobotCommandRecord { Name = "MOVE" },
            new RobotCommandRecord { Name = "RIGHT" },
            new RobotCommandRecord { Name = "MOVE" },
            new RobotCommandRecord { Name = "REPORT" }
        })
    { Id = 1 }
};

var nextId = 2;

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
    public List<RobotCommandRecord> Commands { get; set; }

    public CommandSet(string comment, List<RobotCommandRecord> commands)
    {
        Id = 0;
        Comment = comment;
        Commands = commands ?? new List<RobotCommandRecord>();
    }
}

public record RobotCommandRecord
{
    public string Name { get; set; } = string.Empty;
    public int? X { get; set; }
    public int? Y { get; set; }
    public string? Direction { get; set; }
    public string? Comment { get; set; }
}
