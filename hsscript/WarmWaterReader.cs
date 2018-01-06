using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


	
string _logName="WarmWaterScript";

public void Main(object[] Parms)
{
	LogToHomeseer("Start WarmWater script" );
	string cachebash=DateTime.Now.ToString("yyyyMMddHHmmss");
	System.Net.WebRequest webRequest = System.Net.WebRequest.Create(@"http://localhost:1234/?cachebash="+cachebash);
	webRequest.Headers.Set(System.Net.HttpRequestHeader.CacheControl, "max-age=0, no-cache, no-store");
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
		LogToHomeseer(strContent);
		var formattedData=SplitContent(strContent);
		foreach(var temp in formattedData)
		{
			LogToHomeseer(temp);
		}
	UpdateHomeSeerDevices(formattedData);
	}
	LogToHomeseer("Done");
	
}

public void UpdateHomeSeerDevices(System.Collections.Generic.List<string> formattedData)
{
		SetHsDevice(265,formattedData[1]);
		SetHsDevice(266,formattedData[0]);
		SetHsDevice(267,formattedData[2]);
		
}

public void SetHsDevice(int deviceNumber, string valueToUpdateTo)
{
	
	hs.SetDeviceString(deviceNumber,valueToUpdateTo,true);
	if(deviceNumber==265)
	{
		var intValue=ConvertStringToInt(valueToUpdateTo);
		hs.SetDeviceValueByRef(deviceNumber,intValue,true);	
	}	
}

public int ConvertStringToInt(string intValueAsString)
{
	int returnValue=-99;
	 if(int.TryParse(intValueAsString,out returnValue))
	 {
		 LogToHomeseer("Conversion from string to int worked");
	 }
	return returnValue;
}

public System.Collections.Generic.List<string> SplitContent(string lineOfText)
{
	var resultList=new System.Collections.Generic.List<string>();
	var splitBySemiColon=lineOfText.Split(';');
	if(splitBySemiColon.Length<2) return new System.Collections.Generic.List<string>();
	
	foreach(var stringFragment in splitBySemiColon)
	{
		var splitByEquals=stringFragment.Split('=');
		if(splitByEquals.Length>1)
		{
			var dataAsString=splitByEquals[1];
			resultList.Add(dataAsString);
		}
	}

	return resultList;
}


public void LogToHomeseer(string message)
{	
	hs.WriteLog(_logName, message);
}