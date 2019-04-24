using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseApi.Dao {
    public static class BooksQuery {

        public static dynamic GetBookInfoAsync (this IQueryable<Book> query, string isbn) {
            return query.Select (x => new {
                x.Id, x.Isbn, x.Title, x.Thumbnail, x.Description, x.PublishedDate, Authors = x.Writtens.Select (a => a.Author.Name),
                    Tags = x.Categorized.Select (c => c.Category.Name).Union (x.Taggeds.Select (t => t.Tag.Name))
            }).FirstOrDefaultAsync (x => x.Isbn == isbn);
        }
        public static IQueryable<Book> GetBooks (this IQueryable<Book> query, string orderBy) {
            return (orderBy.Equals ("asc") ? query.OrderBy (x => x.Title) : query.OrderByDescending (x => x.Title)).AsQueryable ();
        }
        public static IQueryable<dynamic> GetListedBooks (this IQueryable<Book> query, int skip, int take) {
            return query.Skip (skip)
                .Take (take)
                .Select (x => new {
                    x.Id, x.Isbn, x.Title, x.Thumbnail, Tags = x.Categorized.Select (c => c.Category.Name).Union (x.Taggeds.Select (t => t.Tag.Name))
                });
        }
        public static IQueryable<Book> GetBooksByAuthor (this IQueryable<Written> query, string author) {
            return query.Where (x => x.Author.Name.ToUpper () == author.ToUpper ())
                .Select (a => a.Book);
        }
        public static IQueryable<Book> GetBooksByTag (this IQueryable<Tagged> query, string category) {
            return query.Where (x => x.Tag.Name.ToUpper () == category.ToUpper ())
                .Select (c => c.Book);
        }

        public static IQueryable<Book> GetBooksByTags (this IQueryable<Tagged> query, IEnumerable<string> category) {
            return query.Where (x => category.Contains (x.Tag.Name.ToUpper ()))
                .Select (c => c.Book);
        }
        public static IQueryable<Book> GetBooksByCategory (this IQueryable<Categorized> query, string category) {
            return query.Where (x => x.Category.Name.ToUpper () == category.ToUpper ())
                .Select (c => c.Book);
        }
        public static IQueryable<Book> GetBooksByCategories (this IQueryable<Categorized> query, IEnumerable<string> category) {
            return query.Where (x => category.Contains (x.Category.Name.ToUpper ()))
                .Select (c => c.Book);
        }
    }
}