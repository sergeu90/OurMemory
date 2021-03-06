﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using OurMemory.Domain.Entities;

namespace OurMemory.Data.Infrastructure
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private ApplicationDbContext dataContext;
        private readonly IDbSet<T> dbset;

        public Repository(IDatabaseFactory databaseFactory)
        {
            DatabaseFactory = databaseFactory;
            dbset = DataContext.Set<T>();
        }

        public void DetachAllEntities()
        {
            foreach (var entity in dataContext.ChangeTracker.Entries().Where(e => e.State == EntityState.Unchanged))
            {
                this.dataContext.Entry(entity.Entity).State = EntityState.Detached;
            }
        }

        protected IDatabaseFactory DatabaseFactory
        {
            get;
            private set;
        }

        protected ApplicationDbContext DataContext
        {
            get { return dataContext ?? (dataContext = DatabaseFactory.Get()); }
        }
        public virtual void Add(T entity)
        {
            dbset.Add(entity);
        }
        public virtual void Update(T entity)
        {
            dbset.Attach(entity);
            dataContext.Entry(entity).State = EntityState.Modified;
        }
        public virtual void Delete(T entity)
        {
            var entry = dataContext.Entry(entity);
            if (entry.State == EntityState.Detached)
                dbset.Attach(entity);

            dbset.Remove(entity);
        }
        public virtual void Delete(Expression<Func<T, bool>> where)
        {
            IEnumerable<T> objects = dbset.Where<T>(where).AsEnumerable();

            foreach (T obj in objects)
                dbset.Remove(obj);
        }
        public virtual T GetById(long id)
        {
            var entity = dbset.Find(id);

            return entity;
        }
        public virtual T GetById(string id)
        {
            T find = dbset.Find(id);

            return find;
        }
        public virtual IEnumerable<T> GetAll()
        {
            var enumerable = dbset.ToList();

            return enumerable;
        }

        public virtual IQueryable<T> GetSpec(Expression<Func<T, bool>> specification)
        {
            return dbset.Where(specification);
        }


        public T Get(Expression<Func<T, bool>> where)
        {
            return dbset.Where(where).FirstOrDefault<T>();
        }
    }
}