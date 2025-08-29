using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModuloAPI.Dominio.DTOs;
using ModuloAPI.Dominio.Entidades;

namespace ModuloAPI.Dominio.Interfaces
{
    public interface iAdministradorServico
    {
        Administrador Login(LoginDTO loginDTO);

        Administrador Incluir(Administrador administrador);

        List<Administrador> Todos(int? pagina);

        Administrador? BuscaPorId(int id);
        // Administrador ObterPorId(int id);
        void Apagar(Administrador administrador);
    }
}