using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Graphics;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using Xyzzer.AsyncUI;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using Windows.Storage;

namespace ReasonCam
{
    static public class ImageController
    {
        static public String ImageToBase64(BitmapImage image)
        {
            byte[] imagedata = ImageController.convertToBytes(image);
            return ImageController.ImageToBase64(imagedata);
        }

        static public String ImageToBase64(byte[] imagebytes)
        {
            return Convert.ToBase64String(imagebytes);
        }

        static public Object Base64ToImage(String imagedata)
        {
            return Convert.FromBase64String(imagedata);
        }

        static public byte[] convertToBytes(BitmapImage image)
        {
            byte[] pixeBuffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                int i = image.PixelHeight;
                int i2 = image.PixelWidth;

                WriteableBitmap wb = new WriteableBitmap(image.PixelWidth, image.PixelHeight);

                Stream s1 = wb.PixelBuffer.AsStream();
                s1.CopyTo(ms);

                pixeBuffer = ms.ToArray();
            }

            return pixeBuffer;
        }

        static public BitmapImage GetImage(byte[] imagebytes)
        {
            BitmapImage bi = new BitmapImage();

            using (MemoryRandomAccessStream ras = new MemoryRandomAccessStream(imagebytes))
            {
                bi.SetSource(ras);
            }

            return bi;
        }

        static async public Task<BitmapImage> GetImage(StorageFile file)
        {
            byte[] bytes = await ImageController.ReadFile(file);
            return ImageController.GetImage(bytes);
        }

        static public async Task<byte[]> ReadFile(StorageFile file)
        {
            byte[] fileBytes = null;
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }

            return fileBytes;
        }
    }
}
