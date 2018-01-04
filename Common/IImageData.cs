using System;

namespace Common
{
	public class CommonImageData : IImageData
	{
		public int Id { get; set; }
		public double Probabillity { get; set; }
		public int Rotation { get; set; }
		public DateTime FileChangedDateTime { get; set; }
		public ProcessingResultType ProcessingResult { get; set; }
	}
	public interface IImageData
	{
		int Id { get; set; }
		double Probabillity { get; set; }
		int Rotation { get; set; }
		DateTime FileChangedDateTime { get; set; }
		ProcessingResultType ProcessingResult { get; set; }
	}
}