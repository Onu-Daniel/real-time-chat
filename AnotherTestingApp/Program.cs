using AnotherTestingApp.Hub;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IDictionary<string, UserGroupConnection>>(opt => new Dictionary<string, UserGroupConnection>());
builder.Services.AddCors(options =>
{
    // Configure CORS policies
    options.AddDefaultPolicy(builder =>
    {
        // Set the allowed origins, headers, methods, and credentials
        builder.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors();
app.UseEndpoints(endpoints =>
{
    // Map the ChatHub to the "/chat" endpoint
    endpoints.MapHub<ChatHub>("/chat");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
