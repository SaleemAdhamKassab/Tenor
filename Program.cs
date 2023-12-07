using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Tenor.ActionFilters;
using Tenor.Data;
using Tenor.Helper;
using Tenor.Services;
using Tenor.Services.AuthServives;
using Tenor.Services.CountersService;
using Tenor.Services.DevicesService;
using Tenor.Services.KpisService;
using Tenor.Services.SubsetsService;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(setup =>
{
    
    // Include 'SecurityScheme' to use JWT Authentication
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });

    setup.OperationFilter<SwaggerTenantParam>();

});
//Add Windows auth-----------------------
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
//----------------------------------------
builder.Services.AddHttpContextAccessor();
//-------------------------------------
builder.Services.AddDbContext<TenorDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RemoteConnection")));
// add services

builder.Services.AddScoped<IDevicesService, DevicesService>();
builder.Services.AddScoped<ISubsetsService, SubsetsService>();
builder.Services.AddScoped<ICountersService, CountersService>();
builder.Services.AddScoped<IKpisService, KpisService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IWindowsAuthService, WindowsAuthService>();
//-------------
builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
