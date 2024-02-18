using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace Cashrewards3API.Tests.Helpers
{
    public class ConsoleLoggerMock<T> : Mock<ILogger<T>>
    {
        public LogLevel LogLevel { get; set; }

        public ConsoleLoggerMock(LogLevel logLevel = LogLevel.Information)
        {
            LogLevel = logLevel;

            Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Callback(new InvocationAction(invocation =>
                {
                    var logLevel = (LogLevel)invocation.Arguments[0];
                    var eventId = (EventId)invocation.Arguments[1];
                    var state = invocation.Arguments[2];
                    var exception = (Exception)invocation.Arguments[3];
                    var formatter = invocation.Arguments[4];
                    var invokeMethod = formatter.GetType().GetMethod("Invoke");
                    var logMessage = (string)invokeMethod?.Invoke(formatter, new[] { state, exception });
                    if (logLevel >= LogLevel)
                    {
                        Console.WriteLine($"[{logLevel}]: {logMessage}");
                    }
                }));
        }
    }
}
