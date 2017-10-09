using System.Security.Cryptography.X509Certificates;

namespace RemoteWcfService.DataModels
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class RemConDB : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Device> Devices { get; set; }


        public RemConDB()
            : base("name=RemConDB")
        {
           
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
