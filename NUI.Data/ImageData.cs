using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
namespace NUI.Data
{
    /// <summary>
    /// 图像数据类
    /// </summary>
    public class ImageData
    {
        protected Image _image = new Image();
        public System.Windows.Controls.Image Image
        {
            get
            {
                return _image;
            }
        }
        public void SetImage(byte[] pixels, int width, int height)
        {
            int stride = width * 4;
            _image.Source = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
        }
    }
}
