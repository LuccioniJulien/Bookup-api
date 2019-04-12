using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BaseApi.Models {
    public class Author {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }
        public ICollection<Written> Writtens { get; set; }
    }
}