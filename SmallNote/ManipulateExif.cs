using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace SmallNote
{
    class ManipulateExif
    {

        /// <summary>
        /// Exifタグが付与されているかどうか調べる
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        int FindExifMaker(Stream stream)
        {
            int n = 0;
            byte[] buf = new byte[2];
            while (true)
            {
                if (n + 2 > stream.Length) break;
                stream.Seek(n, SeekOrigin.Begin);
                stream.Read(buf, 0, 2);
                if (buf[0] == 0xFF && buf[1] == 0xE1)
                {
                    return n;
                }
                n++;
                if (n > 2048) break;
            }
            return -1;
        }



        /// <summary>
        /// </summary>
        /// <returns>回転種別</returns>
        public int GetJpegOrientation(Stream stream)
        {
            var exifIdx = FindExifMaker(stream);
            if (exifIdx < 0)
            {
                return -1;
            }
            stream.Seek(exifIdx, SeekOrigin.Begin);

            int n = 0;
            byte[] buf = new byte[2];
            while (true)
            {
                if (n + 2 > stream.Length) break;
                stream.Seek(n, SeekOrigin.Begin);
                stream.Read(buf, 0, 2);
                if (buf[0] == 0x01 && buf[1] == 0x12)
                {
                    n += 2;
                    stream.Seek(n, SeekOrigin.Begin);
                    stream.Read(buf, 0, 2);
                    return buf[0] * 256 + buf[1];
                }

                n++;
                if (n > 2048) break;
            }
            return -1;
        }

        /*
          
回転種別	説明
1	通常
2	左右反転
3	時計回りに180度回転
4	上下反転
5	時計回りに270度回転＆上下反転
6	時計回りに90度回転
7	時計回りに90度回転＆上下反転
8	時計回りに270度回転
         
         */
    }
}
