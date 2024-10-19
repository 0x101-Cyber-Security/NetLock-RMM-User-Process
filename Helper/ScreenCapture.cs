using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Helper
{
    internal class ScreenCapture
    {
        // Native API to query the number of screens and their properties
        [DllImport("User32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("Gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        private const int HORZRES = 8; // Width of the screen
        private const int VERTRES = 10; // Height of the screen

        // Method to capture the screen and return it as a Base64 string
        public static async Task<string> CaptureScreenToBase64(int screenIndex)
        {
            try
            {
                // Desktop-DC holen
                IntPtr desktopDC = GetDC(IntPtr.Zero);

                // Determine screen size (with GetDeviceCaps)
                int screenWidth = GetDeviceCaps(desktopDC, HORZRES);
                int screenHeight = GetDeviceCaps(desktopDC, VERTRES);

                // Creating a bitmap for the screenshot
                using (Bitmap bmpScreenshot = new Bitmap(screenWidth, screenHeight, PixelFormat.Format32bppArgb))
                {
                    // Create graphic object for drawing
                    using (Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot))
                    {
                        // Take a screenshot of the screen (whole screen)
                        gfxScreenshot.CopyFromScreen(0, 0, 0, 0, new Size(screenWidth, screenHeight), CopyPixelOperation.SourceCopy);
                    }

                    // Bitmap in einen MemoryStream speichern als JPEG
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Qualität der JPEG-Kompression einstellen
                        var encoderParameters = new EncoderParameters(1);
                        encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L); // Qualität 0-100

                        // JPEG-Encoder auswählen
                        ImageCodecInfo jpegCodec = GetEncoder(ImageFormat.Jpeg);
                        bmpScreenshot.Save(ms, jpegCodec, encoderParameters);
                        byte[] imageBytes = ms.ToArray(); // Receive the image as a byte array

                        // In Base64 kodieren
                        return Convert.ToBase64String(imageBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing screen: {ex.Message}");
                return null;
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            try
            {
                ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
                foreach (ImageCodecInfo codec in codecs)
                {
                    if (codec.FormatID == format.Guid)
                    {
                        return codec;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting encoder: {ex.Message}");
                return null;
            }
            
        }
    }
}