using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPages_OIDC.Models.Services;

namespace RazorPages_OIDC.Pages
{
    [Authorize]
    public class CallAPIModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public string ApiResponse { get; set; }

        public CallAPIModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult OnGetLogout()
        {
            SignOut("cookie", "oidc");
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnGetCallApiAsUser()
        {
            var httpClient = _httpClientFactory.CreateClient("user_client");
            var response = await httpClient.GetStringAsync("test");
            ApiResponse = response.ToString();

            return Page();
        }

        public async Task<IActionResult> OnGetCallApiAsUserTyped([FromServices] TypedUserClient client)
        {
            var response = await client.CallApi();
            ApiResponse = response.ToString();
            return Page();
        }
    }
}
