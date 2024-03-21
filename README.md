# ASP.NET_Core_RazorPages_RepositoryPattern
This Repo consists of sample ASP.NET Core Razor page Development including sample like Create, Update, Delete, FileUpload samples with Repository Pattern

Following key **"JwtConfig:key": "CuYM000OLlMQG6VVLp1OH7Xzyw3eHuw1qvUC5dcGt8FLI"** in **appsettings json** should be placed or stored under secured environment like (KeyValut)

Authentication Flow


    -> Login 

      -> tokengenerate save actual token in DB and get the mapping token, store the mapping token in session 
    
        -> Middleware(mapping token from session, and get the actual token, assign auth header) 
    
          -> (based on JWTConfig in program.cs) Authorize Attribute validate the claims.**

Repo consists of Following features
1. JWT Authentication
2. Global Exception Handler
3. Generic Repository Pattern
4. UnitOfWork Pattern with Transaction enabled
5. CRUD Operations for an Entity(CREATE, UPDATE, DELETE, GET)
6. FileUpload
7. Attributes (asp-for, asp-page, asp-route, asp-items, asp-page-handler)
8. Jquery code in Razor page
9. asp-page handler sample
10. Multiple Forms in same page
11. Dependency Injection
  
