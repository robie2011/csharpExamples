using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace EfSqlite.Tests
{
    public class InverseNavigationExample
    {
        private readonly ITestOutputHelper _output;

        public InverseNavigationExample(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public void UseNavigationProperty()
        {
            using var context = new SqliteInMemoryContextBuilder()
                .AddEntity<Forum>()
                .AddEntity<Thread>()
                .AddEntity<Comment>()
                .Build(GetLoggerFactory());
            context.Database.EnsureCreated();
            context.AddRange(GenerateTestData());
            context.SaveChanges();

            var top10Comments = context.Set<Comment>()
                .Where(c => c.Thread.ForumId == 3)
                .OrderByDescending(c => c.Id)
                .Take(10)
                .ToList();
            
            /*
             generated query:
                  SELECT "c"."Id", "c"."Text", "c"."ThreadId"
                  FROM "Comment" AS "c"
                  INNER JOIN "Thread" AS "t" ON "c"."ThreadId" = "t"."Id"
                  WHERE "t"."ForumId" = 3
                  ORDER BY "c"."Id" DESC
                  LIMIT @__p_0
             */
            
            Assert.Equal(10 , top10Comments.Count);
        }
        
        [Fact]
        public void UseInverseNavigationProperty_UseSelectMany()
        {
            using var context = new SqliteInMemoryContextBuilder()
                .AddEntity<Forum>()
                .AddEntity<Thread>()
                .AddEntity<Comment>()
                .Build(GetLoggerFactory());
            context.Database.EnsureCreated();
            context.AddRange(GenerateTestData());
            context.SaveChanges();

            var top10Comments = context.Set<Forum>()
                .Where(f => f.Id == 3)
                .SelectMany(f => f.Threads.SelectMany(t => t.Comments))
                .OrderByDescending(c => c.Id)
                .Take(10)
                .ToList();
            /*
             generated query:
                  SELECT "t0"."Id", "t0"."Text", "t0"."ThreadId"
                  FROM "Forum" AS "f"
                  INNER JOIN (
                      SELECT "c"."Id", "c"."Text", "c"."ThreadId", "t"."Id" AS "Id0", "t"."ForumId"
                      FROM "Thread" AS "t"
                      INNER JOIN "Comment" AS "c" ON "t"."Id" = "c"."ThreadId"
                  ) AS "t0" ON "f"."Id" = "t0"."ForumId"
                  WHERE "f"."Id" = 3
                  ORDER BY "t0"."Id" DESC
                  LIMIT @__p_0
             */
            
            Assert.Equal(10 , top10Comments.Count);
        }
        

        [Fact]
        public void UseInverseNavigationProperty_UseCustomJoin()
        {
            using var context = new SqliteInMemoryContextBuilder()
                .AddEntity<Forum>()
                .AddEntity<Thread>()
                .AddEntity<Comment>()
                .Build(GetLoggerFactory());
            context.Database.EnsureCreated();
            context.AddRange(GenerateTestData());
            context.SaveChanges();
            var top10Comments = context.Set<Forum>()
                .Where(f => f.Id == 3)
                .Join(context.Set<Thread>(),
                    forum => forum.Id,
                    thread => thread.Id,
                    (_, thread) => thread.Id)
                .Join(context.Set<Comment>(),
                    threadId => threadId,
                    comment => comment.ThreadId,
                    (_, comment) => comment
                    )
                .OrderByDescending(c => c.Id)
                .Take(10)
                .ToList();

            /*
             generated query:
                  SELECT "c"."Id", "c"."Text", "c"."ThreadId"
                  FROM "Forum" AS "f"
                  INNER JOIN "Thread" AS "t" ON "f"."Id" = "t"."Id"
                  INNER JOIN "Comment" AS "c" ON "t"."Id" = "c"."ThreadId"
                  WHERE "f"."Id" = 3
                  ORDER BY "c"."Id" DESC
                  LIMIT @__p_0
             */
            
            Assert.Equal(10 , top10Comments.Count);
        }
        
        private ILoggerFactory GetLoggerFactory()
            => LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.AddXUnit(_output);
            });

        private static IEnumerable<Forum> GenerateTestData(int nForums = 10, int nThreads = 10, int nComments = 10) =>
            Enumerable.Range(1, nForums).Select(forumId =>
                new Forum
                {
                    Name = $"Forum {forumId}",
                    Threads = Enumerable.Range(1, nThreads).Select(threadId => new Thread
                    {
                        Name = $"Thread {forumId}.{threadId}",
                        Comments = Enumerable.Range(1, nComments).Select(commentId => new Comment
                        {
                            Text = $"Comment {forumId}.{threadId}.{commentId}"
                        }).ToList()
                    }).ToList()
                });

        internal sealed class Forum
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }

            public string Name { get; set; }
            
            public ICollection<Thread> Threads { get; set; }
        }

        internal sealed class Thread
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }

            public string Name { get; set; }
            
            public int ForumId { get; set; }
            public Forum Forum { get; set; }
            
            public ICollection<Comment> Comments { get; set; }
        }

        internal sealed class Comment
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }

            public string Text { get; set; }
            
            public int ThreadId { set; get; }
            
            public Thread Thread { get; set; }
        }
    }
}