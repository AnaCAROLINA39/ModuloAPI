using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuloAPI.Dominio.DTOs;
using ModuloAPI.Dominio.Entidades;
using ModuloAPI.Dominio.Enuns;
using ModuloAPI.Dominio.Interfaces;
using ModuloAPI.Dominio.ModelViews;
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

// Login
app.MapPost("administradores/login", ([FromBody] LoginDTO loginDTO, iAdministradorServico administradorServico) =>
{
    if (administradorServico.Login(loginDTO) != null)
        return Results.Ok("Login realizado com sucesso!");
    else
        return Results.Unauthorized();
}).WithTags("Administradores");

// Listar todos (com paginação opcional)
app.MapGet("administradores", ([FromQuery] int? pagina, iAdministradorServico administradorServico) =>
{
    return Results.Ok(administradorServico.Todos(pagina));
}).WithTags("Administradores");

// Buscar por ID
app.MapGet("administradores/{id}", ([FromRoute] int id, iAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscaPorId(id);
    if (administrador == null)
        return Results.NotFound();
    return Results.Ok(administrador);
}).WithTags("Administradores");

// Criar novo administrador
app.MapPost("administradores", ([FromBody] AdiministradorDTO administradorDTO, iAdministradorServico administradorServico) =>
{
    var validacao = new ErrosDeValidacao { Mensagens = new List<string>() };

    if (string.IsNullOrEmpty(administradorDTO.Email) || !administradorDTO.Email.Contains("@"))
        validacao.Mensagens.Add("Email inválido.");

    if (string.IsNullOrEmpty(administradorDTO.Senha) || administradorDTO.Senha.Length < 6)
        validacao.Mensagens.Add("Senha deve ter pelo menos 6 caracteres.");

    if (administradorDTO.Perfil == null)
        validacao.Mensagens.Add("Perfil é obrigatório.");
  
    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    Administrador? administrador = new Administrador
    {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString()  ?? Perfil.editor.ToString()
    };

    administradorServico.Incluir(administrador);

    return Results.Created($"/administradores/{administrador?.Id}", administrador);
}).WithTags("Administradores");

#endregion

#region Veiculos
ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosDeValidacao { Mensagens = new List<string>() };
    if (string.IsNullOrEmpty(veiculoDTO.Nome) || veiculoDTO.Nome.Length < 3)
        validacao.Mensagens.Add("Nome do veículo deve ter pelo menos 3 caracteres.");
    if (string.IsNullOrEmpty(veiculoDTO.Marca) || veiculoDTO.Marca.Length < 2)
        validacao.Mensagens.Add("Marca do veículo deve ter pelo menos 2 caracteres.");
    if (veiculoDTO.Ano < 1950 || veiculoDTO.Ano > DateTime.Now.Year + 1)
        validacao.Mensagens.Add($"Ano do veículo deve estar entre 1950 e {DateTime.Now.Year + 1}.");
    return validacao;
}


app.MapPost("Veiculos", ([FromBody] VeiculoDTO veiculoDTO, iVeiculoServico veiculoServico) =>
{
    var validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);

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

        var validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);



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
