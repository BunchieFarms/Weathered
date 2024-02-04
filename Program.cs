using Blazored.LocalStorage;
using GoogleApi.Extensions;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Weathered.Data;
using Weathered.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<WeatheredService>();

builder.Services.AddMudServices();

var dbConn = builder.Configuration["WeatheredDB"];
builder.Services.AddDbContext<WeatheredContext>(options => options.UseNpgsql(dbConn));
builder.Services.AddScoped<WeatheredContext>();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddGoogleApiClients();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
