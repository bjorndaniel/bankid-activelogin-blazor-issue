using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BankId.Pages;
public class ExternalLoginModel :PageModel
{
    private readonly IHttpContextAccessor _accessor;

    public ExternalLoginModel(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

	public IActionResult OnGet(string provider)
    {
        var request = _accessor.HttpContext!.Request;
        var baseURL = $"https://{request.Host}";
        var props = new AuthenticationProperties
        {
            RedirectUri = $"{baseURL}/externallogincallback",
            Items =
    {
        {"returnUrl", "~/"},
        {"scheme", provider}
    }
        };

        return Challenge(props, provider);
    }
}

