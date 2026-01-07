using Abc.BusinessService;
using Abc.Infrastructure;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApp.Middlewares;
using WebApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.RegisterRepositories(builder.Configuration.GetConnectionString("sql"));
builder.Services.RegisterBusinessService();

builder.Services.AddTransient<IUserInfoService, UserInfoService>();
builder.Services.AddSingleton<IJwtAuthentication, JWTAuthentication>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtConfig:issuer"],
        ValidAudience = builder.Configuration["JwtConfig:audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:key"]))
    };

      JwtBearerEvents events = new JwtBearerEvents();

    events.OnMessageReceived =async (context) => {
        var cToken = context.Request.Headers.Authorization.FirstOrDefault();

        if(!string.IsNullOrEmpty(cToken)){
            var bearer = cToken.Split(' ');
            
            if(bearer.Length==1)
            return;

            var token = bearer[1];
            var tokenService = builder.Services.BuildServiceProvider().GetService<ITokenService>();

            if(tokenService!=null){
                var tokenData = await tokenService.GetActualToken(token);
                if(tokenData!=null){
                    context.Request.Headers.Authorization = "Bearer "+tokenData.ActualToken;
                }
            }
        }
    };
    
    // uncomment the line if you want to validate token in this event or use middleware
    //options.Events = events;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{    
    app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = new CustomExceptionHandler().Invoke });
}

app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();

//Add User session
app.UseSession();

/* Handled this in Middleware code so commented out below line */
//Add token to all incoming HTTP Request Header
//app.Use(async (context, next) =>
//{
//    var token = context.Session.GetString("jwt");
//    if (!string.IsNullOrEmpty(token))
//    {
//        context.Request.Headers.Add("Authorization", "Bearer " + token);
//    }
//    await next();
//});

app.UseMiddleware<JwtMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
