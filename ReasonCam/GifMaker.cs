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

namespace ReasonCam
{
    public sealed class GifMaker
    {
        readonly List<byte[]> byteframes = new List<byte[]>();
        readonly List<BitmapImage> imageframes = new List<BitmapImage>();
        readonly List<StorageFile> fileframes = new List<StorageFile>();

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

        public void AppenFrameImage([ReadOnlyArray]BitmapImage image)
        {
            imageframes.Add(image);
        }

        public void AppendFrameFile([ReadOnlyArray]StorageFile file)
        {
            fileframes.Add(file);
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

        public IAsyncInfo GenerateFromFilesAsync(StorageFile file, int delay)
        {
            return AsyncInfo.Run(async ctx =>
            {
                var outStream = await file.OpenAsync(FileAccessMode.ReadWrite);

                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.GifEncoderId, outStream);

                // set repeat property
                var containerProperties = new BitmapPropertySet
                        {
                            { "/appext/Application", new BitmapTypedValue(Encoding.UTF8.GetBytes("NETSCAPE2.0"), PropertyType.UInt8Array) },
                      //      { "/appext/Data", new BitmapTypedValue(netscapeData.ToArray(), PropertyType.UInt8Array) }
                            { "/appext/Data", new BitmapTypedValue(new byte[] { 3, 1, 0, 0, 0 }, PropertyType.UInt8Array) },
                        };
                await encoder.BitmapContainerProperties.SetPropertiesAsync(containerProperties);

                try
                {
                    for (int i = 0; i < fileframes.Count; i++)
                    {
                        using (IRandomAccessStream fileStream = await fileframes[i].OpenAsync(FileAccessMode.Read))
                        {
                            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                            PixelDataProvider pixels = await decoder.GetPixelDataAsync();

                            Debug.WriteLine("decoder info: width-{0}, height-{1}, x-{2}, y-{3}", decoder.OrientedPixelWidth.ToString(), decoder.OrientedPixelHeight.ToString(), decoder.DpiX.ToString(), decoder.DpiY.ToString());

                            encoder.SetPixelData(decoder.BitmapPixelFormat, BitmapAlphaMode.Ignore,
                                decoder.OrientedPixelWidth, decoder.OrientedPixelHeight,
                                decoder.DpiX, decoder.DpiY,
                                pixels.DetachPixelData());

                            //BitmapImage currBM = imageframes[i];
                            //byte[] bitmapBytes = ImageController.convertToBytes(currBM);

                            //encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                            //                     (uint)currBM.PixelHeight, (uint)currBM.PixelWidth,
                            //                     92.0, 92.0,
                            //                     bitmapBytes);

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

                            if (i < fileframes.Count - 1)
                                await encoder.GoToNextFrameAsync();
                        }
                    }

                    //byte   1       : 33 (hex 0x21) GIF Extension code
                    //byte   2       : 255 (hex 0xFF) Application Extension Label
                    //byte   3       : 11 (hex 0x0B) Length of Application Block 
                    //                 (eleven bytes of data to follow)
                    //bytes  4 to 11 : "NETSCAPE"
                    //bytes 12 to 14 : "2.0"
                    //byte  15       : 3 (hex 0x03) Length of Data Sub-Block (three bytes of data to follow)
                    //byte  16       : 1 (hex 0x01)
                    //bytes 17 to 18 : 0 to 65535, an unsigned integer in lo-hi byte format. This indicate the number of iterations the loop should be executed.
                    //byte  19       : 0 (hex 0x00) a Data Sub-Block Terminator.
             

                    // one method - http://www.win8usa.com/question-2052/how-do-i-make-a-gif-repeat-in-loop-when-generating-with-bitmapencoder.html
                    //String appextData = "/appext/Data";

                    //List<String> props = new List<string>();
                    //props.Add(appextData);

                    //BitmapPropertySet repeatProperties = await encoder.BitmapProperties.GetPropertiesAsync(props);
                    //repeatProperties.Add(new KeyValuePair<string,BitmapTypedValue>("/appext/Application",new BitmapTypedValue(Encoding.UTF8.GetBytes("NETSCAPE2.0"), Windows.Foundation.PropertyType.UInt8Array)));
                    //repeatProperties.Add(new KeyValuePair<string,BitmapTypedValue>("/appext/Data",new BitmapTypedValue(new byte[] { 3, 1, 0, 0, 0 }, Windows.Foundation.PropertyType.UInt8Array)));

                    ////repeatProperties = new BitmapPropertySet
                    ////{
                    ////    {
                    ////        "/appext/Application",
                    ////        new BitmapTypedValue(Encoding.UTF8.GetBytes("NETSCAPE2.0"), Windows.Foundation.PropertyType.UInt8Array)
                    ////    },
                    ////    { 
                    ////    "/appext/Data",
                    ////    new BitmapTypedValue(new byte[] { 3, 1, 0, 0, 0 }, Windows.Foundation.PropertyType.UInt8Array)
                    ////    },
                    ////};

                    //await encoder.BitmapProperties.SetPropertiesAsync(repeatProperties);


                    // another method - http://pastebin.com/7KGmzYR5

                    //   set auto repeat properties
                    //uint repeatCount = 0;
                    //using (var netscapeData = new MemoryStream(4))
                    //using (var writer = new BinaryWriter(netscapeData))
                    //{
                    //    writer.Write((byte)2); // Number of following data bytes
                    //    writer.Write((byte)1); // Always 1
                    //    writer.Write(repeatCount);
                    //    writer.Flush();

                      //  var containerProperties = new BitmapPropertySet
                      //  {
                      //      { "/appext/Application", new BitmapTypedValue(Encoding.UTF8.GetBytes("NETSCAPE2.0"), PropertyType.UInt8Array) },
                      ////      { "/appext/Data", new BitmapTypedValue(netscapeData.ToArray(), PropertyType.UInt8Array) }
                      //      { "/appext/Data", new BitmapTypedValue(new byte[] { 3, 1, 0, 0, 0 }, PropertyType.UInt8Array) },
                      //  };
                      //  await encoder.BitmapContainerProperties.SetPropertiesAsync(containerProperties);
                    //}


                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("exception caught: " + ex.ToString());
                }
                await encoder.FlushAsync();
                outStream.Dispose();

                Debug.WriteLine("gif properties added");
            });


        }

        public IAsyncInfo GenerateFromBitmapsAsync(StorageFile file, int delay)
        {
            return AsyncInfo.Run(async ctx =>
            {
                var outStream = await file.OpenAsync(FileAccessMode.ReadWrite);

                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.GifEncoderId, outStream);

                for (int i = 0; i < imageframes.Count; i++)
                {
                    try
                    {

                        using (MemoryRandomAccessStream memStream = new MemoryRandomAccessStream(ImageController.convertToBytes(imageframes[i])))
                        {

                            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(memStream);
                            PixelDataProvider pixels = await decoder.GetPixelDataAsync();


                            encoder.SetPixelData(decoder.BitmapPixelFormat, BitmapAlphaMode.Ignore,
                                decoder.OrientedPixelWidth, decoder.OrientedPixelHeight,
                                decoder.DpiX, decoder.DpiY,
                                pixels.DetachPixelData());

                            await encoder.FlushAsync();
                            outStream.Dispose();

                        //BitmapImage currBM = imageframes[i];
                        //byte[] bitmapBytes = ImageController.convertToBytes(currBM);

                        //encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                        //                     (uint)currBM.PixelHeight, (uint)currBM.PixelWidth,
                        //                     92.0, 92.0,
                        //                     bitmapBytes);

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
