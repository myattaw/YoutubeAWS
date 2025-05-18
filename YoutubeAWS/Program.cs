using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var filePath = Path.Combine(AppContext.BaseDirectory, "youtube.txt");

app.MapPost("/playlist", async ([FromBody] string id) =>
{
    if (string.IsNullOrWhiteSpace(id))
    {
        return Results.BadRequest("Invalid ID.");
    }

    try
    {
        await File.AppendAllTextAsync(filePath, id.Trim() + Environment.NewLine);
        return Results.Ok("ID added.");
    }
    catch (Exception ex)
    {
        return Results.Problem("Failed to write ID. " + ex.Message);
    }
});

app.MapGet("/playlist", () =>
{
    if (!File.Exists(filePath))
        return Results.NotFound("No playlist available.");

    var lines = File.ReadAllLines(filePath)
        .Where(l => !string.IsNullOrWhiteSpace(l))
        .OrderBy(_ => Guid.NewGuid())
        .ToList();

    if (lines.Count == 0)
        return Results.NotFound("Playlist is empty.");

    var url = "https://www.youtube.com/watch_videos?video_ids=" + string.Join(",", lines);
    return Results.Ok(new { playlistUrl = url });
});

app.Run();