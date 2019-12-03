using Project3;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
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
    }
}
