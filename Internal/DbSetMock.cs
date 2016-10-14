using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tonic
{
    /// <summary>
    /// DbSet in-memory mock
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class DbSetMock<T> : DbSet<T>, IEnumerable<T>, IEnumerable, IQueryable, IDbAsyncEnumerable<T>, IDbAsyncEnumerable
       where T : class
    {

        public DbSetMock(ObservableCollection<T> list)
        {
            local = list;
            query = list.AsQueryable();
            this.provider = new TestDbAsyncQueryProvider<T>(query.Provider);
        }

        private ObservableCollection<T> local;
        private IQueryable<T> query;
        public override ObservableCollection<T> Local
        {
            get
            {
                return this.Local;
            }
        }

        public override T Add(T entity)
        {
            local.Add(entity);
            return entity;
        }

        public override IEnumerable<T> AddRange(IEnumerable<T> entities)
        {
            foreach (var e in entities)
                local.Add(e);
            return entities;
        }

        public override T Attach(T entity)
        {
            return entity;
        }

        public override TDerivedEntity Create<TDerivedEntity>()
        {
            throw new NotImplementedException("This method is not implemented on the DbSetMock class");
        }

        public override DbQuery<T> AsNoTracking()
        {
            return this;
        }

        [Obsolete("Obsolete because parent member is also obsolete")]
        public override DbQuery<T> AsStreaming()
        {
            return this;
        }

        public override DbQuery<T> Include(string path)
        {
            return this;
        }

        public override T Remove(T entity)
        {
            local.Remove(entity);
            return entity;
        }

        public override IEnumerable<T> RemoveRange(IEnumerable<T> entities)
        {
            foreach (var i in entities)
                Remove(i);
            return entities;
        }

        public override DbSqlQuery<T> SqlQuery(string sql, params object[] parameters)
        {
            throw new NotImplementedException("This method is not implemented");
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
