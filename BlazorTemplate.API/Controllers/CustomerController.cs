using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlazorTemplate.API.Utility;
using BlazorTemplate.Database;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BlazorTemplate.APi.Controllers
{
    public class CustomerController : Controller
    {
        private ApplicationDbContext _db;
        public CustomerController(ApplicationDbContext dbContext)
        {
            _db = dbContext;
        }

        [Authorize]
        [HttpPost]
        [Route("api/v1/customer")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        async public Task<IActionResult> CustomerCreate([FromBody] Common.Customer model)
        {
            string userId = User.GetUserId();

            if (string.IsNullOrEmpty(model.Name))
            {
                return BadRequest("Customer name is required");
            }
            
            Database.Customer customer = new Database.Customer()
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                BirthDate = model.BirthDate,
                Address = model.Address,
                City = model.City,
                State = model.State,
                Postal = model.Postal,
                Notes = model.Notes,
                ImageBase64 = model.ImageBase64,
                Active = model.Active,
                Gender = model.Gender,
                OwnerId = userId
            };

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            return Ok(customer.Id);
        }

        [Authorize]
        [HttpGet]
        [Route("api/v1/customer/{customerId}")]
        [ProducesResponseType<Common.Customer>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        async public Task<IActionResult> CustomerGetById(string customerId)
        {
            string userId = User.GetUserId();

            var customer = await _db.Customers.Where(c => c.OwnerId == userId && c.Id == customerId).FirstOrDefaultAsync();
            if (customer == null)
                return BadRequest("Customer not found");

            var response = new Common.Customer()
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                City = customer.City,
                State = customer.State,
                Postal = customer.Postal,
                Notes = customer.Notes,
                BirthDate = customer.BirthDate.HasValue ? customer.BirthDate.Value : null,
                Gender = customer.Gender.Value,
                Active = customer.Active.Value,
                ImageBase64 = customer.ImageBase64
            };

            return Ok(customer);
        }

        [Authorize]
        [HttpPut]
        [Route("api/v1/customer/{customerId}")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        async public Task<IActionResult> CustomerUpdateById([FromBody] Common.Customer model, string customerId)
        {
            string userId = User.GetUserId();

            if (string.IsNullOrEmpty(model.Name))
            {
                return BadRequest("Customer name is required");
            }

            var customer = await _db.Customers.Where(c => c.OwnerId == userId && c.Id == customerId).FirstOrDefaultAsync();
            if (customer == null)
            {
                return BadRequest("Customer not found");
            }

            customer.Name = model.Name;
            customer.Email = model.Email;
            customer.Phone = model.Phone;
            customer.BirthDate = model.BirthDate;
            customer.Address = model.Address;
            customer.City = model.City;
            customer.State = model.State;
            customer.Postal = model.Postal;
            customer.Notes = model.Notes;
            customer.ImageBase64 = model.ImageBase64;
            customer.Active = model.Active;
            customer.Gender = model.Gender;
            customer.UpdateOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(customer.Id);
        }

        [Authorize]
        [HttpDelete]
        [Route("api/v1/customer/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        async public Task<IActionResult> CustomerDeleteById(string customerId)
        {
            string userId = User.GetUserId();

            var customer = await _db.Customers.Where(c => c.OwnerId == userId && c.Id == customerId).FirstOrDefaultAsync();

            if(customer == null)
            {
                return BadRequest("Customer not found");
            }

            _db.Customers.Remove(customer);
            await _db.SaveChangesAsync();
            return Ok();
        }


        [Authorize]
        [HttpPost]
        [Route("api/v1/customers")]
        [ProducesResponseType<Common.SearchResponse<Common.Customer>>(StatusCodes.Status200OK)]
        async public Task<IActionResult> CustomerSearch([FromBody] Common.Search model)
        {
            string userId = User.GetUserId();

            var query = _db.Customers.Where(c => c.OwnerId == userId);

            if (!string.IsNullOrEmpty(model.FilterText))
            {
                query = query.Where(i => i.Name.ToLower().Contains(model.FilterText.ToLower()) ||
                                        i.Email.ToLower().ToLower().Contains(model.FilterText.ToLower()) ||
                                        i.Phone.ToLower().Contains(model.FilterText.ToLower()) ||
                                        i.Address.ToLower().Contains(model.FilterText.ToLower()) ||
                                        i.State.ToLower().Contains(model.FilterText.ToLower()) ||
                                        i.Postal.ToLower().Contains(model.FilterText.ToLower()) ||
                                        i.Notes.ToLower().Contains(model.FilterText.ToLower()));
            }

            if (model.SortBy == nameof(Common.Customer.Name))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.Name)
                            : query.OrderByDescending(c => c.Name);
            }
            else if (model.SortBy == nameof(Common.Customer.State))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.State)
                            : query.OrderByDescending(c => c.State);
            }
            else if (model.SortBy == nameof(Common.Customer.Gender))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.Gender)
                            : query.OrderByDescending(c => c.Gender);
            }
            else if (model.SortBy == nameof(Common.Customer.Active))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.Active)
                            : query.OrderByDescending(c => c.Active);
            }
            else
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.Name)
                            : query.OrderByDescending(c => c.Name);
            }

            Common.SearchResponse<Common.Customer> response = new Common.SearchResponse<Common.Customer>();
            response.Total = await query.CountAsync();

            var dataResponse = await query.Skip(model.Page * model.PageSize)
                                        .Take(model.PageSize)
                                        .ToListAsync();

            response.Results = dataResponse.Select(c => new Common.Customer()
            {
                Id = c.Id,
                Name = c.Name,
                City = c.City,
                State = c.State,
                Postal = c.Postal,
                Gender = c.Gender.Value,
                Active = c.Active.Value,
            }).ToList();

            return Ok(response);
        }



        [HttpGet]
        [Authorize]
        [Route("api/v1/seed/customers/{number}")]
        async public Task<IActionResult> SeedCustomers(int number)
        {
            string userId = User.GetUserId();

            for (int a = 0; a < number; a++)
            {
                var customer = new Database.Customer()
                {
                    Name = LoremNET.Lorem.Words(2),
                    Gender = (Common.Enumerations.Gender)LoremNET.Lorem.Number(0, 2),
                    Email = LoremNET.Lorem.Email(),
                    Phone = LoremNET.Lorem.Number(1111111111, 9999999999).ToString(),
                    Address = $"{LoremNET.Lorem.Number(100, 10000).ToString()} {LoremNET.Lorem.Words(1)}",
                    City = LoremNET.Lorem.Words(1),
                    State = LoremNET.Lorem.Words(1),
                    Postal = LoremNET.Lorem.Number(11111, 99999).ToString(),
                    BirthDate = LoremNET.Lorem.DateTime(1923, 1, 1),
                    Notes = LoremNET.Lorem.Paragraph(5, 10, 10),
                    Active = LoremNET.Lorem.Number(0, 1) == 0 ? false : true,
                    OwnerId = userId
                };

                _db.Customers.Add(customer);
                await _db.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
