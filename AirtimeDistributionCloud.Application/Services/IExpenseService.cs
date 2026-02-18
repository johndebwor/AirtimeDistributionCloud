using AirtimeDistributionCloud.Application.DTOs;
using AirtimeDistributionCloud.Core.Enums;

namespace AirtimeDistributionCloud.Application.Services;

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseDto>> GetAllExpensesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpenseDto>> GetExpensesByStatusAsync(DepositStatus status, CancellationToken cancellationToken = default);
    Task<ExpenseDto> CreateExpenseAsync(CreateExpenseRequest request, string userId, CancellationToken cancellationToken = default);
    Task<ExpenseDto> ApproveExpenseAsync(int expenseId, string approvedByUserId, string notes, CancellationToken cancellationToken = default);
    Task<ExpenseDto> RejectExpenseAsync(int expenseId, string rejectedByUserId, string notes, CancellationToken cancellationToken = default);
}
