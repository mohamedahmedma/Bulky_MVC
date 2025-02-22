using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
	public class Repository<T> : IRepository<T> where T : class
	{
		private readonly ApplicationDbContext _context;
		internal DbSet<T> Set;
		public Repository(ApplicationDbContext context)
		{
			_context = context;
			this.Set = _context.Set<T>();
			_context.Products.Include(u => u.Category)/*.Include(u => u.Id)*/;
		}
		public void Add(T entity)
		{
			_context.Add(entity);
		}

		public T Get(Expression<Func<T, bool>> filter , string? includeProperties = null , bool tracked = false)
		{
			IQueryable<T> query;
			if (tracked)
			{
				query = Set;
			}
			else
			{
				query = Set.AsNoTracking();
			}

			query = query.Where(filter);
			if (!string.IsNullOrEmpty(includeProperties))
			{
				foreach (var item in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(item);
				}
			}
			return query.FirstOrDefault();
		}

		// Category , CategoryId
		public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
		{
			IQueryable<T> query = Set;
			if( filter != null)
			{
                query = query.Where(filter);
            }
            if (!string.IsNullOrEmpty(includeProperties))
			{
				foreach (var item in includeProperties
					.Split(new char[] {','} , StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(item);
				}
			}
			return query.ToList();
		}

		public void Remove(T entity)
		{
			Set.Remove(entity);
		}

		public void RemoveRange(IEnumerable<T> entity)
		{
			Set.RemoveRange(entity);
		}
	}
}
