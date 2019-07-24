
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using WebPWrapper;
using WebPWrapper.Encoder;
using System.Linq;
using Microsoft.Extensions.Logging;
namespace ImageToWebp.Impls
{
    class ImageCompressor : ICompressor
    {
        /// <summary>
        /// 将图片按百分比压缩
        /// </summary>
        /// <param name="iSource"></param>
        /// <param name="outPath"></param>
        /// <param name="flag">flag取值1到100，越小压缩比越大</param>
        /// <returns></returns>
        static bool compress(Bitmap iSource, string outPath, int flag)
        {
            if (iSource.Size.Width > 16383 || iSource.Size.Height > 16383)
            {
                int newwidth = iSource.Size.Width;
                int newheight = iSource.Size.Height;
                if (newwidth > 16383)
                {
                    newheight = (int)(newheight * (16383.0 / newwidth));
                    newwidth = 16383;
                }
                if (newheight > 16383)
                {
                    newwidth = (int)(newwidth * (16383.0 / newheight));
                    newheight = 16383;
                }
                var newbitmap = new Bitmap(newwidth, newheight, iSource.PixelFormat);
                using (Graphics g = Graphics.FromImage(newbitmap))
                {
                    g.DrawImage(iSource, new Rectangle(0, 0, newwidth, newheight), new Rectangle(Point.Empty, iSource.Size), GraphicsUnit.Pixel);
                }
                iSource.Dispose();
                iSource = newbitmap;
            }

            ImageFormat tFormat = iSource.RawFormat;

            EncoderParameters ep = new EncoderParameters();

            long[] qy = new long[1];

            qy[0] = flag;

            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);

            ep.Param[0] = eParam;

            try

            {

                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageDecoders();

                ImageCodecInfo jpegICIinfo = null;

                for (int x = 0; x < arrayICI.Length; x++)

                {

                    if (arrayICI[x].FormatDescription.Equals("JPEG"))

                    {

                        jpegICIinfo = arrayICI[x];

                        break;

                    }

                }

                if (jpegICIinfo != null)

                    iSource.Save(outPath, jpegICIinfo, ep);

                else

                    iSource.Save(outPath, tFormat);

                return true;

            }

            catch

            {

                return false;

            }

            iSource.Dispose();

        }

        public void Compress(IServiceProvider serviceProvider, string srcFile, string dstFile)
        {
            using (var bitmap = (Bitmap)Bitmap.FromFile(srcFile))
            {
                compress(bitmap, dstFile, 50);

            }
        }
    }
}
