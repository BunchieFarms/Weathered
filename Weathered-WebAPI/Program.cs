using GoogleApi.Extensions;
using Weathered_WebAPI.Business;
using Weathered_Lib.Mongo;
using Weathered_Lib.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<WeatheredBusiness>();

builder.Services.AddGoogleApiClients();

string AllowLocalCors = "AllowLocal";
string corsOrigin = "http://weathered.bryceohmer.com";
#if DEBUG
corsOrigin = "http://localhost:5291";
#endif

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowLocalCors,
                      policy =>
                      {
                          policy.WithOrigins(corsOrigin)
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

MongoBase.SetupMongoBase(builder.Configuration["WeatheredDB"]);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapPost("api/getWeathered", async (WeatheredRequest req, WeatheredBusiness _wb) =>
{
    return await _wb.GetWeatheredResponse(req);
});

app.MapPost("api/getHistoricalForecast", async (PirateWeatheredRequest req, WeatheredBusiness _wb) =>
{
    return await _wb.GetHistoricalForecast(req);
});

app.UseCors(AllowLocalCors);

app.Run();
