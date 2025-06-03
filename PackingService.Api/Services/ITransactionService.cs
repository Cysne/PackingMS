using Microsoft.EntityFrameworkCore.Storage;

namespace PackingService.Api.Services
{
    public interface ITransactionService
    {
        IDbContextTransaction BeginTransaction();
    }
}