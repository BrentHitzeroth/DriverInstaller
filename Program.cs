using System;
using System.Diagnostics;

namespace DriverInstallerConsole
{
    internal class Program
    {
        static int Run(string fileName, string arguments, out string output, out string error)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var p = Process.Start(psi)!;
            output = p.StandardOutput.ReadToEnd();
            error = p.StandardError.ReadToEnd();
            p.WaitForExit();
            return p.ExitCode;
        }

        static int Main(string[] args)
        {
            // Usage:
            // DriverInstallerConsole.exe "C:\Drivers\MyDriver\mydriver.inf"
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: DriverInstallerConsole.exe <full-path-to-driver.inf>");
                return 1;
            }

            string infPath = args[0];

            Console.WriteLine($"Installing driver from: {infPath}");
            int code = Run("pnputil.exe", $"/add-driver \"{infPath}\" /install", out var installOut, out var installErr);

            Console.WriteLine(installOut);
            if (code != 0)
            {
                Console.Error.WriteLine("Install failed:");
                Console.Error.WriteLine(installErr);
                return code;
            }

            Console.WriteLine("Install command succeeded. Verifying driver store...");

            code = Run("pnputil.exe", "/enum-drivers", out var enumOut, out var enumErr);
            if (code != 0)
            {
                Console.Error.WriteLine("Verification failed (could not enumerate drivers):");
                Console.Error.WriteLine(enumErr);
                return code;
            }

            // Basic verification: INF filename appears in the driver list
            // (For stricter verification, match provider/version/class too.)
            string infFileName = System.IO.Path.GetFileName(infPath);
            bool found = enumOut.Contains(infFileName, StringComparison.OrdinalIgnoreCase);

            if (found)
            {
                Console.WriteLine($"Verified: {infFileName} found in driver store.");
                return 0;
            }

            Console.WriteLine($"Warning: {infFileName} not found in enum output.");
            return 2;
        }
    }
}