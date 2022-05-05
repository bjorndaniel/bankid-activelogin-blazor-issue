using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace BankIdIssue.Pages;
public class ExternalLoginCallbackModel : PageModel
{
    private readonly IHttpContextAccessor _accessor;
    private readonly ILogger<ExternalLoginCallbackModel> _logger;

    public ExternalLoginCallbackModel(IHttpContextAccessor accessor, ILogger<ExternalLoginCallbackModel> logger)
    {
        _accessor = accessor;
        _logger = logger;
    }

    public async Task<IActionResult> OnGet()
    {
        var result = await _accessor.HttpContext!.AuthenticateAsync();
        if (result?.Succeeded != true)
        {
            throw new Exception("External authentication error");
        }
        if (_accessor.HttpContext?.User?.Identity is ClaimsIdentity identity)
        {
            var extra = identity.Claims.FirstOrDefault(_ => _.Type == "ExtraClaim")?.Value;
            Console.WriteLine($"ExtraClaim: {extra}");
        }
        return Redirect("~/info");
    }
}