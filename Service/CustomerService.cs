using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorTemplate.Data;

namespace BlazorTemplate.Service
{
    public class CustomerService
    {

        private DBContext _db;

        public CustomerService(DBContext dbContext)
        {
            _db = dbContext;
        }

        async public Task<ServiceResponse<string>> CustomerCreate(Common.Customer model, string userId)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            if (string.IsNullOrEmpty(model.Name))
            {
                response.Success = false;
                response.Message = "Customer name is required";
                return response;
            }

            Customer customer = new Customer()
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

            response.Data = customer.Id;
            return response;
        }

        async public Task<Common.Customer> CustomerGetById(string customerId, string userId)
        { 
            var customer = await _db.Customers.Where(c => c.OwnerId == userId && c.Id == customerId).FirstOrDefaultAsync();
            if (customer == null)
                return default(Common.Customer);

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
                Gender = customer.Gender,
                Active = customer.Active,
                ImageBase64 = customer.ImageBase64
            };

            return response;
        }

        async public Task<ServiceResponse<string>> CustomerUpdateById(Common.Customer model, string customerId, string userId)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();
            if (string.IsNullOrEmpty(model.Name))
            {
                response.Success = false;
                response.Message = "Customer name is required";
                return response;
            }

            var customer = await _db.Customers.Where(c => c.OwnerId == userId && c.Id == customerId).FirstOrDefaultAsync();
            if (customer == null)
            {
                response.Success = false;
                response.Message = "Customer not found";
                return response;
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

            response.Data = customer.Id;

            return response;
        }

        async public Task CustomerDeleteById(string customerId, string userId)
        {
            var customer = await _db.Customers.Where(c => c.OwnerId == userId && c.Id == customerId).FirstOrDefaultAsync();

            if (customer != null)
            {
                _db.Customers.Remove(customer);
                await _db.SaveChangesAsync();
            }
        }


        async public Task<Common.SearchResponse<Common.Customer>> CustomerSearch(Common.Search model, string userId)
        {
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
            else if (model.SortBy == nameof(Common.Customer.City))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.State)
                            : query.OrderByDescending(c => c.State);
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

            response.Data = dataResponse.Select(c => new Common.Customer()
            {
                Id = c.Id,
                Name = c.Name,
                City = c.City,
                State = c.State,
                Postal = c.Postal,
                Gender = c.Gender,
                Active = c.Active,
            }).ToList();

            return response;
        }
    }
}
