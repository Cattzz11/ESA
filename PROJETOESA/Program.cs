﻿using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using PROJETOESA.Data;
using PROJETOESA.Models;
using PROJETOESA.Services;
using Square;
using Square.Http.Client;
using Square.Apis;
using Square.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AeroHelperContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AeroHelperContext") ?? throw new InvalidOperationException("Connection string 'AeroHelperContext' not found.")));

builder.Services.AddHttpClient("SkyscannerAPI", client =>
{
    client.BaseAddress = new Uri("https://sky-scanner3.p.rapidapi.com/get-config");
    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", "4beef29efdmshcaf7d7db7c061a7p14d35bjsna36a2627a624");
    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "sky-scanner3.p.rapidapi.com");
});

builder.Services.AddHttpClient("CountriesAPI", client =>
{
    client.BaseAddress = new Uri("https://restcountries.com/");

});


string accessToken = "EAAAl7w8P9qlJgAIDzJYhYIN3XivD0gDTpSRreKD2nLgYIVqdOJDwy8DpvL_-kYU";
SquareClient squareClient = new SquareClient.Builder()
    .Environment(Square.Environment.Sandbox)
    .AccessToken(accessToken)
    .Build();


builder.Services.AddSingleton(squareClient);


/*builder.Services.AddHttpClient("SquareAPI", client =>
{
    client.BaseAddress = new Uri("https://squareecommerceserg-osipchukv1.p.rapidapi.com/get-config");
    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", "5aeddf19cdmsh124dd08fde357b5p1533bfjsn58b2fe37abe3");
    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "SquareECommerceserg-osipchukV1.p.rapidapi.com");
});*/

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<SkyscannerService>();
builder.Services.AddScoped<DataService>();

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<AeroHelperContext>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adiciona as configurações do EmailSettings a partir do appsettings.json
builder.Services.Configure<EmailSettings>(
builder.Configuration.GetSection("EmailSender"));

// Adiciona o serviço de envio de e-mails
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddScoped<CodeGeneratorService>();

var app = builder.Build();

app.MapIdentityApi<ApplicationUser>();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
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
