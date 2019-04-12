using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BaseApi.Models {
    public class Book {
        public Guid Id { get; set; }

        [Required]
        public string Isbn { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public ICollection<Tagged> Taggeds { get; set; }
        public ICollection<Written> Writtens { get; set; }
    }
}