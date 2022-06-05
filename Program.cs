using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using RazorPages_OIDC.Models.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// OIDC Service Adding
#region OIDC
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "cookie";
    options.DefaultChallengeScheme = "oidc";
})
    .AddCookie("cookie", options =>
    {
        options.Cookie.Name = "rzpcookie";

        options.Events.OnSigningOut = async e =>
        {
            await e.HttpContext.RevokeUserRefreshTokenAsync();
        };
    })
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = "https://demo.duendesoftware.com";

        options.ClientId = "interactive.confidential.short";
        options.ClientSecret = "secret";

        // code flow + PKCE (PKCE is turned on by default)
        options.ResponseType = "code";
        options.UsePkce = true;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("offline_access");
        options.Scope.Add("api");

        // not mapped by default
        options.ClaimActions.MapJsonKey("website", "website");

        // keeps id_token smaller
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

//builder.Services.AddAccessTokenManagement();
//builder.Services.AddClientAccessTokenManagement();

// registers HTTP client that uses the managed user access token
builder.Services.AddUserAccessTokenHttpClient("user_client", configureClient: client =>
{
    client.BaseAddress = new Uri("https://demo.duendesoftware.com/api/");
});

// registers HTTP client that uses the managed client access token
builder.Services.AddClientAccessTokenHttpClient("client", configureClient: client =>
{
    client.BaseAddress = new Uri("https://demo.duendesoftware.com/api/");
});

// registers a typed HTTP client with token management support
builder.Services.AddHttpClient<TypedUserClient>(client =>
{
    client.BaseAddress = new Uri("https://demo.duendesoftware.com/api/");
})
    .AddUserAccessTokenHandler()
    .AddClientAccessTokenHandler();

builder.Services.AddAccessTokenManagement(options =>
{
    // client config is inferred from OpenID Connect settings
    // if you want to specify scopes explicitly, do it here, otherwise the scope parameter will not be sent
    options.Client.DefaultClient.Scope = "api";
});



#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
    //.RequireAuthorization();

app.Run();

