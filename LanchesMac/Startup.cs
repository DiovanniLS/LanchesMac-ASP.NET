using LanchesMac.Context;
using LanchesMac.Models;
using LanchesMac.Repositories;
using LanchesMac.Repositories.Interfaces;
using LanchesMac.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
namespace LanchesMac;
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

        services.AddTransient<ILancheRepository, LancheRepository>();
        services.AddTransient<IPedidoRepository, PedidoRepository>();
        services.AddTransient<ICategoriaRepository, CategoriaRepository>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped(sp => CarrinhoCompra.GetCarrinho(sp));
        services.AddScoped<ISeedUserRoleInitial, SeedUserRoleInitial>();

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = 10;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
        });

        services.Configure<IdentityOptions>(options =>
        {
            options.Lockout.MaxFailedAccessAttempts = 5;

            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(15);
        });

        services.Configure<IdentityOptions>(options =>
        {
            options.SignIn.RequireConfirmedEmail = true;
        });

        services.AddAuthorization(options => 
        {
            options.AddPolicy("Admin", politica =>
            {
                politica.RequireRole("Admin");
            });
        });

        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
        });

        services.AddControllersWithViews();
        
        services.AddMemoryCache();
        services.AddSession();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;

            options.Cookie.SecurePolicy =
                CookieSecurePolicy.Always;

            options.SlidingExpiration = true;

            options.ExpireTimeSpan =
                TimeSpan.FromHours(1);

            options.LoginPath = "/Account/Login";

            options.AccessDeniedPath =
                "/Account/AccessDenied";
        });

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env,ISeedUserRoleInitial seedUserRoleInitial)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseRouting();


        seedUserRoleInitial.SeedRolesAsync().GetAwaiter().GetResult();
        seedUserRoleInitial.SeedUsersAsync().GetAwaiter().GetResult();

        app.UseSession();
        
        app.UseAuthentication();
        app.UseAuthorization();




        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });



    }
}