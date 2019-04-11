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

    [Route ("api/[controller]")]
    public class HelloController : Controller {

        [HttpGet]
        public ActionResult<string> Get () {
            return "Hello word";
        }

    }
}