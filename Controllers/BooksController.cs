using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BaseApi.Helper;
using BaseApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BaseApi.Controllers {
    // [Authorize (AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces ("application/json")]
    [Route ("api/[controller]")]
    public class BooksController : Controller {

        private readonly DBcontext _context;
        public BooksController (DBcontext context) {
            this._context = context;
        }
        /// <summary>
        /// Get books
        /// </summary>
        /// <param name="category">
        /// optional
        /// </param>
        /// <param name="skip">
        /// Skip
        /// </param>
        /// <param name="take">
        /// Max
        /// </param>
        /// <param name="orderBy">
        /// asc or desc
        /// </param>
        /// <returns>Books</returns>
        /// <response code="200">Returns books</response>
        /// <response code="400"></response>
        [HttpGet]
        public ActionResult<IEnumerable<Book>> Get ([FromQuery] string category, [FromQuery] int skip = 0, [FromQuery] int take = 15, [FromQuery] string orderBy = "asc") {
            // construction de la query
            IQueryable<Book> query;
            IOrderedQueryable<Book> queryOrdered;

            if (!string.IsNullOrEmpty (category)) {
                Guid? categoryId = _context.Categories.FirstOrDefault (x => x.Name == category)?.Id;
                query = _context.Categorizeds.Where (x => x.CategoryId == categoryId).Select (c => c.Book);
                queryOrdered = (orderBy.Equals ("asc") ? query.OrderBy (x => x.Title) : query.OrderByDescending (x => x.Title));
            } else {
                queryOrdered = (orderBy.Equals ("asc") ? _context.Books.OrderBy (x => x.Title) : _context.Books.OrderByDescending (x => x.Title));
            }

            var limitedQuery = queryOrdered
                .Skip (skip)
                .Take (take)
                .Select (x => new { x.Id, x.Isbn, x.Title, x.Thumbnail, x.PublishedDate, Description = x.Description ?? "", Categories = x.Categorized.Select (c => new { c.Category.Id, c.Category.Name }) });

            // la query est executée et le resultat est recuperé en mémoire
            var books = limitedQuery.ToList ();
            return Ok (Format.ToMessage (books, 200));
        }
    }
}