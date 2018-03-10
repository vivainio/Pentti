using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Pentti
{
    // stuff familiar from python "os", "os.path" and "shutil" modules
    public static class POs
    {
        public static string GetCwd() => Directory.GetCurrentDirectory();
        public static void Chdir(string pth) => Directory.SetCurrentDirectory(pth);
        public static string[] ListDir(string path) => Directory.GetFileSystemEntries(path);
        public static IEnumerable<string> Walk(string path) => Directory.EnumerateFileSystemEntries(path, "*.*", SearchOption.AllDirectories);
        public static bool IsFile(string path) => File.Exists(path);
        public static bool IsDir(string path) => Directory.Exists(path);
        public static void MakeDirs(string path) => Directory.CreateDirectory(path);
        public static FileInfo Stat(string path) => new FileInfo(path);
        public static void RmTree(string path) => Directory.Delete(path, true);
        public static string AbsPath(string path) => Path.GetFullPath(path);
    }
    
    // System.Diagnostics.Process is a clunky low level API. Some convenience functions on top of that
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


        // run command, return exit code, stdout and stderr. Does not use shell in between
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
                ReadStreams(proc, @out, err); return (proc.ExitCode, @out, err);
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
        private static void ReadStreams(Process proc, List<string> @out, List<string> err)
        {
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
        }


    }
}
