using System.Linq.Expressions;

namespace NewfoundlandGenealogy.CensusData;

public static class CensusDbContextExtensions
{
    public static async Task<T> GetOrAddAsync<T>(
        this CensusDbContext dbContext,
        Expression<Func<T, bool>> uniqueIdentifier,
        Func<CancellationToken, Task<T>> creator,
        CancellationToken cancellationToken)
        where T : class
    {
        var entity = await dbContext.Set<T>().FindAsync([uniqueIdentifier], cancellationToken);

        if (entity is null)
        {
            entity = await creator(cancellationToken);
            dbContext.Add(entity);
        }
        
        return entity;
    }
}