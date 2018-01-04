using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using LiteDB;

namespace DataStorage
{
    public class Repository: ILiteDbRepository
    {
	    public void SaveData(IImageData processingResult)
	    {
		    var dbPath = GetDbFullPath();
			using (var db = new LiteDatabase(dbPath))
		    {
			    var imageDataCollection= db.GetCollection<ImageData>("ImageData");
				var imageData=new ImageData()
				{
					Probabillity = processingResult.Probabillity,
					Rotation = processingResult.Rotation,
					ProcessingDateTime = DateTime.Now,
					FileChangedDateTime = processingResult.FileChangedDateTime
				};
			    imageDataCollection.Insert(imageData);

		    }
	    }

		public IImageData GetLatestImageData()
		{
			CommonImageData returnData = null;
			var dbPath = GetDbFullPath();
			using (var db = new LiteDatabase(dbPath))
			{
				var imageDataCollection = db.GetCollection<ImageData>("ImageData");
				imageDataCollection.EnsureIndex(x => x.ProcessingDateTime);

				// Use Linq to query documents
				var result = imageDataCollection.Find(Query.All("ProcessingDateTime",Query.Descending),0,1);
				if (result != null && result.Count()==1)
				{
					var foundImageData = result.First();
					returnData=new CommonImageData(){
						Id = foundImageData.Id,
						Probabillity = foundImageData.Probabillity,
						Rotation = foundImageData.Rotation,
						FileChangedDateTime = foundImageData.FileChangedDateTime
					};
				}
			}
			return returnData;

		}

	    private string GetDbFullPath()
	    {
			var applicationPath = System.AppDomain.CurrentDomain.BaseDirectory;
		    return Path.Combine(applicationPath, "Warmwater.db");
		}
	}
}
