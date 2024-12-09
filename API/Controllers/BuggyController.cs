using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class BuggyController(DataContext context): BaseApiController
    {
        [HttpGet("auth")]
        public ActionResult<string> GetAuth()
        {
            return "secrect text";
        }

        [HttpGet("not-found")]
        public ActionResult<AppUser> GetNotFound()
        {
            var thing = context.Users.Find(-1);

            if(thing == null) return NotFound();

            return thing;
        }

        [HttpGet("server-error")]
        public ActionResult<AppUser> GetServerError()
        {
            var thing = context.Users.Find(-1) ?? throw new Exception("A bad thing has happened");
            return thing;
        }

        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest()
        {
            return BadRequest("This is a bad request");
        }
    }
}