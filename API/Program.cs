using Microsoft.OpenApi;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "RepairCenter API", Version = "v1", Description = "API учёта ремонта оргтехники для администратора" }));
builder.Services.AddSingleton<RepairCenter.Api.Data.SqlConnectionFactory>();
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "RepairCenter API v1"); c.RoutePrefix = "swagger"; });
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (SqlException ex)
    {
        if (context.Response.HasStarted) throw;
        context.Response.Clear();
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = ex.Number == 2627 || ex.Number == 2601 || ex.Number == 547 ? StatusCodes.Status409Conflict : StatusCodes.Status500InternalServerError;
        var message = context.Response.StatusCode == StatusCodes.Status409Conflict ? "Операция нарушает уникальность или ссылочную целостность базы данных." : "Ошибка SQL Server. Проверьте подключение, права доступа и схему таблиц.";
        await context.Response.WriteAsJsonAsync(new { message });
    }
});
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.Run();
