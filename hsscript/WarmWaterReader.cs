using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

int highestTemp = -99;
int lowestTemp = 99;
string logName="GuahtdimYrBade";

public object Main(object[] Parms)
{
	

	
	int virtualDeviceId = 154;
	int virtualDeviceIdHighestTemp = 166;
	int virtualDeviceIdLowestTemp = 167;
	List<string> placeIds=new List<string> {"283","340"};
	string countyId = "01";
	
	hs.WriteLog(logName,"Start Yr badetemp" );
	string cachebash=DateTime.Now.ToString("yyyyMMddHHmmss");
	System.Net.WebRequest webRequest = System.Net.WebRequest.Create(@"http://om.yr.no/badetemperatur/badetemperatur.xml?cachebash="+cachebash);
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
		int startPoint = strContent.IndexOf("\n", StringComparison.InvariantCulture) + 1;
		strContent = strContent.Substring(startPoint);
		System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
		xmlDocument.LoadXml(strContent);
		System.Xml.XmlNode ostfoldNode = xmlDocument.SelectSingleNode("//badetemp/badetemperaturer/county[@id='" + countyId + "']");		
		if (ostfoldNode != null)
		{
			System.Xml.XmlNodeList tempNodes = ostfoldNode.SelectNodes("//county[@id='" + countyId + "']/place/temperature");
			if (tempNodes != null)
			{
				int totaltTemp = 0;
				int counter = 0;
		
				string highestTempPlace=string.Empty;
				string lowestTempPlace=string.Empty;
				
				foreach (System.Xml.XmlNode tempNode in tempNodes)
				{
					string placeId=GetParentNodePlaceId(tempNode);
					if(!placeIds.Contains(placeId)) continue;
					System.Xml.XmlAttribute test = tempNode.Attributes["water"];
					totaltTemp += int.Parse(test.Value);
					counter++;
					if (IsHighestTemp(tempNode))
					{
						highestTemp = int.Parse(test.Value);
						highestTempPlace = GetParentNodePlaceShortName(tempNode);
					}
					if (IsLowestTemp(tempNode))
					{
						lowestTemp = int.Parse(test.Value);
						lowestTempPlace = GetParentNodePlaceShortName(tempNode);
					}
				}
        
				decimal avgTemp =(decimal) (totaltTemp/counter);				 
				string badeTempString = "Vanntemperatur Jeløya(snitt): " + avgTemp.ToString("0.0") + "\u00B0C";// ("+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+")";
				hs.SetDeviceString(virtualDeviceId,badeTempString,true);
				
				string highTemp="Høyest: "  +  highestTemp.ToString() + "\u00B0C - " + highestTempPlace;
				hs.SetDeviceString(virtualDeviceIdHighestTemp,highTemp,true);
				
				string lowTemp="Lavest: " + lowestTemp.ToString() + "\u00B0C - " + lowestTempPlace;
				hs.SetDeviceString(virtualDeviceIdLowestTemp,lowTemp,true);
				
				hs.WriteLog(logName,"high: " + highestTemp + " " + highestTempPlace);
				hs.WriteLog(logName,"low: " + lowestTemp + " " + lowestTempPlace);
				hs.WriteLog(logName,badeTempString );
			}
		}
	}
	hs.WriteLog(logName,"Done" );
	return 0;
}

public string GetParentNodePlaceShortName(XmlNode tempNode)
{
	var parent = tempNode.ParentNode;
	var shortName = parent.Attributes["shortname"].Value;
	hs.WriteLog(logName,shortName);
	return shortName;
}

public string GetParentNodePlaceId(XmlNode tempNode)
{
	var parent = tempNode.ParentNode;
	var placeId = parent.Attributes["id"].Value;
	return placeId;
}

  public bool IsLowestTemp(XmlNode tempNode)
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

  public bool IsHighestTemp(XmlNode tempNode)
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
