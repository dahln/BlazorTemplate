using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlazorTemplate.API.Utility;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using BlazorTemplate.Common;
using BlazorTemplate.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.Data;
using System.Drawing;

namespace BlazorTemplate.API.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationDbContext _db;
        private UserManager<IdentityUser> _userManager;
        private SignInManager<IdentityUser> _signInManager;
        public AccountController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _db = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        [Route("api/v1/account/register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            // Validate model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault());
            }
            var settings = await _db.SystemSettings.FirstOrDefaultAsync();
            //If setting is DISABLED, do not allow registration
            if(settings.RegistrationEnabled == false)
                return BadRequest("Registration Disabled");
            
            //If setting is enabled, limit registration by email "Domain".
            //Example: Only allow users who's email address ends with a '@BlazorTemplateapp.com'
            if(string.IsNullOrEmpty(settings.EmailDomainRestriction) == false)
            {
                var validDomains = settings.EmailDomainRestriction.Split(",").ToList();
                for(int a = 0; a < validDomains.Count; a++)
                {
                    validDomains[a] = validDomains[a].Replace("@","").ToLower();
                }
                var registrationDomain = model.Email.ToLower().Split("@").LastOrDefault();
                if(validDomains.Contains(registrationDomain) == false)
                    return BadRequest($"Invalid domain. You must use an email ending in: {settings.EmailDomainRestriction}");
            }

            // Create a new ApplicationUser with the provided details
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
            };

            // Attempt to create the user
            var result = await _userManager.CreateAsync(user, model.Password);

            // Check if user creation was successful
            if (result.Succeeded)
            {
                var usersCount = await _db.Users.CountAsync();
                if(usersCount == 1)
                {
                    var identity = await _userManager.FindByEmailAsync(model.Email);
                    await _userManager.AddToRoleAsync(identity, "Administrator");
                }

                // var newUser = await _userManager.FindByEmailAsync(model.Email);
                // BlazorTemplate.Database.Note tutorialNote = new BlazorTemplate.Database.Note()
                // {
                //     OwnerId = newUser.Id,
                //     Title = "Getting Started",
                //     Content = "Welcome to BlazorTemplate!\n\nUse BlazorTemplate to keep notes. You can create note or lists. You can share your notes with other people (this comes in handy if you need a shared grocery list).\n\nThis video shows a quick demo on how to use BlazorTemplate:\n\nhttps://BlazorTemplate.org#HowTo",
                //     Color = "#39FF14",
                //     SortOrder = 2                 
                // };
                // _db.Notes.Add(tutorialNote);
                // await _db.SaveChangesAsync();

                // BlazorTemplate.Database.Note tutorialList = new BlazorTemplate.Database.Note()
                // {
                //     OwnerId = newUser.Id,
                //     Title = "Shopping List",
                //     ListEnabled = true,
                //     Content = "",  
                //     Color = "",
                //     SortOrder = 1                  
                // };
                // _db.Notes.Add(tutorialList);
                // await _db.SaveChangesAsync();

                // _db.NoteListItems.Add(new Database.NoteListItem() { NoteId = tutorialList.Id, SortOrder = 0, Content = "Bread" });
                // _db.NoteListItems.Add(new Database.NoteListItem() { NoteId = tutorialList.Id, SortOrder = 1, Content = "Milk" });
                // _db.NoteListItems.Add(new Database.NoteListItem() { NoteId = tutorialList.Id, SortOrder = 2, Content = "Eggs" });
                // await _db.SaveChangesAsync();

                return Ok();
            }

            // If there were errors, add them to the ModelState
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("errors", error.Description);
            }

            // Return a bad request with the errors
            return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault());
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
        async public Task<IActionResult> AccountExistsByEmail([FromBody]AccountEmail model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            return Ok(user != null);
        }

        [Authorize]
        [HttpDelete]
        [Route("api/v1/account")]
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
        public async Task<IActionResult> AccountAllowAllOperations()
        {
            var settings = await _db.SystemSettings.FirstOrDefaultAsync();
            if(settings == null)
                return BadRequest("Settings Missing");

            var sendGridApi =  settings.SendGridKey;
            var sendGridSystemEmailAddress = settings.SendGridSystemEmailAddress;

            var allowAllOperations = false;

            if(!string.IsNullOrEmpty(sendGridApi) && !string.IsNullOrEmpty(sendGridSystemEmailAddress))
                allowAllOperations = true;

            return Ok(allowAllOperations);
        }

        [HttpGet]
        [Route("api/v1/account/operations/registration")]
        public async Task<IActionResult> AccountAllowRegistrationOperations()
        {
            var settings = await _db.SystemSettings.FirstOrDefaultAsync();
            if(settings == null)
                return BadRequest("Settings Missing");

            return Ok(settings.RegistrationEnabled);
        }

        [Authorize]
        [HttpGet]
        [Route("api/v1/account/roles")]
        async public Task<IActionResult> GeCurrentUserRoles()
        {
            string userId = User.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
                return BadRequest();
            var userRoles = await _userManager.GetRolesAsync(user);
            return Ok(userRoles);
        }


        [HttpGet]
        [Route("api/v1/account/2fa")]
        public async Task<IActionResult> AccountTwoFactorEnabled()
        {
            string userId = User.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            return Ok(isTwoFactorEnabled);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        [Route("api/v1/user/{userId}")]
        async public Task<IActionResult> ToggleUserAdministratorRole(string userId)
        {
            string currentUserId = User.GetUserId();

            if(currentUserId == userId)
                return BadRequest("You cannot toggle your own administrative role");

            var user = await _userManager.FindByIdAsync(userId);
            var IsAdministrator = await _userManager.IsInRoleAsync(user, "Administrator");
            if(IsAdministrator)
                await _userManager.RemoveFromRoleAsync(user, "Administrator");
            else
                await _userManager.AddToRoleAsync(user, "Administrator");

            return Ok();
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete]
        [Route("api/v1/user/{userId}")]
        async public Task<IActionResult> DeleteUser(string userId)
        {
            string currentUserId = User.GetUserId();
            if(currentUserId == userId)
                return BadRequest("Cannot delete this account.");
                
            var user = await _userManager.FindByIdAsync(userId);
            if(user != null)
                await _userManager.DeleteAsync(user);

            return Ok();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [Route("api/v1/users")]
        async public Task<IActionResult> UserSearch([FromBody] Common.Search model)
        {
            string userId = User.GetUserId();

            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(model.FilterText))
            {
                query = query.Where(i => i.Email.ToLower().ToLower().Contains(model.FilterText.ToLower()));
            }

            if (model.SortBy == nameof(Common.User.Email))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.Email)
                            : query.OrderByDescending(c => c.Email);
            }
            else
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.Email)
                            : query.OrderByDescending(c => c.Email);
            }

            Common.SearchResponse<Common.User> response = new Common.SearchResponse<Common.User>();
            response.Total = await query.CountAsync();

            var dataResponse = await query.Skip(model.Page * model.PageSize)
                                        .Take(model.PageSize)
                                        .ToListAsync();

            response.Results = dataResponse.Select(c => new Common.User()
            {
                Id = c.Id,
                Email = c.Email,
                IsAdministrator = false //Populate this in the next step.
            }).ToList();

            foreach(var user in response.Results)
            {
                user.IsAdministrator = await _userManager.IsInRoleAsync(dataResponse.FirstOrDefault(x => x.Id == user.Id), "Administrator");
                user.IsSelf = user.Id == userId;
            }

            return Ok(response);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut]
        [Route("api/v1/settings")]
        async public Task<IActionResult> UpdateSystemSettings([FromBody] Common.SystemSettings model)
        {
            var settings = await _db.SystemSettings.FirstOrDefaultAsync();
            settings.SendGridKey = model.SendGridKey.Trim() == "--- NOT DISPLAYED FOR SECURITY ---" ? settings.SendGridKey : model.SendGridKey;
            settings.SendGridSystemEmailAddress = model.SendGridSystemEmailAddress;
            settings.RegistrationEnabled = model.RegistrationEnabled;
            settings.EmailDomainRestriction = model.EmailDomainRestriction;
            await _db.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        [Route("api/v1/settings")]
        async public Task<IActionResult> GetSystemSettings()
        {
            var settings = await _db.SystemSettings.FirstOrDefaultAsync();

            var response = new Common.SystemSettings()
            {
                SendGridKey = "--- NOT DISPLAYED FOR SECURITY ---",
                SendGridSystemEmailAddress = settings.SendGridSystemEmailAddress,
                RegistrationEnabled = settings.RegistrationEnabled,
                EmailDomainRestriction = settings.EmailDomainRestriction
            };

            return Ok(response);
        }
        
    }
}



