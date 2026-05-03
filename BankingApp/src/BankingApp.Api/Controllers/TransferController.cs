// +ADw-copyright file+AD0AIg-TransferController.cs+ACI- company+AD0AIg-CtrlC CtrlV+ACIAPg-
// Copyright (c) CtrlC CtrlV. All rights reserved.
// +ADw-/copyright+AD4-
// +ADw-summary+AD4-
// Contains the TransferController class.
// +ADw-/summary+AD4-

using BankingApp.Application.DataTransferObjects.Transfer+ADs-
using BankingApp.Application.DTOs.Transfer+ADs-
using BankingApp.Application.Services.Transfers+ADs-
using Microsoft.AspNetCore.Mvc+ADs-

namespace BankingApp.Api.Controllers+ADs-

/// +ADw-summary+AD4-
///     Controller responsible for handling transfer-related operations.
///     All endpoints are accessible under the /api/transfer route.
/// +ADw-/summary+AD4-
+AFs-ApiController+AF0-
+AFs-Route(+ACI-api/+AFs-controller+AF0AIg-)+AF0-
public class TransferController : ApiControllerBase
+AHs-
    private readonly ITransferService +AF8-transferService+ADs-

    /// +ADw-summary+AD4-
    ///     Initializes a new instance of the +ADw-see cref+AD0AIg-TransferController+ACI- /+AD4- class.
    /// +ADw-/summary+AD4-
    /// +ADw-param name+AD0AIg-transferService+ACIAPg-The transfer service used to handle business logic.+ADw-/param+AD4-
    public TransferController(ITransferService transferService)
    +AHs-
        +AF8-transferService +AD0- transferService+ADs-
    +AH0-

    /// +ADw-summary+AD4-
    ///     Creates a new transfer for the currently authenticated user.
    /// +ADw-/summary+AD4-
    /// +ADw-param name+AD0AIg-request+ACIAPg-The transfer creation request.+ADw-/param+AD4-
    /// +ADw-returns+AD4-
    ///     201 Created with a +ADw-see cref+AD0AIg-TransferResponse+ACI- /+AD4- on success,
    ///     or an error response if validation, authorization, or persistence fails.
    /// +ADw-/returns+AD4-
    +AFs-HttpPost+AF0-
    public IActionResult CreateTransfer(+AFs-FromBody+AF0- CreateTransferRequest request)
    +AHs-
        int userId +AD0- GetAuthenticatedUserId()+ADs-
        return ToActionResult(
            +AF8-transferService.CreateTransfer(request, userId),
            transfer +AD0APg- CreatedAtAction(nameof(GetHistory), new +AHs- +AH0-, transfer))+ADs-
    +AH0-

    /// +ADw-summary+AD4-
    ///     Retrieves the transfer history for the currently authenticated user.
    /// +ADw-/summary+AD4-
    /// +ADw-returns+AD4-
    ///     200 OK with a list of +ADw-see cref+AD0AIg-TransferResponse+ACI- /+AD4- on success,
    ///     or an error response if the operation fails.
    /// +ADw-/returns+AD4-
    +AFs-HttpGet+AF0-
    public IActionResult GetHistory()
    +AHs-
        int userId +AD0- GetAuthenticatedUserId()+ADs-
        return ToActionResult(
            +AF8-transferService.GetHistory(userId),
            history +AD0APg- Ok(history))+ADs-
    +AH0-
+AH0-