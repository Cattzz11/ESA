using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PROJETOESA.Data;
using PROJETOESA.Models;
using PROJETOESA.Services;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PeopleAngularServerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PeopleAngularServerContext") ?? throw new InvalidOperationException("Connection string 'PeopleAngularServerContext' not found.")));

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<PeopleAngularServerContext>();

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
