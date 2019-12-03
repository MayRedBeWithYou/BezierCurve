using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Project3
{
    public enum States
    {
        None,
        Playing,
        Stopped
    }

    public enum Rotations
    {
        None,
        Naive,
        Filtered
    }

    public enum Animations
    {
        None,
        Rotation,
        MovingOnCurve
    }

    public partial class Form1 : Form
    {
        private Bitmap image;

        private Bitmap thumbnail;

        private Bitmap grayscaleImage;

        private Bitmap grayscaleThumbnail;

        private bool IsGrayscale = false;

        private int currentPoint;

        private Timer timer;

        private States State { get; set; }

        private Animations Animation { get; set; } = Animations.MovingOnCurve;

        private Rotations Rotation { get; set; } = Rotations.Naive;

        private bool IsPolylineVisible { get; set; } = true;

        private int PointCount => ControlPoints.Count;

        private Vertex Start { get; set; } = new Vertex();

        private Vertex End { get; set; } = new Vertex();

        private Vertex SelectedVertex { get; set; }

        private List<Vertex> ControlPoints => Polyline.ControlPoints;

        private Point MousePos { get; set; }

        private Polyline Polyline { get; } = new Polyline();

        private List<Point> Bezier { get; set; } = new List<Point>();

        private List<Vector2> Tangents { get; set; } = new List<Vector2>();

        private int deg = 0;
        private double Degree => deg * Math.PI / 180;

        private double Tan => Math.Atan2(Tangents[currentPoint].Y, Tangents[currentPoint].X);

        public Form1()
        {
            InitializeComponent();
            Bitmap bitmap = new Bitmap(BitmapCanvas.Width, BitmapCanvas.Height);
            BitmapCanvas.Image = bitmap;
            image = new Bitmap(Project3.Properties.Resources.check);
            thumbnail = new Bitmap(image, new Size(50, 50));
            grayscaleImage = new Bitmap(image);
            for (int x = 0; x < grayscaleImage.Width; x++)
            {
                for (int y = 0; y < grayscaleImage.Height; y++)
                {
                    Color color = grayscaleImage.GetPixel(x, y);
                    int grayScale = (int)((color.R * 0.3) + (color.G * 0.59) + (color.B * 0.11));
                    Color newColor = Color.FromArgb(color.A, grayScale, grayScale, grayScale);
                    grayscaleImage.SetPixel(x, y, newColor);
                }
            }
            grayscaleThumbnail = new Bitmap(grayscaleImage, new Size(50, 50));

            ImageBox.Image = thumbnail;
            ImageBox.Refresh();

            Start.Position = new Vector2(BitmapCanvas.Width / 5, BitmapCanvas.Height / 2);
            Start.Size = 11;
            End.Size = 11;
            End.Position = new Vector2(4 * BitmapCanvas.Width / 5, BitmapCanvas.Height / 2 + 1);
            Start.Color = Color.DarkRed;
            End.Color = Color.DarkRed;

            ControlPoints.Add(Start);
            ControlPoints.Add(End);


            timer = new Timer();
            timer.Interval = 16;
            timer.Tick += Timer_Tick;
            GeneratePoints(null, null);
            ControlPoints[1].Position = new Vector2(ControlPoints[1].Position.X, ControlPoints[1].Position.Y - BitmapCanvas.Height / 3);
            ControlPoints[3].Position = new Vector2(ControlPoints[3].Position.X, ControlPoints[3].Position.Y + BitmapCanvas.Height / 3);
            CalculateBezier();
            RefreshCanvas();
        }

        private void RefreshCanvas()
        {
            Bitmap bitmap = new Bitmap(BitmapCanvas.Width, BitmapCanvas.Height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                if (IsPolylineVisible)
                {

                    Pen cyan = new Pen(Color.Cyan);
                    for (int i = 0; i < PointCount - 1; i++)
                    {
                        g.DrawLine(cyan, ControlPoints[i].Point, ControlPoints[i + 1].Point);
                    }
                }

                foreach (Point p in Bezier)
                {
                    if (p.X < bitmap.Width && p.Y < bitmap.Height && p.X > 0 && p.Y > 0)
                        bitmap.SetPixel(p.X, p.Y, Color.Black);
                }

                foreach (Vertex v in ControlPoints)
                {
                    SolidBrush b = new SolidBrush(v.Color);
                    g.FillEllipse(b, v.X - v.Size / 2, v.Y - v.Size / 2, v.Size, v.Size);
                }

                if (State == States.Playing)
                {
                    if (Animation == Animations.MovingOnCurve)
                    {

                        if (Rotation == Rotations.Naive)
                        {
                            Bitmap rotated = IsGrayscale ? Algorithms.Rotate(Tan, grayscaleImage) : Algorithms.Rotate(Tan, image);
                            Point p = new Point(Bezier[currentPoint].X - rotated.Width / 2, Bezier[currentPoint].Y - rotated.Height / 2);
                            g.DrawImage(rotated, p);
                        }
                        else
                        {
                            Point p = new Point(Bezier[currentPoint].X - image.Width / 2, Bezier[currentPoint].Y - image.Height / 2);
                            if (IsGrayscale)
                            {
                                g.DrawImage(grayscaleImage, p);
                            }
                            else
                            {
                                g.DrawImage(image, p);
                            }
                        }
                    }
                    else if (Animation == Animations.Rotation)
                    {
                        if (Rotation == Rotations.Naive)
                        {
                            Bitmap rotated = IsGrayscale ? Algorithms.Rotate(Degree, grayscaleImage) : Algorithms.Rotate(Degree, image);
                            Point p = new Point(Bezier[currentPoint].X - rotated.Width / 2, Bezier[currentPoint].Y - rotated.Height / 2);
                            g.DrawImage(rotated, p);
                        }
                    }
                }
                Bitmap old = (Bitmap)BitmapCanvas.Image;
                BitmapCanvas.Image = bitmap;
                old.Dispose();
                BitmapCanvas.Refresh();
            }
        }

        private void GeneratePoints(object sender, EventArgs e)
        {
            int count = (int)PointCounter.Value;
            float hx = (End.X - Start.X) / (float)(count - 1);
            float hy = (End.Y - Start.Y) / (float)(count - 1);
            float X = Start.X;
            float Y = Start.Y;
            ControlPoints.Clear();
            ControlPoints.Add(Start);
            for (int i = 1; i < count - 1; i++)
            {
                X += hx;
                Y += hy;
                ControlPoints.Add(new Vertex((int)X, (int)Y));
            }
            ControlPoints.Add(End);
            CalculateBezier();
            RefreshCanvas();
        }

        private void BitmapCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            Point pos = new Point(e.Location.X, e.Location.Y);
            Bitmap bitmap = (Bitmap)BitmapCanvas.Image;

            foreach (Vertex v in ControlPoints)
            {
                if (Algorithms.Distance(v, pos) <= v.Size / 2)
                {
                    SelectedVertex = v;
                    MousePos = pos;
                    return;
                }
            }
        }

        private void BitmapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && SelectedVertex != null)
            {
                int diffX = MousePos.X - e.X;
                int diffY = MousePos.Y - e.Y;
                SelectedVertex.Position = new Vector2(SelectedVertex.Position.X - diffX, SelectedVertex.Position.Y - diffY);
                MousePos = e.Location;
                CalculateBezier();
                RefreshCanvas();
            }
        }

        private void CalculateBezier()
        {
            Bezier.Clear();
            Tangents.Clear();
            GC.Collect();
            List<Point> points = new List<Point>();

            for (double i = 0d; i < 5000; i++)
            {
                double t = i / 5000d;
                double X = 0;
                double Y = 0;
                for (int c = 0; c < PointCount; c++)
                {
                    X += Algorithms.Binom(PointCount - 1, c) * Math.Pow(1d - t, PointCount - 1 - c) * Math.Pow(t, c) * (double)ControlPoints[c].X;
                    Y += Algorithms.Binom(PointCount - 1, c) * Math.Pow(1d - t, PointCount - 1 - c) * Math.Pow(t, c) * (double)ControlPoints[c].Y;
                }
                if (points.Count == 0 || points[points.Count - 1].X != (int)X || points[points.Count - 1].Y != (int)Y)
                {
                    points.Add(new Point((int)X, (int)Y));
                    X = 0;
                    Y = 0;
                    for (int c = 0; c < PointCount - 1; c++)
                    {
                        X += Algorithms.Binom(PointCount - 2, c) * Math.Pow(1d - t, PointCount - 2 - c) * Math.Pow(t, c) * (double)(ControlPoints[c + 1].X - ControlPoints[c].X) * (PointCount - 1);
                        Y += Algorithms.Binom(PointCount - 2, c) * Math.Pow(1d - t, PointCount - 2 - c) * Math.Pow(t, c) * (double)(ControlPoints[c + 1].Y - ControlPoints[c].Y) * (PointCount - 1);
                    }
                    Tangents.Add(new Vector2((float)X, (float)Y));
                }
            }
            Bezier = points.ToList();
            currentPoint = Math.Min(Bezier.Count - 1, currentPoint);
        }

        private void BitmapCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            SelectedVertex = null;
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            RefreshCanvas();
        }

        private void IsPolylineVisibleBox_CheckedChanged(object sender, EventArgs e)
        {
            IsPolylineVisible = isPolylineVisibleBox.Checked;
            RefreshCanvas();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "XML file | *.xml";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var serializer = new XmlSerializer(typeof(List<Point>));
                using (var stream = File.Create(dialog.FileName))
                {
                    List<Point> points = new List<Point>();
                    foreach (Vertex v in ControlPoints)
                    {
                        points.Add(v.Point);
                    }
                    serializer.Serialize(stream, points);
                }
            }
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "XML file | *.xml";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var serializer = new XmlSerializer(typeof(List<Point>));
                using (var stream = File.OpenRead(dialog.FileName))
                {
                    var points = (List<Point>)(serializer.Deserialize(stream));
                    ControlPoints.Clear();
                    foreach (Point p in points)
                    {
                        ControlPoints.Add(new Vertex(p.X, p.Y));
                    }

                    Start = ControlPoints[0];
                    End = ControlPoints[ControlPoints.Count - 1];
                    Start.Size = 11;
                    End.Size = 11;
                    Start.Color = Color.DarkRed;
                    End.Color = Color.DarkRed;
                    RefreshCanvas();
                }
            }
        }

        private void LoadImageButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "image files (*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.jpeg;*.png";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
                grayscaleImage = new Bitmap(dialog.FileName);
                thumbnail = new Bitmap(image, new Size(50, 50));
                for (int x = 0; x < grayscaleImage.Width; x++)
                {
                    for (int y = 0; y < grayscaleImage.Height; y++)
                    {
                        Color color = grayscaleImage.GetPixel(x, y);
                        int grayScale = (int)((color.R * 0.3) + (color.G * 0.59) + (color.B * 0.11));
                        Color newColor = Color.FromArgb(color.A, grayScale, grayScale, grayScale);
                        grayscaleImage.SetPixel(x, y, newColor);
                    }
                }

                grayscaleThumbnail = new Bitmap(grayscaleImage, new Size(50, 50));
                IsGrayscaleBox_CheckedChanged(null, null);
            }
        }

        private void IsGrayscaleBox_CheckedChanged(object sender, EventArgs e)
        {
            IsGrayscale = IsGrayscaleBox.Checked;
            if (IsGrayscaleBox.Checked)
            {
                ImageBox.Image = grayscaleThumbnail;
            }
            else
            {
                ImageBox.Image = thumbnail;
            }
            ImageBox.Refresh();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            StartButton.Enabled = false;
            StopButton.Enabled = true;
            State = States.Playing;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Animation == Animations.MovingOnCurve)
            {
                currentPoint++;
                if (currentPoint >= Bezier.Count)
                {
                    currentPoint = 0;
                    GC.Collect();
                }
            }
            else if (Animation == Animations.Rotation)
            {
                deg = deg + 1;
                if (deg == 360) deg = 0;
            }
            RefreshCanvas();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            StartButton.Enabled = true;
            StopButton.Enabled = false;
            State = States.Stopped;
            timer.Stop();
            RefreshCanvas();
        }

        private void NaiveRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (NaiveRadioButton.Checked)
            {
                Rotation = Rotations.Naive;
            }
            else
            {
                Rotation = Rotations.Filtered;
            }
        }

        private void RotationRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (RotationRadioButton.Checked)
            {
                Animation = Animations.Rotation;
            }
            else
            {
                Animation = Animations.MovingOnCurve;
            }
        }
    }
}
