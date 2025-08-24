using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ModuloAPI.infraestrutura.DB
{
  
    public class DbContexto : DbContext
    {
          private readonly IConfiguration _configuracaoAppSettings;
        public DbContexto(IConfiguration configuracaoAppSettings)
        {
            _configuracaoAppSettings = configuracaoAppSettings;
        }
        public DbSet<Dominio.Entidades.Administrador> Administradores { get; set; } = default!;
        public DbSet<Dominio.Entidades.Veiculo> Veiculos { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dominio.Entidades.Administrador>().HasData(new Dominio.Entidades.Administrador
            {
                Id = 1,
                Email = "Administrador@teste.com",
                Senha = "123456",
                Perfil = "Adm"
            });
        }
    
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            {
                if (!optionsBuilder.IsConfigured)
                {
                    var connectionString = _configuracaoAppSettings.GetConnectionString("mysql")?.ToString();
                    if (!string.IsNullOrEmpty(connectionString))
                        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                    //else
                    // optionsBuilder.UseMySql("string de conexao", ServerVersion.AutoDetect("string de conexao"));
                }
            }
        }


    }

}