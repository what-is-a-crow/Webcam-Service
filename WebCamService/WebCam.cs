using System;
using System.Linq;
using TouchlessLib;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using Encoder = System.Drawing.Imaging.Encoder;

namespace WebCamService
{
    public class WebCam
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private readonly Camera _camera;
        private readonly string _outputPath;
        private readonly ImageCodecInfo _encoder;
        private readonly int _imageQuality;

        public WebCam(TouchlessMgr mgr, string outputPath, string cameraName, int captureWidth, int captureHeight, int imageQuality)
        {
            _outputPath = outputPath;
            _imageQuality = imageQuality;

            _log.Info("Found {0} cameras.", mgr.Cameras.Count);
            mgr.CurrentCamera = mgr.Cameras.Where(c => c.ToString().Contains(cameraName)).SingleOrDefault();
            if (mgr.CurrentCamera == null)
            {
                _log.Error("Could not find camera with name {0}.", cameraName);
                return;
            }

            _encoder = _encoder ?? ImageCodecInfo.GetImageEncoders().Where(enc => enc.MimeType == "image/jpeg").SingleOrDefault();

            _log.Info("Setting resolution to {0} x {1}.", captureWidth, captureHeight);
            mgr.CurrentCamera.CaptureWidth = captureWidth;
            mgr.CurrentCamera.CaptureHeight = captureHeight;

            _camera = mgr.CurrentCamera;
        }

        public void Render(Object arg)
        {
            var img = _camera.GetCurrentImage();
            if (img == null)
            {
                _log.Error("Current image was null.");
                return;
            }

            _log.Info("Building image.");

            using (var g = Graphics.FromImage(img))
            {
                var format = new StringFormat { Alignment = StringAlignment.Near };
                g.DrawString(string.Format("{0:G}", DateTime.Now), new Font(FontFamily.GenericSansSerif, 14.0f, FontStyle.Regular), new SolidBrush(Color.Black), 20, 0, format);
                g.DrawString(string.Format("{0:G}", DateTime.Now), new Font(FontFamily.GenericSansSerif, 14.0f, FontStyle.Regular), new SolidBrush(Color.White), 21, 1, format);
                g.DrawString("Boulevard East, Weehawken, NJ", new Font(FontFamily.GenericSansSerif, 10.0f, FontStyle.Italic), new SolidBrush(Color.Black), 20, 25, format);
                g.DrawString("Boulevard East, Weehawken, NJ", new Font(FontFamily.GenericSansSerif, 10.0f, FontStyle.Italic), new SolidBrush(Color.White), 21, 26, format);
            }

            Save(img, _imageQuality);
        }

        void Save(Image img, long jpegQuality)
        {
            var @params = new EncoderParameters { Param = new[] { new EncoderParameter(Encoder.Quality, jpegQuality) } };
            var path = GetPath(jpegQuality);

            try
            {
                _log.Info("Saving image to {0}.", path);
                img.Save(path, _encoder, @params);
            }
            catch (Exception ex)
            {
                _log.ErrorException(string.Format("Error saving image to path {0}", path), ex);
                _log.Error("{0}{1}{2}", ex.Message, ex.StackTrace, Environment.NewLine);
            }
        }

        string GetPath(long jpegQuality)
        {
            var folder = Path.GetDirectoryName(_outputPath);
            var fileName = Path.GetFileNameWithoutExtension(_outputPath);
            var ext = Path.GetExtension(_outputPath);

            return Path.Combine(folder, string.Format("{0}_{1}{2}", fileName, jpegQuality, ext));
        }
    }
}