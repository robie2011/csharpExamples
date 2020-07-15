using System;
using System.Linq;
using EfSqlite.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using static EfSqlite.Entities.InitDb;

namespace EfSqlite.Tests
{
    public class InverseNavigationPropertyTest
    {
        [Fact]
        public void AccessInverseProperty_InverseAvailable_CanNavigate()
        {
            using var context = new SqliteInMemoryContextBuilder()
                .AddEntity<Tree>()
                .AddEntity<Leaf>()
                .Build();
            
            context.Database.ExecuteSqlRaw(ReCreateTreeTable);
            context.Database.ExecuteSqlRaw(ReCreateLeafTable);

            var trees = Enumerable.Range(1, 3)
                .Select(treeId => new Tree
                {
                    Name = $"Tree {treeId}",
                    Leaves = Enumerable.Range(1, 100).Select(leafId => new Leaf
                    {
                        Name = $"Leaf {treeId}.{leafId}"
                    }).ToList() 
                });
            
            context.AddRange(trees);
            context.SaveChanges();

            var x = context.Set<Tree>()
                .Where(t => t.Id == 1)
                .SelectMany(t => t.Leaves)
                .OrderBy(l => l.Id)
                .Take(10);

            // testing complex navigation query
            Assert.Equal(
                100, 
                context.Set<Tree>()
                    .First()
                    .Leaves
                    .Single(l => l.Name == "Leaf 1.10")
                    .Tree
                    .Leaves
                    .Count);
        }
    }
}