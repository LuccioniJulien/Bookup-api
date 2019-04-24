using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseApi.Dao;
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
                var queryTags = _context.Tags.GetNamesTags (word);
                var queryCategory = _context.Categories.GetNamesCategory (word);
                var query = queryTags.Union (queryCategory).Take (take);
                List<string> Tags = await query.ToListAsync ();
                return Ok (Format.ToMessage (Tags, 200));
            } catch (Exception e) {
                return StatusCode (500);
            }
        }

        [HttpGet ("[action]")]
        public async Task<ActionResult<IEnumerable<Tag>>> GetAll () {
            try {
                var queryTags = _context.Tags.GetNamesTags ();
                var queryCategory = _context.Categories.GetNamesCategory ();
                List<string> result = await (queryTags.Union (queryCategory)).ToListAsync ();
                return Ok (Format.ToMessage (result, 200));
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
                var queryTags = _context.Tags.GetNamesTags ();
                var queryCategory = _context.Categories.GetNamesCategory ();
                var query = queryTags.Union (queryCategory).Distinct ();

                int queryCount = query.Count ();
                if (number > queryCount) return BadRequest ("number is to hight".ToBadRequest ());
                var result = new List<string> ();

                for (int i = 0; i < number; i++) {
                    var random = new Random ();
                    int num = random.Next (queryCount - 1);
                    string tag = await query.Skip (num).FirstAsync ();
                    result.Add (tag);
                }

                return Ok (Format.ToMessage (result, 200));
            } catch (Exception e) {
                return StatusCode (500);
            }
        }
    }
}