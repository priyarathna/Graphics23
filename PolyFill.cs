namespace GrayBMP {
   class PolyFill {
      public void AddLine (int x0, int y0, int x1, int y1) => mLines.Add (new (x0, y0, x1, y1));

      public void Fill (GrayBMP bmp) {
         var width = bmp.Width - 1;
         for (int i = 0; i < width; i++) {
            var pts = new List<(int x, int y)> ();
            foreach (var (x0, y0, x1, y1) in mLines) {
               if (IsIntersect (x0, y0, x1, y1, 1, i + 0.5, width, out double x, out double y))
                  pts.Add (new ((int)x, (int)y));
            }
            pts = pts.OrderBy (x => x.x).ToList ();
            for (int j = 0; j < pts.Count; j += 2)
               bmp.DrawHorizontalLine (pts[j].x, pts[j + 1].x, pts[j].y);
         }
      }

      bool IsIntersect (int x0, int y0, int x1, int y1, int x2, double y2, int x3, out double x4, out double y4) {
         x4 = y4 = 0;
         double a1 = y0 - y1, b1 = x1 - x0; if (a1 == 0 && b1 == 0) return false;
         double c1 = x0 * (y1 - y0) + y0 * (x0 - x1);
         double b2 = x3 - x2;
         double c2 = y2 * (x2 - x3);
         double factor = a1 * b2; if (factor == 0) return false;
         x4 = (b1 * c2 - b2 * c1) / factor; y4 = (- c2 * a1) / factor;
         var lie = LieOn (x0, y0, x1, y1, x4, y4);
         if (lie >= 0 && lie <= 1) return true;
         return false;
      }

      double LieOn (int x0, int y0, int x1, int y1, double x4, double y4) {
         double dx = x1 - x0, dy = y1 - y0;
         if (Math.Abs (dx) > Math.Abs (dy)) return (x4 - x0) / dx;
         return (y4 - y0) / dy;
      }

      List<(int x0, int y0, int x1, int y1)> mLines = new  ();
   }
}
