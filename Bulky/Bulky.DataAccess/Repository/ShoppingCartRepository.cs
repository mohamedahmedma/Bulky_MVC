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
	public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
	{
		private readonly ApplicationDbContext _context;
		internal DbSet<ShoppingCart> shoppingCarts;
		public ShoppingCartRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
			this.shoppingCarts = _context.Set<ShoppingCart>();
		}
		public void Update(ShoppingCart obj)
		{
			_context.ShoppingCarts.Update(obj);
		}
	}
}
