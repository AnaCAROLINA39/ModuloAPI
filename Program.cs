using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuloAPI.Dominio.DTOs;
using ModuloAPI.Dominio.Entidades;
using ModuloAPI.Dominio.Interfaces;
using ModuloAPI.Dominio.Servicos;
#region builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<iAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<iVeiculoServico, VeiculoServico>();

// Add services to the container.
builder.Services.AddControllers(); // habilita controllers
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ModuloAPI.infraestrutura.DB.DbContexto>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("mysql"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql")));
});


var app = builder.Build();

// Configuração do Swagger

app.UseHttpsRedirection();

app.UseAuthorization();

// Mapeia todas as controllers da pasta Controllers
app.MapControllers();
#endregion
#region Home
app.MapGet("/", () => "API - Módulo de Administração");

#endregion


#region Administradores
app.MapPost("administradores/Login", ([FromBody] LoginDTO loginDTO, iAdministradorServico administradorServico) =>
{
    if (administradorServico.Login(loginDTO) != null)
        return Results.Ok("Login realizado com sucesso!");
    else
        return Results.Unauthorized();
}).WithTags("Administradores");
#endregion

#region Veiculos
app.MapPost("Veiculos", ([FromBody] VeiculoDTO veiculoDTO, iVeiculoServico veiculoServico) =>
{
    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoServico.Incluir(veiculo);
    return Results.Created($"/Veiculos/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("Veiculos", ([FromQuery] int? pagina, iVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);
    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("Veiculos{id}", ([FromRoute] int id, iVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
        return Results.NotFound();
    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapPut("Veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDTO, iVeiculoServico veiculoServico) =>
{
    var veiculoExistente = veiculoServico.BuscaPorId(id);
    if (veiculoExistente == null)
        return Results.NotFound();

    veiculoExistente.Nome = veiculoDTO.Nome;
    veiculoExistente.Marca = veiculoDTO.Marca;
    veiculoExistente.Ano = veiculoDTO.Ano;

    veiculoServico.Alterar(veiculoExistente);
    return Results.Ok(veiculoExistente);
}).WithTags("Veiculos");


app.MapDelete("Veiculos/{id}", ([FromRoute] int id, iVeiculoServico veiculoServico) =>
{
    var veiculoExistente = veiculoServico.BuscaPorId(id);
    if (veiculoExistente == null)
        return Results.NotFound();

    veiculoServico.Apagar(veiculoExistente);
    return Results.NoContent();
}).WithTags("Veiculos");
#endregion
#region app 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#endregion
app.Run();
