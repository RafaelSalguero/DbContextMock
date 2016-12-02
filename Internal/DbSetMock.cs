using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tonic.Internal;

namespace Tonic
{

    interface IDbSetMock
    {
        DbSetMock genericSet { get; }
    }
    /// <summary>
    /// DbSet in-memory mock
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class DbSetMock<T> : DbSet<T>, IEnumerable<T>, IEnumerable, IQueryable, IDbAsyncEnumerable<T>, IDbAsyncEnumerable, IDbSetMock
       where T : class
    {

        public DbSetMock(ObservableCollection<T> list)
        {
            this.genericSet = new DbSetMock(typeof(T), list);
            local = list;
            query = list.AsQueryable();
            this.provider = new TestDbAsyncQueryProvider<T>(query.Provider);
        }

        readonly IReadOnlyList<string> primaryKey;
        private ObservableCollection<T> local;
        private IQueryable<T> query;
        public readonly DbSetMock genericSet;

        DbSetMock IDbSetMock.genericSet => genericSet;

        public override ObservableCollection<T> Local
        {
            get
            {
                return this.local;
            }
        }

        public override T Add(T entity)
        {
            return (T)genericSet.Add(entity);
        }

        public override IEnumerable<T> AddRange(IEnumerable<T> entities)
        {
            return (IEnumerable<T>)genericSet.AddRange(entities);
        }

        public override T Attach(T entity)
        {
            return (T)genericSet.Add(entity);
        }

        public override TDerivedEntity Create<TDerivedEntity>()
        {
            return (TDerivedEntity)genericSet.Create(typeof(TDerivedEntity));
        }


        public override T Remove(T entity)
        {
            return (T)genericSet.Remove(entity);
        }

        public override IEnumerable<T> RemoveRange(IEnumerable<T> entities)
        {
            return (IEnumerable<T>)genericSet.RemoveRange(entities);
        }


        public override T Find(params object[] keyValues)
        {
            return (T)genericSet.Find(keyValues);
        }

        public override Task<T> FindAsync(CancellationToken token, params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }

        public override Task<T> FindAsync(params object[] keyValues)
        {
            return FindAsync(new CancellationToken(), keyValues);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return local.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return local.GetEnumerator();
        }

        Type IQueryable.ElementType
        {
            get
            {
                return query.ElementType;
            }
        }

        Expression IQueryable.Expression
        {
            get
            {
                return query.Expression;
            }
        }

        readonly IQueryProvider provider;
        IQueryProvider IQueryable.Provider
        {
            get
            {
                return provider;
            }
        }

        IDbAsyncEnumerator<T> IDbAsyncEnumerable<T>.GetAsyncEnumerator()
        {
            return new TestDbAsyncEnumerator<T>(local.GetEnumerator());
        }

        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return new TestDbAsyncEnumerator<T>(local.GetEnumerator());
        }
    }
}
