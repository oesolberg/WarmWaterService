using System;
using Nancy;

namespace NancyWebServer
{
	public class MainModule:NancyModule
	{
		public MainModule()
		{
			Get["/"] = x =>
			{
				var dbStorage=new DataStorage.Repository();
				var lastResult = dbStorage.GetLatestImageData();
				//return new RotationData()
				//{
				//	Probabillity = lastResult.Probabillity,
				//	Rotation = lastResult.Rotation,
				//	FileChangedDate = lastResult.FileChangedDateTime,
				//	DeliveryDate = DateTime.Now
				//};
				return "Probabillity=" + lastResult.Probabillity + ";Rotation=" + lastResult.Rotation + ";FileChangedDate=" + lastResult.FileChangedDateTime.ToString("yyyy-MM-dd HH:mm:ss") + ";DeliveryDate=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


			};
		}
		
	}
}