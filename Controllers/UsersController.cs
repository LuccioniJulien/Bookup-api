using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BaseApi.Classes;
using BaseApi.Helper;
using BaseApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Core;

namespace BaseApi.Controllers {
    [Authorize (AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces ("application/json")]
    [Route ("api/[controller]")]
    public class UsersController : Controller {
        private readonly DBcontext _context;
        private readonly Logger _log;
        public UsersController (DBcontext context, LoggerConfiguration config) {
            this._context = context;
            this._log = config.WriteTo.Console ()
                .CreateLogger ();
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
        /// <response code="201">Return the created User</response>
        /// <response code="400">If the User is null</response> 
        [AllowAnonymous]
        [HttpPost ("[action]")]
        public async Task<ActionResult<User>> Register ([FromBody] User user) {
            try {
                if (user == null) {
                    return BadRequest ("user object is null".ToBadRequest ());
                }

                _log.Information ("User trying to sign up, Body: {@body} on {date}", user.ToMessage (), DateTime.Now);

                if (!ModelState.IsValid) {
                    return BadRequest (ModelState.ToBadRequest (400));
                }

                var userWithSameEMail = await _context.Users.FirstOrDefaultAsync (u => u.Email == user.Email);
                if (userWithSameEMail != null) {
                    return BadRequest ("Email already taken".ToBadRequest ());
                }

                user.SetPasswordhHash ();
                _context.Add (user);
                await _context.SaveChangesAsync ();

                _log.Information ("User created : {@User} on {date} ", user.ToMessage (), DateTime.Now);
                // Envoi d'un mail
                bool isEmailSend = await MailerSendGrid.Send (user.Email);

                if (isEmailSend) {
                    _log.Information ("Email was sent for {User} on {date} ", user.Id, DateTime.Now);
                } else {
                    _log.Information ("Email was not sent for {User} on {date} ", user.Id, DateTime.Now);
                }

                return Created ("register", Format.ToMessage (user.ToMessage (), 201));
            } catch (Exception e) {
                _log.Fatal (e.Message + "on Register User on {Date}", DateTime.Now);
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
        /// <response code="201">Return the user and the jwt token</response>
        /// <response code="400">If the User is null, if the email is wrong</response>
        /// <response code="401">If the password is wrong</response> 
        [AllowAnonymous]
        [HttpPost ("[action]")]
        public async Task<ActionResult<string>> Auth ([FromBody] Login user) {
            try {
                if (user == null) {
                    return BadRequest ("Body is null".ToBadRequest ());
                }
                _log.Information ("User {email} trying to Sign in on {date} ", user.Email, DateTime.Now);
                if (!ModelState.IsValid) {
                    return BadRequest (ModelState.ToBadRequest ());
                }
                User userFromDb = await _context.Users.FirstOrDefaultAsync (u => u.Email == user.Email);
                if (userFromDb == null) {
                    return BadRequest ("Wrong email".ToBadRequest ());
                }
                if (!userFromDb.Compare (user.Password)) {
                    return Unauthorized ("Wrong password".ToBadRequest ());
                }
                var token = JWT.GetToken (userFromDb);
                _log.Information ("User {email} Sign in on {date} ", user.Email, DateTime.Now);
                return Created ("login", Format.ToMessage (userFromDb.ToMessage (), 201, token));
            } catch (Exception e) {
                _log.Fatal (e.Message + "on Auth User on {Date}", DateTime.Now);
                return StatusCode (500);
            }
        }

        [HttpGet]
        public async Task<ActionResult<User>> Get () {
            try {
                var uuid = Guid.Parse (User.Identity.Name);
                User userFromTokenId = await _context.Users.FirstOrDefaultAsync (u => u.Id == uuid);
                if (userFromTokenId == null) {
                    return Unauthorized ();
                }
                _log.Information ("Information  of {User} was sent on {date} ", userFromTokenId.Id, DateTime.Now);
                return Ok (Format.ToMessage (userFromTokenId.ToMessage (), 201));
            } catch (Exception e) {
                _log.Fatal (e.Message + "on Get User on {Date}", DateTime.Now);
                return StatusCode (500);
            }
        }

        /// <summary>
        ///  Change informations of an user
        /// </summary>
        /// <param name="infos">
        ///  {  name:string  , email:string }
        /// </param>
        /// <response code="201">Sucess</response>
        /// <response code="400">Bad request </response>
        /// <response code="401">If the jwt is wrong</response> 
        [HttpPatch]
        public async Task<ActionResult<User>> Patch ([FromBody] InfoUser infos) {
            try {
                var uuid = Guid.Parse (User.Identity.Name);
                User userFromTokenId = await _context.Users.FirstOrDefaultAsync (u => u.Id == uuid);

                if (userFromTokenId == null) {
                    return Unauthorized ();
                }
                if (!ModelState.IsValid) {
                    return BadRequest (ModelState.ToBadRequest ());
                }

                var (name, email) = infos;
                var userWithSameEmail = await _context.Users.FirstOrDefaultAsync (u => u.Email == email);
                if (userWithSameEmail != null && userWithSameEmail.Email != userFromTokenId.Email) {
                    return BadRequest ("Email is already in use".ToBadRequest ());
                }

                userFromTokenId.Name = name;
                userFromTokenId.Name = email;

                _context.Update (userFromTokenId);
                await _context.SaveChangesAsync ();
                _log.Information ("User {Email} has changed his/her information on {date} ", userFromTokenId.Email, DateTime.Now);
                return NoContent ();
            } catch (Exception e) {
                _log.Fatal (e.Message + "on Patch User on {Date}", DateTime.Now);
                return StatusCode (500);
            }
        }

        /// <summary>
        ///  Change password of an user
        /// </summary>
        /// <param name="passwords">
        ///  {  password:string  , passwordConfirmation:string }
        /// </param>
        /// <response code="201">success</response>
        /// <response code="400">Bad request </response>
        /// <response code="401">If the jwt is wrong</response> 
        [HttpPatch ("[action]")]
        public async Task<ActionResult<User>> ChangePassword ([FromBody] PasswordHelper passwords) {
            try {
                var uuid = Guid.Parse (User.Identity.Name);
                User userFromTokenId = await _context.Users.FirstOrDefaultAsync (u => u.Id == uuid);

                if (userFromTokenId == null) {
                    return Unauthorized ();
                }
                if (!ModelState.IsValid) {
                    return BadRequest (ModelState.ToBadRequest ());
                }

                var (password, _) = passwords;
                userFromTokenId.Password = password;
                userFromTokenId.SetPasswordhHash ();
                _context.Update (userFromTokenId);
                await _context.SaveChangesAsync ();
                _log.Information ("User {Email} has changed his/her password on {date} ", userFromTokenId.Email, DateTime.Now);
                return NoContent ();
            } catch (Exception e) {
                _log.Fatal (e.Message + "on ChangePassword User on {Date}", DateTime.Now);
                return StatusCode (500);
            }
        }

        /// <summary>
        ///  Set avatar url
        /// </summary>
        /// <param name="UplodedFile">
        ///  A png or jpg
        /// </param>
        /// <returns>url of the avatar</returns>
        /// <response code="201">Return the user updated</response>
        /// <response code="400">Bad request </response>
        /// <response code="401">If the jwt is wrong</response> 
        [HttpPut]
        public async Task<ActionResult> Put (FIleUploadAPI UplodedFile) {
            var uuid = Guid.Parse (User.Identity.Name);
            User userFromTokenId = await _context.Users.FirstOrDefaultAsync (u => u.Id == uuid);
            if (userFromTokenId == null) {
                return Unauthorized ();
            }

            if (UplodedFile.files == null) {
                return BadRequest ("No files found".ToBadRequest ());
            }
            var type = UplodedFile.files.ContentType;
            if (type != "image/jpeg" && type != "image/png" && type != "application/x-jpg") {
                return BadRequest ("Only image supported".ToBadRequest ());
            }
            try {
                string name = await AmazonS3Helper.SaveImageToBucket (UplodedFile.files);
                if (string.IsNullOrEmpty (name)) {
                    return BadRequest ("Save failed".ToBadRequest ());
                }
                userFromTokenId.SetAvatar (name);
                _context.Update (userFromTokenId);
                await _context.SaveChangesAsync ();
                _log.Information ("User {Email} has changed his/her avatar_url on {date} ", userFromTokenId.Email, DateTime.Now);
                return Created ("img", Format.ToMessage (userFromTokenId.ToMessage (), 201));
            } catch (Exception e) {
                _log.Fatal (e.Message + "on Put User on {Date}", DateTime.Now);
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