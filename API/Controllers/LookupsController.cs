using Microsoft.AspNetCore.Mvc;
using RepairCenter.Api.Data;
using System.Data;
namespace RepairCenter.Api.Controllers;
[ApiController]
[Route("api/lookups")]
public sealed class LookupsController : ControllerBase
{
 private readonly SqlConnectionFactory _factory; public LookupsController(SqlConnectionFactory factory)=>_factory=factory;
 [HttpGet("order-statuses")] public async Task<IActionResult> Statuses(){var rows=new List<object>();await using var c=_factory.Create();await c.OpenAsync();await using var cmd=c.CreateCommand();cmd.CommandText="SELECT OrderStatusID,StatusName,IsFinal FROM dbo.Course_project_0502_OrderStatuses ORDER BY OrderStatusID";await using var r=await cmd.ExecuteReaderAsync();while(await r.ReadAsync())rows.Add(new{ID=r.GetInt32(0),Name=r.GetString(1),IsFinal=r.GetBoolean(2)});return Ok(rows);}
 [HttpGet("equipment-models")] public async Task<IActionResult> Models(){var rows=new List<object>();await using var c=_factory.Create();await c.OpenAsync();await using var cmd=c.CreateCommand();cmd.CommandText="SELECT m.EquipmentModelID,t.TypeName,b.BrandName,m.ModelName FROM dbo.Course_project_0502_EquipmentModels m JOIN dbo.Course_project_0502_EquipmentBrands b ON b.EquipmentBrandID=m.EquipmentBrandID JOIN dbo.Course_project_0502_EquipmentTypes t ON t.EquipmentTypeID=m.EquipmentTypeID ORDER BY t.TypeName,b.BrandName,m.ModelName";await using var r=await cmd.ExecuteReaderAsync();while(await r.ReadAsync())rows.Add(new{ID=r.GetInt32(0),Type=r.GetString(1),Brand=r.GetString(2),Model=r.GetString(3)});return Ok(rows);}
 [HttpGet("specializations")] public async Task<IActionResult> Specializations(){var rows=new List<object>();await using var c=_factory.Create();await c.OpenAsync();await using var cmd=c.CreateCommand();cmd.CommandText="SELECT SpecializationID,SpecializationName FROM dbo.Course_project_0502_Specializations ORDER BY SpecializationName";await using var r=await cmd.ExecuteReaderAsync();while(await r.ReadAsync())rows.Add(new{ID=r.GetInt32(0),Name=r.GetString(1)});return Ok(rows);}
}
