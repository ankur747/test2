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
    public class BankPfmsCopiesController : ControllerBase
    {
        private readonly BankMasterLocalContext _context;

        public BankPfmsCopiesController(BankMasterLocalContext context)
        {
            _context = context;
        }
        
        // GET: api/BankPfmsCopies
        [HttpGet]
        public IActionResult Get()
        {
            var msg = _context.BankPfmsCopy.Select(p => p).ToList();
            return Ok(msg);
        }
    }
}
