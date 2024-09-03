var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseHttpsRedirection();
app.UseMiddleware<DecompressionMiddleware>();

app.MapPost("/api/demo", async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body, System.Text.Encoding.UTF8);
    var text = await reader.ReadToEndAsync();
    Console.WriteLine(text);
    return Results.Ok(text);
});

app.Run();
