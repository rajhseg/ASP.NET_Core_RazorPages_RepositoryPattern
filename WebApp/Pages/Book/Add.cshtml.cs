using Abc.BusinessService;
using Abc.UnitOfWorkLibrary;
using ABC.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using WebApp.Models;

namespace WebApp.Pages.Book
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
    public class AddModel : PageModel
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IBookService bookService;
        private readonly IAuthorService authorService;

        public AddModel(IUnitOfWork unitOfWork, IBookService bookService, IAuthorService authorService)
        {
            this.unitOfWork = unitOfWork;
            this.bookService = bookService;
            this.authorService = authorService;
        }

        [BindProperty]
        public BookModel Book { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> Authors { get; set; }

        public async Task OnGet()
        {
            var listData = await this.authorService.GetAllAuthors();            
            this.Authors = listData.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }).ToList();
            this.Book = new BookModel { };
        }

        public async Task<IActionResult> OnPost(BookModel book)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            using (var trans = await this.unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    await this.bookService.AddBook(new ABC.Models.Book { Title = book.Title, Description = book.Description, AuthorId = book.AuthorId });
                    await this.unitOfWork.CommitTransactionAsync(trans);
                    return RedirectToPage("Index");
                }
                catch (Exception)
                {
                    await this.unitOfWork.RollbackTransactionAsync(trans);
                    throw;
                }
                finally
                {
                    await trans.DisposeAsync();
                }
            }
        }
    }
}
