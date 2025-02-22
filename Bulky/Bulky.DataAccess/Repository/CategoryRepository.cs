using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
	public class CategoryRepository : Repository<Category>, ICategoryRepository
	{
		private readonly ApplicationDbContext _context;
		internal DbSet<Category> Categories;
		public CategoryRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
			this.Categories = _context.Set<Category>();
		}
		public void Update(Category obj)
		{
			_context.Categories.Update(obj);
		}
	}
}
