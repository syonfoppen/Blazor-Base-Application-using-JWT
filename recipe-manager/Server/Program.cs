global using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer  {token}\"",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddScoped<IAuthRepository, AuthRepository>();

builder.Services.AddSingleton<RsaSecurityKey>(provider => {
    // It's required to register the RSA key with depedency injection.
    // If you don't do this, the RSA instance will be prematurely disposed.

    RSA rsa = RSA.Create();
    rsa.ImportRSAPublicKey(
        source: Convert.FromBase64String(builder.Configuration.GetSection("JWT:PublicKey").Value),
        bytesRead: out int _
    );

    return new RsaSecurityKey(rsa);
});


builder.Services.AddAuthentication("Asymmetric")
    .AddJwtBearer("Asymmetric", options =>
    {
        SecurityKey rsa = builder.Services.BuildServiceProvider().GetRequiredService<RsaSecurityKey>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = rsa,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            ValidIssuer = builder.Configuration.GetSection("JWT:Issuer").Value,
            ValidAudience = builder.Configuration.GetSection("JWT:Audience").Value
        };

    });

builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("AdminsOnly", policy => policy.RequireClaim("Admin"));
});
var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}



app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blazor API V1");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
