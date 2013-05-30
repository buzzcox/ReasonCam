using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using SAClientWRC;

namespace ReasonCam
{
  //  public delegate void MethodDel(object sender);


    static class CommsHelper
    {
        static public SAClient saclient = null;
        static private int CurrentAppID = 1004;
        static private bool IsConnected = false;

        public delegate void commsReadyOnline(object sender);
        static public event commsReadyOnline commsReady;

        public delegate void snapTakeAction(object sender);
        static public event snapTakeAction snapTake;

        public delegate void goHomeAction(object sender);
        static public event goHomeAction goHome;

        public delegate void stopReturnAction(object sender);
        static public event stopReturnAction stopReturn;


        static public void Initialize()
        {
        //    Messenger.Default.Register<string>(this, NotificationId, m => Console.WriteLine("hello world with context: " + m.Context));

            saclient = new SAClientWRC.SAClient(CurrentAppID);

            saclient.ConnectionStatusEvent += SA_ConnectionStatusEvent;
            saclient.DataArrivedEvent += SA_DataArrivedEvent;
            saclient.BroadcastDataArrivedEvent += SA_BroadcastDataArrivedEvent;
            saclient.JSONBroadcastDataArrivedEvent += SA_JSONBroadcastDataArrivedEvent;
            saclient.ExceptionEvent += SA_ExceptionEvent;
            saclient.DeviceMapUpdatedEvent += SA_DeviceMapUpdatedEvent;
            saclient.JSONDeviceMapUpdatedEvent += SA_JSONDeviceMapUpdatedEvent;

            saclient.InitialiseClient();
        }

        static public void Current_Resuming()
        {
            //We need to tell the server that this APP instance on this machine is now available
            if (saclient == null)
                saclient = new SAClientWRC.SAClient(CurrentAppID);

            saclient.InitialiseClient();
        }

        static public void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            //We need to tell the server that this APP instance on this machine is no longer running
            if (saclient != null)
                saclient.Dispose();

            saclient = null;
        }

        #region SA Client Events

        static void SA_JSONDeviceMapUpdatedEvent(object sender, string e)
        {
            //int a = 0;
        //    addStatus("json device map updated event");
            Debug.WriteLine("json device map: " + e);
        }

        static void SA_DeviceMapUpdatedEvent(object sender, DeviceMap e)
        {
            Debug.WriteLine("device map updated event");

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

        static void SA_ExceptionEvent(object sender, string e)
        {
            Debug.WriteLine("device map updated event");
            //this.AddEventListItem(e, ItemState.ERROR);
        }

        static void SA_ConnectionStatusEvent(object sender, SAClientWRC.Status e)
        {
            Debug.WriteLine("Client status event: " + (e.IsConnected ? "connected" : "not connected"));

            //    this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            //    {
            IsConnected = (e.IsConnected == true) ? true : false;

            if (e.IsReadyToConnect == true)
            {
                Debug.WriteLine("Device id: " + e.CurrentDeviceID + " is ready to connect");
                saclient.Connect();
            }
            else
                Debug.WriteLine("no configuration found");


            if (IsConnected == true)
            {
                commsReady(null);
                Debug.WriteLine("CONNECTED to server!");
                saclient.StartListening();
            }
            else
                Debug.WriteLine("Connection CLOSED!");


            if (e.IsListening == true)
                Debug.WriteLine("Listening for broadcast");
            else
                Debug.WriteLine("not listening");


            if (e.MessageSent == true)
                Debug.WriteLine("Message sent!");

            if (e.StatusException != "")
                Debug.WriteLine("Exception occured!: " + e.StatusException);

            //   });
        }

        static void SA_JSONBroadcastDataArrivedEvent(object sender, string e)
        {
            Debug.WriteLine("device map updated event");
            //int a = 0;
        }

        static void SA_DataArrivedEvent(object sender, SocketData e)
        {
            Debug.WriteLine("data arrived for me");

            //try
            //{

            //    Debug.WriteLine("deserialize object: " + recievedData.ToString());
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine("error deserializing: " + ex.Message);
            //}


            //if (e.BroadcastPacket == false)
            //    this.AddDirectEventListItem(e);
            //else
            //    this.AddEventListItem("Data Arrived Event", ItemState.NOTIFICATION);
        }

        static void SA_BroadcastDataArrivedEvent(object sender, SocketData e)
        {
            Debug.WriteLine("Broadcast data arrived");
          //  Debug.WriteLine(String.Format("Message received: {0}", e.Message));
         //   Debug.WriteLine(String.Format("Data received: {0}", e.Data));

            try
            {
             //   SocketData sd = SAHelper.DeserializeToSocketData(e.Message);

                Debug.WriteLine(String.Format("Message received: {0}", e.Message));
                Debug.WriteLine(String.Format("Data received: {0}", e.Data));

                TransMessage tm = (TransMessage)SAHelper.ByteArrayObjToObject(e.Data, typeof(TransMessage));

                if (tm != null)
                {
                    Debug.WriteLine("RECEIVED MESSAGE - " + tm.ToString());
                    processMessage(tm);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR deserializing: " + ex.Message);
            }

        }

        #endregion

        #region comms helper methods

        static public void sendMessage(CommandMessage command, Object data)
        {
            if (!IsConnected) return;
            TransMessage tm = new TransMessage(command, data);
            Debug.WriteLine("SENDING MESSAGE - " + tm.ToString());
            saclient.SendObject(tm, false);
        }

        static void processMessage(TransMessage tm)
        {
            switch (tm.Command)
            {
                case (CommandMessage.StopReturn):
                    stopReturn(null);
                    break;
                case (CommandMessage.StartSnap):
                    snapTake(null);
                    break;
                case (CommandMessage.GoHome):
                    goHome(null);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
