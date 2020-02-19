using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Test2;

namespace Test2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlphasController : ControllerBase
    {
        private readonly testContext _context;

        public AlphasController(testContext context)
        {
            _context = context;
        }

        // GET: api/Alphas
        [HttpGet]
        public IActionResult Get()
        {
            var msg = _context.Alpha.Select(p => p).ToList();
            return Ok(msg);
        }
        // GET: api/Alphas/id
        [HttpGet("{id}")]
    public IActionResult Get(int id)
        {
            var msg = _context.Find<Alpha>(id);

            return Ok(msg);
        }
        // Post: api/Alphas
        [HttpPost]
        public IActionResult Post([FromBody] Alpha a)
        {
            _context.Add(a);

            _context.SaveChanges();

            return CreatedAtRoute(new { id = a.Id }, a);
        }
    }
}
