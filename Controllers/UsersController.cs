using System;
using System.Linq;
using System.Threading.Tasks;
using BaseApi.Helper;
using BaseApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseApi.Controllers {
    [Authorize (AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces ("application/json")]
    [Route ("api/[controller]")]
    public class UsersController : Controller {
        private readonly DBcontext _context;

        public UsersController (DBcontext context) {
            this._context = context;
        }
        /// <summary>
        /// Register a User
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/users/Register
        ///     {
        ///        "name":"mouiaa",
        ///        "email":"juju@ju.ju,
        ///        "password":"Warcraft3?",
        ///        "passwordConfirmation":"Warcraft3?"
        ///     }
        /// 
        /// </remarks>
        /// <param name="user">
        /// { "name", "email", "password", "passwordConfirmation" }
        /// </param>
        /// <returns>A user</returns>
        /// <response code="201">Returns the created User</response>
        /// <response code="400">If the User is null</response> 
        [AllowAnonymous]
        [HttpPost ("[action]")]
        [ProducesResponseType (201)]
        [ProducesResponseType (400)]
        public async Task<ActionResult<User>> Register ([FromBody] User user) {
            try {
                if (user == null) {
                    return BadRequest ("user object is null".ToBadRequest ());
                }

                if (!ModelState.IsValid) {
                    return BadRequest (ModelState.ToBadRequest (400));
                }

                bool isEmailAlreadyTaken = await _context.Users.FirstOrDefaultAsync (u => u.Email == user.Email) != null;
                if (isEmailAlreadyTaken) {
                    return BadRequest ("Email already taken".ToBadRequest ());
                }

                user.SetPasswordhHash ();
                _context.Add (user);
                await _context.SaveChangesAsync ();
                return Created ("register", Format.ToMessage (user.ToMessage (), 201));
            } catch (Exception e) {
                return StatusCode (500);
            }
        }
        /// <summary>
        /// Login with a user
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/users/Auth
        ///     {
        ///        "email":"juju@ju.ju,
        ///        "password":"Warcraft3?",
        ///     }
        /// 
        /// </remarks>
        /// <param name="user">
        /// { "email", "password" }
        /// </param>
        /// <returns>A user</returns>
        /// <response code="201">Returns the user and the jwt token</response>
        /// <response code="400">If the User is null, if the email is wrong</response>
        /// <response code="401">If the password is wrong</response> 
        [AllowAnonymous]
        [HttpPost ("[action]")]
        [ProducesResponseType (200)]
        [ProducesResponseType (400)]
        [ProducesResponseType (401)]
        public async Task<ActionResult<string>> Auth ([FromBody] User user) {
            try {
                if (user == null) {
                    return BadRequest ();
                }

                User userFromDb = await _context.Users.FirstOrDefaultAsync (u => u.Email == user.Email);
                if (userFromDb == null) {
                    return BadRequest ("Wrong email".ToBadRequest ());
                }

                if (!userFromDb.Compare (user.Password)) {
                    return Unauthorized ("Wrong password".ToBadRequest ());
                }
                var token = JWT.GetToken (userFromDb);

                return Created ("login", Format.ToMessage (userFromDb.ToMessage (), 201, token));
            } catch (Exception e) {
                return StatusCode (500);
            }
        }

        [HttpPut ("{id}")]
        public async Task<ActionResult<string>> Put (Guid id, [FromBody] User user) {
            try {
                if (user == null) {
                    return BadRequest ("user object is null".ToBadRequest ());
                }

                var uuid = Guid.Parse (User.Identity.Name);
                var uuidFromQuery = id;
                User userFromDb = await _context.Users.FirstOrDefaultAsync (u => u.Id == uuidFromQuery);
                User userFromTokenId = await _context.Users.FirstOrDefaultAsync (u => u.Id == uuid);
                if ((userFromTokenId == null) || (userFromDb?.Id != userFromTokenId?.Id)) {
                    return Unauthorized ();
                }

                User userWithSameLogin = await _context.Users.FirstOrDefaultAsync (u => u.Email == user.Email);
                if (userWithSameLogin != null) {
                    return BadRequest ("Login already taken".ToBadRequest ());
                }

                if (!ModelState.IsValid) {
                    return BadRequest (ModelState.ToBadRequest ());
                }

                var (name, email, password, passwordConfirmation) = user;
                userFromDb.Name = name;
                userFromDb.Email = email;
                userFromDb.Password = password;
                userFromDb.PasswordConfirmation = passwordConfirmation;
                userFromDb.SetPasswordhHash ();

                _context.Update (userFromDb);
                await _context.SaveChangesAsync ();

                return StatusCode (201);
            } catch (Exception e) {
                return StatusCode (500);
            }
        }

        // [HttpDelete ("{id}")]
        // public async Task<ActionResult> Delete (Guid id) {
        //     try {
        //         var uuid = Guid.Parse (User.Identity.Name);
        //         var uuidFromQuery = id;

        //         User userFromDb = await _context.Users.FirstOrDefaultAsync (u => u.Id == uuidFromQuery);
        //         User userFromTokenId = await _context.Users.FirstOrDefaultAsync (u => u.Id == uuid);

        //         if ((userFromTokenId == null) || (userFromDb?.Id != userFromTokenId?.Id)) {
        //             return Unauthorized ();
        //         }

        //         _context.Remove (userFromDb);
        //         await _context.SaveChangesAsync ();

        //         return StatusCode (204);
        //     } catch (Exception e) {
        //         return StatusCode (500);
        //     }
        // }
    }
}