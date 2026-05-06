// <copyright file="ExchangeControllerTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankingApp.Api.Controllers;
using BankingApp.Application.DTOs;
using BankingApp.Application.DTOs.TeamB;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.TeamB;
using BankingApp.Domain.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BankingApp.Api.Tests.Controller;

public class ExchangeControllerTests
{
    private readonly Mock<IExchangeService> _mockExchangeService;
    private readonly Mock<IBillPaymentRepository> _mockBillPaymentRepository;
    private readonly ExchangeController _controller;

    public ExchangeControllerTests()
    {
        _mockExchangeService = new Mock<IExchangeService>();
        _mockBillPaymentRepository = new Mock<IBillPaymentRepository>();
        _controller = new ExchangeController(_mockExchangeService.Object, _mockBillPaymentRepository.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = 1;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
        };
    }

    [Fact]
    public void GetPreview_WhenPreviewAndLockBothSucceed_ReturnsOkWithPreviewDto()
    {
        // Arrange
        var previewDto = new ExchangeTransactionResponseDto { ExchangeRate = 1.2m };
        var lockedRate = new LockedRate { Rate = 1.2m };

        _mockExchangeService
            .Setup(s => s.GetRatePreview("EUR", "USD", 100m))
            .Returns(previewDto);

        _mockExchangeService
            .Setup(s => s.LockRate(1, "EUR", "USD"))
            .Returns(lockedRate);

        // Act
        var result = _controller.GetPreview("EUR", "USD", 100m);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualDto = Assert.IsType<ExchangeTransactionResponseDto>(okResult.Value);
        Assert.Equal(1.2m, actualDto.ExchangeRate);
    }

    [Fact]
    public void GetPreview_WhenGetRatePreviewFails_ReturnsMappedError()
    {
        // Arrange
        var error = Error.Validation("Code", "Description");
        _mockExchangeService
            .Setup(s => s.GetRatePreview("EUR", "USD", 100m))
            .Returns(error);

        // Act
        var result = _controller.GetPreview("EUR", "USD", 100m);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ApplicationErrorResponse>(badRequestResult.Value);
        Assert.Equal("Code", errorResponse.ErrorCode);
    }

    [Fact]
    public void GetPreview_WhenLockRateFails_ReturnsMappedError()
    {
        // Arrange
        var previewDto = new ExchangeTransactionResponseDto { ExchangeRate = 1.2m };
        var error = Error.Validation("Code", "Description");

        _mockExchangeService
            .Setup(s => s.GetRatePreview("EUR", "USD", 100m))
            .Returns(previewDto);

        _mockExchangeService
            .Setup(s => s.LockRate(1, "EUR", "USD"))
            .Returns(error);

        // Act
        var result = _controller.GetPreview("EUR", "USD", 100m);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ApplicationErrorResponse>(badRequestResult.Value);
        Assert.Equal("Code", errorResponse.ErrorCode);
    }
}
