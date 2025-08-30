using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModuloAPI.Dominio.Enuns;

namespace ModuloAPI.Dominio.ModelViews
{
    public record AdministradorLogado
    {
        public int Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Perfil { get; set; } = default!;
        public string Token { get; set; } = default!;
    }
}