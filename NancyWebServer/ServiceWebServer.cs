using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Hosting.Self;
using System.Configuration;

namespace NancyWebServer
{
	public class ServiceWebServer : NancyHost
	{
		private NancyHost _host;

		public void CreateServer()
		{
			var port = "1234";
			var foundValueInConfig= ConfigurationManager.AppSettings["port"];
			if (!string.IsNullOrEmpty(foundValueInConfig))
			{
				port = foundValueInConfig;
			}
			_host = new NancyHost(new Uri("http://localhost:"+ port));
			_host.Start();

		}

		public void StopServer()
		{
			_host.Stop();
		}
	}
}
