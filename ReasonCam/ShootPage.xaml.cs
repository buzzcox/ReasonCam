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
using Windows.UI;
using Windows.Media.Capture;
using Windows.Devices.Enumeration;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Animation;
using System.Threading.Tasks;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using SAClientWRC;
using Windows.ApplicationModel.DataTransfer;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ReasonCam
{

    enum ViewSelect
    {
        Loading = 1,
        Waiting = 2,
        Shooting = 4,
        Sequence = 8,
    };

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShootPage : Page
    {
        MediaCapture mediaCaptureMgr = new Windows.Media.Capture.MediaCapture();
        DeviceInformation captureDevice;
        int counter = 5;
        int photoCount = 0;
        String photoId = "photo_sequence";

        String folderPrefix = "SnapReload";
        StorageFolder currentFolder;

        int NumberOfPhotos = 8;
        int MillisBetweenSnap = 300;

        bool stopSequence = false;
        bool screenSaverMode = false;

        bool reasonScreenSaver = true;

        public  ShootPage()
        {
            this.InitializeComponent();

            setupDefaultShots();

            showView(ViewSelect.Waiting);

            CommsHelper.Initialize();
            CommsHelper.saclient.ShareDataRequested += component_ShareDataRequested;

            CommsHelper.commsReady += client_commsReady;
            CommsHelper.snapTake += client_snapAction;
            CommsHelper.goHome += CommsHelper_goHome;
            CommsHelper.stopReturn += CommsHelper_stopReturn;


            EnumerateWebcamsAsync();

            Application.Current.Suspending += Current_Suspending;
            Application.Current.Resuming += Current_Resuming;


        }

        public async void setupDefaultShots()
        {
            try
            {

            String defaultFolder = String.Format("{0}_0",folderPrefix);
            StorageFolder destinationFolder = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFolderAsync(defaultFolder, CreationCollisionOption.ReplaceExisting);

            for (int p = 0; p <= 8; p++)
            {
                StorageFile copyFile = await StorageFile.GetFileFromApplicationUriAsync(new System.Uri(String.Format("ms-appx:///Assets/SnapReload_0/photo_sequence_{0}.jpg",p.ToString())));
                StorageFile destFile = await destinationFolder.CreateFileAsync(copyFile.Name,CreationCollisionOption.ReplaceExisting);
                await copyFile.CopyAndReplaceAsync(destFile);
            }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("error copying default images: {0}", ex);

            }

        }

        void Current_Resuming(object sender, object e)
        {
            //We need to tell the server that this APP instance on this machine is now available
            CommsHelper.Initialize();
        }

        void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {            
            //We need to tell the server that this APP instance on this machine is no longer running
            CommsHelper.deconstruct();
        }

        //protected override void OnLostFocus(RoutedEventArgs e)
        //{
        //    base.OnLostFocus(e);
        //    //We need to tell the server that this APP instance on this machine is no longer running
        //    CommsHelper.deconstruct();

        //}

        //protected override void OnGotFocus(RoutedEventArgs e)
        //{
        //    base.OnGotFocus(e);

        //    //We need to tell the server that this APP instance on this machine is now available
        //    CommsHelper.Initialize();

        //    CommsHelper.commsReady += client_commsReady;
        //    CommsHelper.snapTake += client_snapAction;
        //    CommsHelper.goHome += CommsHelper_goHome;
        //    CommsHelper.stopReturn += CommsHelper_stopReturn;
        //    CommsHelper.saclient.InitShareCharmHandler();
        //    CommsHelper.saclient.ShareDataRequested += component_ShareDataRequested;
        //}

        public async Task<int> totalShots()
        {
            List<StorageFolder> sflist = await folderList();
            return sflist.Count;
        }

        public async Task<List<StorageFolder>> folderList()
        {
            List<StorageFolder> folders = new List<StorageFolder>();
            IReadOnlyList<IStorageItem> snapList = await Windows.Storage.KnownFolders.PicturesLibrary.GetItemsAsync();
            for (int i = 0; i < snapList.Count; i++)
            {
                if (snapList[i].Name.StartsWith(folderPrefix) && snapList[i].IsOfType(StorageItemTypes.Folder))
                {
                    folders.Add((StorageFolder)snapList[i]);
                }
            }
            return folders;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        #region View management methods

        private void showView(ViewSelect vs)
        {
            // reset some app defaults
            bottomBar.IsOpen = false;
            stopSequence = true;
            stopScreenSaver();  // view reset screen saver

            switch (vs)
            {
                case ViewSelect.Loading:
                    {
                        ShootingView.Visibility = Visibility.Collapsed;
                        WaitingView.Visibility = Visibility.Collapsed;
                        LoadingView.Visibility = Visibility.Visible;
                        SequenceView.Visibility = Visibility.Collapsed;
                        bottomBar.IsEnabled = false;
                    }
                    break;
                case ViewSelect.Shooting:
                    {
                        ShootingView.Visibility = Visibility.Visible;
                        WaitingView.Visibility = Visibility.Collapsed;
                        LoadingView.Visibility = Visibility.Collapsed;
                        SequenceView.Visibility = Visibility.Collapsed;
                        bottomBar.IsEnabled = false;
                    }
                    break;
                case ViewSelect.Waiting:
                    {
                        startScreenSaver();
                        ShootingView.Visibility = Visibility.Collapsed;
                        WaitingView.Visibility = Visibility.Visible;
                        LoadingView.Visibility = Visibility.Collapsed;
                        SequenceView.Visibility = Visibility.Collapsed;
                        bottomBar.IsEnabled = false;
                    }
                    break;
                case ViewSelect.Sequence:
                    {
                        ShootingView.Visibility = Visibility.Collapsed;
                        WaitingView.Visibility = Visibility.Collapsed;
                        LoadingView.Visibility = Visibility.Collapsed;
                        SequenceView.Visibility = Visibility.Visible;
                        SnapMessage.Visibility = Visibility.Collapsed;
                        bottomBar.IsEnabled = true;
                        CancelButton.IsEnabled = false;
                    }
                    break;
            };

        }

        private async Task<String> getCurrentSnapFolder()
        {
            List<StorageFolder> folders = await folderList();

            if (folders.Count == 0) return String.Format("{0}_0", folderPrefix);

            int topFolderNumber = 0;

            for (int i = 0; i < folders.Count; i++)
            {
                try
                {
                    int folderNum = Convert.ToInt32(folders[i].Name.Replace(String.Format("{0}_", folderPrefix), ""));
                    topFolderNumber = Math.Max(topFolderNumber, folderNum);
                }
                catch (Exception)
                {
                    Debug.WriteLine("Can't get folder number, possible a custom folder has been place without app management");
                }
            }

            return String.Format("{0}_{1}", folderPrefix, topFolderNumber);
        }

        private async Task<String> getNextSnapFolder()
        {
            List<StorageFolder> folders = await folderList();

            if (folders.Count == 0) return String.Format("{0}_0", folderPrefix);

            int topFolderNumber = 0;

            for (int i = 0; i < folders.Count; i++)
            {
                try
                {
                    int folderNum = Convert.ToInt32(folders[i].Name.Replace(String.Format("{0}_", folderPrefix), ""));
                    topFolderNumber = Math.Max(topFolderNumber, folderNum);
                }
                catch (Exception)
                {
                    Debug.WriteLine("Can't get folder number, possible a custom folder has been place without app management");
                }
            }

            string nextSnapFolder = String.Format("{0}_{1}", folderPrefix, (topFolderNumber + 1));

            Debug.WriteLine("next folder: {0}", nextSnapFolder);

            return String.Format(nextSnapFolder);
        }

        #endregion

        #region waiting page controls

        DispatcherTimer screenSaverTimer = null;
        DispatcherTimer glowTapTimer = null;
        private async void startScreenSaver()
        {
            StartAnim.Begin();
            loadingDisplay.Opacity = 1.0;
            if (screenSaverTimer != null)
            {
                screenSaverTimer.Stop();
                screenSaverTimer = null;
                glowTapTimer.Stop();
                glowTapTimer = null;
            }

            Random rnd = new Random();

            screenSaverTimer = new DispatcherTimer();
            screenSaverTimer.Tick += screenSaverTimer_tick;
            screenSaverTimer.Interval = new TimeSpan(0, 0, rnd.Next(8, 18));

            glowTapTimer = new DispatcherTimer();
            glowTapTimer.Tick += glowTapTimer_tick;
            glowTapTimer.Interval = new TimeSpan(0, 0, rnd.Next(5, 10));
            
            screenSaverMode = false;

            screensaverView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            loadingDisplay.Visibility = Windows.UI.Xaml.Visibility.Visible;

            if (await this.totalShots() > 0)
            {
                screenSaverTimer.Start();
            }

            glowTapTimer.Start();
        }

        private void stopScreenSaver()
        {
            screenSaverMode = false;
            stopSequence = true;

            if (screenSaverTimer != null)
            {
                screenSaverTimer.Stop();
                screenSaverTimer = null;
                glowTapTimer.Stop();
                glowTapTimer = null;
            }
            // stop timers here
            screensaverView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            loadingDisplay.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void glowTapTimer_tick(object sender, object e)
        {
            StartAnim.Begin();
        }
        
        private async void screenSaverTimer_tick(object sender, object e)
        {
            if (screenSaverMode)    // show waiting screen
            {
                screenSaverMode = false;
                stopSequence = true;

                // start timers here to scroll through sequence
                screensaverView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                loadingDisplay.Visibility = Windows.UI.Xaml.Visibility.Visible;
                tapScreenText.Visibility = Visibility.Visible;
                loadingDisplay.Opacity = 1.0;
            }
            else // show screen saver
            {
                screenSaverMode = true;

                // start timers here to scroll through sequence
                screensaverView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                loadingDisplay.Visibility = Windows.UI.Xaml.Visibility.Visible;
                tapScreenText.Visibility = Visibility.Collapsed;
                loadingDisplay.Opacity = 0.6;

                Random rnd = new Random();
                List<StorageFolder> folders = await folderList();
                int folderIdx = rnd.Next(folders.Count);
                StorageFolder toUse = folders[folderIdx];

                if (reasonScreenSaver)
                {
                    reasonScreenSaver = false;
                    for (int i = 0; i < folders.Count; i++)
                    {
                        if (folders[i].Name == String.Format("{0}_0", folderPrefix))
                            toUse = folders[i];
                    }
                }
                else
                {
                    reasonScreenSaver = true;
                }

                stopSequence = false;
                showSequenceForFolder(toUse,true);
            }
        }


        private void ClickThroughButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (waitingRing.IsActive) return;

            waitingRing.IsActive = true;

            CommsHelper.sendMessage(CommandMessage.StartSnap, null);
        }

        #endregion

        #region Client control methods

        void client_snapAction(object sender)
        {
            waitingRing.IsActive = false;

            showView(ViewSelect.Shooting);

            doCountDown();
        }

        void client_commsReady(object sender)
        {
            waitingRing.IsActive = false;
        }

        #endregion

        #region PhotoCapture methods

        private async void EnumerateWebcamsAsync()
        {
            try
            {
            //    addStatus("Enumerating Webcams...");
                DeviceInformationCollection devInfoCollection = null;
                List<DeviceInformation> deviceList = new List<DeviceInformation>();

                deviceList.Clear();

                devInfoCollection = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                if (devInfoCollection.Count == 0)
                {
                        
                 //   addStatus("No WebCams found.");
                }
                else
                {
                    for (int i = 0; i < devInfoCollection.Count; i++)
                    {
                        var devInfo = devInfoCollection[i];
                        deviceList.Add(devInfo);
                    }

                    captureDevice = deviceList[0];
                    this.startCapture();
                  //  addStatus("Enumerating Webcams completed successfully.");
                }
            }
            catch (Exception)
            {
            // //   addStatus(e.Message);
            }
        }

        private async void startCapture()
        {
            try
            {

                var settings = new Windows.Media.Capture.MediaCaptureInitializationSettings();

                settings.VideoDeviceId = captureDevice.Id;
                settings.StreamingCaptureMode = StreamingCaptureMode.Video;


                await mediaCaptureMgr.InitializeAsync(settings);

            }
            catch(Exception){}

            try
            {
                cameraView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                cameraElement.Source = mediaCaptureMgr;
                await mediaCaptureMgr.StartPreviewAsync();

            }
            catch(Exception){}

        }

        private async void doCountDown()
        {

            currentFolder = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFolderAsync(await this.getNextSnapFolder(), CreationCollisionOption.ReplaceExisting);

            counter = 5;
            this.TimerView.Visibility = Visibility.Visible;
            this.txtCounter.Visibility = Visibility.Visible;
            this.txtCounter.Text = counter.ToString();

            DispatcherTimer TextCountDownTrm = new DispatcherTimer();
            TextCountDownTrm.Tick += TextCountDownTrm_Tick;
            TextCountDownTrm.Interval = new TimeSpan(0, 0, 1);
            TextCountDownTrm.Start();

            txtCounter.RenderTransform = new ScaleTransform();
            CountdownAnim.Begin();
        }

        void TextCountDownTrm_Tick(object sender, object e)
        {
            counter--;
            this.txtCounter.Text = counter.ToString();
            this.txtCounter.Opacity = 1.0;
            CountdownAnim.Begin();

            if (counter <= 0)   // we've hit the timer, stop counting
            {
                DispatcherTimer tmr = (DispatcherTimer)sender;
                tmr.Stop();
                this.txtCounter.Visibility = Visibility.Collapsed;
                this.TimerView.Visibility = Visibility.Collapsed;

                photoCount = 0;
                DispatcherTimer ShootTimer = new DispatcherTimer();
                ShootTimer.Tick += ShootTimer_Tick;
                ShootTimer.Interval = new TimeSpan(0,0,0,0,MillisBetweenSnap);
                ShootTimer.Start();
            }
        }

        private async void ShootTimer_Tick(object sender, object e)
        {
            if (photoCount >= NumberOfPhotos)
            {
                DispatcherTimer tmr = (DispatcherTimer)sender;
                tmr.Stop();

                showView(ViewSelect.Loading);

                this.GenerateSequence();
            }

            SnapScreen.Opacity = 100;
            await Task.Delay(10);

            try
            {
                // Take photo
                ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
                String photoFilename = photoId + "_" + photoCount.ToString() + "_TMP";
                StorageFile tempPhotoStorageFile = await currentFolder.CreateFileAsync(photoFilename, CreationCollisionOption.ReplaceExisting);
                //     StorageFile tempPhotoStorageFile = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFileAsync(photoFilename, CreationCollisionOption.ReplaceExisting);

                await mediaCaptureMgr.CapturePhotoToStorageFileAsync(imageProperties, tempPhotoStorageFile);


                StorageFile photoStorageFile = await ReencodePhotoAsync(tempPhotoStorageFile, Windows.Storage.FileProperties.PhotoOrientation.Normal);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Exception when re-encoding file: {0}", ex.Message));
            }

            await Task.Delay(10);
            SnapScreen.Opacity = 0;
            photoCount++;

        //    IRandomAccessStream photoStream = await photoStorageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
        //    BitmapImage bmpimg = new BitmapImage();
        //    bmpimg.SetSource(photoStream);
  //          photoElement.Source = bmpimg;

      //      byte[] imageBytes = convertToBytes(bmpimg);

        }

        private async void GenerateSequence()
        {
           await Task.Delay(TimeSpan.FromSeconds(1.5));

           showView(ViewSelect.Sequence);

           this.showCurrentSequence();

   //        this.generateGif();
        }

        private async void generateGif()
        {
            Debug.WriteLine("Generating Gif");

            IReadOnlyList<IStorageItem> snapList = await currentFolder.GetItemsAsync();

            StorageFile currFile = null;
            for (int i = 0; i < snapList.Count; i++)
            {
                currFile = await currentFolder.GetFileAsync(snapList[i].Name);
                break;
            }

            GifMaker gm = new GifMaker(800, 600);
            gm.AppenFrameImage(await ImageController.GetImage(currFile));

            StorageFile storageFile = await KnownFolders.PicturesLibrary.CreateFileAsync("thegif.gif", CreationCollisionOption.ReplaceExisting);
            gm.GenerateFromBitmapsAsync(storageFile,20);
        }

        private async Task<Windows.Storage.StorageFile> ReencodePhotoAsync(Windows.Storage.StorageFile tempStorageFile, Windows.Storage.FileProperties.PhotoOrientation photoRotation)
        {
            Windows.Storage.Streams.IRandomAccessStream inputStream = null;
            Windows.Storage.Streams.IRandomAccessStream outputStream = null;
            Windows.Storage.StorageFile photoStorage = null;

            try
            {

                String newPhotoFilename = photoId + "_" + photoCount.ToString() + ".jpg";

                inputStream = await tempStorageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);

                var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(inputStream);

                photoStorage = await currentFolder.CreateFileAsync(newPhotoFilename, Windows.Storage.CreationCollisionOption.GenerateUniqueName);

                outputStream = await photoStorage.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

                outputStream.Size = 0;

                var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);

                var properties = new Windows.Graphics.Imaging.BitmapPropertySet();
                properties.Add("System.Photo.Orientation", new Windows.Graphics.Imaging.BitmapTypedValue(photoRotation, Windows.Foundation.PropertyType.UInt16));

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

                var asyncAction = tempStorageFile.DeleteAsync(Windows.Storage.StorageDeleteOption.PermanentDelete);
            }

            return photoStorage;
        }

        #endregion

        #region sequence + screen saver methods

        DispatcherTimer MessageTimer = new DispatcherTimer();
        DispatcherTimer ReturnTimer = new DispatcherTimer();
        DispatcherTimer ShareTimer = new DispatcherTimer();
        DispatcherTimer ReturnCounterTimer = new DispatcherTimer();
        int returnCounter = 15;
        private void StartTimers()
        {
            MessageTimer.Stop();
            ReturnTimer.Stop();
            ShareTimer.Stop();
            ReturnCounterTimer.Stop();

            MessageTimer = null;
            ReturnTimer = null;
            ShareTimer = null;
            ReturnCounterTimer = null;

            ShareTimer = new DispatcherTimer();
            MessageTimer = new DispatcherTimer();
            ReturnTimer = new DispatcherTimer();
            ReturnCounterTimer = new DispatcherTimer();

            MessageTimer.Tick += MessageTimer_Tick;
            MessageTimer.Interval = TimeSpan.FromSeconds(45);
            MessageTimer.Start();

            ReturnTimer.Tick += ReturnTimer_Tick;
            ReturnTimer.Interval = TimeSpan.FromSeconds(60);
            ReturnTimer.Start();

            ShareTimer.Tick += ShareTimer_Tick;
            Random rnd = new Random();
            ShareTimer.Interval = TimeSpan.FromSeconds(rnd.Next(7,14));
            ShareTimer.Start();

            returnCounter = 15;
            ReturnCounterTimer.Tick += ReturnCounterTimer_Tick;
            ReturnCounterTimer.Interval = TimeSpan.FromSeconds(1);
        }

        private void MessageTimer_Tick(object sender, object e)
        {
            SnapMessage.Visibility = Visibility.Visible;
            CancelButton.IsEnabled = true;

            returnTextCounter.Text = returnCounter.ToString();
            ReturnCounterTimer.Start();
        }

        void ReturnCounterTimer_Tick(object sender, object e)
        {
            returnCounter--;
            returnTextCounter.Text = returnCounter.ToString();
        }

        private void ReturnTimer_Tick(object sender, object e)
        {
            MessageTimer.Stop();
            ReturnTimer.Stop();
            ShareTimer.Stop();
            ReturnCounterTimer.Stop();
            showView(ViewSelect.Waiting);
        }

        private void ShareTimer_Tick(object sender, object e)
        {
        //    Share_hint.Visibility = Windows.UI.Xaml.Visibility.Visible;
            ShareAnim.Begin();
            NewHintAnim.Begin();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // broadcast stop command
            CommsHelper.sendMessage(CommandMessage.StopReturn, null);
        }

        private void showCurrentSequence()
        {
            stopSequence = false;
            this.showSequenceForFolder(currentFolder, true);


            StartTimers();
        }

        private async void showSequenceForFolder(StorageFolder sf, bool repeat)
        {
            if (stopSequence) return;

            IReadOnlyList<IStorageItem> snapList = await sf.GetItemsAsync();

            try
            {

            for (int i=0; i < snapList.Count; i++)
            {
                StorageFile currFile = await sf.GetFileAsync(snapList[i].Name);

                BitmapImage bmpimg = await ImageController.GetImage(currFile);
                sequenceElement.Source = bmpimg;
                screensaverView.Source = bmpimg;

                await Task.Delay(TimeSpan.FromMilliseconds(150));
            }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("exception caught in sequence: " + ex.Message);
            }

            if (repeat) this.showSequenceForFolder(sf,repeat);
        }

        void CommsHelper_stopReturn(object sender)
        {
            Debug.WriteLine("stop sequence selected");
            SnapMessage.Visibility = Visibility.Collapsed;

            StartTimers();
        }

#endregion

        #region Share functions

        async void component_ShareDataRequested(object sender, ShareInfo e)
        {
            try
            {
                // FB details
                e.FacebookCredentials = new Credentials("test@withreason.co.uk", "CheeseFlute-71");

                e.FacbookPageID = "393352484117082";

                // Twitter details 
                e.TwitterCredentials = new Credentials("reasonsoftarray", "CheeseFlute-71");

                IReadOnlyList<IStorageItem> snapList = await this.currentFolder.GetItemsAsync();
                StorageFile currFile = await this.currentFolder.GetFileAsync(snapList[0].Name); //<-- get current folder

                RandomAccessStreamReference imageStreamRef = RandomAccessStreamReference.CreateFromFile(currFile);
                e.ImageStream = imageStreamRef;

                //    e.TextData = "";

                e.Complete();
            }
            catch (Exception)
            {

                throw;
            }
        }


 
        #endregion

        #region AppBar methods

        void CommsHelper_goHome(object sender)
        {
            this.showView(ViewSelect.Waiting);
        }

        private void newShot(object sender, RoutedEventArgs e)
        {
            this.ClickThroughButton_Click_1(this, null);
        }

        private void goHome(object sender, RoutedEventArgs e)
        {
            CommsHelper.sendMessage(CommandMessage.GoHome, null);
        }

        #endregion
    }
}
