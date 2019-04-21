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
                var queryCategory = _context.Categories.Where (x => x.Name.ToUpper ().Contains (word))
                    .Select (x => x.Name);
                var query = queryTags.Union (queryCategory);

                List<string> tags = await query.ToListAsync ();
                return Ok (Format.ToMessage (tags.Take (take), 200));
            } catch (Exception e) {
                return StatusCode (500);
            }
        }

        [HttpGet ("[action]")]
        public async Task<ActionResult<IEnumerable<Tag>>> GetAll () {
            try {
                var tags = _context.Categories.Select (x => x.Name);
                var categories = _context.Tags.Select (x => x.Name);
                var result = await (tags.Union (categories)).ToListAsync ();
                return Ok (Format.ToMessage (tags.Union (categories), 200));
            } catch (Exception e) {
                return StatusCode (500);
            }
        }

        /// <summary>
        /// Get random Tags and Categories
        /// </summary>
        /// <param name="number">
        /// Number of random tag to return
        /// </param>
        /// <returns>Tags and categories</returns>
        /// <response code="200">Return tags and gategories</response>
        /// <response code="400">Wrong number of tags</response>
        [HttpGet ("{number}")]
        public async Task<ActionResult<IEnumerable<Tag>>> Get (int number) {
            if (number < 1) return BadRequest ("number must be > 0".ToBadRequest ());
            try {
                var tags = await _context.Tags.Select (x => x.Name).ToListAsync ();
                var categories = await _context.Categories.Select (x => x.Name).ToListAsync ();
                tags.AddRange (categories);
                tags = tags.Distinct ().ToList ();
                if (number > tags.Count) return BadRequest ("number is to hight".ToBadRequest ());
                var result = new List<string> ();
                for (int i = 0; i < number; i++) {
                    var random = new Random ();
                    int num = random.Next (tags.Count - 1);
                    result.Add (tags[num]);
                }
                return Ok (Format.ToMessage (result, 200));
            } catch (Exception e) {
                return StatusCode (500);
            }
        }
    }
}