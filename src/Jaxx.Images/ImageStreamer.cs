using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Jaxx.Images
{
    public class ImageStreamer
    {
        private readonly ILogger<ImageStreamer> logger;
        public ImageStreamer(ILogger<ImageStreamer> logger)
        {
            this.logger = logger;
        }
        public byte[] ReadImageFile (string filePath)
        {
            logger.LogDebug("Enter method, filePath: " + filePath);
            byte[] buffer;
            FileStream fileStream;
            try
            {
                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                int length = (int)fileStream.Length;  // get file length                
                buffer = new byte[length];            // create buffer
                int count;                            // actual number of bytes read
                int sum = 0;                          // total number of bytes read

                // read until Read method returns 0 (end of the stream has been reached)
                while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;  // sum is a buffer offset for next reading                
                fileStream.Close();
                return buffer;
            }
            catch (Exception e)
            {
                logger.LogDebug(e.Message);
                return null;
            }

        }
    }
}
