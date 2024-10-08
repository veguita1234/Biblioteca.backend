﻿using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Repositories.Entities
{
    public class Solicitud
    {
        [Key]
        public Guid SolicitudId { get; set; }
        public string Tipo { get; set; }
        public DateTime Date {  get; set; }
        public string UserName { get; set; }
        public string Book { get; set; }
        public string? Observation { get; set; }
    }
}
