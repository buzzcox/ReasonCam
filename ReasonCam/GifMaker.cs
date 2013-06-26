using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using System.Collections;
using Windows.Storage.Streams;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ReasonCam
{
    public sealed class ReasonGifGenerator
    {
        private readonly uint frameWidth;
        private readonly uint frameHeight;

        public ReasonGifGenerator(uint width, uint height)
        {
            frameWidth = width;
            frameHeight = height;
        }

        public IAsyncInfo GenerateGif(StorageFile file, int delay, bool repeat, List<BitmapImage> sourceBitmaps)
        {
            List<byte[]> source = new List<byte[]>();
            foreach (BitmapImage bi in sourceBitmaps)
                source.Add(ImageController.convertToBytes(bi));

            return GenerateGif(file, delay, repeat, source);
        }

        public async Task<IAsyncInfo> GenerateGif(StorageFile file, int delay, bool repeat, List<StorageFile> sourceFiles)
        {
            List<byte[]> source = new List<byte[]>();
            foreach (StorageFile sf in sourceFiles)
                source.Add(await ImageController.ReadFile(sf));

            return GenerateGif(file, delay, repeat, source);
        }

        public IAsyncInfo GenerateGif(StorageFile outputFile, int delay, bool repeat, List<byte[]> sourceBytes)
        {
            return AsyncInfo.Run(async ctx =>
            {
                var outStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite);

                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.GifEncoderId, outStream);

                if (repeat)     // set repeat property
                {
                    var containerProperties = new BitmapPropertySet
                        {
                            { "/appext/Application", new BitmapTypedValue(Encoding.UTF8.GetBytes("NETSCAPE2.0"), PropertyType.UInt8Array) },
                            { "/appext/Data", new BitmapTypedValue(new byte[] { 3, 1, 0, 0, 0 }, PropertyType.UInt8Array) },
                        };

                    await encoder.BitmapContainerProperties.SetPropertiesAsync(containerProperties);
                }

                try
                {
                    for (int i = 0; i < sourceBytes.Count; i++)
                    {

                        using (MemoryRandomAccessStream frameStream = new MemoryRandomAccessStream(sourceBytes[i]))
                        {
                            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(frameStream);
                            PixelDataProvider pixels = await decoder.GetPixelDataAsync();

                            encoder.SetPixelData(decoder.BitmapPixelFormat, BitmapAlphaMode.Ignore,
                                decoder.OrientedPixelWidth, decoder.OrientedPixelHeight,
                                decoder.DpiX, decoder.DpiY,
                                pixels.DetachPixelData());

                            if (i == 0)
                            {
                                var properties = new BitmapPropertySet{ { "/grctlext/Delay", new BitmapTypedValue(delay / 10, PropertyType.UInt16) } };
                                await encoder.BitmapProperties.SetPropertiesAsync(properties);
                            }

                            if (i < sourceBytes.Count - 1)
                                await encoder.GoToNextFrameAsync();
                        }
                    }

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("exception caught: " + ex.ToString());
                    //   throw ex;
                }
                await encoder.FlushAsync();
                outStream.Dispose();

            });
        }

    }

}
