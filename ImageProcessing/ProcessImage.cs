using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace ImageProcessing
{
	public class ProcessImage
	{


		public void Execute(string filePath)
		{
			CutAfterFiveMinutesAndThreeRetries(filePath);
		}

		private void CutAfterFiveMinutesAndThreeRetries(string fileToProcess)
		{
			var numberOfRetries = 0;
			IImageData lastResult = null;

			var dbStore = new DataStorage.Repository();
			do
			{
				lastResult = RunFileToProcess(fileToProcess);
				numberOfRetries++;
			} while (numberOfRetries < 3 && lastResult
			!=null && lastResult.ProcessingResult == ProcessingResultType.Cancelled);
			
			if (lastResult!=null && lastResult.ProcessingResult == ProcessingResultType.Ok)
			{

				dbStore.SaveData(lastResult);
			}
		}

		private IImageData RunFileToProcess(string fileToProcess)
		{

			CancellationTokenSource cts = new CancellationTokenSource();
			Task<IImageData> processImageAsTask = Task.Factory.StartNew<IImageData>(() => DoProcessingAsTask(fileToProcess, cts.Token));
			IImageData resultData = null;

			// wait for max 5 minutes
			if (processImageAsTask.Wait(300000, cts.Token))
			{
				resultData = processImageAsTask.Result;
				return resultData;

			}
			else
			{
				// it did not finish within allotted time
				cts.Cancel();
				return new CommonImageData() { ProcessingResult = ProcessingResultType.Cancelled };

			}

		}

		private IImageData DoProcessingAsTask(string fileToProcess, CancellationToken ct)
		{
			if (ct.IsCancellationRequested == true)
			{
				ct.ThrowIfCancellationRequested();
			}

			//IImageData imageData = null;


			var imageHandler = new WarmWaterImageHandler();
			return imageHandler.DoImageProcessing(fileToProcess);

		}

		//private List<MissingFileData> FindAnyMissingFiles(List<string> dbFilesList, List<string> diskFileList)
		//{
		//	var missingFileList = new List<MissingFileData>();
		//	foreach (var filepath in diskFileList)
		//	{
		//		if (!dbFilesList.Contains(Path.GetFileName(filepath)))
		//		{
		//			missingFileList.Add(new MissingFileData() { Filepath = filepath, CreatedDateTime = File.GetLastWriteTime(filepath) });
		//		}
		//	}
		//	if (missingFileList.Any())
		//	{
		//		missingFileList = missingFileList.OrderBy(f => f.CreatedDateTime).ToList();
		//	}
		//	return missingFileList;
		//}


	}
}

