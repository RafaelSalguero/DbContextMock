using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tonic.Internal
{
    public class DbSetMock : DbSet
    {
        public DbSetMock(Type entityType, IList observableCollectionList) : base()
        {
            if (observableCollectionList.GetType().GetGenericTypeDefinition() != typeof(ObservableCollection<>))
            {
                throw new ArgumentException("El parametro debe de ser de tipo ObservableCollection<>");
            }

            this.entityType = entityType;
            this.observableCollectionList = observableCollectionList;
            this.primaryKey = entityType
              .GetProperties()
              .Select(x => new { name = x.Name, att = x.GetCustomAttribute<KeyAttribute>(), order = x.GetCustomAttribute<ColumnAttribute>()?.Order ?? 0 })
              .Where(x => x.att != null)
              .OrderBy(x => x.order)
              .Select(x => x.name)
              .ToList();
        }

        readonly IList observableCollectionList;
        readonly Type entityType;
        readonly IReadOnlyList<string> primaryKey;

        public override object Find(params object[] keyValues)
        {
            var finds = new List<object>();
            foreach (var o in observableCollectionList)
            {
                var values = primaryKey.Select(x => entityType.GetProperty(x).GetValue(o));
                if (values.SequenceEqual(keyValues))
                {
                    finds.Add(o);
                }
            }

            if (finds.Count == 0)
                return null;
            else if (finds.Count == 1)
                return finds[0];
            else
                throw new InvalidOperationException();
        }

        public override Task<object> FindAsync(CancellationToken token, params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }

        public override Task<object> FindAsync(params object[] keyValues)
        {
            return FindAsync(new CancellationToken(), keyValues);
        }

        public override object Add(object entity)
        {
            observableCollectionList.Add(entity);
            return entity;
        }

        public override IEnumerable AddRange(IEnumerable entities)
        {
            foreach (var o in entities)
            {
                Add(o);
            }
            return entities;
        }
        public override object Remove(object entity)
        {
            observableCollectionList.Remove(entity);
            return entity;
        }

        public override IEnumerable RemoveRange(IEnumerable entities)
        {
            foreach (var o in entities)
            {
                Remove(o);
            }
            return entities;
        }

        public override Type ElementType
        {
            get
            {
                return this.entityType;
            }
        }

        public override IList Local
        {
            get
            {
                return observableCollectionList;
            }
        }

        public override object Attach(object entity)
        {
            return entity;
        }

        public override object Create()
        {
            return Activator.CreateInstance(entityType);
        }

        public override object Create(Type derivedEntityType)
        {
            return Activator.CreateInstance(derivedEntityType);
        }

    }
}
