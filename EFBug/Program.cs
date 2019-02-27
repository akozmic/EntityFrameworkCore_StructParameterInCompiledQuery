using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace EFBug
{
    class Program
    {
        public static readonly LoggerFactory LoggerFactory
            = new LoggerFactory(new ILoggerProvider[]
            {
                new DebugLoggerProvider()
            });

            
        static void Main(string[] args)
        {
            var options = new DbContextOptionsBuilder<BuggyDbContext>()
                    .UseLoggerFactory(LoggerFactory)
                    .UseSqlServer("Server=localhost;Database=BuggyDbContext;Trusted_Connection=True;")
                .Options;

            //this query uses a struct as an input parameter and does NOT work as expected.
            // it loads ALL users from the database and pages in-memory with potential app-busting perf impact
            var badQuery = EF.CompileQuery<BuggyDbContext, PagingOptions, IEnumerable<User>>((context, paging) =>
                context.Users
                    .OrderBy(u => u.LastName)
                    .Skip(paging.Skip)
                    .Take(paging.Take));

            //this query takes the 2 integers as input parameters instead of being baked into a struct and works 
            // as expected.
            var goodQuery = EF.CompileQuery<BuggyDbContext, int,int, IEnumerable<User>>((context, skip, take) =>
                context.Users
                    .OrderBy(u => u.LastName)
                    .Skip(skip)
                    .Take(take));

            using (var dbContext = new BuggyDbContext(options))
            {
                dbContext.Database.EnsureCreated();
                var pagingOptions = new PagingOptions {
                    Skip = 0,
                    Take = 25
                };

                //query results in logs:
                //Microsoft.EntityFrameworkCore.Query:Warning: The LINQ expression 'Skip(__paging.Skip)' could not be translated and will be evaluated locally.
                //Microsoft.EntityFrameworkCore.Query:Warning: The LINQ expression 'Take(__paging.Take)' could not be translated and will be evaluated locally.
                var firstPageEvaluatedInMemory = badQuery.Invoke(dbContext, pagingOptions).ToList();
                
                //query works as expected:
                var firstPageEvaluatedInSql = goodQuery.Invoke(dbContext, pagingOptions.Skip, pagingOptions.Take).ToList();
            }
        }

    }
}
