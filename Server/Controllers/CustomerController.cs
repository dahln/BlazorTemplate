using BlazorTemplate.Server.Utility;
using BlazorTemplate.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorTemplate.Server.Controllers
{
    public class CustomerController : Controller
    {
        private CustomerService _customerService;

        public CustomerController(CustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost]
        [Route("api/v1/customer")]
        [Authorize]
        async public Task<IActionResult> CustomerCreate([FromBody] Common.Customer model)
        {
            string userId = User.GetUserId();

            var result = await _customerService.CustomerCreate(model, userId);
            if(result.Success == false)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpGet]
        [Route("api/v1/customer/{customerId}")]
        [Authorize]
        async public Task<IActionResult> CustomerGetById(string customerId)
        {
            string userId = User.GetUserId();

            var customer = await _customerService.CustomerGetById(customerId, userId);
            if (customer == null)
                return BadRequest("Customer not found");

            return Ok(customer);
        }

        [HttpPut]
        [Route("api/v1/customer/{customerId}")]
        [Authorize]
        async public Task<IActionResult> CustomerUpdateById([FromBody] Common.Customer model, string customerId)
        {
            string userId = User.GetUserId();

            var result = await _customerService.CustomerUpdateById(model, customerId, userId);

            if(result.Success == false)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpDelete]
        [Route("api/v1/customer/{customerId}")]
        [Authorize]
        async public Task<IActionResult> CustomerDeleteById(string customerId)
        {
            string userId = User.GetUserId();

            await _customerService.CustomerDeleteById(customerId, userId);

            return Ok();
        }


        [Authorize]
        [HttpPost]
        [Route("api/v1/customers")]
        async public Task<IActionResult> CustomerSearch([FromBody] Common.Search model)
        {
            string userId = User.GetUserId();

            var searchResponse = await _customerService.CustomerSearch(model, userId);
            
            return Ok(searchResponse);
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/seed/create/{number}")]
        async public Task<IActionResult> SeedCustomers(int number)
        {
            string userId = User.GetUserId();

            for (int a = 0; a < number; a++)
            {
                var customer = new Common.Customer()
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
                    Active = LoremNET.Lorem.Number(0, 1) == 0 ? false : true
                };

                await _customerService.CustomerCreate(customer, userId);
            }

            return Ok();
        }


        [HttpGet]
        [Route("api/v1/ping")]
        public IActionResult Ping()
        {
            return Ok("Received...");
        }
    }//End Controller
}
