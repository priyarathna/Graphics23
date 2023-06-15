using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace A25;

class MyWindow : Window {
   public MyWindow () {
      Width = 800; Height = 600;
      Left = 50; Top = 50;
      WindowStyle = WindowStyle.None;
      Image image = new Image () {
         Stretch = Stretch.None,
         HorizontalAlignment = HorizontalAlignment.Left,
         VerticalAlignment = VerticalAlignment.Top,
      };
      RenderOptions.SetBitmapScalingMode (image, BitmapScalingMode.NearestNeighbor);
      RenderOptions.SetEdgeMode (image, EdgeMode.Aliased);

      mBmp = new WriteableBitmap ((int)Width, (int)Height,
         96, 96, PixelFormats.Gray8, null);
      mStride = mBmp.BackBufferStride;
      image.Source = mBmp;
      Content = image;

      //DrawMandelbrot (-0.5, 0, 1);

      MouseDown += OnMouseDown;
   }

   void OnMouseDown (object sender, MouseButtonEventArgs e) {
      var pt = e.GetPosition (this);
      if (mStartPt == null) mStartPt = pt;
      else {
         try {
            mBmp.Lock ();
            mBase = mBmp.BackBuffer;
            // Bresenham's line algorithm
            int x0 = (int)mStartPt.Value.X, y0 = (int)mStartPt.Value.Y, x1 = (int)pt.X, y1 = (int)pt.Y;
            if (Math.Abs (y1 - y0) < Math.Abs (x1 - x0)) {
               if (x0 > x1) (x0, y0, x1, y1) = (x1, y1, x0, y0);
               PlotLineLow (x0, y0, x1, y1);
            } else {
               if (y0 > y1) (x0, y0, x1, y1) = (x1, y1, x0, y0);
               PlotLineHigh (x0, y0, x1, y1);
            }
         } finally { mBmp.Unlock (); mStartPt = null; }
      }

      void PlotLineLow (int x0, int y0, int x1, int y1) {
         int deltaX = x1 - x0, deltaY = y1 - y0, x = x0, y = y0, delta = 1;
         if (deltaY < 0) {
            delta = -1; deltaY = -deltaY;
         }
         var factor = (2 * deltaY) - deltaX;
         for (; x < x1; x++) {
            SetPixel (x, y, 255);
            mBmp.AddDirtyRect (new Int32Rect (x, y, 1, 1));
            if (factor > 0) {
               y += delta;
               factor += 2 * (deltaY - deltaX);
            } else factor += 2 * deltaY;
         }
      }

      void PlotLineHigh (int x0, int y0, int x1, int y1) {
         int deltaX = x1 - x0, deltaY = y1 - y0, x = x0, y = y0, delta = 1;
         if (deltaX < 0) {
            delta = -1; deltaX = -deltaX;
         }
         var factor = (2 * deltaX) - deltaY;
         for (; y < y1; y++) {
            SetPixel (x, y, 255);
            mBmp.AddDirtyRect (new Int32Rect (x, y, 1, 1));
            if (factor > 0) {
               x += delta;
               factor += 2 * (deltaX - deltaY);
            } else factor += 2 * deltaX;
         }
      }

   }

   void DrawMandelbrot (double xc, double yc, double zoom) {
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         int dx = mBmp.PixelWidth, dy = mBmp.PixelHeight;
         double step = 2.0 / dy / zoom;
         double x1 = xc - step * dx / 2, y1 = yc + step * dy / 2;
         for (int x = 0; x < dx; x++) {
            for (int y = 0; y < dy; y++) {
               Complex c = new Complex (x1 + x * step, y1 - y * step);
               SetPixel (x, y, Escape (c));
            }
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, dx, dy));
      } finally {
         mBmp.Unlock ();
      }
   }

   byte Escape (Complex c) {
      Complex z = Complex.Zero;
      for (int i = 1; i < 32; i++) {
         if (z.NormSq > 4) return (byte)(i * 8);
         z = z * z + c;
      }
      return 0;
   }

   void OnMouseMove (object sender, MouseEventArgs e) {
      if (e.LeftButton == MouseButtonState.Pressed) {
         try {
            mBmp.Lock ();
            mBase = mBmp.BackBuffer;
            var pt = e.GetPosition (this);
            int x = (int)pt.X, y = (int)pt.Y;
            SetPixel (x, y, 255);
            mBmp.AddDirtyRect (new Int32Rect (x, y, 1, 1));
         } finally {
            mBmp.Unlock ();
         }
      }
   }

   void DrawGraySquare () {
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         for (int x = 0; x <= 255; x++) {
            for (int y = 0; y <= 255; y++) {
               SetPixel (x, y, (byte)x);
            }
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, 256, 256));
      } finally {
         mBmp.Unlock ();
      }
   }

   void SetPixel (int x, int y, byte gray) {
      unsafe {
         var ptr = (byte*)(mBase + y * mStride + x);
         *ptr = gray;
      }
   }

   WriteableBitmap mBmp;
   int mStride;
   nint mBase;
   Point? mStartPt;
}

internal class Program {
   [STAThread]
   static void Main (string[] args) {
      Window w = new MyWindow ();
      w.Show ();
      Application app = new Application ();
      app.Run ();
   }
}
