using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModuloAPI.Dominio.Entidades;

namespace ModuloAPI.Dominio.Interfaces
{
    public interface iVeiculoServico
    {
        List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null);

        Veiculo? BuscaPorId(int id);

        void Incluir(Veiculo veiculo);

        void Alterar(Veiculo veiculo);
        
        void Apagar(Veiculo veiculo);
    }
}