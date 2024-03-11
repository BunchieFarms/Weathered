using GoogleApi.Extensions;
using Weathered_WebAPI.Business;
using Weathered_Lib.Mongo;
using Weathered_WebAPI.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<WeatheredBusiness>();

builder.Services.AddGoogleApiClients();

MongoBase.SetupMongoBase(builder.Configuration["WeatheredDB"]);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapPost("api/getWeathered", async (WeatheredRequest req, WeatheredBusiness _wb) =>
{
    return await _wb.GetWeatheredResponse(req);
});

app.Run();
