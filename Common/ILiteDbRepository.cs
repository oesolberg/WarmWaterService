using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface ILiteDbRepository
    {
	    void SaveData(IImageData processingResult);
	    IImageData GetLatestImageData();


    }
}
