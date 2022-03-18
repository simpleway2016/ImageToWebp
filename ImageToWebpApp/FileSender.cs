using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace ImageToWebp
{
    class FileSender
    {
        /// <summary>
        /// 断点下载
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fullpath"></param>
        /// <returns></returns>
        static async Task RangeDownload(HttpContext context, string fullpath,DateTime lastwriteTime)
        {
            context.Response.Headers.LastModified = lastwriteTime.ToUniversalTime().ToString("R");
            long size, start, end, length, fp = 0;
            using (StreamReader reader = new StreamReader(File.OpenRead(fullpath)))
            {

                size = reader.BaseStream.Length;
                start = 0;
                end = size - 1;
                length = size;
                // Now that we've gotten so far without errors we send the accept range header
                /* At the moment we only support single ranges.
                 * Multiple ranges requires some more work to ensure it works correctly
                 * and comply with the spesifications: http://www.w3.org/Protocols/rfc2616/rfc2616-sec19.html#sec19.2
                 *
                 * Multirange support annouces itself with:
                 * header('Accept-Ranges: bytes');
                 *
                 * Multirange content must be sent with multipart/byteranges mediatype,
                 * (mediatype = mimetype)
                 * as well as a boundry header to indicate the various chunks of data.
                 */
                context.Response.Headers.Add("Accept-Ranges", "0-" + size);
                // header('Accept-Ranges: bytes');
                // multipart/byteranges
                // http://www.w3.org/Protocols/rfc2616/rfc2616-sec19.html#sec19.2

                if (!String.IsNullOrEmpty(context.Request.Headers["Range"]))
                {
                    long anotherStart = start;
                    long anotherEnd = end;
                    string[] arr_split = context.Request.Headers["Range"].FirstOrDefault().Split(new char[] { Convert.ToChar("=") });
                    string range = arr_split[1];

                    // Make sure the client hasn't sent us a multibyte range
                    if (range.IndexOf(",") > -1)
                    {
                        // (?) Shoud this be issued here, or should the first
                        // range be used? Or should the header be ignored and
                        // we output the whole content?
                        context.Response.Headers.Add("Content-Range", "bytes " + start + "-" + end + "/" + size);
                        context.Response.StatusCode = 416;
                        context.Response.StatusCode = 416;
                        await context.Response.WriteAsync("Requested Range Not Satisfiable");
                    }

                    // If the range starts with an '-' we start from the beginning
                    // If not, we forward the file pointer
                    // And make sure to get the end byte if spesified
                    if (range.StartsWith("-"))
                    {
                        // The n-number of the last bytes is requested
                        anotherStart = size - Convert.ToInt64(range.Substring(1));
                    }
                    else
                    {
                        arr_split = range.Split(new char[] { Convert.ToChar("-") });
                        anotherStart = Convert.ToInt64(arr_split[0]);
                        long temp = 0;
                        anotherEnd = (arr_split.Length > 1 && Int64.TryParse(arr_split[1].ToString(), out temp)) ? Convert.ToInt64(arr_split[1]) : size;
                    }
                    /* Check the range and make sure it's treated according to the specs.
                     * http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html
                     */
                    // End bytes can not be larger than $end.
                    anotherEnd = (anotherEnd > end) ? end : anotherEnd;
                    // Validate the requested range and return an error if it's not correct.
                    if (anotherStart > anotherEnd || anotherStart > size - 1 || anotherEnd >= size)
                    {

                        context.Response.Headers.Add("Content-Range", "bytes " + start + "-" + end + "/" + size);
                        context.Response.StatusCode = 416;
                        await context.Response.WriteAsync("Requested Range Not Satisfiable");
                    }
                    start = anotherStart;
                    end = anotherEnd;

                    length = end - start + 1; // Calculate new content length
                    fp = reader.BaseStream.Seek(start, SeekOrigin.Begin);
                    context.Response.StatusCode = 206;
                }
            }
            // Notify the client the byte range we'll be outputting
            context.Response.Headers.Add("Content-Range", "bytes " + start + "-" + end + "/" + size);
            context.Response.Headers.Add("Content-Length", length.ToString());
            // Start buffered download
            await context.Response.SendFileAsync(fullpath, fp, length);
        }

        public static Task SendFile(HttpContext context, string file,DateTime lastWriteTime)
        {
         
            try
            {
                var since = context.Request.Headers["If-Modified-Since"].ToString();

                if ( !string.IsNullOrEmpty(since) && lastWriteTime.ToString() == Convert.ToDateTime( since).ToString())
                {
                    context.Response.StatusCode = 304;
                    context.Response.ContentLength = 0;
                    return Task.CompletedTask;
                }
            }
            catch
            {

            }

            return RangeDownload(context, file,lastWriteTime);
        }
    }
}
