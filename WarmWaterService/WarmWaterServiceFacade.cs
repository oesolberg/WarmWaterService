using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NancyWebServer;

namespace WarmWaterService
{
	partial class WarmWaterServiceFacade : ServiceBase
	{
		private int _sleepInterval;
		private string _folderToWatch;
		private string _filefilter;

		private readonly string _eventSource = "WarmWaterFacade";
		private readonly string _eventLog = "WarmWaterService";
		private ServiceWebServer _server;

		public WarmWaterServiceFacade()
		{
			InitializeComponent();
			fileSystemWatcher.Created += fileSystemWatcher_Changed;
		}

		protected override void OnStart(string[] args)
		{
			LogToEventLog("Starting ElectricityMeterReaderService", EventLogEntryType.Information);
			_filefilter = ConfigurationManager.AppSettings["filter"];
			fileSystemWatcher.Filter = _filefilter;
			_folderToWatch = ConfigurationManager.AppSettings["folder"];
			fileSystemWatcher.Path = _folderToWatch;
			_sleepInterval = 10000;
			var sleepIntervalFromConfig = ConfigurationManager.AppSettings["sleepinterval"];
			int sleepIntervalFromConfigConvertedToInt;
			if (!string.IsNullOrWhiteSpace(sleepIntervalFromConfig) && int.TryParse(sleepIntervalFromConfig, out sleepIntervalFromConfigConvertedToInt))
			{
				_sleepInterval = sleepIntervalFromConfigConvertedToInt;
			}
			_server=new NancyWebServer.ServiceWebServer();
			_server.CreateServer();

		}

		protected override void OnStop()
		{
			_server.StopServer();
			LogToEventLog("ElectricityMeterReaderService stopped", EventLogEntryType.Information);
		}

		private void fileSystemWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
		{
			try
			{


				Thread.Sleep(_sleepInterval);
				if ((e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Changed) && FileExists(e.FullPath))
				{
					var logString = "WarmWaterService sensed a change and started processing file "+ e.FullPath;
					LogToConsoleIfPossible(logString);
					LogToEventLog(logString, EventLogEntryType.Information);

					var stopWatch = new Stopwatch();
					//Some file has changed.
					stopWatch.Start();

					var imageProcessor = new ImageProcessing.ProcessImage();
					imageProcessor.Execute(e.FullPath);
					if (stopWatch != null)
					{
						stopWatch.Stop();
						var stringToLog = "Added missing images in " + stopWatch.ElapsedMilliseconds.ToString("N1") + " ms.";
						LogToConsoleIfPossible(stringToLog);
						LogToEventLog(stringToLog, EventLogEntryType.Information);

					}
				}
			}
			catch (Exception ex)
			{
				LogToEventLog(ex);
				throw;
			}
		}

		private void LogToEventLog(Exception exception)
		{
			CreateEventLogIfMissing();
			using (EventLog eventLog = new EventLog(_eventLog))
			{
				eventLog.Source = _eventSource;
				eventLog.WriteEntry(exception.Message +
									Environment.NewLine +
									exception.Source +
									Environment.NewLine +
									exception.StackTrace, EventLogEntryType.Error);
			}

		}

		private void LogToEventLog(string stringToLog, EventLogEntryType eventLogEntryType)
		{
			CreateEventLogIfMissing();
			using (EventLog eventLog = new EventLog(_eventLog))
			{
				eventLog.Source = _eventSource;
				eventLog.WriteEntry(stringToLog, eventLogEntryType);
			}
		}

		private void CreateEventLogIfMissing()
		{
			if (!EventLog.SourceExists(_eventLog))
				EventLog.CreateEventSource(_eventSource, _eventLog);
		}

		private bool FileExists(string fullPath)
		{
			return System.IO.File.Exists(fullPath);
		}

		private void LogToConsoleIfPossible(string stringToWrite)
		{
			if (Environment.UserInteractive)
			{
				Console.WriteLine(stringToWrite);
			}
		}


	}
}
