using System;
using System.Collections.Generic;
using System.Linq;
using EfSqlite.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EfSqlite.Tests
{
    public class Test1 : IDisposable
    {
        private SqliteDbContext _ctx = new SqliteDbContext();

        [Fact]
        public async void Operation_TreeGraph()
        {
            CleanDb(_ctx);

            // Navigational property enables to create nested resp. dependent objects easily.
            // Note: Without this feature we have to create tree first
            // and use it's id to refer it in Leaf
            var trees = new[]
            {
                new Tree
                {
                    Name = "Hello",
                    Leaves =
                    {
                        new Leaf
                        {
                            Name = "Leaf 1"
                        },
                        new Leaf
                        {
                            Name = "Leaf 2"
                        },
                    }
                }
            };

            _ctx.AddRange(trees);
            _ctx.SaveChanges();

            Assert.Equal(2, await _ctx.Leaf.CountAsync());
            Assert.Equal(2, _ctx.Tree.Single().Leaves.Count);

            var tree = await _ctx.Tree.FirstAsync();
            
            // setting an empty list does not delete children but set it's foreign key to null
            tree.Leaves = new List<Leaf>();
            await _ctx.SaveChangesAsync();
            Assert.Equal(0, await _ctx.Leaf.CountAsync());

            await _ctx.Leaf
                .ForEachAsync(l => Assert.Null(l.Tree));
        }
        

        [Fact]
        public async void Operation_Cascading_TreeGraph()
        {
            CleanDb(_ctx);

            // Navigational property enables to create nested resp. dependent objects easily.
            // Note: Without this feature we have to create tree first
            // and use it's id to refer it in Leaf
            var trees = new[]
            {
                new Tree
                {
                    Name = "Hello",
                    Leaves =
                    {
                        new Leaf
                        {
                            Name = "Leaf 1"
                        },
                        new Leaf
                        {
                            Name = "Leaf 2"
                        },
                    }
                }
            };

            _ctx.AddRange(trees);
            _ctx.SaveChanges();
            
            var ctx = new SqliteDbContext();
            var treeNew = await ctx.Tree
                .Include(t => t.Leaves)
                .FirstAsync();

            ctx.Remove(treeNew);
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public void Validate_Annotation()
        {
            _ctx.Quote.Add(new Quote
            {
                Text = "ho"
            });
            _ctx.SaveChanges();
        }
        
        private static void CleanDb(SqliteDbContext ctx)
        {
            ctx.Database.ExecuteSqlRaw(InitDb.Query);
        }

        public void Dispose() => _ctx.Dispose();
    }

    static class ForEachExtension
    {
        public static void ForEach<T>(this IEnumerable<T> elements, Action<T> action)
        {
            foreach (var element in elements)
            {
                action(element);
            }
        }
    }
}            