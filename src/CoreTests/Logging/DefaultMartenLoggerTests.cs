using System;
using System.Text;
using Marten;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shouldly;
using Xunit;

namespace CoreTests.Logging
{
    public class DefaultMartenLoggerTests
    {
        [Fact]
        public void log_success()
        {
            var sb = new StringBuilder();
            var logger = new DefaultMartenLogger(new XunitLogger(sb));
            var command = new NpgsqlCommand()
            {
                CommandText = "select * from users where id = @id and name = @name",
                Parameters =
                {
                    new NpgsqlParameter("id", "{1}"),
                    new NpgsqlParameter("name", "{2}")
                }
            };
            logger.LogSuccess(command);

            var result = sb.ToString().Trim();
            result.ShouldBe($"Marten executed in 0 ms, SQL: select * from users where id = @id and name = @name\n  id: {{1}}{Environment.NewLine}  name: {{2}}");
        }

        [Fact]
        public void log_failure_should_not_throw()
        {
            var sb = new StringBuilder();
            var logger = new DefaultMartenLogger(new XunitLogger(sb));
            var command = new NpgsqlCommand()
            {
                CommandText = "select * from users where id = @id",
                Parameters =
                {
                        new NpgsqlParameter("id", "{1}")
                }
            };
            logger.LogFailure(command, new Exception());

            Assert.Equal("Marten encountered an exception executing \nselect * from users where id = @id\n  id: {1}", sb.ToString().Trim());
        }
    }

    internal class XunitLogger : ILogger
    {
        private readonly StringBuilder _sb;
        public XunitLogger(StringBuilder sb) => _sb = sb;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            _sb.AppendLine(message);
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();
    }
}
