using System.ServiceProcess;

namespace WebCamService
{
    static class Program
    {
        static void Main()
        {
            var servicesToRun = new ServiceBase[] { new WebCamService() };
            ServiceBase.Run(servicesToRun);
        }
    }
}