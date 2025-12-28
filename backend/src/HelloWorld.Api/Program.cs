using HelloWorld.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure secrets
builder.Configuration.AddUserSecrets<Program>();
builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection("OpenAI"));

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure OpenAI and file upload settings
builder.Services.Configure<OpenAISettings>(
    builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<FileUploadSettings>(
    builder.Configuration.GetSection("FileUpload"));

// Register services
builder.Services.AddScoped<HelloWorld.Api.Services.IOpenAIService, HelloWorld.Api.Services.OpenAIService>();

// Configure CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://frontend:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Development");
}

app.UseAuthorization();
app.MapControllers();

app.Run();
