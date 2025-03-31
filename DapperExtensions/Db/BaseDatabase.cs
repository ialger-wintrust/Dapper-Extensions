using System;
using System.Data;

namespace DapperExtensions.Db;

public interface IBaseDatabase : IDisposable
{
    bool HasActiveTransaction { get; }
    IDbConnection Connection { get; }

    void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

    void Commit();

    void Rollback();

    void RunInTransaction(Action action);

    T RunInTransaction<T>(Func<T> func);
}

public abstract class BaseDatabase : IBaseDatabase
{
    protected IDbTransaction _transaction;

    protected BaseDatabase(IDbConnection connection)
    {
        Connection = connection;

        if (Connection.State != ConnectionState.Open)
        {
            Connection.Open();
        }
    }

    public bool HasActiveTransaction
    {
        get
        {
            return _transaction != null;
        }
    }

    public IDbConnection Connection { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (Connection.State != ConnectionState.Closed)
            {
                if (_transaction != null)
                {
                    _transaction.Rollback();
                }

                Connection.Close();
            }
        }
    }

    public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        _transaction = Connection.BeginTransaction(isolationLevel);
    }

    public void Commit()
    {
        _transaction.Commit();
        _transaction = null;
    }

    public void Rollback()
    {
        _transaction.Rollback();
        _transaction = null;
    }

    public void RunInTransaction(Action action)
    {
        BeginTransaction();
        try
        {
            action?.Invoke();
            Commit();
        }
        catch (Exception)
        {
            if (HasActiveTransaction)
            {
                Rollback();
            }

            throw;
        }
    }

    public T RunInTransaction<T>(Func<T> func)
    {
        BeginTransaction();
        try
        {
            var result = func.Invoke();
            Commit();
            return result;
        }
        catch (Exception)
        {
            if (HasActiveTransaction)
            {
                Rollback();
            }

            throw;
        }
    }

    protected virtual void ClearCache(IDapperImplementor dapper)
    {
        dapper.SqlGenerator.Configuration.ClearCache();
    }
}