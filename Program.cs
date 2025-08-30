using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ModuloAPI.Dominio.DTOs;
using ModuloAPI.Dominio.Entidades;
using ModuloAPI.Dominio.Enuns;
using ModuloAPI.Dominio.Interfaces;
using ModuloAPI.Dominio.ModelViews;
using ModuloAPI.Dominio.Servicos;
#region builder
var builder = WebApplication.CreateBuilder(args);
var Key = builder.Configuration.GetSection("Jwt").ToString();
if (string.IsNullOrEmpty(Key))
    throw new Exception("Chave de autenticação não informada");
builder.Services.AddAuthentication(
    options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Key)),
        ValidateIssuer = false,
        ValidateAudience = false,
    ValidateIssuerSigningKey = true, // 👈 importante
        //ValidateIssuerSigningKey = true,

        // ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddScoped<iAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<iVeiculoServico, VeiculoServico>();

// Add services to the container.
builder.Services.AddControllers(); // habilita controllers
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT Aqui=>"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] { }
    }
});
});
builder.Services.AddDbContext<ModuloAPI.infraestrutura.DB.DbContexto>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("mysql"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql")));
});
var app = builder.Build();

app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();


// Mapeia todas as controllers da pasta Controllers
app.MapControllers();
#endregion
#region Home
//app.MapGet("/", () => "API - Módulo de Administração");

#endregion

#region Administradores
string GerarTokenJwt(Administrador administrador)
{
    if (string.IsNullOrEmpty(Key))
        return string.Empty;
    var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    var Claims = new List<Claim>
    {
        new Claim("Email", administrador.Email),
        new Claim(ClaimTypes.Role, administrador.Perfil)
    };
    var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
        claims: Claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );
    return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
}
// Login
app.MapPost("administradores/login", ([FromBody] LoginDTO loginDTO, iAdministradorServico administradorServico) =>
{
    var adm = administradorServico.Login(loginDTO);
    if (adm != null)
    {
        // if (administradorServico.Login(loginDTO) != null)
        string token = GerarTokenJwt(adm);
        return Results.Ok(new AdministradorLogado
        {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
        });
    }
    else
        return Results.Unauthorized();
}).AllowAnonymous().WithTags("Administradores");

// Listar todos (com paginação opcional)
app.MapGet("administradores", ([FromQuery] int? pagina, iAdministradorServico administradorServico) =>
{
    var adms = new List<AdministradorModelView>();
    var administradores = administradorServico.Todos(pagina);
    foreach (var Adm in administradores)
    {
        adms.Add(new AdministradorModelView
        {
            Id = Adm.Id,
            Email = Adm.Email,
            //Perfil = Enum.Parse<Perfil>(Adm.Perfil, ignoreCase: true), 
            Perfil = Adm.Perfil
        });
        Console.WriteLine(Adm.Perfil);
    }

    return Results.Ok(adms); // 👈 retorno para o Swagger
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm" }).WithTags("Administradores");


// app.MapGet("administradores", ([FromQuery] int? pagina, iAdministradorServico administradorServico) =>
// {
//     return Results.Ok(administradorServico.Todos(pagina));
// }).WithTags("Administradores");

// Buscar por ID
app.MapGet("administradores/{id}", ([FromRoute] int id, iAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscaPorId(id);
    if (administrador == null)
        return Results.NotFound();
    return Results.Ok((new AdministradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        //  Perfil = Enum.Parse<Perfil>(Adm.Perfil, ignoreCase: true), 
        Perfil = administrador.Perfil
    }));
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm" })
.WithTags("Administradores");

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
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
    };

    administradorServico.Incluir(administrador);

    if (administrador == null)
        return Results.StatusCode(500); // Ou outro tratamento de erro apropriado

    return Results.Created($"/administradores/{administrador.Id}", new AdministradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm" }).WithTags("Administradores");


app.MapDelete("Administradores/{id}", ([FromRoute] int id, iAdministradorServico administradorServico) =>
{
    var administradorExistente = administradorServico.BuscaPorId(id);
    if (administradorExistente == null)
        return Results.NotFound(new { mensagem = "Administrador não encontrado" });

    administradorServico.Apagar(administradorExistente);

    return Results.Ok(new { mensagem = "Excluído com sucesso" });
})
.RequireAuthorization().WithTags("Administradores");

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
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm,Editor" })
.WithTags("Veiculos");

app.MapGet("Veiculos", ([FromQuery] int? pagina, iVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);
    return Results.Ok(veiculos);
}).RequireAuthorization().WithTags("Veiculos");

app.MapGet("Veiculos{id}", ([FromRoute] int id, iVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
        return Results.NotFound();
    return Results.Ok(veiculo);
}).RequireAuthorization().WithTags("Veiculos");

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
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm" }).WithTags("Veiculos");


app.MapDelete("Veiculos/{id}", ([FromRoute] int id, iVeiculoServico veiculoServico) =>
{
    var veiculoExistente = veiculoServico.BuscaPorId(id);
    if (veiculoExistente == null)
        return Results.NotFound();

    veiculoServico.Apagar(veiculoExistente);
    return Results.NoContent();
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm" }).WithTags("Veiculos");
#endregion
#region app 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}

#endregion
app.Run();
