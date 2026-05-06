// <copyright file="BillPaymentsControllerTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BankingApp.Api.Controllers;
using BankingApp.Application.DTOs.BillPayment;
using BankingApp.Application.DTOs.BillPayments;
using BankingApp.Application.Services.BillPayments;
using BankingApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BankingApp.Api.Tests.Controller;

public class BillPaymentsControllerTests
{
    private readonly Mock<IBillPaymentService> _mockBillPaymentService;
    private readonly BillPaymentsController _controller;

    public BillPaymentsControllerTests()
    {
        _mockBillPaymentService = new Mock<IBillPaymentService>();
        _controller = new BillPaymentsController(_mockBillPaymentService.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = 1;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }
}
