using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace OSD_Auto_Unlock
{
   [ServiceContract]
    public interface MBAMKeyInterface
    {
        [OperationContract]
        string GetMBAMkey(string keyID);
    }
}
