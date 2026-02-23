using AirtimeDistributionCloud.Application.DTOs;
using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Interfaces;

namespace AirtimeDistributionCloud.Application.Services;

public class ExpenseCategoryService(IUnitOfWork unitOfWork) : IExpenseCategoryService
{
    public async Task<IReadOnlyList<ExpenseCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await unitOfWork.Repository<ExpenseCategory>().GetAllAsync(cancellationToken);
        var expenses = await unitOfWork.Repository<Expense>().GetAllAsync(cancellationToken);
        var countsByCategory = expenses.GroupBy(e => e.ExpenseCategoryId)
            .ToDictionary(g => g.Key, g => g.Count());

        return categories
            .OrderBy(c => c.Name)
            .Select(c => MapToDto(c, countsByCategory.GetValueOrDefault(c.Id, 0)))
            .ToList();
    }

    public async Task<IReadOnlyList<ExpenseCategoryDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var categories = await unitOfWork.Repository<ExpenseCategory>()
            .FindAsync(c => c.IsActive, cancellationToken);
        return categories.OrderBy(c => c.Name).Select(c => MapToDto(c, 0)).ToList();
    }

    public async Task<ExpenseCategoryDto> CreateAsync(CreateExpenseCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await unitOfWork.Repository<ExpenseCategory>()
            .FindAsync(c => c.Name == request.Name, cancellationToken);
        if (existing.Any())
            throw new InvalidOperationException($"A category named '{request.Name}' already exists.");

        var category = new ExpenseCategory
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true
        };
        await unitOfWork.Repository<ExpenseCategory>().AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(category, 0);
    }

    public async Task<ExpenseCategoryDto> UpdateAsync(UpdateExpenseCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await unitOfWork.Repository<ExpenseCategory>().GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Expense category with Id {request.Id} not found.");

        // Check name uniqueness (excluding itself)
        var sameNameOther = await unitOfWork.Repository<ExpenseCategory>()
            .FindAsync(c => c.Name == request.Name && c.Id != request.Id, cancellationToken);
        if (sameNameOther.Any())
            throw new InvalidOperationException($"A category named '{request.Name}' already exists.");

        category.Name = request.Name;
        category.Description = request.Description;
        category.IsActive = request.IsActive;
        await unitOfWork.Repository<ExpenseCategory>().UpdateAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(category, 0);
    }

    public async Task SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        var category = await unitOfWork.Repository<ExpenseCategory>().GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Expense category with Id {id} not found.");

        category.IsActive = isActive;
        await unitOfWork.Repository<ExpenseCategory>().UpdateAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ExpenseCategoryDto MapToDto(ExpenseCategory c, int expenseCount) =>
        new(c.Id, c.Name, c.Description, c.IsActive, expenseCount);
}
