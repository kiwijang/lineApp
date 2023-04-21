using API.Data;
using API.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 註冊 DbContext Pool
builder.Services.AddDbContextFactory<DataContext>(options =>
{
    // 設定連接字串及其他選項
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));

    // 設定 DbContext Pool 相關選項
    options.EnableSensitiveDataLogging()
        .EnableDetailedErrors();
});

// 設定 CookieAuthentication 服務
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie("jwt", options =>
    {
        options.Cookie.Name = "jwt";
        options.Cookie.HttpOnly = true;
        options.SlidingExpiration = true;
        options.Cookie.Domain = "localhost";
    })
    .AddCookie("notify", options =>
    {
        options.Cookie.Name = "notify";
        options.Cookie.HttpOnly = true;
        options.SlidingExpiration = true;
        options.Cookie.Domain = "localhost";
    });

builder.Services.AddHttpClient();

builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IJWTService, JWTService>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors(builder =>
    builder.WithOrigins("http://localhost:3030")
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials()
);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 啟用身份驗證中間件
app.UseAuthentication();

app.MapControllers();

app.Run();
