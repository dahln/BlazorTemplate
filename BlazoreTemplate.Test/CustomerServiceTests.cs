using BlazorTemplate.Database;
using BlazorTemplate.Service;
using BlazorTemplate.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Xunit;
using System.Threading.Tasks;
using System;
using System.Linq;

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
    public async Task AddCustomer_ShouldAddCustomer()
    {
        var db = GetInMemoryDbContext();
        var service = GetCustomerService(db);
        var userId = "user1";
        var customer = new BlazorTemplate.Dto.Customer { Name = "Test Customer", Email = "test@example.com" };
        var customerId = await service.CreateCustomerAsync(customer, userId);
        var result = await service.GetCustomerAsync(customerId, userId);
        Assert.NotNull(result);
        Assert.Equal("Test Customer", result.Name);
    }


    [Fact]
    public async Task GetCustomer_ShouldReturnNull_WhenNoCustomer()
    {
        var db = GetInMemoryDbContext();
        var service = GetCustomerService(db);
        var userId = "user1";
        var result = await service.GetCustomerAsync("nonexistent-id", userId);
        Assert.Null(result);
    }


    [Fact]
    public async Task UpdateCustomer_ShouldModifyCustomer()
    {
        var db = GetInMemoryDbContext();
        var service = GetCustomerService(db);
        var userId = "user1";
        var customer = new BlazorTemplate.Dto.Customer { Name = "Old Name", Email = "old@example.com" };
       
        var customerId = await service.CreateCustomerAsync(customer, userId);
        var added = await service.GetCustomerAsync(customerId, userId);
        Assert.NotNull(added);
        
        added.Name = "New Name";
        var updatedResult = await service.UpdateCustomer(added, userId);
        Assert.True(updatedResult);
        
        var updated = await service.GetCustomerAsync(customerId, userId);
        Assert.NotNull(updated);
        Assert.Equal("New Name", updated.Name);
    }


    [Fact]
    public async Task DeleteCustomer_ShouldRemoveCustomer()
    {
        var db = GetInMemoryDbContext();
        var service = GetCustomerService(db);
        var userId = "user1";
        
        var customer = new BlazorTemplate.Dto.Customer { Name = "To Delete", Email = "delete@example.com" };
        
        var customerId = await service.CreateCustomerAsync(customer, userId);
        var deleted = await service.DeleteCustomerByIdAsync(customerId, userId);
        Assert.True(deleted);

        var result = await service.GetCustomerAsync(customerId, userId);
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchCustomers_ShouldGetList()
    {
        var db = GetInMemoryDbContext();
        var service = GetCustomerService(db);
        var userId = "user1";

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