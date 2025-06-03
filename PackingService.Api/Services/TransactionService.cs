using Microsoft.EntityFrameworkCore.Storage;
using PackingService.Api.Data;

namespace PackingService.Api.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly PackingDbContext _dbContext;

        public TransactionService(PackingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IDbContextTransaction BeginTransaction()
        {
            return _dbContext.Database.BeginTransaction();
        }
    }
}