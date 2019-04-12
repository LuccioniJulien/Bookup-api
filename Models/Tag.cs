using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BaseApi.Models {
    public class Tag {
        public Guid Id { get; set; }

        [Required]
        public string Email { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
        public ICollection<Tagged> Taggeds { get; set; }
    }
}