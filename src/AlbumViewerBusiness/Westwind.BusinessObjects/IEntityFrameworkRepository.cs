using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Westwind.Utilities;

namespace Westwind.BusinessObjects
{
    public interface IEntityFrameworkRepository<TContext, TEntity>
        where TContext : DbContext
        where TEntity : class, new()
    {
        bool AutoValidate { get; set; }
        TContext Context { get; set; }
        Exception ErrorException { get; set; }
        string ErrorMessage { get; set; }
        bool ThrowExceptions { get; set; }
        ValidationErrorCollection ValidationErrors { get; }

        TEntity Create();
        T Create<T>() where T : class, new();
        bool Delete(object id, bool saveChanges = false, bool useTransaction = false);
        bool Delete(TEntity entity, bool saveChanges = true, bool useTransaction = false);
        Task<TEntity> Load(object id);
        Task<T> Load<T>(object id) where T : class, new();
        bool Save(TEntity entity = null);
        Task<bool> SaveAsync(TEntity entity = null);
        Task<bool> SaveAsync(TEntity entity, bool useTransaction);
        void SetError();
        void SetError(Exception ex, bool checkInnerException = false);
        void SetError(string Message);
        bool Validate(TEntity entity);
    }
}