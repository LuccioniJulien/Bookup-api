using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using BaseApi.Helper;
using BaseApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseApi.Controllers {
    [Authorize (AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces ("application/json")]
    [Route ("api/[controller]")]
    public class UsersController : Controller {
        private readonly DBcontext _context;
        public IHostingEnvironment _environment;
        public UsersController (DBcontext context) {
            this._context = context;
            this._environment = new HostingEnvironment ();
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
                await MailerSendGrid.Send (user.Email);
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

        // [HttpPatch ("action]")]
        // public async Task<ActionResult<string>> Password ([FromBody] User user) {
        //     try {
        //         if (user == null) {
        //             return BadRequest ();
        //         }

        //         User userFromDb = await _context.Users.FirstOrDefaultAsync (u => u.Email == user.Email);
        //         if (userFromDb == null) {
        //             return BadRequest ("Wrong email".ToBadRequest ());
        //         }

        //         if (!userFromDb.Compare (user.Password)) {
        //             return Unauthorized ("Wrong password".ToBadRequest ());
        //         }
        //         var token = JWT.GetToken (userFromDb);

        //         return Created ("login", Format.ToMessage (userFromDb.ToMessage (), 201, token));
        //     } catch (Exception e) {
        //         return StatusCode (500);
        //     }
        // }

        [HttpGet]
        public async Task<ActionResult<User>> Get () {
            try {
                var uuid = Guid.Parse (User.Identity.Name);
                User userFromTokenId = await _context.Users.FirstOrDefaultAsync (u => u.Id == uuid);
                if (userFromTokenId == null) {
                    return Unauthorized ();
                }
                return Ok (Format.ToMessage (userFromTokenId.ToMessage (), 201));
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

        /// <summary>
        ///  Set avatar url
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
        /// <param name="files">
        ///  A png or jpg
        /// </param>
        /// <returns>url of the avatar</returns>
        /// <response code="201">Returns the user updated</response>
        /// <response code="400">Bad request </response>
        /// <response code="401">If the jwt is wrong</response> 
        [HttpPut]
        public async Task<ActionResult> Put (FIleUploadAPI files) {
            var uuid = Guid.Parse (User.Identity.Name);
            User userFromTokenId = await _context.Users.FirstOrDefaultAsync (u => u.Id == uuid);
            if (userFromTokenId == null) {
                return Unauthorized ();
            }
            if (files.files == null) {
                return BadRequest ("No files found".ToBadRequest ());
            }
            var type = files.files.ContentType;
            if (type != "image/jpeg" && type != "image/png" && type != "application/x-jpg") {
                return BadRequest ("Only image supported".ToBadRequest ());
            }
            try {
                var (id, key) = new AmazonCreditential ();
                using (var client = new AmazonS3Client (id, key, RegionEndpoint.EUWest3)) {
                    using (var newMemoryStream = new MemoryStream ()) {
                        var name = Guid.NewGuid ().ToString () + ".png";
                        files.files.CopyTo (newMemoryStream);
                        var uploadRequest = new TransferUtilityUploadRequest {
                            InputStream = newMemoryStream,
                            Key = name,
                            BucketName = "bookupstorapeapi",
                            CannedACL = S3CannedACL.PublicRead
                        };
                        var fileTransferUtility = new TransferUtility (client);
                        await fileTransferUtility.UploadAsync (uploadRequest);
                        string url = "https://s3.eu-west-3.amazonaws.com/bookupstorapeapi/" + name;
                        userFromTokenId.Avatar_url = url;
                        _context.Update (userFromTokenId);
                        await _context.SaveChangesAsync ();
                        return Created ("img", Format.ToMessage (userFromTokenId.ToMessage (), 201));
                    }
                }
            } catch (Exception e) {
                Console.WriteLine (e.Message);
                return StatusCode (500);
            }

        }
        // [AllowAnonymous]
        // [HttpPost]
        // public ActionResult<string> Post (FIleUploadAPI files) {
        //     try {
        //         if (!Directory.Exists (_environment.WebRootPath + "\\uploads\\")) {
        //             Directory.CreateDirectory (_environment.WebRootPath + "\\uploads\\");
        //         }
        //         using (FileStream filestream = System.IO.File.Create (_environment.WebRootPath + "\\uploads\\" + files.files.FileName)) {
        //             files.files.CopyTo (filestream);
        //             filestream.Flush ();
        //         }
        //         return Created ("image", "\\uploads\\" + files.files.FileName);
        //     } catch (Exception e) {
        //         return StatusCode (500);
        //     }
        // }

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