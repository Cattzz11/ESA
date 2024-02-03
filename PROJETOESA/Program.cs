using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PROJETOESA.Data;
using PROJETOESA.Models;

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

builder.Services.AddTransient<IEmailSender, EmailSender>(i =>
  new EmailSender(
      builder.Configuration["EmailSender:Host"],
      builder.Configuration.GetValue<int>("EmailSender:Port"),
      builder.Configuration.GetValue<bool>("EmailSender:EnableSSL"),
      builder.Configuration["EmailSender:UserName"],
      builder.Configuration["EmailSender:Password"]
  )
);

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
