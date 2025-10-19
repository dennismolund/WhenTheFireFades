using System;
using Microsoft.EntityFrameworkCore;
using WhenTheFireFades.Data;

namespace WhenTheFireFades.Tests.TestHelpers;

public static class DbContextFactory
{
    public static ApplicationDbContext CreateContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}