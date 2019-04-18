using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseApi.Helper;
using BaseApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseApi.Controllers {
    [Produces ("application/json")]
    [Route ("api/[controller]")]
    public class TagsController : Controller {
        private readonly DBcontext _context;
        public TagsController (DBcontext context) {
            this._context = context;
        }

        /// <summary>
        ///  Get Tags and Categories
        /// </summary>
        /// <param name="predicat">
        ///  predicat for the search
        /// </param>
        /// <param name="take">
        ///  limit, 6 by default, optional
        /// </param>
        /// <returns>Tags and categories</returns>
        /// <response code="200">Return tags and gategories</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> Get ([FromQuery] string predicat, [FromQuery] int take = 6) {
            try {
                var word = predicat?.ToUpper ();
                var queryTags = _context.Tags.Where (x => x.Name.ToUpper ().Contains (word))
                    .Select (x => x.Name);
                var test = queryTags.ToList ();
                var queryCategory = _context.Categories.Where (x => x.Name.ToUpper ().Contains (word))
                    .Select (x => x.Name);
                var test2 = queryCategory.ToList ();
                var query = queryTags.Union (queryCategory);

                List<string> tags = await query.ToListAsync ();
                return Ok (Format.ToMessage (tags.Take (take), 200));
            } catch (Exception e) {
                return StatusCode (500);
            }

        }
    }
}