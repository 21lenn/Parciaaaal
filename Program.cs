using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Parciaaaal.Data;
using Parciaaaal.Models; 

var builder = WebApplication.CreateBuilder(args);

// Configurar cadena de conexión (usa la de appsettings.json)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Agregar DbContext con SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Habilitar página de errores de base de datos (solo desarrollo)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configurar Identity con roles y sin confirmación de cuenta obligatoria
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>() // necesario para el rol Coordinador
.AddEntityFrameworkStores<ApplicationDbContext>();

// Agregar controladores y vistas
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configurar pipeline HTTP
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

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();


// Inicializar datos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Asegurarse de que la BD esté migrada
    context.Database.Migrate();

    // Crear cursos iniciales
    if (!context.Cursos.Any())
    {
        context.Cursos.AddRange(
            new Curso { Codigo = "CS101", Nombre = "Programación I", Creditos = 4, CupoMaximo = 30, HorarioInicio = new TimeSpan(8, 0, 0), HorarioFin = new TimeSpan(10, 0, 0), Activo = true },
            new Curso { Codigo = "MA201", Nombre = "Matemática II", Creditos = 3, CupoMaximo = 25, HorarioInicio = new TimeSpan(10, 0, 0), HorarioFin = new TimeSpan(12, 0, 0), Activo = true },
            new Curso { Codigo = "HI101", Nombre = "Historia del Perú", Creditos = 2, CupoMaximo = 20, HorarioInicio = new TimeSpan(14, 0, 0), HorarioFin = new TimeSpan(16, 0, 0), Activo = true }
        );
        context.SaveChanges();
    }

    // Rol Coordinador
    if (!await roleManager.RoleExistsAsync("Coordinador"))
    {
        await roleManager.CreateAsync(new IdentityRole("Coordinador"));
    }

    // Crear usuario Coordinador
    var email = "coordinador@uni.edu";
    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
    {
        user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        await userManager.CreateAsync(user, "Admin123!");
        await userManager.AddToRoleAsync(user, "Coordinador");
    }
}

app.Run();


