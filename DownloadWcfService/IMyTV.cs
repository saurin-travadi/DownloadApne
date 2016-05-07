using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace MyTV
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IMyTV
    {
        [OperationContract]
        [WebGet]
        string[] GetSerials();

        [OperationContract]
        [WebGet]
        string GetURL(string serial, string day);

    }
}
