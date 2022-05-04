using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BankIdIssue.Pages;
public class LogoutModel : PageModel
{
    private readonly IHttpContextAccessor _accessor;

    public LogoutModel(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }
    public async Task<IActionResult> OnGet()
    {
        await _accessor.HttpContext!.SignOutAsync();
        var request = _accessor.HttpContext!.Request;
        var _baseURL = $"{request.Scheme}://{request.Host}/";
        return Redirect(_baseURL);
    }
}

