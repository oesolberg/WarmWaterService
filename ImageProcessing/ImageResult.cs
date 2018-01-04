using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Util;
using Emgu.Util;
using System.Drawing.Imaging;


namespace ImageProcessing
{
	public class ImageResult
	{
		public double ProbabillityOfCorrect { get; set; }
		public int Rotation { get; set; }
	}
}