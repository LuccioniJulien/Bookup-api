using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BaseApi.Dao;
using BaseApi.Helper;
using BaseApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Core;

namespace BaseApi.Controllers {
    // [Authorize (AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces ("application/json")]
    [Route ("api/[controller]")]
    public class BooksController : Controller {

        private readonly DBcontext _context;
        private readonly Logger _log;
        public BooksController (DBcontext context, LoggerConfiguration config) {
            this._context = context;
            this._log = config.WriteTo.Console ()
                .CreateLogger ();
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
        /// <response code="200">Return books</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> Get ([FromQuery] string author, [FromQuery] string category, [FromQuery] int skip = 0, [FromQuery] int take = 15, [FromQuery] string orderBy = "asc") {
            try {
                _log.Information ("Get Books requested on {Date}", DateTime.Now);
                // construction de la requête
                IQueryable<Book> queryOrdered = _context.Books.GetBooks (orderBy);

                if (!string.IsNullOrEmpty (category)) {
                    var queryBooksByCategory = _context.Categorizeds.GetBooksByCategory (category);
                    var queryBooksByTag = _context.Taggeds.GetBooksByTag (category);
                    var querybookByTagAndCategory = queryBooksByCategory.Union (queryBooksByTag).Distinct ();
                    queryOrdered = queryOrdered.Intersect (querybookByTagAndCategory);
                }
                if (!string.IsNullOrEmpty (author)) {
                    queryOrdered = queryOrdered.Intersect (_context.Writtens.GetBooksByAuthor (author));
                }
                var limitedQuery = queryOrdered.GetListedBooks (skip, take);
                // la requête est executée et le resultat est recuperé en mémoire
                var books = await limitedQuery.ToListAsync ();
                return Ok (Format.ToMessage (books, 200));
            } catch (Exception e) {
                _log.Fatal (e.Message + "on Get Books on {Date}", DateTime.Now);
                return StatusCode (500);
            }
        }

        /// <summary>
        /// Get books
        /// </summary>
        /// <param name="categories">
        /// string with category concatened by a ',' : category=leto,scout
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
        /// <response code="200">Return books</response>
        [HttpGet ("[action]")]
        public async Task<ActionResult<IEnumerable<Book>>> GetByCategories ([FromQuery] string categories, [FromQuery] int skip = 0, [FromQuery] int take = 15, [FromQuery] string orderBy = "asc") {
            try {
                _log.Information ("GetByCategories Books requested on {Date}", DateTime.Now);
                // construction de la query
                IQueryable<Book> queryOrdered = _context.Books.GetBooks (orderBy);

                if (!string.IsNullOrEmpty (categories)) {
                    IEnumerable<string> categoriesUpper = categories.Split (",").Select (x => x.ToUpper ());
                    var queryBooksByCategories = _context.Categorizeds.GetBooksByCategories (categoriesUpper);
                    var queryBooksByTags = _context.Taggeds.GetBooksByTags (categoriesUpper);
                    var querybookByTagAndCategory = queryBooksByCategories.Union (queryBooksByTags).Distinct ();
                    queryOrdered = queryOrdered.Intersect (querybookByTagAndCategory);
                } else {
                    return BadRequest ("no categories found".ToBadRequest ());
                }
                var limitedQuery = queryOrdered.GetListedBooks (skip, take);
                // la query est executée et le resultat est recuperé en mémoire
                var books = await limitedQuery.ToListAsync ();
                return Ok (Format.ToMessage (books, 200));
            } catch (Exception e) {
                _log.Fatal (e.Message + "on GetByCategories Books on {Date}", DateTime.Now);
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
            try {
                _log.Information ("Get by isbn Books requested on {Date}", DateTime.Now);
                var result = await _context.Books.GetBookInfoAsync (isbn);
                if (result == null) {
                    var isFound = await Book.SaveNewBookFromGoogle (isbn: isbn);

                    if (!isFound) return NotFound ();

                    result = await _context.Books.GetBookInfoAsync (isbn);
                }
                return Ok (Format.ToMessage (result, 200));
            } catch (Exception e) {
                _log.Fatal (e.Message + "on Register User on {Date}", DateTime.Now);
                return StatusCode (500);
            }
        }

        /// <summary>
        /// Add a tag to a book
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT /api/Books
        ///     {
        ///        "name":"Humour Noir",
        ///     }
        /// 
        /// </remarks>
        /// <param name="tag">
        ///  a tag, required : { name:string }
        /// </param>
        /// <param name="id">
        ///  required: id:int
        /// id of the book
        /// </param>
        /// <returns>Tag</returns>
        /// <response code="201">Return message "Created"</response>
        /// <response code="400">Bad request</response>
        [HttpPut ("{id}")]
        public async Task<ActionResult> Put (Guid id, [FromBody] Tag tag) {
            try {
                _log.Information ("Add tag to Books requested on {Date}", DateTime.Now);
                var tagFromDb = await _context.Tags.FirstOrDefaultAsync (x => x.Name == tag.Name);
                var bookFromDb = await _context.Books.FirstOrDefaultAsync (x => x.Id == id);

                if (bookFromDb == null) {
                    return BadRequest ("Book not found".ToBadRequest ());
                }

                if (tagFromDb != null) {
                    var asso = await _context.Taggeds.FirstOrDefaultAsync (x => x.BookId == id || x.TagId == tagFromDb.Id);

                    if (asso != null) return Created ("Tag Already exist", Format.ToMessage ("Created", 201));

                    var newAssociation = new Tagged { TagId = tagFromDb.Id, BookId = id };
                    _context.Taggeds.Add (newAssociation);
                    await _context.SaveChangesAsync ();
                    return Created ("Add tag", Format.ToMessage ("Created", 201));
                }

                if (!ModelState.IsValid) {
                    return BadRequest (ModelState.ToBadRequest (400));
                }

                _context.Tags.Add (tag);
                await _context.SaveChangesAsync ();

                var newTagged = new Tagged { TagId = tag.Id, BookId = id };
                _context.Taggeds.Add (newTagged);
                await _context.SaveChangesAsync ();

                return Created ("Add tag", Format.ToMessage ("Created", 201));
            } catch (Exception e) {
                _log.Fatal (e.Message + "on Get Books on {Date}", DateTime.Now);
                return StatusCode (500);
            }
        }
    }
}