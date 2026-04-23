// <copyright file="ServiceCollectionExtensionsTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankApp.Infrastructure.Tests.DependencyInjection;

/// <summary>
///     Regression tests for <see cref="ServiceCollectionExtensions.AddInfrastructure" /> ensuring
///     that missing required secrets cause an <see cref="InvalidOperationException" /> at startup
///     rather than the application silently running in a misconfigured state.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddInfrastructure_WhenOtpSecretMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        IConfiguration configuration = BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:BankingAppDb"] = "Server=.;Database=Test;Trusted_Connection=True;",
                ["Jwt:Secret"] = "any-placeholder-jwt-secret",
            });

        // Act
        Action serviceCollectionInstanceCallsAddInfrastructure = () =>
        {
            new ServiceCollection().AddInfrastructure(configuration);
        };

        // Assert
        serviceCollectionInstanceCallsAddInfrastructure
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*Otp:Secret*");
    }

    [Fact]
    public void AddInfrastructure_WhenJwtSecretMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        IConfiguration configuration = BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:BankingAppDb"] = "Server=.;Database=Test;Trusted_Connection=True;",
                ["Otp:Secret"] = "any-placeholder-otp-secret",
            });

        // Act
        Action serviceCollectionInstanceCallsAddInfrastructure = () =>
        {
            new ServiceCollection().AddInfrastructure(configuration);
        };

        // Assert
        serviceCollectionInstanceCallsAddInfrastructure
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*Jwt:Secret*");
    }

    [Fact]
    public void AddInfrastructure_WhenConnectionStringMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        IConfiguration configuration = BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "any-placeholder-jwt-secret",
                ["Otp:Secret"] = "any-placeholder-otp-secret",
            });

        // Act
        Action serviceCollectionInstanceCallsAddInfrastructure = () =>
        {
            new ServiceCollection().AddInfrastructure(configuration);
        };

        // Assert
        serviceCollectionInstanceCallsAddInfrastructure
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*ConnectionStrings:BankingAppDb*");
    }

    [Fact]
    public void AddInfrastructure_WhenAllSecretsPresent_DoesNotThrow()
    {
        // Arrange
        IConfiguration configuration = BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:BankingAppDb"] = "Server=.;Database=Test;Trusted_Connection=True;",
                ["Jwt:Secret"] = "any-placeholder-jwt-secret",
                ["Otp:Secret"] = "any-placeholder-otp-secret",
            });

        // Act
        Action serviceCollectionInstanceCallsAddInfrastructure = () =>
        {
            new ServiceCollection().AddInfrastructure(configuration);
        };

        // Assert
        serviceCollectionInstanceCallsAddInfrastructure
            .Should()
            .NotThrow();
    }

    /// <summary>
    ///     Builds an <see cref="IConfiguration" /> from an in-memory dictionary.
    /// </summary>
    /// <param name="values">Key-value pairs to include in the configuration.</param>
    /// <returns>An <see cref="IConfiguration" /> populated with the supplied values.</returns>
    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}