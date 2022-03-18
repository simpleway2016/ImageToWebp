
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
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace ImageToWebp
{
    public class Factory
    {
       static ICompressor[] compressors = new ICompressor[] {

            };

        public static void Enable(IApplicationBuilder app)
        {
           

            app.Use((context , task)=> {
                try
                {
                    var configuration = app.ApplicationServices.GetService<IConfiguration>();
                    var originalUrl = context.Request.Path;
                   


                    var match = System.Text.RegularExpressions.Regex.Match(originalUrl, @"^(.+)\.(?<ext>png|jpg|jpeg|bmp)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match != null && match.Length > 0)
                    {
                        string sourcefile = configuration["wwwroot"] + originalUrl;
                        if(File.Exists(sourcefile) == false)
                        {
                            return task();
                        }
                    

                        var nodejsPath = "./squoosh";
                        if(Directory.Exists(nodejsPath) == false)
                        {
                            return context.Response.WriteAsync("miss squoosh folder");
                        }
                        string codedfile;
                        string type;
                        if (context.Request.Headers["Accept"].ToString().ToLower().Contains("image/webp"))
                        {
                            context.Response.ContentType = "image/webp";

                            codedfile = sourcefile + ".webp";
                            type = "webp";
                        }
                        else
                        {
                            codedfile = sourcefile + ".coded";
                            type = Path.GetExtension(originalUrl).Substring(1).ToLower();
                            switch (type)
                            {
                                case "png":
                                    context.Response.ContentType = "image/png";
                                    break;
                                default:
                                    context.Response.ContentType = "image/jpeg";
                                    break;
                            }
                        }

                        if(File.Exists(codedfile) == false)
                        {
                            var info = new ProcessStartInfo();
                            info.FileName = configuration["nodePath"];
                            info.Arguments = $"\"{AppDomain.CurrentDomain.BaseDirectory}squoosh/index.js\" \"{sourcefile}\" {type} \"{codedfile}\"";
                            info.RedirectStandardError = true;
                            info.RedirectStandardOutput = true;
                            var process = System.Diagnostics.Process.Start(info);
                            process.WaitForExit();
                            var ret = process.StandardOutput.ReadToEnd();
                            if(ret.StartsWith( "ok"))
                            {
                                return FileSender.SendFile(context, codedfile);
                            }
                            else
                            {
                                context.Response.ContentType = "text/html";
                                return context.Response.WriteAsync(ret);
                            }
                            
                        }
                        else
                        {
                            return FileSender.SendFile(context, codedfile);
                        }
                    }

                }
                catch(Exception ex)
                {
                    context.Response.ContentType = "text/html";
                    return context.Response.WriteAsync(ex.ToString());
                }

                return task();
            });
        }


    }

   
}
