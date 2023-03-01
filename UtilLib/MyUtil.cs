using System;

namespace UtilLibrary
{
    static class MyUtil
    {
        public static Font getFont(float fontSize, FontStyle style)
        {
            Font font = new Font("font", fontSize, style);
            return font;
        }

        public static long checkRange(long start, long now, long end)
        {
            return Math.Min(end, Math.Max(now, start));
        }

        public static bool isRange(long start, long now, long end)
        {
            return now == MyUtil.checkRange(start, now, end);
        }

        public static double checkRange(double start, double now, double end)
        {
            return Math.Min(end, Math.Max(now, start));
        }

        public static bool isRange(double start, double now, double end)
        {
            return now == MyUtil.checkRange(start, now, end);
        }
    }

    public class QuickDataGridView :DataGridView
    {
        public QuickDataGridView()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            DoubleBuffered = true;
        }
    }

    class QuickListView :ListView
    {
        public QuickListView()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            DoubleBuffered = true;
        }
    }

    class QuickChart :Chart
    {
        public QuickChart()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            DoubleBuffered = true;
        }
    }
}