using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlazorTemplate.API.Utility;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using BlazorTemplate.Common;
using BlazorTemplate.Database;
using Microsoft.EntityFrameworkCore;

namespace BlazorTemplate.API.Controllers
{
    public class AccountController : Controller
    {
        private IConfiguration _configuration;
        private ApplicationDbContext _db;
        private UserManager<IdentityUser> _userManager;
        private SignInManager<IdentityUser> _signInManager;
        public AccountController(IConfiguration configuration, ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _configuration = configuration;
            _db = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Call this BEFORE allowing the change of email.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Route("api/v1/account/logout")]
        async public Task<IActionResult> AccountLogout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        /// <summary>
        /// Call this BEFORE allowing the change of email.
        /// WHY? Because the identity API doesn't check if an email is unique when updating user email with POST:manage/info.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("api/v1/account/exists")]
        [ProducesResponseType<bool>(StatusCodes.Status200OK)]
        async public Task<IActionResult> AccountExistsByEmail([FromBody]AccountEmail model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            return Ok(user != null);
        }

        [Authorize]
        [HttpDelete]
        [Route("api/v1/account")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        async public Task<IActionResult> DeleteAccount()
        {
            string userId = User.GetUserId();
            var customersOwnedByThisUser = _db.Customers.Where(x => x.OwnerId == userId);
            _db.Customers.RemoveRange(customersOwnedByThisUser);
            await _db.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(userId);
            await _userManager.DeleteAsync(user);
            return Ok();
        }

        /// <summary>
        /// Return True or False to let the App know if the API/Server will allow all operations. 
        /// For example, if the Server does not have a SendGrid API key then Password Recovery and Changing Email is 
        /// not allowed because the recovery and confirmation emails will never be sent.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/account/operations")]
        [ProducesResponseType<bool>(StatusCodes.Status200OK)]
        public IActionResult AccountAllowAllOperations()
        {
            var sendGridApi = _configuration.GetValue<string>("SendGridKey");
            var sendGridSystemEmailAddress = _configuration.GetValue<string>("SendGridSystemEmailAddress");

            var allowAllOperations = false;

            if(!string.IsNullOrEmpty(sendGridApi) && !string.IsNullOrEmpty(sendGridSystemEmailAddress))
                allowAllOperations = true;

            return Ok(allowAllOperations);
        }


        [HttpGet]
        [Route("api/v1/account/2fa")]
        [ProducesResponseType<bool>(StatusCodes.Status200OK)]
        public async Task<IActionResult> AccountTwoFactorEnabled()
        {
            string userId = User.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            return Ok(isTwoFactorEnabled);
        }
        
    }
}
