using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BaseApi.Models {
    public class Tag {
        public Guid Id { get; set; }

        [Required][MinLength (3)][MaxLength (30)]
        public string Name { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
        public ICollection<Tagged> Taggeds { get; set; }
    }
}