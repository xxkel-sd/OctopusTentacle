using System;
using System.Text;
using Autofac.Core;
using Octopus.Diagnostics;
using Octopus.Shared.Diagnostics;

namespace Octopus.Shared.Startup
{
    public class ConsoleHost : ICommandHost, ICommandRuntime
    {
        readonly ILog log = Log.Octopus();
        readonly string displayName;

        public ConsoleHost(string displayName)
        {
            this.displayName = displayName;
        }

        public void Run(Action<ICommandRuntime> start, Action shutdown)
        {
            try
            {
                Console.ResetColor();
                Console.Title = displayName;

                start(this);

                Console.ResetColor();
                shutdown();
                Console.ResetColor();
            }
            catch (DependencyResolutionException rex) when (rex.InnerException is ControlledFailureException)
            {
                throw rex.InnerException;
            }
        }

        public void OnExit(int exitCode)
        {
            // Lower exit codes shouldn't be treated as unexpected errors, they're related to user input
            if (exitCode <= (int)OctopusProgram.ExitCode.ControlledFailureException) return;

            var sb = new StringBuilder()
                .AppendLine(new string('-', 79))
                .AppendLine($"Terminating process with exit code {exitCode}")
                .AppendLine("Full error details are available in the log files at:");
            foreach (var logDirectory in OctopusLogsDirectoryRenderer.LogsDirectoryHistory)
            {
                sb.AppendLine(logDirectory);
            }
            sb.AppendLine("If you need help, please send these log files to https://octopus.com/support");
            sb.AppendLine(new string('-', 79));
            log.Fatal(sb.ToString());
        }

        public void WaitForUserToExit()
        {
            Console.Title = displayName + " - Running";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Running. Press <enter> to shut down...");
            Console.ResetColor();

            while (true)
            {
                var line = (Console.ReadLine() ?? string.Empty).ToLowerInvariant();
                if (line == "cls" || line == "clear")
                {
                    Console.Clear();
                }
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }
            }

            Console.ResetColor();
            Console.Title = displayName + " - Shutting down...";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
        }
    }
}