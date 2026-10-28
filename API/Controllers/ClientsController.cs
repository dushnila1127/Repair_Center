using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RepairCenter.Api.Data;
using RepairCenter.Api.Models;
using System.Data;

namespace RepairCenter.Api.Controllers;

[ApiController]
[Route("api/clients")]
public sealed class ClientsController : ControllerBase
{
    private readonly SqlConnectionFactory _factory;
    public ClientsController(SqlConnectionFactory factory) => _factory = factory;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var rows = new List<object>(); await using var c = _factory.Create(); await c.OpenAsync(); await using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT ClientID,FullName,Phone,Address FROM dbo.Course_project_0502_Clients WHERE @s IS NULL OR FullName LIKE @p OR Phone LIKE @p OR Address LIKE @p ORDER BY FullName";
        cmd.Parameters.Add("@s", SqlDbType.NVarChar,150).Value = string.IsNullOrWhiteSpace(search)?DBNull.Value:search.Trim(); cmd.Parameters.Add("@p",SqlDbType.NVarChar,500).Value=string.IsNullOrWhiteSpace(search)?DBNull.Value:"%"+search.Trim()+"%";
        await using var r=await cmd.ExecuteReaderAsync(); while(await r.ReadAsync()) rows.Add(new { ClientID=r.GetInt32(0),FullName=r.GetString(1),Phone=r.GetString(2),Address=r.GetString(3) }); return Ok(rows);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        await using var c=_factory.Create(); await c.OpenAsync(); await using var cmd=c.CreateCommand(); cmd.CommandText="SELECT ClientID,FullName,Phone,Address FROM dbo.Course_project_0502_Clients WHERE ClientID=@id"; cmd.Parameters.Add("@id",SqlDbType.Int).Value=id;
        await using var r=await cmd.ExecuteReaderAsync(); if(!await r.ReadAsync()) return NotFound(); return Ok(new { ClientID=r.GetInt32(0),FullName=r.GetString(1),Phone=r.GetString(2),Address=r.GetString(3) });
    }

    [HttpPost]
    public async Task<IActionResult> Create(ClientRequest request)
    {
        await using var c=_factory.Create(); await c.OpenAsync(); await using var cmd=c.CreateCommand(); cmd.CommandText="INSERT INTO dbo.Course_project_0502_Clients(FullName,Phone,Address) OUTPUT INSERTED.ClientID VALUES(@n,@p,@a)";
        cmd.Parameters.Add("@n",SqlDbType.NVarChar,150).Value=request.FullName.Trim(); cmd.Parameters.Add("@p",SqlDbType.NVarChar,20).Value=request.Phone.Trim(); cmd.Parameters.Add("@a",SqlDbType.NVarChar,250).Value=request.Address.Trim(); var id=Convert.ToInt32(await cmd.ExecuteScalarAsync()); return CreatedAtAction(nameof(Get),new{id},new{ClientID=id});
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id,ClientRequest request)
    {
        await using var c=_factory.Create(); await c.OpenAsync(); await using var cmd=c.CreateCommand(); cmd.CommandText="UPDATE dbo.Course_project_0502_Clients SET FullName=@n,Phone=@p,Address=@a WHERE ClientID=@id"; cmd.Parameters.Add("@id",SqlDbType.Int).Value=id; cmd.Parameters.Add("@n",SqlDbType.NVarChar,150).Value=request.FullName.Trim(); cmd.Parameters.Add("@p",SqlDbType.NVarChar,20).Value=request.Phone.Trim(); cmd.Parameters.Add("@a",SqlDbType.NVarChar,250).Value=request.Address.Trim(); return await cmd.ExecuteNonQueryAsync()==0?NotFound():NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await using var c=_factory.Create(); await c.OpenAsync(); await using var cmd=c.CreateCommand(); cmd.CommandText="DELETE FROM dbo.Course_project_0502_Clients WHERE ClientID=@id"; cmd.Parameters.Add("@id",SqlDbType.Int).Value=id; return await cmd.ExecuteNonQueryAsync()==0?NotFound():NoContent(); }
        catch(SqlException ex) when(ex.Number==547) { return Conflict(new {message="Нельзя удалить клиента, у которого зарегистрирована техника."}); }
    }
}
