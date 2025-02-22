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
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private  ApplicationDbContext _context;
        internal DbSet<Company> Companys;
        public CompanyRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;

        }
        public void Update(Company obj)
        {
            _context.Company.Update(obj);
        }
    }
}
