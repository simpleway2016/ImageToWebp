
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
namespace ImageToWebp
{
    public class Factory
    {
       static ICompressor[] compressors = new ICompressor[] {
                new Impls.WebpCompressor(),
                new Impls.ImageCompressor()
            };

        public static void Enable(IApplicationBuilder app, IHostingEnvironment env)
        {
           

            app.Use((context , task)=> {
                try
                {
                    var originalUrl = context.Request.Path;
                    string newfilepath;
                    string sourcefile = env.WebRootPath + originalUrl;
                    ICompressor compressor;

                    var match = System.Text.RegularExpressions.Regex.Match(originalUrl, @"^(.+)\.(?<ext>png|jpg|jpeg)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match != null && match.Length > 0)
                    {

                        //判断请求头是否包括image/webp
                        if (context.Request.Headers["Accept"].ToString().ToLower().Contains("image/webp"))
                        {
                            #region webp
                            var result = $"{originalUrl}.webp";
                            newfilepath = env.WebRootPath + result;                            
                            if (File.Exists(sourcefile) == false)
                                return task();

                            context.Response.ContentType = "image/webp";
                            compressor = compressors[0];

                            #endregion
                        }
                        else
                        {
                            #region 正常的图片压缩
                            var ext = match.Groups["ext"].Value.ToLower();
                          
                            var result = $"{originalUrl}.c";
                            newfilepath = env.WebRootPath + result;
                            if (File.Exists(sourcefile) == false)
                                return task();
                           
                            if (ext == "jpg")
                                ext = "jpeg";

                            context.Response.ContentType = $"image/{ext}";
                            compressor = compressors[1];
                            #endregion
                        }

                        var srcfileinfo = new System.IO.FileInfo(sourcefile);
                        if (File.Exists(newfilepath))
                        {
                            var fileinfo = new System.IO.FileInfo(newfilepath);
                            if (fileinfo.LastWriteTime == srcfileinfo.LastWriteTime)
                            {
                                return FileSender.SendFile(context, fileinfo, newfilepath);
                            }
                        }

                        compressor.Compress(app.ApplicationServices, sourcefile, newfilepath);

                        System.IO.File.SetLastWriteTime(newfilepath, srcfileinfo.LastWriteTime);
                        if (true)
                        {
                            var fileinfo = new System.IO.FileInfo(newfilepath);
                            return FileSender.SendFile(context, fileinfo, newfilepath);
                        }

                    }

                }
                catch(Exception ex)
                {
                    var loggerFactory = (ILoggerFactory)app.ApplicationServices.GetService(typeof(ILoggerFactory));
                    var logger = loggerFactory.CreateLogger("ImageToWebp");
                    logger.LogError(ex , "");
                }

                return task();
            });
        }


    }

   
}
