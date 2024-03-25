using Microsoft.EntityFrameworkCore;
using PROJETOESA.Data;
using PROJETOESA.Models;
using PROJETOESA.Services;
using Square;
using Square.Http.Client;
using Square.Apis;
using Square.Models;
using Microsoft.Extensions.Localization;
using PROJETOESA.Controllers;
using Microsoft.Extensions.DependencyInjection;
using PROJETOESA.Services.AeroDataBoxService;
using PROJETOESA.Services.CodeGeneratorService;
using PROJETOESA.Services.DataService;
using PROJETOESA.Services.EmailService;
using PROJETOESA.Services.FlightService;
using PROJETOESA.Services.SkyscannerService;
using Square.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<StatisticsService>();

builder.Services.AddDbContext<AeroHelperContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AeroHelperContext") ?? throw new InvalidOperationException("Connection string 'AeroHelperContext' not found.")));

builder.Services.AddHttpClient("SkyscannerAPI", client =>
{
    client.BaseAddress = new Uri("https://sky-scanner3.p.rapidapi.com/get-config");
    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", "e1615ff456msh56f2dd1e1017e8dp1527a2jsn8e9f84f026aa");
    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "sky-scanner3.p.rapidapi.com");
});


builder.Services.AddHttpClient("CountriesAPI", client =>
{
    client.BaseAddress = new Uri("https://restcountries.com/");

});

string accessToken = "EAAAl7w8P9qlJgAIDzJYhYIN3XivD0gDTpSRreKD2nLgYIVqdOJDwy8DpvL_-kYU";

BearerAuthModel bearerAuthModel = new BearerAuthModel.Builder(accessToken)
    .Build();

SquareClient squareClient = new SquareClient.Builder()
    .Environment(Square.Environment.Sandbox)
    .BearerAuthCredentials(bearerAuthModel)
    .Build();


builder.Services.AddSingleton(squareClient);
builder.Services.AddScoped<SquareService>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddHttpClient<FlightService>();

builder.Services.AddControllersWithViews()
    .AddDataAnnotationsLocalization();

builder.Services.AddTransient<IStringLocalizer<PaymentController>, StringLocalizer<PaymentController>>();


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

// Adiciona as configurações do EmailSettings a partir do appsettings.json
//builder.Services.Configure<EmailSettings>(
//builder.Configuration.GetSection("EmailSender"));

// Adiciona o serviço de envio de e-mails
//builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddScoped<ICodeGeneratorService, CodeGeneratorService>();


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
