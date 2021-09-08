using System;
using System.Diagnostics;
using System.Windows;

namespace HostPerformanceAnalyze
{
    public class CountersHelper
    {
        public static bool CheckCounterEnv()
        {
            try
            {
                var counter = new PerformanceCounter("Process", "% Processor Time", "dwm");
                var value = counter.NextValue();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static string ResetCounterEnv()
        {
            string result = string.Empty;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/c C:\\Windows\\System32\\cmd.exe";
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.Verb = "RunAs";
                Process process = new Process();
                process.StartInfo = psi;
                process.Start();
                process.StandardInput.WriteLine(@"c:");
                process.StandardInput.WriteLine(@"cd c:\Windows\SysWOW64");
                process.StandardInput.WriteLine(@"lodctr /R");
                process.StandardInput.WriteLine(@"winmgmt.exe /RESYNCPERF");
                process.StandardInput.WriteLine(@"cd C:\Windows\system32");
                process.StandardInput.WriteLine(@"lodctr /R");
                process.StandardInput.WriteLine(@"winmgmt.exe /RESYNCPERF");
                process.StandardInput.WriteLine(@"exit");
                result += process.StandardOutput.ReadToEnd();
                //Bootinitext.AppendText("\n" + strRst);
                process.WaitForExit();
            }
            catch (Exception ex)
            {

                MessageBox.Show("Reset Counters failed! \r\n" + ex.ToString());
            }


            return result;
        }
    }
}
