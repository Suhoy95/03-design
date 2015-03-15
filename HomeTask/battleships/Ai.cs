using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NLog;

namespace battleships
{
	public class Ai
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();
		private Process process;
		private readonly string exePath;
        public event EventHandler<Process> InitNewProcess;
        public bool Crashed { get; private set; }
	
        public Ai(string exePath)
		{
			this.exePath = exePath;
            Crashed = false;
		}

		public string Name
		{
			get { return Path.GetFileNameWithoutExtension(exePath); }
		}

		public Vector Init(int width, int height, int[] shipSizes)
		{
            if (process == null || process.HasExited) 
                process = RunProcess();
			  
            SendMessage("Init {0} {1} {2}", width, height, string.Join(" ", shipSizes));
			return ReceiveNextShot();
		}

		public Vector GetNextShot(Vector lastShotTarget, ShootEffect lastShot)
		{
			    SendMessage("{0} {1} {2}", lastShot, lastShotTarget.X, lastShotTarget.Y);
			    return ReceiveNextShot();
		}

		private void SendMessage(string messageFormat, params object[] args)
		{
			var message = string.Format(messageFormat, args);
			process.StandardInput.WriteLine(message);
			log.Debug("SEND: " + message);
		}

		public void Dispose()
		{
			if (process == null || process.HasExited) return;
			log.Debug("CLOSE");
			process.StandardInput.Close();
			if (!process.WaitForExit(500))
				log.Info("Not terminated {0}", process.ProcessName);
			try
			{
				process.Kill();
			}
			catch
			{
				//nothing to do
			}
			process = null;
		}

		private Process RunProcess()
		{
			var startInfo = new ProcessStartInfo
			{
				FileName = exePath,
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden
			};
			var aiProcess = Process.Start(startInfo);
		    
            if (InitNewProcess != null)
		        InitNewProcess(this, aiProcess);
			
            return aiProcess;
		}

		private Vector ReceiveNextShot()
		{
		    if (Crashed) return null;

            try
            {
			    var output = process.StandardOutput.ReadLine();
			    log.Debug("RECEIVE " + output);
			    if (output == null)
			    {
				    var err = process.StandardError.ReadToEnd();
				    Console.WriteLine(err);
				    log.Info(err);
				    throw new Exception("No ai output");
			    }
			
				var parts = output.Split(' ').Select(int.Parse).ToList();
				return new Vector(parts[0], parts[1]);
			}
			catch (Exception e)
			{
                Crashed = true;
                log.Info("Ai {0} crashed", Name);
                log.Error(e);
                return null;
			}
		}
	}
}