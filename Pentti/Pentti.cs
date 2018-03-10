using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Pentti
{
    public static class POs
    {
        public static string GetCwd() => Directory.GetCurrentDirectory();
        public static void Chdir(string pth) => Directory.SetCurrentDirectory(pth);
    }
    public static class PSubprocess
    {
        public static bool Verbose = false;

        private static void PatchProcInfo(ProcessStartInfo pi)
        {
            pi.UseShellExecute = false;
            pi.RedirectStandardOutput = true;
            pi.RedirectStandardError = true;
            pi.WindowStyle = ProcessWindowStyle.Hidden;
            pi.WorkingDirectory = Directory.GetCurrentDirectory();
        }

        // run command, return exit code, stdout and stderr
        public static (int ExitCode, ICollection<string> @out, ICollection<string> err) Exec(string cmd, string args)
        {
            if (Verbose) {
                Console.WriteLine($"> {cmd} {args}");
            }

            using (var proc = new Process())
            {
                PatchProcInfo(proc.StartInfo);
                var @out = new List<string>();
                var err = new List<string>();
                proc.StartInfo.FileName = cmd;
                proc.StartInfo.Arguments = args;
                proc.OutputDataReceived += new DataReceivedEventHandler((sender, d) =>
                {
                    if (!string.IsNullOrEmpty(d.Data))
                    {
                        @out.Add(d.Data);
                    }

                });
                proc.ErrorDataReceived += new DataReceivedEventHandler((sender, d) =>
                {
                    if (!string.IsNullOrEmpty(d.Data))
                    {
                        err.Add(d.Data);
                    }
                });
                var started = proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
                return (proc.ExitCode, @out, err);
            }
        }

        public static ICollection<string> CheckOutput(string cmd, string args)
        {
            var (status, @out, err) = Exec(cmd, args);
            if (status != 0)
            {
                throw new SystemException($"Command failed: ${cmd} {args}: {String.Concat(err)}");
            }
            return @out;
        }


    }
}
