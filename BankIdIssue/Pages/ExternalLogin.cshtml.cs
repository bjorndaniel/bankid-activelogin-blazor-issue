using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BankIdIssue.Pages;
public class ExternalLoginModel :PageModel
{
    private readonly IHttpContextAccessor _accessor;

    public ExternalLoginModel(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

	public IActionResult OnGet(string provider)
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = $"/externallogincallback",
            Items =
    {
        {"returnUrl", "~/"},
        {"scheme", provider}
    }
        };

        return Challenge(props, provider);
    }
}

