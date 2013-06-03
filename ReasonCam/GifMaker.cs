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

namespace ReasonCam
{
    public sealed class GifMaker
    {
        readonly List<byte[]> byteframes = new List<byte[]>();
        readonly List<BitmapImage> imageframes = new List<BitmapImage>();

        private readonly uint frameWidth;
        private readonly uint frameHeight;

        public GifMaker(uint width, uint height)
        {
            frameWidth = width;
            frameHeight = height;
        }

        public void AppendFrameBytes([ReadOnlyArray]byte[] frame)
        {
            byteframes.Add(frame);
        }

        public void AppenFrameImage(BitmapImage image)
        {
            imageframes.Add(image);
        }

        //public async void generateGif(StorageFile file)
        //{
        //    //IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite);
        //    //BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
        //    //Stream pixelStream = bmp.PixelBuffer.AsStream();
        //    //byte[] pixels = new byte[pixelStream.Length];
        //    //await pixelStream.ReadAsync(pixels, 0, pixels.Length);

        //    //encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)bmp.PixelWidth, (uint)bmp.PixelHeight, 96.0, 96.0, pixels);
        //    //await encoder.FlushAsync();
        //}

        public IAsyncInfo GenerateFromBitmapsAsync(StorageFile file, int delay)
        {
            return AsyncInfo.Run(async ctx =>
            {
                var outStream = await file.OpenAsync(FileAccessMode.ReadWrite);

                var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateAsync(Windows.Graphics.Imaging.BitmapEncoder.GifEncoderId, outStream);

                for (int i = 0; i < imageframes.Count; i++)
                {
                    try
                    {
                        BitmapImage currBM = imageframes[i];
                        byte[] bitmapBytes = ImageController.convertToBytes(currBM);

                        encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore,
                                             (uint)currBM.PixelWidth, (uint)currBM.PixelHeight,
                                             96.0, 96.0,
                                             bitmapBytes);

                       /* if (i == 0)
                        {
                            var properties = new BitmapPropertySet
                        {
                            {
                                "/grctlext/Delay",
                                new BitmapTypedValue(delay / 10, PropertyType.UInt16)
                            }
                        };

                            await encoder.BitmapProperties.SetPropertiesAsync(properties);
                        }

                        if (i < byteframes.Count - 1)
                            await encoder.GoToNextFrameAsync();*/

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("exception caught: " + ex.ToString());
                    }
                }

                await encoder.FlushAsync();
                outStream.Dispose();
            });

        }

        public IAsyncInfo GenerateFromBytesAsync(StorageFile file, int delay)
        {
            return AsyncInfo.Run(async ctx =>
            {
                var outStream = await file.OpenAsync(FileAccessMode.ReadWrite);

                var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateAsync(Windows.Graphics.Imaging.BitmapEncoder.GifEncoderId, outStream);

                for (int i = 0; i < byteframes.Count; i++)
                {
                    try
                    {

                        var pixels = byteframes[i];
                        encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore,
                                             frameWidth, frameHeight,
                                             92.0, 92.0,
                                             pixels);

                        if (i == 0)
                        {
                            var properties = new BitmapPropertySet
                        {
                            {
                                "/grctlext/Delay",
                                new BitmapTypedValue(delay / 10, PropertyType.UInt16)
                            }
                        };

                            await encoder.BitmapProperties.SetPropertiesAsync(properties);
                        }

                        if (i < byteframes.Count - 1)
                            await encoder.GoToNextFrameAsync();

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("exception caught: " + ex.ToString());
                    }
                }

                await encoder.FlushAsync();
                outStream.Dispose();
            });

        }

    }

}
