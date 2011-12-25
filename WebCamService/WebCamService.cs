using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;
using TouchlessLib;

namespace WebCamService
{
    public partial class WebCamService : ServiceBase
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private Timer _timer;
        private WebCam _webCam;
        private TouchlessMgr _mgr;

        public WebCamService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _log.Info("{0}{0}Starting service.", Environment.NewLine);

            var outputPath = Config("OutputPath");
            var interval = ConfigInt("Interval");
            var cameraName = Config("CameraName");
            var captureWidth = ConfigInt("ImageWidth");
            var captureHeight = ConfigInt("ImageHeight");
            var imageQuality = ConfigInt("ImageQuality");

            // note: these must be stored as fields in this service
            _mgr = new TouchlessMgr();
            _webCam = new WebCam(_mgr, outputPath, cameraName, captureWidth, captureHeight, imageQuality);
            _timer = new Timer(_webCam.Render, null, 0, interval);
        }

        private static int ConfigInt(string key)
        {
            return Convert.ToInt32(Config(key));
        }

        private static string Config(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        protected override void OnStop()
        {
            _log.Info("Stopping service.");
            _timer.Dispose();
        }
    }
}