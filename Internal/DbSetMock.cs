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

            this.primaryKey = typeof(T)
                .GetProperties()
                .Select(x => new { name = x.Name, att = x.GetCustomAttribute<KeyAttribute>(), order = x.GetCustomAttribute<ColumnAttribute>()?.Order ?? 0 })
                .Where(x => x.att != null)
                .OrderBy(x => x.order)
                .Select(x => x.name)
                .ToList();

        }

        readonly IReadOnlyList<string> primaryKey;

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

        public override T Find(params object[] keyValues)
        {
            var expr = PredicateBuilder.PredicateEqual<T>(primaryKey.Zip(keyValues, (a, b) => Tuple.Create(a, b)));
            var finds = query.Where(expr).ToList();
            if (finds.Count == 0)
                return null;
            else if (finds.Count == 1)
                return finds[0];
            else
                throw new InvalidOperationException();
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
