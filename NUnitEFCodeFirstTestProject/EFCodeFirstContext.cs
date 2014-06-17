using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace NUnitEFCodeFirstTestProject
{
    class EFCodeFirstContext: DbContext
    {       
        public DbSet<HockeyEntity> Hockey { get; set; }
        public DbSet<PersonEntity> Person { get; set; }
        public DbSet<GameEntity> Game { get; set; }

        public EFCodeFirstContext()
            : base("EFCodeFirstTestFixture")
        {
            
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<HockeyEntity>().ToTable("HOCKEY", "HOCKEY");
            modelBuilder.Entity<PersonEntity>().ToTable("PERSON", "HOCKEY");
            modelBuilder.Entity<GameEntity>().ToTable("GAME", "HOCKEY");
        }
    }
}
