using System.Collections.Generic;
using System.Linq;
using EfSqlite.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static EfSqlite.Entities.InitDb;

namespace EfSqlite.Tests
{
    public class CreateContextTest
    {
        [Fact]
        public void Persistence_PersistsData_CanLoadSavedData()
        {
            using var context = new ContextBuilder()
                .AddEntity<Tree>()
                .AddEntity<Leaf>()
                .Build();
            
            context.Database.ExecuteSqlRaw(ReCreateTreeTable);
            context.Database.ExecuteSqlRaw(ReCreateLeafTable);
            var treeWithLeafs = CreateTreeWithTwoLeafs();
            context.Add(treeWithLeafs);
            context.SaveChanges();
            
            Assert.Equal(1, context.Set<Tree>().Count());
            Assert.Equal(2, context.Set<Leaf>().Count());
            
            Assert.Equal(2, context.Set<Tree>().Single().Leaves.Count);
            Assert.Equal(2, context.Set<Leaf>().First().Tree.Leaves.Count);
        }

        [Fact]
        public void Persistence_DifferentContexts_HasNoSharedState()
        {
            using var context = new ContextBuilder()
                .AddEntity<Tree>()
                .AddEntity<Leaf>()
                .Build();
            
            context.Database.ExecuteSqlRaw(ReCreateTreeTable);
            context.Database.ExecuteSqlRaw(ReCreateLeafTable);
            context.Add(CreateTreeWithTwoLeafs());
            context.SaveChanges();
            
            using var context2 = new ContextBuilder()
                .AddEntity<Tree>()
                .AddEntity<Leaf>()
                .Build();
            context2.Database.ExecuteSqlRaw(ReCreateTreeTable);
            context2.Database.ExecuteSqlRaw(ReCreateLeafTable);

            Assert.Equal(0, context2.Set<Tree>().Count());
            Assert.Equal(0, context2.Set<Leaf>().Count());
        }

        private static Tree CreateTreeWithTwoLeafs()
        {
            return new Tree
            {
                Name = "Check",
                Leaves = new List<Leaf>
                {
                    new Leaf
                    {
                        Name = "L1"
                    },
                    new Leaf
                    {
                        Name = "L2"
                    }
                }
            };
        }
    }
}