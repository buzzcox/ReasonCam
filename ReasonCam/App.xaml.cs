using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace ReasonCam
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(ShootPage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }


        //--------------- NETWORK MANAGMENT --------------------
        /*
         void Current_Resuming(object sender, object e)
{
    //We need to tell the server that this APP instance on this machine is now available
    if (CommsHelper.saclient == null)
    {
        CommsHelper.saclient = new SAClientWRC.SAClient(CommsHelper.CurrentAppID);

        CommsHelper.saclient.InitialiseClient();
    }
}

void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
{
    //We need to tell the server that this APP instance on this machine is no longer running
    if (CommsHelper.saclient != null)
        CommsHelper.saclient.Dispose();

    CommsHelper.saclient = null;
}


protected override void OnLostFocus(RoutedEventArgs e)
{
    //We need to tell the server that this APP instance on this machine is no longer running
    if (CommsHelper.saclient != null)
    {
        CommsHelper.saclient.Dispose();
        CommsHelper.saclient = null;
    }
}

protected override void OnGotFocus(RoutedEventArgs e)
{
    if (CommsHelper.saclient == null)
    {
        CommsHelper.saclient = new SAClientWRC.SAClient(CommsHelper.CurrentAppID);
        CommsHelper.saclient.InitialiseClient();
    }
}
*/
        //-------------------------------------------------------
    }
}
