using System.Collections.Generic;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace HRM_Practise.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<HRM_ATD_MachineData> HRM_ATD_MachineDatas { get; set; }
    }
}
