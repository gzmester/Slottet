using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class PhoneNumber
    {
        public int Id { get; set; }
        public required string Number { get; set; }
        public string? AssignedTo { get; set; }
    }
}