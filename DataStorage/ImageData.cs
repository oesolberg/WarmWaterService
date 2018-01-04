using System;

namespace DataStorage
{
	public class ImageData
	{
		public int Id { get; set; }
		public int Rotation{ get; set; }
		public double Probabillity{ get; set; }
		public DateTime ProcessingDateTime { get; set; }
		public DateTime FileChangedDateTime { get; set; }
	}
}