using Microsoft.EntityFrameworkCore;

namespace Drzewo.Models
{
    public class TreeDbContext : DbContext
    {
        public DbSet<Node> Nodes { get; set; }

        public TreeDbContext(DbContextOptions<TreeDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Node>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasMany(x => x.Children)
                    .WithOne(x => x.Parent)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Example data
            modelBuilder.Entity<Node>().HasData(
                new Node
                {
                    Id = 1,
                    Name = "Program Files",
                    Depth = 1,
                    ParentId = null
                },
                new Node
                {
                    Id = 2,
                    Name = "Programy",
                    Depth = 1,
                    ParentId = null
                },
                new Node
                {
                    Id = 3,
                    Name = "Projekty",
                    Depth = 1,
                    ParentId = null
                },
                new Node
                {
                    Id = 4,
                    Name = "Adobe",
                    Depth = 2,
                    ParentId = 1
                },
                new Node
                {
                    Id = 5,
                    Name = "Microsoft",
                    Depth = 2,
                    ParentId = 1
                },
                new Node
                {
                    Id = 6,
                    Name = "Windows Defender",
                    Depth = 3,
                    ParentId = 5
                },
                new Node
                {
                    Id = 7,
                    Name = "Azure",
                    Depth = 3,
                    ParentId = 5
                },
                new Node
                {
                    Id = 8,
                    Name = "Temp",
                    Depth = 3,
                    ParentId = 5
                },
                new Node
                {
                    Id = 9,
                    Name = "MPC HC",
                    Depth = 2,
                    ParentId = 2
                },
                new Node
                {
                    Id = 10,
                    Name = "HoneyView",
                    Depth = 2,
                    ParentId = 2
                }
            );
        }
    }
}
