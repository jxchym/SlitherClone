using System;
using System.Collections.Generic;
using System.Drawing;

namespace SliterIOSnake
{
    internal class Snake
    {
        public PointF Head { get; set; }
        public List<PointF> Body { get; private set; }
        public List<float> SegmentSizes { get; private set; }
        public float Speed { get; set; } = 3;

        private float headAngle;
        private List<float> bodyAngles; 

        public Snake(int startX, int startY)
        {
            Head = new PointF(startX, startY);
            Body = new List<PointF>();
            SegmentSizes = new List<float>();
            bodyAngles = new List<float>();
        }

        public void Draw(Graphics g)
        {
             
            

             
            for (int i = 0; i < Body.Count; i++)
            {
                 
                if (i < bodyAngles.Count && i < SegmentSizes.Count)
                {
                    DrawRotatedRectangle(g, Brushes.Green, Body[i], SegmentSizes[i], SegmentSizes[i], bodyAngles[i]);
                }
            }
            DrawRotatedRectangle(g, Brushes.Red, Head, 20, 15, headAngle);
        }

        private void DrawRotatedRectangle(Graphics g, Brush brush, PointF position, float width, float height, float angle)
        {
            using var matrix = g.Transform.Clone();
            g.TranslateTransform(position.X, position.Y);
            g.RotateTransform(angle);
            g.FillRectangle(brush, -width / 2, -height / 2, width, height);
            g.Transform = matrix;
        }

        internal void Move(Point position, int width, int height)
        {
            #region MoveHead
            var dx = position.X - Head.X;
            var dy = position.Y - Head.Y;

            var angle = MathF.Atan2(dy, dx) * (180 / MathF.PI);
            headAngle = angle;

            var distance = MathF.Sqrt(dx * dx + dy * dy);
            var length = MathF.Min(Speed, distance);

            var x = MathF.Cos(angle * (MathF.PI / 180)) * length;
            var y = MathF.Sin(angle * (MathF.PI / 180)) * length;

            if (!(Head.X + x < 10 || Head.X + x > width - 10 || Head.Y + y < 10 || Head.Y + y > height - 10))
            {
                Head = new PointF(Head.X + x, Head.Y + y);
            }
            #endregion

            #region MoveBody
            for (int i = 0; i < Body.Count; i++)
            {
                PointF target;
                if (i == 0)
                {
                    target = Head;
                }
                else
                {
                    target = Body[i - 1];
                }

                dx = target.X - Body[i].X;
                dy = target.Y - Body[i].Y;

                angle = MathF.Atan2(dy, dx) * (180 / MathF.PI);

                if (i < bodyAngles.Count)
                {
                    bodyAngles[i] = angle;
                }
                else
                {
                    bodyAngles.Add(angle);
                }

                distance = MathF.Sqrt(dx * dx + dy * dy);
                if (distance > 10)
                {
                    length = MathF.Min(Speed, distance);

                    x = MathF.Cos(angle * (MathF.PI / 180)) * length;
                    y = MathF.Sin(angle * (MathF.PI / 180)) * length;

                    Body[i] = new PointF(Body[i].X + x, Body[i].Y + y);
                }
            }
            #endregion

            if (SegmentSizes.Count > 0)
            {
                SegmentSizes[^1] -= 0.1f;
                if (SegmentSizes[^1] <= 0)
                {
                    Body.RemoveAt(Body.Count - 1);
                    SegmentSizes.RemoveAt(SegmentSizes.Count - 1);
                    bodyAngles.RemoveAt(bodyAngles.Count - 1);
                }
            }

            for (int i = 0; i < SegmentSizes.Count - 1; i++)
            {
                if (SegmentSizes[i] < 10)
                {
                    SegmentSizes[i] = 10;
                }
            }

            SyncBodyAndAngles();
        }

        private void SyncBodyAndAngles()
        {
            while (bodyAngles.Count < Body.Count)
            {
                bodyAngles.Add(headAngle);
            }

            while (bodyAngles.Count > Body.Count)
            {
                bodyAngles.RemoveAt(bodyAngles.Count - 1);
            }

            
            while (SegmentSizes.Count < Body.Count)
            {
                SegmentSizes.Add(10);
            }

            while (SegmentSizes.Count > Body.Count)
            {
                SegmentSizes.RemoveAt(SegmentSizes.Count - 1);
            }
        }

        public void AddSegment()
        {
             
            if (Body.Count == 0)
            {
                Body.Add(new PointF(Head.X, Head.Y));
                bodyAngles.Add(headAngle);
                SegmentSizes.Add(10);
            }
            else
            {
                var lastSegment = Body[^1];
                Body.Add(new PointF(lastSegment.X, lastSegment.Y));
                bodyAngles.Add(bodyAngles[^1]);
                SegmentSizes.Add(10);
            }
        }
    }
}


