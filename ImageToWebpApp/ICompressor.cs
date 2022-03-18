using System;
using System.Collections.Generic;
using System.Text;

namespace ImageToWebp
{
    interface ICompressor
    {
        void Compress(IServiceProvider serviceProvider, string srcFile, string dstFile);
    }
}
