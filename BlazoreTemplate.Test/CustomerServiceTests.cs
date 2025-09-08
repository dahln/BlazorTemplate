using BlazorTemplate.Database;
using BlazorTemplate.Service;
using BlazorTemplate.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Xunit;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace BlazoreTemplate.Test;

public class CustomerServiceTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private CustomerService GetCustomerService(ApplicationDbContext db)
    {
        return new CustomerService(db);
    }


    [Fact]
    public async Task AddCustomerAndGetCustomerById()
    {
        var db = GetInMemoryDbContext();
        var service = GetCustomerService(db);
        var userId = "user1";

        //Test Create
        var customer = new BlazorTemplate.Dto.Customer { Name = "Test Customer", Email = "test@example.com" };
        var customerId = await service.CreateCustomerAsync(customer, userId);
        Assert.NotNull(customerId);

        //Test Get
        var result = await service.GetCustomerAsync(customerId, userId);
        Assert.NotNull(result);
        Assert.Equal("Test Customer", result.Name);
    }

    [Fact]
    public async Task UpdateCustomer()
    {
        var db = GetInMemoryDbContext();
        var service = GetCustomerService(db);
        var userId = "user1";
        var customer = new BlazorTemplate.Dto.Customer { Name = "Old Name", Email = "old@example.com" };
       
       //Test Create
        var customerId = await service.CreateCustomerAsync(customer, userId);
        var added = await service.GetCustomerAsync(customerId, userId);
        Assert.NotNull(added);
        
        //Update Customer
        added.Name = "New Name";
        var updatedResult = await service.UpdateCustomer(added, userId);
        Assert.True(updatedResult);
        
        //Get Customer and validate update
        var updated = await service.GetCustomerAsync(customerId, userId);
        Assert.NotNull(updated);
        Assert.Equal("New Name", updated.Name);
    }


    [Fact]
    public async Task DeleteCustomer()
    {
        var db = GetInMemoryDbContext();
        var service = GetCustomerService(db);
        var userId = "user1";
        
        var customer = new BlazorTemplate.Dto.Customer { Name = "To Delete", Email = "delete@example.com" };
        
        //Test Create and Delete new customer
        var customerId = await service.CreateCustomerAsync(customer, userId);
        var deleted = await service.DeleteCustomerByIdAsync(customerId, userId);
        Assert.True(deleted);

        //Try to get deleted customer - should be null
        var result = await service.GetCustomerAsync(customerId, userId);
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchCustomers()
    {
        var db = GetInMemoryDbContext();
        var service = GetCustomerService(db);
        var userId = "user1";

        //Add 100 customers and then test search
        await service.SeedCustomers(100, userId);

        Search search = new Search()
        {
            Page = 1,
            PageSize = 10,
            SortBy = "Name",
            SortDirection = SortDirection.Ascending,
            FilterText = string.Empty
        };
        var searchResults = await service.SearchCustomersAsync(search, userId);
        Assert.Equal(10, searchResults.Results.Count);
    }

}