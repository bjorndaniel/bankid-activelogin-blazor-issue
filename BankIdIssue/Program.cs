using ActiveLogin.Authentication.BankId.AspNetCore;
using ActiveLogin.Authentication.BankId.AspNetCore.Launcher;
using BankIdIssue.Data;
using BankIdIssue.Helpers;
using BankIdIssue.Shared.Resources;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", false);
ConfigureBankId(builder);
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });
builder.Services.AddSingleton<WeatherForecastService>();

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
app.MapControllers();
app.MapDefaultControllerRoute();
app.MapBlazorHub();
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

app.Run();


static void ConfigureBankId(WebApplicationBuilder builder)
{
    builder.Services.AddTransient<IBankIdLauncher, CustomBankIdLauncher>();
    builder.Services
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie()
        .AddBankId(bankId =>
        {
            bankId
                .AddDebugEventListener();
            bankId.UseQrCoderQrCodeGenerator();
            bankId.UseUaParserDeviceDetection();
            bankId.AddSameDevice(BankIdDefaults.SameDeviceAuthenticationScheme, "BankID på den här enheten", options => { });

            var rootCertEncoded = CertificateResources.BankIdRootTestCert;
            var rootCertBytes = Convert.FromBase64String(rootCertEncoded);
            bankId.UseTestEnvironment()
                .UseRootCaCertificate(() =>
                    new X509Certificate2(rootCertBytes, (SecureString)null!, X509KeyStorageFlags.MachineKeySet)
                )
                .UseClientCertificate(() =>
                    new X509Certificate2(
                        builder.Configuration.GetValue<string>("ActiveLogin:BankId:ClientCertificate:FilePath"),
                        builder.Configuration.GetValue<string>("ActiveLogin:BankId:ClientCertificate:Password")
                    )
                );

            bankId.Configure(options =>
            {
                options.Events = new RemoteAuthenticationEvents
                {
                    OnTicketReceived = async context =>
                    {
                        if (context?.Principal?.Identity is ClaimsIdentity identity)
                        {
                            identity.AddClaim(new Claim("ExtraClaim", Guid.NewGuid().ToString()));
                        }
                    }
                };
            });
        });

    builder.Services.AddAuthorization();
}
