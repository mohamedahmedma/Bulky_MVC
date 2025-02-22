using BulkyWebRazor.Data;
using BulkyWebRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor.Pages.Categories
{
    public class DeleteModel : PageModel
    {
		private readonly AppDbContext _db;
		[BindProperty]
		public Category Category { get; set; }
		public DeleteModel(AppDbContext db)
		{
			_db = db;
		}
		public void OnGet(int? id)
		{
			if (id != null && id != 0)
			{
				Category = _db.Categories.FirstOrDefault(x => x.Id == id);
			}
		}

		public IActionResult OnPost()
		{
			Category? obj = _db.Categories.FirstOrDefault(x => x.Id == Category.Id);
			if (obj == null)
			{
				return NotFound();
			}
				_db.Categories.Remove(obj);
				_db.SaveChanges();
				return RedirectToPage("index");
		}
	}
}
