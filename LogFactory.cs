namespace W2PW
{
    using System;
    using Serilog;
    using Serilog.Events;

    public static class LogFactory
    {
        public static ILogger CreateLog()
        {
            //log messages can get pretty big, so it's nice to have a lot of space to view them:
            Console.WindowWidth = (int)(Console.LargestWindowWidth * 0.75);
            Console.BufferWidth = 300; //for extra long messages
            Console.WindowHeight = (int)(Console.LargestWindowHeight * 0.75);
            Console.BufferHeight = 2000; //so everything is visible

            var config = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.ColoredConsole(LogEventLevel.Debug);

            return config.CreateLogger();
        }
    }
}