using Abc.BusinessService;
using ABC.Entities.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Auth
{
    public class logoutModel : PageModel
    {
         private readonly ITokenService tokenService;

        private readonly IUnitOfWork unitOfWork;

        public logoutModel(
        ITokenService tokenService,
            IUnitOfWork unitOfWork)
        {          
             this.tokenService = tokenService;
            this.unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> OnGet()
        {
            using(var trans = await this.unitOfWork.BeginTransactionAsync()){

                    try{
                        string jwTSessionToken = HttpContext.Session.GetString("jwt");

                        var tokenData = await this.tokenService.GetActualToken(jwTSessionToken);
                        await this.tokenService.DeleteToken(tokenData.Id);
                        await this.unitOfWork.CommitTransactionAsync(trans);
                        this.HttpContext.Session.Clear();
                        return RedirectToPage("/auth/login");
                    }
                    catch{
                        await this.unitOfWork.RollbackTransactionAsync(trans);
                        return BadRequest();
                    }
                    finally{
                        await trans.DisposeAsync();
                    }
                  }

     
        }
    }
}
