// <copyright file="DashboardResponse.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the DashboardResponse class.
// </summary>

namespace BankApp.Application.DataTransferObjects.Dashboard;

/// <summary>
///     Represents the response containing dashboard data for a user.
/// </summary>
public class DashboardResponse
{
    /// <summary>
    ///     Gets or sets the current user information.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public UserSummaryDataTransferObject? CurrentUser { get; set; }

    /// <summary>
    ///     Gets or sets the list of cards belonging to the user.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public List<CardDataTransferObject> Cards { get; set; } = [];

    /// <summary>
    ///     Gets or sets the list of recent transactions.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public List<TransactionDataTransferObject> RecentTransactions { get; set; } = new();

    /// <summary>
    ///     Gets or sets the count of unread notifications.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UnreadNotificationCount { get; set; }
}