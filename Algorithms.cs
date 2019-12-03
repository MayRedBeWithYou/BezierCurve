using Project3;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;

namespace Project3
{
    public static class Algorithms
    {
        public static double Distance(Vertex p1, Point p2)
        {
            return Distance(p1.Point, p2);
        }

        public static double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow((p1.X - p2.X), 2) + Math.Pow((p1.Y - p2.Y), 2));
        }

        public static double Binom(int n, int k)
        {
            double result = 1d;
            for (int i = 1; i <= k; i++)
            {
                result *= n - (k - i);
                result /= i;

            }
            return result;
        }

        public static Bitmap Rotate(double deg, Bitmap bitmap)
        {
            double cos = Math.Cos(deg);
            double sin = Math.Sin(deg);
            double sq = Math.Sqrt(2);
            Bitmap newBitmap = new Bitmap((int)(sq * bitmap.Width), (int)(sq * bitmap.Height));
            for (int x = 0; x < newBitmap.Width; x++)
            {
                for (int y = 0; y < newBitmap.Height; y++)
                {
                    double sx = cos * (x - newBitmap.Width / 2) + sin * (y - newBitmap.Height / 2) + bitmap.Width / 2;
                    double sy = -sin * (x - newBitmap.Width / 2) + cos * (y - newBitmap.Height / 2) + bitmap.Height / 2;
                    if (sx >= 0 && sx < bitmap.Width && sy >= 0 && sy < bitmap.Height)
                        newBitmap.SetPixel(x, y, bitmap.GetPixel((int)sx, (int)sy));
                }
            }
            return newBitmap;
        }
    }
}
