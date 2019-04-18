using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BaseApi.Helper;
using BaseApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            /// optional, name of the category
            /// </param>
            /// <param name="author">
            /// optional, name of the author
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
            [HttpGet]
            public async Task<ActionResult<IEnumerable<Book>>> Get ([FromQuery] string author, [FromQuery] string category, [FromQuery] int skip = 0, [FromQuery] int take = 15, [FromQuery] string orderBy = "asc") {
                try {
                    // construction de la query
                    IQueryable<Book> queryOrdered = (orderBy.Equals ("asc") ? _context.Books.OrderBy (x => x.Title) : _context.Books.OrderByDescending (x => x.Title)).AsQueryable ();

                    if (!string.IsNullOrEmpty (category)) {
                        queryOrdered = queryOrdered.Intersect (_context.Categorizeds.Where (x => x.Category.Name == category)
                            .Select (c => c.Book));
                        queryOrdered = queryOrdered.Intersect (_context.Taggeds.Where (x => x.Tag.Name == category)
                            .Select (c => c.Book));
                    }
                    if (!string.IsNullOrEmpty (author)) {
                        queryOrdered = queryOrdered.Intersect (_context.Writtens.Where (x => x.Author.Name == author)
                            .Select (a => a.Book));
                    }

                    var limitedQuery = queryOrdered
                        .Skip (skip)
                        .Take (take)
                        .Select (x => new {
                            x.Id, x.Isbn, x.Title, x.Thumbnail, Tags = x.Categorized.Select (c => c.Category.Name).Union (x.Taggeds.Select (t => t.Tag.Name))
                        });

                    // la query est executée et le resultat est recuperé en mémoire
                    var books = await limitedQuery.ToListAsync ();
                    return Ok (Format.ToMessage (books, 200));
                } catch (Exception e) {
                    return StatusCode (500);
                }
            }

            /// <summary>
            /// Get book
            /// </summary>
            /// <param name="isbn">
            ///  required, isbn
            /// </param>
            /// <returns>Books</returns>
            /// <response code="200">Return book</response>
            /// <response code="404">Not found</response>
            [HttpGet ("{isbn}")]
            public async Task<ActionResult<Book>> Get (string isbn) {
                // construction de la query
                try {
                    var result = await _context.Books.Select (x => new {
                            x.Id, x.Isbn, x.Title, x.Thumbnail, x.Description, x.PublishedDate, Authors = x.Writtens.Select (a => a.Author.Name),
                                Tags = x.Categorized.Select (c => c.Category.Name).Union (x.Taggeds.Select (t => t.Tag.Name))
                                })
                        .FirstOrDefaultAsync (b => b.Isbn == isbn);
                        if (result == null) {
                            return NotFound ();
                        }
                        // result
                        // var book = new
                        return Ok (Format.ToMessage (result, 200));
                    }
                    catch (System.Exception) {
                        return StatusCode (500);
                    }

                }

                [HttpPost]
                public async Task<ActionResult<IEnumerable<Book>>> Post () {

                    return Ok (Format.ToMessage ("books", 200));
                }
            }
        }