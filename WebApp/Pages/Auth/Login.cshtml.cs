using Abc.BusinessService;
using Abc.UnitOfWorkLibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Middlewares;
using WebApp.Models;

namespace WebApp.Pages.Auth
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly string password = "AAAAFFFFSDGTWSTY1234";

        private readonly IUserInfoService _userInfoService;
        private readonly IJwtAuthentication _jwtAuthentication;

        [BindProperty]
        public UserInfo UserInfoData { get; set; }

      private readonly ITokenService tokenService;

        private readonly IUnitOfWork unitOfWork;

        public LoginModel(IUserInfoService userInfoService, IJwtAuthentication jwtAuthentication,
        ITokenService tokenService,
            IUnitOfWork unitOfWork)
        {
            _userInfoService = userInfoService;
            _jwtAuthentication = jwtAuthentication;
             this.tokenService = tokenService;
            this.unitOfWork = unitOfWork;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostLoginUser()
        {
            var data = this._userInfoService.GetUserInfo(UserInfoData.UserName);

            if(data == null || data.Password != UserInfoData.Password)
            {
                return RedirectToPage("/Index");
            }

            var token = this._jwtAuthentication.GenerateJwtToken(data.UserName, data.Role);
            var encodedToken = EncDecHelper.EncryptedData(Guid.NewGuid().ToString(), password);
            var refreshToken = EncDecHelper.EncryptedData(Guid.NewGuid().ToString(), password);
            
            using(var trans = await this.unitOfWork.BeginTransactionAsync()){
                try{        
                    var tokenData = new ABC.Models.Token();
                    tokenData.ActualToken = token;
                    tokenData.ClientToken = encodedToken;
                    tokenData.RefreshToken = refreshToken;
                    await this.tokenService.AddToken(tokenData);
                    await this.unitOfWork.CommitTransactionAsync(trans);                    
                    this.HttpContext.Session.SetString("jwt", encodedToken);
                    return RedirectToPage("/Author/Index");
                }
                catch{
                    await this.unitOfWork.RollbackTransactionAsync(trans);
                    return Unauthorized();
                }
                finally{
                    await trans.DisposeAsync();                    
                }
            }

        }
    }
}
