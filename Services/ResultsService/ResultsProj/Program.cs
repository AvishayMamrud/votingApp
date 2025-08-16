using Microsoft.EntityFrameworkCore;
using Application.Interfaces;
using Business.Logic;
using Infrastructure;
using DAL.DbContext;
using Data;
using Infrastructure.Messaging;
using Application.Messaging;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ------------------- Database -------------------
builder.Services.AddDbContext<ResultsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ResultsDb")));

// ------------------- Logic Layer -------------------
builder.Services.AddScoped<ResultsLogic>();
builder.Services.AddScoped<IResultsUpdateHandler>(sp => sp.GetRequiredService<ResultsLogic>());
builder.Services.AddScoped<ISubscriptionService>(sp => sp.GetRequiredService<ResultsLogic>());

// ------------------- DAL / Repository -------------------
builder.Services.AddScoped<IDataAccess, DataAccess>();

// ------------------- Gateway Publisher -------------------
builder.Services.AddScoped<IGatewayPushPublisher, AwsSqsGatewayPushPublisher>();

// ------------------- Live Updates -------------------
builder.Services.AddSingleton<ILiveUpdatesManager, RedisLiveUpdatesManager>();

// ------------------- Listeners -------------------
builder.Services.AddScoped<ISurveyResultsListener, SurveyResultsListener>();
builder.Services.AddScoped<IVoteResultsListener, VoteResultsListener>();

// ------------------- Hosted Background Service -------------------
builder.Services.AddHostedService<LiveUpdatesBackgroundService>();

// ------------------- Redis -------------------
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis")!);
    return ConnectionMultiplexer.Connect(config);
});

var app = builder.Build();

// ------------------- Map endpoints -------------------
app.MapControllers();

app.Run();
