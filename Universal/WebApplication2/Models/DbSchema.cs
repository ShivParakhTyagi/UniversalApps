using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace WebApplication2.Models
{
    public class DbSchema : DbContext
    {
        public DbSchema() : base($"name=DbConnection")
        {
            Database.SetInitializer<DbSchema>(null);
        }

        public virtual DbSet<Users> Users { get; set; }
    }

    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Username { get; set; }
        public DateTime CreateTime { get; set; }
    }
}