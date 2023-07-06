// LinesWin.cs - Demo window for testing the DrawLine and related functions
// ---------------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.IO;

namespace GrayBMP;

class LinesWin : Window {
   public LinesWin () {
      Width = 900; Height = 600;
      Left = 200; Top = 50;
      WindowStyle = WindowStyle.None;
      mBmp = new GrayBMP (Width, Height);

      Image image = new () {
         Stretch = Stretch.None,
         HorizontalAlignment = HorizontalAlignment.Left,
         VerticalAlignment = VerticalAlignment.Top,
         Source = mBmp.Bitmap
      };
      RenderOptions.SetBitmapScalingMode (image, BitmapScalingMode.NearestNeighbor);
      RenderOptions.SetEdgeMode (image, EdgeMode.Aliased);
      Content = image;
      mDX = mBmp.Width; mDY = mBmp.Height;

      foreach (var line in File.ReadAllLines ("C:/etc/leaf-fill.txt"))
         inputs.Add (line.Split (' ').Select (x => Convert.ToInt32 (x)).ToArray ());

      // Start a timer to repaint a new frame every 33 milliseconds
      DispatcherTimer timer = new () {
         Interval = TimeSpan.FromMilliseconds (100), IsEnabled = true,
      };
      timer.Tick += NextFrame2;
   }
   readonly GrayBMP mBmp;
   readonly int mDX, mDY;

   void NextFrame (object sender, EventArgs e) {
      using (new BlockTimer ("Lines")) {
         mBmp.Begin ();
         mBmp.Clear (0);
         for (int i = 0; i < 100000; i++) {
            int x0 = R.Next (mDX), y0 = R.Next (mDY),
                x1 = R.Next (mDX), y1 = R.Next (mDY),
                color = R.Next (256);
            mBmp.DrawLine (x0, y0, x1, y1, color);
         }
         mBmp.End ();
      }
   }
   Random R = new ();

   void NextFrame2 (object sender, EventArgs e) {
      using (new BlockTimer ("Lines")) {
         mBmp.Begin ();
         mBmp.Clear (0);
         var polyFill = new PolyFill ();
         foreach (var line in inputs)
            polyFill.AddLine (line[0], line[1], line[2], line[3]);
         polyFill.Fill (mBmp);
         mBmp.End ();
      }
   }
   List<int[]> inputs = new ();
}

class BlockTimer : IDisposable {
   public BlockTimer (string message) {
      mStart = DateTime.Now;
      mMessage = message;
   }
   readonly DateTime mStart;
   readonly string mMessage;

   public void Dispose () {
      int elapsed = (int)((DateTime.Now - mStart).TotalMilliseconds + 0.5);
      Console.WriteLine ($"{mMessage}: {elapsed}ms");
   }
}