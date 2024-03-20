using System.Net;
using Abc.BusinessService;
using WebApp.Models;

namespace WebApp.Middlewares
{
	public class JwtMiddleware
	{
        private readonly RequestDelegate _next;
        private readonly IConfiguration configuration;
        private readonly IUserInfoService userService;
        private readonly IJwtAuthentication jwtAuthentication;
        private readonly ITokenService service;

        public JwtMiddleware(RequestDelegate next, 
        IConfiguration configuration, 
        IUserInfoService userService, 
        IJwtAuthentication authentication,
        ITokenService tokenService)
        {
            this._next = next;
            this.service = tokenService;
            this.configuration = configuration;
            this.userService = userService;
            this.jwtAuthentication = authentication;
        }

        public async Task Invoke(HttpContext context)
        {
            string jwTSessionToken = context.Session.GetString("jwt");

            if (!string.IsNullOrEmpty(jwTSessionToken))
            {
                var tokenData = await service.GetActualToken(jwTSessionToken);

                if(tokenData!=null){
                    // if we set the Authorization header, JWT configuration inprogram.cs will validate because of UseAuthentication Method
                    // This will populate the Claims in UserContext in HttpContext.
                    context.Request.Headers.Add("Authorization", "Bearer " + tokenData.ActualToken);

                    // Additionally we are setting the user info in Items after validate in system.
                    setUserInfoToContext(context, tokenData.ActualToken);
                }
                else{
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("unauthorized");  
                }       
            }                   

            await _next(context);
        }

        public void setUserInfoToContext(HttpContext context, string token)
        {
            try
            {
                var username = this.jwtAuthentication.ValidateToken(token);
                var userdata = userService.GetUserInfo(username);
                context.Items["userToken"] = new { Username = userdata.UserName, Role = userdata.Role };
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
