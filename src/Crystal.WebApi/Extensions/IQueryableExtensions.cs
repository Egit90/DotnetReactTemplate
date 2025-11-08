using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Extensions;


public record PagedResult<T>(int CurrentPage, int PageSize, int TotalCount, List<T> Items)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public static class IQueryableExtensions
{
    public static async Task<Result<PagedResult<T>>> ToPagedResult<T>(this IQueryable<T> query, int pageNumber, int pageSize, ILogger? logger = null) where T : class
    {
        try

        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var count = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>(TotalCount: count, PageSize: pageSize, CurrentPage: pageNumber, Items: items);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while fetching paged data");
            return Result.Failure<PagedResult<T>>("Error occurred while fetching paged data.");
        }
    }

}