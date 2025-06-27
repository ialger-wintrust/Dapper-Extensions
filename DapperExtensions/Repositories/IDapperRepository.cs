using DapperExtensions.Predicate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DapperExtensions.Repositories;

public interface IDapperRepository<T>
{
    Task<T?> Get(object id);

    Task<IEnumerable<T>?> GetAll(Predicate<T>? predicate = null, IList<ISort>? sorts = null, int page = 1,
        int resultsPerPage = 10, IDbTransaction? transaction = null);

    Task<IEnumerable<T>?> Find(Predicate<T> predicate, IList<ISort>? sorts = null, IDbTransaction? transaction = null);

    Task<dynamic> Add(T entity, IDbTransaction? transaction = null);

    Task Add(IEnumerable<T> entities, IDbTransaction? transaction = null);

    Task<int> Update(T entity, IDbTransaction? transaction = null);

    Task<int> Update(IEnumerable<T> entities, IDbTransaction? transaction = null);

    Task<bool> Delete(T entity, IDbTransaction? transaction = null);

    Task<bool> Delete(IEnumerable<T> entities, IDbTransaction? transaction = null);
}