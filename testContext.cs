using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Test2
{
    public class testContext : DbContext
    {
        public testContext(DbContextOptions<testContext> options): base(options)
        {
        }
        public DbSet<Alpha> Alpha { get; set; }
    }

    public class Alpha
    {
        public int Id { get; set; }
        public string Msg { get; set; }
    }


}
