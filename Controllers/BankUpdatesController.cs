using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Test2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankUpdatesController : ControllerBase
    {
        private readonly BankMasterLocalContext _context;

        public BankUpdatesController(BankMasterLocalContext context)
        {
            _context = context;
        }
        //no updates in controller only get data and returns to nikshay
        //Get: api/BankUpdates
        [HttpGet]
        public IActionResult Get()
        {
            //get list on new branches from pfms and check with bankpfmscopy if not present add them and send them to nikshay
            //if present then check if details have been updated if so then update branch row
            //get list of updated banks
            TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            var date_mmddyyyy = indianTime.ToShortDateString();
            var formatDate = date_mmddyyyy.Split("/");
            formatDate[0] = formatDate[0].Length == 1 ? $"0{formatDate[0]}" : formatDate[0];
            formatDate[1] = formatDate[1].Length == 1 ? $"0{formatDate[1]}" : formatDate[1];
            var formattedDate = $"{formatDate[2]}-{formatDate[0]}-{formatDate[1]}";
            //var updatedBranches = _context.BankPfmsCopy.Where(b => b.IsModified == true).Select(b => b).ToList();
            return Ok(formattedDate);
        }

    }
}