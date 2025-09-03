using System.Net.Http.Headers;
using TicketAI.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowed = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };

const string FrontendCors = "FrontendCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCors, policy =>
        policy
            .WithOrigins("http://localhost:3000")     
                                                      
            .AllowAnyHeader()
            .AllowAnyMethod()
   
    );
});



// HttpClient for OpenAI with auth header injection
builder.Services.AddHttpClient("openai", c =>
{
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
});

// DI for our service
builder.Services.AddScoped<ICategorySuggester, OpenAiSuggester>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(FrontendCors);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
