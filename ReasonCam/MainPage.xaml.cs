using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SAClientWRC;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using Windows.Devices.Enumeration;
using Windows.Media.MediaProperties;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xyzzer.AsyncUI;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ReasonCam
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SAClient saclient = null;
        private int CurrentAppID = 10;
        private bool IsConnected = false;

        private List<DeviceInformation> deviceList = new List<DeviceInformation>();
        MediaCapture mediaCaptureMgr = new Windows.Media.Capture.MediaCapture();

//        private readonly String FOLDER_SAVE_PATH = @"\\BEAST\ReasonCamShare\datafiles";
        private readonly String TEMP_PHOTO_FILE_NAME = "photoTmp.jpg";
        private readonly String PHOTO_FILE_NAME = "photo.jpg";

        public MainPage()
        {
            this.InitializeComponent();
            Application.Current.Suspending += Current_Suspending;
            Application.Current.Resuming += Current_Resuming;
            this.GenerateDefaults();
            EnumerateWebcamsAsync();    //<-- get devices
        }

        void Current_Resuming(object sender, object e)
        {
            //We need to tell the server that this APP instance on this machine is now available
            if (this.saclient == null)
                this.saclient = new SAClientWRC.SAClient(this.CurrentAppID);
        }

        void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            //We need to tell the server that this APP instance on this machine is no longer running
            if (this.saclient != null)
                this.saclient.Dispose();

            this.saclient = null;
        }

        private void GenerateDefaults()
        {
            //TODO: Load this instance APP ID and Machine ID from config file
            this.CurrentAppID = 1;
        }    

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.saclient = new SAClientWRC.SAClient(this.CurrentAppID);
            this.saclient.ConnectionStatusEvent += SA_ConnectionStatusEvent;
            this.saclient.DataArrivedEvent += SA_DataArrivedEvent;
            this.saclient.BroadcastDataArrivedEvent += SA_BroadcastDataArrivedEvent;
            this.saclient.JSONBroadcastDataArrivedEvent += SA_JSONBroadcastDataArrivedEvent;
            this.saclient.ExceptionEvent += SA_ExceptionEvent;
            this.saclient.DeviceMapUpdatedEvent += SA_DeviceMapUpdatedEvent;
            this.saclient.JSONDeviceMapUpdatedEvent += SA_JSONDeviceMapUpdatedEvent;

            this.saclient.InitialiseClient();
        }

        #region SA Client Events

        void SA_JSONDeviceMapUpdatedEvent(object sender, string e)
        {
            //int a = 0;
        //    addStatus("json device map updated event");
            addStatus("json device map: " + e);
        }

        void SA_DeviceMapUpdatedEvent(object sender, DeviceMap e)
        {
            addStatus("device map updated event");

            //this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            //{
            ////Clean up existing
            //this.txtNW.Text = string.Empty;
            //this.txtNE.Text = string.Empty;
            //this.txtSW.Text = string.Empty;
            //this.txtSE.Text = string.Empty;
            //this.txtE.Text = string.Empty;
            //    this.txtW.Text = string.Empty;

            //    //Update the Device Map Display
            //    for (int i = 0; i < e.NW.Count; i++)
            //    {
            //        this.txtNW.Text += e.NW[i];
            //        if ((e.NW.Count > 1) && (i >= 0)) this.txtNW.Text += " ";
            //    }
            //    for (int i = 0; i < e.NE.Count; i++)
            //    {
            //        this.txtNE.Text += e.NE[i];
            //        if ((e.NE.Count > 1) && (i >= 0)) this.txtNE.Text += " ";
            //    }

            //    for (int i = 0; i < e.E.Count; i++)
            //    {
            //        this.txtE.Text += e.E[i];
            //        if ((e.E.Count > 1) && (i >= 0)) this.txtE.Text += " ";
            //    }
            //    for (int i = 0; i < e.SW.Count; i++)
            //    {
            //        this.txtSW.Text += e.SW[i];
            //        if ((e.SW.Count > 1) && (i >= 0)) this.txtSW.Text += " ";
            //    }
            //    for (int i = 0; i < e.SE.Count; i++)
            //    {
            //        this.txtSE.Text += e.SE[i];
            //        if ((e.SE.Count > 1) && (i >= 0)) this.txtSE.Text += " ";
            //    }

            //    for (int i = 0; i < e.W.Count; i++)
            //    {
            //        this.txtW.Text += e.W[i];
            //        if ((e.W.Count > 1) && (i >= 0)) this.txtW.Text += " ";
            //    }
            //});
        }

        void SA_ExceptionEvent(object sender, string e)
        {
            addStatus("device map updated event");
            //this.AddEventListItem(e, ItemState.ERROR);
        }

        void SA_JSONBroadcastDataArrivedEvent(object sender, string e)
        {
            addStatus("device map updated event");
            //int a = 0;
        }

        void SA_BroadcastDataArrivedEvent(object sender, SocketData e)
        {
            addStatus("Broadcast data arrived");

            try
            {
                Dictionary<string, object> recievedData = JsonConvert.DeserializeObject<Dictionary<string, object>>(e.Data.ToString());

                addStatus("deserialize object: " + recievedData.ToString());
            }
            catch (Exception ex)
            {
                addStatus("error deserializing: " + ex.Message);
            }

            TakePhoto();
        }

        void SA_DataArrivedEvent(object sender, SocketData e)
        {
            addStatus("data arrived for me");

            try
            {
                Dictionary<string, object> recievedData = JsonConvert.DeserializeObject<Dictionary<string, object>>(e.Data.ToString());

                addStatus("deserialize object: " + recievedData.ToString());
            }
            catch (Exception ex)
            {
                addStatus("error deserializing: " + ex.Message);
            }


            //if (e.BroadcastPacket == false)
            //    this.AddDirectEventListItem(e);
            //else
            //    this.AddEventListItem("Data Arrived Event", ItemState.NOTIFICATION);
        }

        void SA_ConnectionStatusEvent(object sender, SAClientWRC.Status e)
        {
            addStatus("Client status event: " + (e.IsConnected?"connected":"not connected"));

            this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.IsConnected = (e.IsConnected == true) ? true : false;

                if (e.IsReadyToConnect == true)
                {
                    addStatus("Device id: " + e.CurrentDeviceID + " is ready to connect");
                    this.saclient.Connect();
                }
                else
                    addStatus("no configuration found");


                if (this.IsConnected == true)
                    addStatus("CONNECTED to server!");
                else
                    addStatus("Connection CLOSED!");


                if (e.IsListening == true)
                    addStatus("Listening for broadcast");
                else
                    addStatus("not listening");


                if (e.MessageSent == true)
                    addStatus("Message sent!");

                if (e.StatusException != "")
                    addStatus("Exception occured!: " + e.StatusException);

            });
        }

        #endregion

        private void addStatus(string message)
        {
            this.statusList.Items.Add(message);
            this.statusList.ScrollIntoView(this.statusList.Items.Last());

          //  this.statusText.Text += "\n" + message;
        }

        #region Camera capture events

        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
       //     this.Frame.Navigate(typeof(WaitingPage));

            //addStatus("start streaming !!!!");
            //Dictionary<string,object> objToSend = new Dictionary<string,object>();
            //objToSend.Add("action","takephoto");
            //objToSend.Add("shoottime",new DateTime());
            //this.saclient.SendObject(objToSend, true);

         //   TakePhoto();
        }

        private async void TakePhoto()
        {

            try
            {
                addStatus("Starting device");
               
                var settings = new Windows.Media.Capture.MediaCaptureInitializationSettings();
                DeviceInformation chosenDevInfo = deviceList[0];
                settings.VideoDeviceId = chosenDevInfo.Id;
                settings.StreamingCaptureMode = StreamingCaptureMode.Video;

                if (chosenDevInfo.EnclosureLocation != null && chosenDevInfo.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Back)
                {
            //        m_bRotateVideoOnOrientationChange = true;
            //        m_bReversePreviewRotation = false;
                }
                else if (chosenDevInfo.EnclosureLocation != null && chosenDevInfo.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front)
                {
            //        m_bRotateVideoOnOrientationChange = true;
            //        m_bReversePreviewRotation = true;
                }
                else
                {
            //        m_bRotateVideoOnOrientationChange = false;
                }

                await mediaCaptureMgr.InitializeAsync(settings);

             //   DisplayProperties_OrientationChanged(null);

                addStatus("Device initialized successful");
           //     chkAddRemoveEffect.IsEnabled = true;
           //     m_mediaCaptureMgr.RecordLimitationExceeded += new Windows.Media.Capture.RecordLimitationExceededEventHandler(RecordLimitationExceeded); ;
           //     m_mediaCaptureMgr.Failed += new Windows.Media.Capture.MediaCaptureFailedEventHandler(Failed);
            }
            catch (Exception exception)
            {
                addStatus(exception.Message);
            }

            try
            {
                addStatus("Starting preview");

                cameraView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                cameraElement.Source = mediaCaptureMgr;
                await mediaCaptureMgr.StartPreviewAsync();
                addStatus("Start preview successful");
            }
            catch (Exception exception)
            {
                cameraElement.Source = null;
                addStatus("preview cam error: " + exception.Message);
            }

        }

        private async void EnumerateWebcamsAsync()
        {
            try
            {
                addStatus("Enumerating Webcams...");
                DeviceInformationCollection devInfoCollection = null;

                deviceList.Clear();

                devInfoCollection = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                if (devInfoCollection.Count == 0)
                {
                    addStatus("No WebCams found.");
                }
                else
                {
                    for (int i = 0; i < devInfoCollection.Count; i++)
                    {
                        var devInfo = devInfoCollection[i];
                        deviceList.Add(devInfo);
                    }

                    addStatus("Enumerating Webcams completed successfully.");
                }
            }
            catch (Exception e)
            {
                addStatus(e.Message);
            }
        }

        internal async void ShootButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                addStatus("Taking photo");
                ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();


                StorageFile tempPhotoStorageFile = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFileAsync(TEMP_PHOTO_FILE_NAME, CreationCollisionOption.ReplaceExisting);
                addStatus("Create photo file successful");
                await mediaCaptureMgr.CapturePhotoToStorageFileAsync(imageProperties, tempPhotoStorageFile);
                StorageFile photoStorageFile = await ReencodePhotoAsync(tempPhotoStorageFile, Windows.Storage.FileProperties.PhotoOrientation.Normal);
                addStatus("Photo taken");
                IRandomAccessStream photoStream = await photoStorageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                BitmapImage bmpimg = new BitmapImage();
                bmpimg.SetSource(photoStream);
                photoElement.Source = bmpimg;
                addStatus(photoStorageFile.Path);

             //   byte[] imageBytes = convertToBytes(bmpimg);

                //IRandomAccessStream picturestream = new InMemoryRandomAccessStream();
                //await mediaCaptureMgr.CapturePhotoToStreamAsync(imageProperties, picturestream);
                //BitmapImage bi = new BitmapImage();
                //bi.SetSource(picturestream);

                //await BitmapImageExtensions.WaitForLoadedAsync(bi);
                
                //photoElement.Source = bi;

                //SnapService.RemoteFileInfo file = new SnapService.RemoteFileInfo(imageBytes);
                



                //SnapService.PhotoServiceClient client = new SnapService.PhotoServiceClient();
                //SnapService.UploadFileResponse response = await client.UploadFileAsync(file.FileByteStream);
             //   await client.CloseAsync();

            }
            catch (Exception exception)
            {
                addStatus("Error taking photo: " + exception.Message);
            }
        }

        private async Task<Windows.Storage.StorageFile> ReencodePhotoAsync(
                            Windows.Storage.StorageFile tempStorageFile,
                            Windows.Storage.FileProperties.PhotoOrientation photoRotation)
        {
            Windows.Storage.Streams.IRandomAccessStream inputStream = null;
            Windows.Storage.Streams.IRandomAccessStream outputStream = null;
            Windows.Storage.StorageFile photoStorage = null;

            try
            {
                inputStream = await tempStorageFile.OpenAsync(FileAccessMode.Read);

                var decoder = await BitmapDecoder.CreateAsync(inputStream);

                photoStorage = await KnownFolders.PicturesLibrary.CreateFileAsync(PHOTO_FILE_NAME, CreationCollisionOption.GenerateUniqueName);

                outputStream = await photoStorage.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

                outputStream.Size = 0;

                var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);

                var properties = new BitmapPropertySet();
                properties.Add("System.Photo.Orientation",
                    new Windows.Graphics.Imaging.BitmapTypedValue(photoRotation, PropertyType.UInt16));

                await encoder.BitmapProperties.SetPropertiesAsync(properties);

                await encoder.FlushAsync();
            }
            finally
            {
                if (inputStream != null)
                {
                    inputStream.Dispose();
                }

                if (outputStream != null)
                {
                    outputStream.Dispose();
                }

                var asyncAction = tempStorageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }

            return photoStorage;
        }

        #endregion

    }
}
