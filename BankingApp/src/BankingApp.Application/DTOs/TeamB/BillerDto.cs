// <copyright file="BillerDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the BillerDto record.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries biller master data to the presentation layer.
/// </summary>
/// <param name="Id">The unique identifier of the biller.</param>
/// <param name="Name">The display name of the biller.</param>
/// <param name="Category">The business sector category.</param>
/// <param name="LogoUrl">The URL of the biller's logo, or null.</param>
/// <param name="IsActive">Indicates whether the biller is currently accepting payments.</param>
public record BillerDto(
    int Id,
    string Name,
    BillerCategory Category,
    string? LogoUrl,
    bool IsActive);
