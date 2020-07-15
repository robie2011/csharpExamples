using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using EfSqlite.Tests;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EfSqlite
{
    public class SqliteInMemoryContextBuilder
    {
        private readonly IEnumerable<Type> _types;

        public SqliteInMemoryContextBuilder(IEnumerable<Type>? types = null)
            => _types = types ?? Enumerable.Empty<Type>();

        public SqliteInMemoryContextBuilder AddEntity<T>()
            => new SqliteInMemoryContextBuilder(_types.Append(typeof(T)));

        public SqliteDbContext Build(ILoggerFactory? loggerFactory = null)
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
                    .UseLoggerFactory(loggerFactory ?? new NullLoggerFactory())
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()
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