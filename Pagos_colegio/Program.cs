using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pagos_colegio_web.Data;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 🧱 Configuración de la conexión a la base de datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 🔐 Configuración de Identity (sin confirmación de cuenta y con roles)
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// 🧭 Agrega soporte para MVC y Razor Pages
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ⚙️ Crear roles y usuario administrador por defecto
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await CrearRolesYUsuarioAdmin(services);
}

// 🌐 Configuración del pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // 🔒 Autenticación
app.UseAuthorization();  // ✅ Autorización

// 🖨️ Configuración de Rotativa para generar PDFs
string wwwrootPath = app.Environment.WebRootPath;
RotativaConfiguration.Setup(wwwrootPath, "Rotativa");

// 🗺️ Configuración de rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

/// <summary>
/// Crea roles predeterminados y un usuario administrador si no existen.
/// </summary>
/// <param name="serviceProvider">Proveedores de servicios de la aplicación</param>
static async Task CrearRolesYUsuarioAdmin(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // 🎭 Lista de roles a crear
    string[] roles = { "Admin", "Usuario", "Familia" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // 👤 Datos del usuario administrador por defecto
    var adminEmail = "admin@colegio.com";
    var adminPassword = "Admin123!";

    // Crear usuario administrador si no existe
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var user = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}


