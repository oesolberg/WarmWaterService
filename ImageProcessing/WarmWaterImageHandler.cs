using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Common;
using Emgu.CV;
using Emgu.CV.Structure;

namespace ImageProcessing
{
	public class WarmWaterImageHandler
	{
		private readonly string _eventSource = "WarmWaterFacade";
		private readonly string _eventLog = "WarmWaterService";
		public IImageData DoImageProcessing(string fileToProcess)
		{
			using (EventLog eventLog = new EventLog(_eventLog))
			{
				eventLog.Source = _eventSource;
				eventLog.WriteEntry("starting doimageproc",EventLogEntryType.Information);
			}
			return TestRotation(fileToProcess);
		}

		private IImageData TestRotation(string fileToProcess)
		{
			var applicationPath = System.AppDomain.CurrentDomain.BaseDirectory;
			var pathToTemplate =Path.Combine(applicationPath, "Templates","KorrektNaalMedGraatt.jpg");
			//var pathToTemplate = Path.Combine(Environment.CurrentDirectory, "Templates", "KorrektNaalMedGraatt.jpg");
			using (EventLog eventLog = new EventLog(_eventLog))
			{
				eventLog.Source = _eventSource;
				eventLog.WriteEntry("path to template rotation "+ pathToTemplate, EventLogEntryType.Information);
			}

			var templateImage = new Image<Bgr, byte>(pathToTemplate);
			var listOfResults = new List<ImageResult>();
			var totalRotation = 360;
			var rotation = 0;
			while (rotation < totalRotation)
			{
				var rotationToUse = 0;
				if (rotation > 180)
				{
					rotationToUse = 180 - rotation;
				}
				else
				{
					rotationToUse = rotation;
				}
			
				var rotatedtemplateImage = templateImage.Rotate(rotationToUse, new Bgr(Color.Gray));
				//CvInvoke.Imshow("rotation", templateImage);
				//CvInvoke.WaitKey(0);
				var imgResult = TryToMatchRotation(fileToProcess,
					rotatedtemplateImage, rotationToUse);
				listOfResults.Add(imgResult);
				rotation++;


			}
			return FindHighestProbabillity(listOfResults, fileToProcess);

		}

		private IImageData FindHighestProbabillity(List<ImageResult> listOfResults, string fullFilePath)
		{
			var ioInfo = new FileInfo(fullFilePath);
			var highestProb = listOfResults.OrderByDescending(x => x.ProbabillityOfCorrect).First();
			return new CommonImageData()
			{
				Probabillity = highestProb.ProbabillityOfCorrect,
				Rotation = highestProb.Rotation,
				ProcessingResult = ProcessingResultType.Ok,
				FileChangedDateTime = ioInfo.LastWriteTime
			};
		}

		public ImageResult TryToMatchRotation(string filepathImageToSearch, Image<Bgr, byte> templateImage, int degreesOfRotation)
		{
			//public ImageResult TryToMatch(string filepathImageToSearch, Image<Bgr, byte> templateImage, string filepathTemplate = null, int degreesOfRotation = 0)

			var imgResult = new ImageResult() { Rotation = degreesOfRotation };
			Image<Bgr, byte> source = new Image<Bgr, byte>(filepathImageToSearch); // Image B

			Image<Bgr, byte> template = null;
			if (templateImage != null) template = templateImage;
			//if (!string.IsNullOrEmpty(filepathTemplate)) template = new Image<Bgr, byte>(filepathTemplate); // Image A

			Image<Bgr, byte> imageToShow = source.Copy();

			using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
			{
				//var imgResult = new ImageResult() { Rotation = degreesOfRotation };
				double[] minValues, maxValues;
				Point[] minLocations, maxLocations;
				result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
				imgResult.ProbabillityOfCorrect = maxValues[0];
				// You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
				if (maxValues[0] > 0.94)
				{
					// This is a match. Do something with it, for example draw a rectangle around it.
					//Rectangle match = new Rectangle(maxLocations[0], template.Size);
					//imageToShow.Draw(match, new Bgr(Color.Red), 3);
					//CvInvoke.Imshow("timageToShowest", imageToShow);
					//CvInvoke.WaitKey(0);

					//return true;
				}
			}
			return imgResult;


			// Show imageToShow in an ImageBox (here assumed to be called imageBox1)
			//imageBox1.Image = imageToShow;
		}


	}
}