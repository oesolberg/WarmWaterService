using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;



	public class WarmTest
	{
			
		static int highestTemp = -99;
		static int lowestTemp = 99;
		static string _logName="WarmWaterScript";

		public static void Main(string[] args)
		{
			Console.WriteLine("Start");
			int virtualDeviceId = 154;
			int virtualDeviceIdHighestTemp = 166;
			int virtualDeviceIdLowestTemp = 167;
			List<string> placeIds=new List<string> {"283","340"};
			string countyId = "01";
			
			LogToHomeseer("Start WarmWater script" );
			string cachebash=DateTime.Now.ToString("yyyyMMddHHmmss");
			System.Net.WebRequest webRequest = System.Net.WebRequest.Create(@"http://localhost:1234/?cachebash="+cachebash);
			webRequest.Headers.Set(HttpRequestHeader.CacheControl, "max-age=0, no-cache, no-store");
			System.IO.Stream content;
			System.Net.WebResponse response = webRequest.GetResponse();
			if (((System.Net.HttpWebResponse)response).ContentEncoding	=="gzip")
			{
				content = new System.IO.Compression.GZipStream(response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
			}
			else
			{
				content = response.GetResponseStream();
			}	
			System.IO.StreamReader reader = new System.IO.StreamReader(content);            
			string strContent = reader.ReadToEnd();
			if (strContent.Length > 0)
			{
				Console.WriteLine(strContent);
				var formattedData=SplitContent(strContent);
				//splitt content Probabillity=0.972492337226868;Rotation=-87;FileChangedDate=2018-01-05 08:13:44;DeliveryDate=2018-01-05 17:39:40
				var splitBySemiColon=strContent.Split(';');

				foreach(var info in splitBySemiColon)
				{
					Console.WriteLine(info);
				}

				//var rot=new ReceivedData(splitBySemiColon[1]);

				Console.WriteLine("----");
				Console.WriteLine(formattedData.Probabillity);

				UpdateHomeSeerDevices(formattedData,args);

			}
			LogToHomeseer("Done");
			
			// return 0;
		}

		public static void UpdateHomeSeerDevices(ReceivedData formattedData,string[] args)
		{
			if(args.Length>2)
			{
				hs.SetDeviceString(arg[0],formattedData.Rotation,true);
				hs.SetDeviceString(arg[1],formattedData.Probabillity,true);
				hs.SetDeviceString(arg[2],formattedData.FileChangedDateTime,true);
			}
		}

		public static ReceivedData SplitContent(string lineOfText)
		{
			var splitBySemiColon=lineOfText.Split(';');
			if(splitBySemiColon.Length<2) return null;
			var receivedData=new ReceivedData();
			foreach(var stringFragment in splitBySemiColon)
			{
				var splitByEquals=stringFragment.Split('=');
				if(splitByEquals.Length>1)
				{
					var dataAsString=splitByEquals[1];
					switch (splitByEquals[0])
					{
						case "Probabillity":receivedData.AddProbabillity(dataAsString);break;
						case "Rotation":receivedData.AddRotation(dataAsString);break;
						case "FileChangedDate":receivedData.AddFileChangedDate(dataAsString);break;
						default:break;
					}
				}
			}

			return receivedData;
		}

		public static void LogToHomeseer(string message)
		{	
			Console.WriteLine(message);
			//hs.WriteLog(logName,"Done" );
		}

		public static string GetParentNodePlaceShortName(XmlNode tempNode)
		{
			var parent = tempNode.ParentNode;
			var shortName = parent.Attributes["shortname"].Value;
			LogToHomeseer(shortName);
			return shortName;
		}

		public static string GetParentNodePlaceId(XmlNode tempNode)
		{
			var parent = tempNode.ParentNode;
			var placeId = parent.Attributes["id"].Value;
			return placeId;
		}

		public static bool IsLowestTemp(XmlNode tempNode)
		{
			XmlAttribute waterAttribute = tempNode.Attributes["water"];
			int waterTemp = int.Parse(waterAttribute.Value);
			if (lowestTemp > waterTemp)
			{
				lowestTemp = waterTemp;
				return true;
			}
			return false;
		}

		public static bool IsHighestTemp(XmlNode tempNode)
		{
			XmlAttribute waterAttribute = tempNode.Attributes["water"];
			int waterTemp = int.Parse(waterAttribute.Value);
			if (highestTemp < waterTemp)
			{
				highestTemp = waterTemp;
				return true;
			}
			return false;
		}
	}

	public class ReceivedData
	{
		// public ReceivedData(string rotation)
		// {
		// 	Rotation=rotation;
		// }

		public int ShowMe()
		{
			return Rotation;
		}

		public void AddProbabillity(string probabAsString)
		{
			double probabillity=-1;
			if(double.TryParse(probabAsString,out probabillity))
			{
				Probabillity=probabillity;
			}
		}

		public void AddRotation(string rotationAsString)
		{
			int rotation=-999;
			if(int.TryParse(rotationAsString,out rotation))
			{
				Rotation=rotation;
			}
		}

		public void AddFileChangedDate(string fileChangedDateAsString)
		{
			DateTime fileChangedDateTime=DateTime.MinValue;
			if(DateTime.TryParse(fileChangedDateAsString,out fileChangedDateTime))
			{
				FileChangedDateTime=fileChangedDateTime;
			}

		}

		public DateTime FileChangedDateTime {get;set;}
		public double Probabillity {get;set;}
		public int Rotation {get;set;}
	}
