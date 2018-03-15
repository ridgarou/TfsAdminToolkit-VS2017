namespace mskold.TfsAdminToolKit
{ 
    using System;
    using System.Diagnostics;

    internal class RunCmd
    { 
        public void CreateTeamProject(string configFile)
        { 
        }

        public string ExecuteCommandSync(string command, string workingDir)
        { 
            try
            { 
                ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd.exe", @"/c " + command);

                procStartInfo.RedirectStandardOutput = false;
                procStartInfo.UseShellExecute = true;
                procStartInfo.CreateNoWindow = false;
                procStartInfo.WorkingDirectory = workingDir;

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                return string.Empty;
            }
            catch (Exception objException)
            { 
                return objException.Message;
            }
        }
    }
}