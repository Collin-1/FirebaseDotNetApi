using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// CORS for frontend to call the API from another port
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5500") // frontend server origin
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Initialize Firebase Admin SDK (server-side)
// This uses service account key file
FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile("serviceAccountKey.json")
});

// ✅ Configure authentication (we will validate Firebase JWTs)
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // 🔥 Replace with your Firebase project id
        var firebaseProjectId = "fir-dotnetapi-f54a7";

        // Firebase issues tokens with these values:
        // issuer: https://securetoken.google.com/<projectId>
        // audience: <projectId>
        options.Authority = $"https://securetoken.google.com/{firebaseProjectId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{firebaseProjectId}",

            ValidateAudience = true,
            ValidAudience = firebaseProjectId,

            ValidateLifetime = true
        };

        // Optional: map claims in a nicer way
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                // Firebase puts the user id in the "user_id" claim sometimes,
                // and also sets "sub". We'll normalize to NameIdentifier.
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                var uid = context.Principal?.FindFirst("user_id")?.Value
                          ?? context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? context.Principal?.FindFirst("sub")?.Value;

                if (uid != null && claimsIdentity != null)
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, uid));
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("AllowLocalFrontend");

// ✅ IMPORTANT: auth must be before controllers
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
