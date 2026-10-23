using System;
using System.Diagnostics;
using System.Linq;

class Program
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

    static void Main()
    {
        // Path to your driver INF (not .cat)
        string infPath = @"C:\Drivers\MyDriver\mydriver.inf";

        // 1) Install/add driver package
        int code = Run("pnputil.exe", $"/add-driver \"{infPath}\" /install", out var out1, out var err1);
        Console.WriteLine(out1);
        if (code != 0)
        {
            Console.Error.WriteLine(err1);
            throw new Exception($"Driver install failed. ExitCode={code}");
        }

        // 2) Verify it was staged/installed (basic check)
        // Optional: filter by original INF file name after install (oemXX.inf mapping will be shown)
        code = Run("pnputil.exe", "/enum-drivers", out var out2, out var err2);
        if (code != 0)
        {
            Console.Error.WriteLine(err2);
            throw new Exception($"Driver enumeration failed. ExitCode={code}");
        }

        bool found = out2.Contains("mydriver.inf", StringComparison.OrdinalIgnoreCase) ||
                     out2.Contains("Provider Name", StringComparison.OrdinalIgnoreCase); // replace with stricter matching

        Console.WriteLine(found
            ? "Driver package appears in driver store."
            : "Driver package not found in enum output.");
    }
}