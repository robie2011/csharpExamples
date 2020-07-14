using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using EfSqlite.Tests;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace EfSqlite
{
    public class ContextBuilder
    {
        private readonly IEnumerable<Type> _types;

        public ContextBuilder(IEnumerable<Type>? types = null)
            => _types = types ?? Enumerable.Empty<Type>();

        public ContextBuilder AddEntity<T>()
            => new ContextBuilder(_types.Append(typeof(T)));

        public SqliteDbContext Build()
        {
            var sqliteConventions = SqliteConventionSetBuilder.Build();
            var modelBuilder = new ModelBuilder(sqliteConventions);
            _types.ForEach(t => modelBuilder.Entity(t));
            
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var optionsBuilder = new DbContextOptionsBuilder()
                    .UseSqlite(connection)
                    .UseModel(modelBuilder.FinalizeModel())
                    .UseSnakeCaseNamingConvention()
                ;
            return new SqliteDbContext(optionsBuilder.Options, connection);
        }

        // https://www.meziantou.net/testing-ef-core-in-memory-using-sqlite.htm
        public class SqliteDbContext : DbContext
        {
            private DbConnection _connection;

            public SqliteDbContext(DbContextOptions options, DbConnection connection)
            : base(options)
            {
                _connection = connection;
            }

            public override void Dispose()
            {
                base.Dispose();
                _connection.Dispose();
            }
        }
    }
}