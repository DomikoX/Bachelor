using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Encoder = System.Drawing.Imaging.Encoder;

namespace DeviceAppLibralies
{
    public class ScreenManager
    {
        private Bitmap _prev;
        private long Quality { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quality">quality of jpg screenshot </param>
        public ScreenManager(long quality = 60L)
        {
            Quality = quality;
        }

        /// <summary>
        /// Capture Actual screen in full resolution
        /// </summary>
        /// <returns> bitmap of actual screen</returns>
        public static Bitmap CaptureScreen()
        {
            Bitmap newBitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                Screen.PrimaryScreen.Bounds.Height);
            using (Graphics graphics = Graphics.FromImage(newBitmap))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, newBitmap.Size);
            }

            return newBitmap;
        }

        /// <summary>
        /// Capture screenshot with optional width and height
        /// </summary>
        /// <param name="width">width od screnshot in px</param>
        /// <param name="height">height of screnshot in px</param>
        /// <returns>bitmap of actual screen</returns>
        public static Bitmap CaptureScreen(int width, int height)
        {
            Bitmap newBitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                Screen.PrimaryScreen.Bounds.Height);
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(newBitmap))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, newBitmap.Size);

                Graphics nb = Graphics.FromImage(bitmap);
                nb.DrawImage(newBitmap, 0, 0, width, height);
            }

            return bitmap;
        }

        /// <summary>
        /// Capture scrennshot in selected ratio
        /// width = actual screen width / ratio
        /// height = actual screen height/ ratio
        /// </summary>
        /// <param name="ratio"></param>
        /// <returns>bitmap of actual screen</returns>
        public static Bitmap CaptureScreen(float ratio)
        {
            int newW = (int) (Screen.PrimaryScreen.Bounds.Width/ratio);
            int newH = (int) (Screen.PrimaryScreen.Bounds.Height/ratio);

            Bitmap newBitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                Screen.PrimaryScreen.Bounds.Height);
            Bitmap bitmap = new Bitmap(newW, newH);
            using (Graphics graphics = Graphics.FromImage(newBitmap))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, newBitmap.Size);

                Graphics nb = Graphics.FromImage(bitmap);
                nb.DrawImage(newBitmap, 0, 0, newW, newH);
            }

            return bitmap;
        }


        public Bitmap GetScreenDiff()
        {
            if (this._prev == null)
            {
                this._prev = CaptureScreen();
                return this._prev;
            }

            Bitmap actualScreen = CaptureScreen();

            Bitmap diff = new Bitmap(_prev.Width, _prev.Height);

            Rectangle bounds = new Rectangle(0, 0, _prev.Width, _prev.Height);
            var bmpDataA = _prev.LockBits(bounds, ImageLockMode.ReadWrite, _prev.PixelFormat);
            var bmpDataB = actualScreen.LockBits(bounds, ImageLockMode.ReadWrite, actualScreen.PixelFormat);
            var bmpData3 = diff.LockBits(bounds, ImageLockMode.ReadWrite, diff.PixelFormat);

            int npixels = _prev.Height*bmpDataA.Stride/4;
            unsafe
            {
                int* pPixelsA = (int*) bmpDataA.Scan0.ToPointer();
                int* pPixelsB = (int*) bmpDataB.Scan0.ToPointer();
                int* pPixels3 = (int*) bmpData3.Scan0.ToPointer();

                for (int i = 0; i < npixels; ++i)
                {
                    if (pPixelsA[i] != pPixelsB[i])
                    {
                        pPixels3[i] = pPixelsB[i];
                    }
                }
            }
            _prev.UnlockBits(bmpDataA);
            actualScreen.UnlockBits(bmpDataB);
            diff.UnlockBits(bmpData3);
            _prev = actualScreen;
            return diff;
        }
        /// <summary>
        /// Get screenshot of actual screen 
        /// When screen did not change since last call of this method return null 
        /// </summary>
        /// <param name="objects">Determine witch methos will be used to capture screen.
        /// Posible options: nothing;  int width, int height ; float ratio
        /// </param>
        /// <returns></returns>
        public byte[] GetNextScreen(params object[] objects)
        {
            Bitmap actualScreen;

            try
            {
                if (objects == null)
                {
                    actualScreen = CaptureScreen();
                    
                }else if (objects.Length == 1)
                {
                    actualScreen = CaptureScreen((float) objects[0]);
                }
                else if (objects.Length == 2)
                {
                    actualScreen = CaptureScreen((int) objects[0], (int) objects[1]);
                }
                else
                {
                    actualScreen = CaptureScreen();
                }
            }
            catch (Exception)
            {
                return null;
            }


            if (this._prev == null)
            {
                this._prev = actualScreen;
                return PackBitmap(this._prev);
            }


            Rectangle bounds = new Rectangle(0, 0, _prev.Width, _prev.Height);
            var bmpDataA = _prev.LockBits(bounds, ImageLockMode.ReadWrite, _prev.PixelFormat);
            var bmpDataB = actualScreen.LockBits(bounds, ImageLockMode.ReadWrite, actualScreen.PixelFormat);

            int npixels = _prev.Height*bmpDataA.Stride/4;
            unsafe
            {
                int* pPixelsA = (int*) bmpDataA.Scan0.ToPointer();
                int* pPixelsB = (int*) bmpDataB.Scan0.ToPointer();

                for (int i = 0; i < npixels; ++i)
                {
                    if (pPixelsA[i] != pPixelsB[i])
                    {
                        _prev.UnlockBits(bmpDataA);
                        actualScreen.UnlockBits(bmpDataB);
                        this._prev = actualScreen;
                        return PackBitmap(actualScreen);
                    }
                }
            }
            _prev.UnlockBits(bmpDataA);
            actualScreen.UnlockBits(bmpDataB);
            return null;
        }
        /// <summary>
        /// Convert selected Bitmap to array of bytes
        /// </summary>
        /// <param name="screen"> Bitmap to packed</param>
        /// <returns>array of bytes from slected Bitmap</returns>
        private byte[] PackBitmap(Bitmap screen)
        {
            MemoryStream ms = new MemoryStream();

            var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
            var encParams = new EncoderParameters()
            {
                Param = new[] {new EncoderParameter(Encoder.Quality, this.Quality)}
            };
            screen.Save(ms, encoder, encParams);
            return ms.ToArray();
        }
    }
}