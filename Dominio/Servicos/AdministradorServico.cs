using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModuloAPI.Dominio.Interfaces;
using ModuloAPI.infraestrutura.DB;
using ModuloAPI.Dominio.DTOs; // Add this line if LoginDTO is in this namespace
using ModuloAPI.Dominio.Entidades; // Add this line if Administrador is in this namespace

namespace ModuloAPI.Dominio.Servicos
{
    public class AdministradorServico : iAdministradorServico
    {
        private readonly DbContexto _contexto;
        public AdministradorServico(DbContexto contexto)
        {
            _contexto = contexto;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault(); 
            return adm;
       
            
        
        }
    }
}