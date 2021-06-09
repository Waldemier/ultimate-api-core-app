using System;
using System.Linq;
using System.Linq.Expressions;
using Contracts;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public abstract class RepositoryBase<T>: IRepositoryBase<T> where T: class
    {
        protected readonly RepositoryContext _repositoryContext;
        public RepositoryBase(RepositoryContext repositoryContext)
        {
            this._repositoryContext = repositoryContext;
        }

        public void Create(T entity) => this._repositoryContext.Set<T>().Add(entity);

        public void Delete(T entity) => this._repositoryContext.Set<T>().Remove(entity);

        public void Update(T entity) => this._repositoryContext.Set<T>().Update(entity);

        public IQueryable<T> FindAll(bool trackChanges) =>
            !trackChanges ? this._repositoryContext.Set<T>().AsNoTracking() // returns for readonly
                :
            this._repositoryContext.Set<T>();

        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges) =>
            !trackChanges ? this._repositoryContext.Set<T>().Where(expression).AsNoTracking() // returns for readonly || Set<T> get the specify table
                :
            this._repositoryContext.Set<T>().Where(expression);
    }
}
