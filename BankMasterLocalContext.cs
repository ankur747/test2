using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test2
{
    public class BankMasterLocalContext : DbContext
    {
        public BankMasterLocalContext(DbContextOptions<BankMasterLocalContext> options) : base(options)
        {
        }
        public DbSet<BankPfmsCopy> BankPfmsCopy { get; set; }
        public DbSet<UpdateLog> UpdateLog { get; set; }
    }

    public class UpdateLog
    {
        public int Id { get; set; }
        public Boolean Success { get; set; }
        public System.DateTime UpdatedAt { get; set; }
    }

    public class BankPfmsCopy
    {
        public int Id { get; set; }
        public string BankName { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public string IfscCode { get; set; }
        public string Address { get; set; }
        public Boolean IsActive { get; set; }
        public Boolean IsModified { get; set; }
        public Boolean IsChecked { get; set; }



    }

}
