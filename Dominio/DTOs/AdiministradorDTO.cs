using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModuloAPI.Dominio.Enuns;

namespace ModuloAPI.Dominio.DTOs
{
    public class AdiministradorDTO
    {

        public string Email { get; set; } = default!;
        public string Senha { get; set; } = default!;
        public Perfil? Perfil { get; set; } = default!;

    }
}