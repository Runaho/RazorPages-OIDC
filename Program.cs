using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RazorPages_OIDC.Models.Configuration;
using RazorPages_OIDC.Models.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

OpenId openIdConfig = builder.Configuration.GetSection("OpenIdLive").Get<OpenId>();

// OIDC Service Adding
#region OIDC
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "cookie";
    options.DefaultChallengeScheme = "oidc";
}).AddCookie("cookie", options =>
   {
       options.Cookie.Name = "rzpcookie";

       options.Events.OnSigningOut = async e =>
       {
           await e.HttpContext.RevokeUserRefreshTokenAsync();
       };
   })
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = openIdConfig.APIUrl;

        options.ClientId = openIdConfig.APIClientID;
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
        if (openIdConfig.CallBack != null)
            options.CallbackPath = openIdConfig.CallBack;

        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

// registers HTTP client that uses the managed user access token
builder.Services.AddUserAccessTokenHttpClient("user_client", configureClient: client =>
{
    client.BaseAddress = new Uri(openIdConfig.ClientRedirection);
});

// registers HTTP client that uses the managed client access token
builder.Services.AddClientAccessTokenHttpClient("client", configureClient: client =>
{
    client.BaseAddress = new Uri(openIdConfig.ClientRedirection);
});

// registers a typed HTTP client with token management support
builder.Services.AddHttpClient<TypedUserClient>(client =>
{
    client.BaseAddress = new Uri(openIdConfig.ClientRedirection);
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

