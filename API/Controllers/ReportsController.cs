using Microsoft.AspNetCore.Mvc;
using RepairCenter.Api.Data;
using System.Data;
namespace RepairCenter.Api.Controllers;
[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
 private readonly SqlConnectionFactory _factory; public ReportsController(SqlConnectionFactory factory)=>_factory=factory;
 [HttpGet("completed-orders")] public async Task<IActionResult> CompletedOrders([FromQuery]DateTime fromDate,[FromQuery]DateTime toDate){if(fromDate==default||toDate==default)return BadRequest(new{message="Укажи fromDate и toDate в формате YYYY-MM-DD."});if(toDate.Date<fromDate.Date)return BadRequest(new{message="Дата окончания раньше даты начала."});await using var c=_factory.Create();await c.OpenAsync();await using var cmd=c.CreateCommand();cmd.CommandText="SELECT ro.OrderNumber,cl.FullName,CONCAT(t.TypeName,N' ',b.BrandName,N' ',m.ModelName),ro.IssuedAt,s.StatusName,ro.RepairCost FROM dbo.Course_project_0502_RepairOrders ro JOIN dbo.Course_project_0502_Equipment e ON e.EquipmentID=ro.EquipmentID JOIN dbo.Course_project_0502_Clients cl ON cl.ClientID=e.ClientID JOIN dbo.Course_project_0502_EquipmentModels m ON m.EquipmentModelID=e.EquipmentModelID JOIN dbo.Course_project_0502_EquipmentBrands b ON b.EquipmentBrandID=m.EquipmentBrandID JOIN dbo.Course_project_0502_EquipmentTypes t ON t.EquipmentTypeID=m.EquipmentTypeID JOIN dbo.Course_project_0502_OrderStatuses s ON s.OrderStatusID=ro.OrderStatusID WHERE ro.IsCancelled=0 AND s.IsFinal=1 AND ro.IssuedAt>=@f AND ro.IssuedAt<DATEADD(day,1,@t) ORDER BY ro.IssuedAt";cmd.Parameters.Add("@f",SqlDbType.Date).Value=fromDate.Date;cmd.Parameters.Add("@t",SqlDbType.Date).Value=toDate.Date;var rows=new List<object>();decimal total=0;await using var r=await cmd.ExecuteReaderAsync();while(await r.ReadAsync()){var cost=r.GetDecimal(5);total+=cost;rows.Add(new{OrderNumber=r.GetString(0),ClientName=r.GetString(1),Equipment=r.GetString(2),IssuedAt=r.GetDateTime(3),Status=r.GetString(4),RepairCost=cost});}return Ok(new{From=fromDate.Date,To=toDate.Date,Count=rows.Count,TotalCost=total,Orders=rows});}
}
