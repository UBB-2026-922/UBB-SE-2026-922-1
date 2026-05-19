namespace BankingApp.Application.Features.ForexRateAlerts.Commands;

using Domain.Aggregates.RateAlertAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Shared.Persistence;

public sealed record DeleteRateAlertCommand(int UserId, int AlertId) : IRequest<ErrorOr<Success>>;

public sealed class DeleteRateAlertCommandHandler(
    IRateAlertRepository rateAlertRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteRateAlertCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteRateAlertCommand command, CancellationToken cancellationToken)
    {
        RateAlert? alert = await rateAlertRepository.GetByIdAsync(command.AlertId, cancellationToken);
        if (alert is null || alert.UserId != command.UserId)
        {
            return RateAlertErrors.NotFound;
        }

        await rateAlertRepository.DeleteAsync(alert, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
