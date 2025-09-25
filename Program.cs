using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Connection strings
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")!;

// console log to verify
Console.WriteLine($"Using connection string: {connStr}");

// Make it available via DI
builder.Services.AddSingleton(new Db.Sql(connStr));

// Minimal API + Swagger for quick testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.MapGet("/_health", () => Results.Ok(new { ok = true, time = DateTime.UtcNow }));

// Map endpoints from our 2 CRUD modules
Address.AddressEndpoints.Map(app);

app.Run();
