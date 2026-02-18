using AirtimeDistributionCloud.Application.DTOs;
using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Enums;
using AirtimeDistributionCloud.Core.Interfaces;

namespace AirtimeDistributionCloud.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ISystemSettingRepository _systemSettingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExpenseService(
        IExpenseRepository expenseRepository,
        ISystemSettingRepository systemSettingRepository,
        IUnitOfWork unitOfWork)
    {
        _expenseRepository = expenseRepository;
        _systemSettingRepository = systemSettingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ExpenseDto>> GetAllExpensesAsync(CancellationToken cancellationToken = default)
    {
        var expenses = await _expenseRepository.GetAllAsync(cancellationToken);
        var branches = (await _unitOfWork.Repository<Branch>().GetAllAsync(cancellationToken)).ToDictionary(b => b.Id);
        return expenses.Select(e => MapToDto(e, branches.GetValueOrDefault(e.BranchId)?.Name ?? "Unknown")).ToList();
    }

    public async Task<IReadOnlyList<ExpenseDto>> GetExpensesByStatusAsync(DepositStatus status, CancellationToken cancellationToken = default)
    {
        var expenses = await _expenseRepository.GetByStatusAsync(status, cancellationToken);
        return expenses.Select(e => MapToDto(e, e.Branch?.Name ?? "Unknown")).ToList();
    }

    public async Task<ExpenseDto> CreateExpenseAsync(CreateExpenseRequest request, string userId, CancellationToken cancellationToken = default)
    {
        var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(request.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException("Branch not found");

        var threshold = await _systemSettingRepository.GetByKeyAsync("ExpenseApprovalThreshold", cancellationToken);
        var thresholdValue = decimal.Parse(threshold?.Value ?? "50000");

        var expense = new Expense
        {
            BranchId = request.BranchId,
            Description = request.Description,
            Category = request.Category,
            Amount = request.Amount,
            ExpenseDate = DateTime.UtcNow,
            ReceiptNumber = request.ReceiptNumber,
            Status = request.Amount < thresholdValue ? DepositStatus.Approved : DepositStatus.Pending
        };

        await _expenseRepository.AddAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(expense, branch.Name);
    }

    public async Task<ExpenseDto> ApproveExpenseAsync(int expenseId, string approvedByUserId, string notes, CancellationToken cancellationToken = default)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId, cancellationToken)
            ?? throw new KeyNotFoundException($"Expense with Id {expenseId} not found");

        if (expense.Status != DepositStatus.Pending)
            throw new InvalidOperationException("Only pending expenses can be approved");

        expense.Status = DepositStatus.Approved;
        expense.ApprovedByUserId = approvedByUserId;
        expense.ApprovalNotes = notes;

        await _expenseRepository.UpdateAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(expense.BranchId, cancellationToken);
        return MapToDto(expense, branch?.Name ?? "Unknown");
    }

    public async Task<ExpenseDto> RejectExpenseAsync(int expenseId, string rejectedByUserId, string notes, CancellationToken cancellationToken = default)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId, cancellationToken)
            ?? throw new KeyNotFoundException($"Expense with Id {expenseId} not found");

        if (expense.Status != DepositStatus.Pending)
            throw new InvalidOperationException("Only pending expenses can be rejected");

        expense.Status = DepositStatus.Rejected;
        expense.ApprovedByUserId = rejectedByUserId;
        expense.ApprovalNotes = notes;

        await _expenseRepository.UpdateAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(expense.BranchId, cancellationToken);
        return MapToDto(expense, branch?.Name ?? "Unknown");
    }

    private static ExpenseDto MapToDto(Expense e, string branchName) =>
        new(e.Id, e.BranchId, branchName, e.Description, e.Category, e.Amount,
            e.ExpenseDate, e.Status, e.ReceiptNumber, e.CreatedDate, e.CreatedBy);
}
