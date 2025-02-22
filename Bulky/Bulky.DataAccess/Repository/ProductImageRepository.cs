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
	public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
	{
		private readonly ApplicationDbContext _context;
		internal DbSet<ProductImage> Categories;
		public ProductImageRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
			this.Categories = _context.Set<ProductImage>();
		}
		public void Update(ProductImage obj)
		{
			_context.ProductImages.Update(obj);
		}
	}
}
