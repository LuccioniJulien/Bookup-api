using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BaseApi.Models {
    public class Category {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }
        public ICollection<Categorized> Categorized { get; set; }

        public static IEnumerable<Category> GetCategoriesFromJSON (JToken json) {
            return json["volumeInfo"]["categories"].Select (x =>
                new Category { Name = (string) x }
            );
        }
    }
}