using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlazorDemoCRUD.Server.Utility;
using BlazorDemoCRUD.Service;
using Newtonsoft.Json;
using System.Text;

namespace BlazorDemoCRUD.Server.Controllers
{
    public class AccountController : Controller
    {
        private AccountService _accountService;
        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        [Route("api/v1/account/email")]
        [Authorize]
        async public Task<IActionResult> ChangeAccountEmail([FromBody] Common.ChangeEmail model)
        {
            string userId = User.GetUserId();

            var result = await _accountService.ChangeAccountEmail(userId, model);

            if (result.Success == false)
                return BadRequest(result.Message);

            return Ok();
        }

        [HttpPost]
        [Route("api/v1/account/password")]
        [Authorize]
        async public Task<IActionResult> ChangeAccountPassword([FromBody] Common.ChangePassword model)
        {
            string userId = User.GetUserId();

            var result = await _accountService.ChangeAccountPassword(userId, model);

            if (result.Success == false)
                return BadRequest(result.Message);

            return Ok();
        }

        [HttpDelete]
        [Route("api/v1/account")]
        [Authorize]
        async public Task<IActionResult> DeleteAccount()
        {
            string userId = User.GetUserId();

            var result = await _accountService.DeleteAccount(userId);

            if (result.Success == false)
                return BadRequest(result.Message);

            return Ok();
        }

    }//End Controller
}
