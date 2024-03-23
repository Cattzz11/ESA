using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using PROJETOESA.Data;
using PROJETOESA.Models;
using PROJETOESA.Services;
using Square;
using PROJETOESA.Services.FlightService;
using PROJETOESA.Services.SkyscannerService;
using PROJETOESA.Services.DataService;
using PROJETOESA.Services.CodeGeneratorService;
using PROJETOESA.Services.AeroDataBoxService;
using System.Configuration;
using PROJETOESA.Services.EmailService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AeroHelperContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AeroHelperContext") ?? throw new InvalidOperationException("Connection string 'AeroHelperContext' not found.")));

builder.Services.AddHttpClient("SkyscannerAPI", client =>
{
    client.BaseAddress = new Uri("https://sky-scanner3.p.rapidapi.com/get-config");
    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", "e1615ff456msh56f2dd1e1017e8dp1527a2jsn8e9f84f026aa");
    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "sky-scanner3.p.rapidapi.com");
});

builder.Services.AddHttpClient("EasyPayAPI", client =>
{
    client.BaseAddress = new Uri("https://api.test.easypay.pt/2.0");
    client.DefaultRequestHeaders.Add("AccountId", "0507c24e-1222-433a-9c2c-ae578391eca7");
    client.DefaultRequestHeaders.Add("ApiKey", "05d2b60e-ee43-46b4-bbf7-7046064af99b");
});

builder.Services.AddHttpClient("CountriesAPI", client =>
{
    client.BaseAddress = new Uri("https://restcountries.com/");

});

builder.Services.AddHttpClient("AeroDataBoxClient", client =>
{
    client.BaseAddress = new Uri("https://aerodatabox.p.rapidapi.com/");
    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", "e1615ff456msh56f2dd1e1017e8dp1527a2jsn8e9f84f026aa");
    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "aerodatabox.p.rapidapi.com");
});

builder.Services.AddHttpClient("FlighteraFlight", client =>
{
    client.BaseAddress = new Uri("https://flightera-flight-data.p.rapidapi.com/");
    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", "e1615ff456msh56f2dd1e1017e8dp1527a2jsn8e9f84f026aa");
    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "flightera-flight-data.p.rapidapi.com");
});

builder.Services.Configure<MailjetSettings>(builder.Configuration.GetSection("MailjetSettings"));

builder.Services.AddTransient<IEmailService, MailjetEmailSender>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin", builder => {
        builder.WithOrigins("https://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<IFlightService, FlightService>();
builder.Services.AddScoped<ISkyscannerService, SkyscannerService>();
builder.Services.AddScoped<IDataService, DataService>();
builder.Services.AddScoped<ICodeGeneratorService, CodeGeneratorService>();
builder.Services.AddScoped<IAeroDataBoxService, AeroDataBoxService>();

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<AeroHelperContext>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<CodeGeneratorService>();

var app = builder.Build();

app.MapIdentityApi<ApplicationUser>();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("AllowOrigin");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
