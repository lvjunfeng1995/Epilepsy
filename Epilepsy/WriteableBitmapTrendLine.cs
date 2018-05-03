using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GDI = System.Drawing;

namespace Epilepsy
{
    public class WriteableBitmapTrendLine : FrameworkElement
    {
        #region DependencyProperties

        public static readonly DependencyProperty LatestQuoteProperty =
            DependencyProperty.Register("LatestQuote", typeof(MinuteQuoteViewModel), typeof(WriteableBitmapTrendLine),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, OnLatestQuotePropertyChanged));

        private static void OnLatestQuotePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WriteableBitmapTrendLine trendLine = (WriteableBitmapTrendLine)d;
            MinuteQuoteViewModel latestQuote = (MinuteQuoteViewModel)e.NewValue;
            if (latestQuote != null)
            {
                trendLine.DrawTrendLine((float)latestQuote.LastPx);
            }            
        }

        public MinuteQuoteViewModel LatestQuote
        {
            get { return (MinuteQuoteViewModel)GetValue(LatestQuoteProperty); }
            set { SetValue(LatestQuoteProperty, value); }
        }

        #endregion

        private int width = 0;
        private int height = 0;

        //Y axis multiple
        private float rate = 1;

        private WriteableBitmap bitmap;

        /// <summary>
        /// 两点之间的距离
        /// </summary>
        private int dx = 1;

        /// <summary>
        /// 当前区域所容纳的值
        /// </summary>
       /* public static float[] prices;
        public static int divider = 500 ;
        public static float coordorigin = (float)110.7;*/

        private float[] prices;

        private float[] exchange;

        /// <summary>
        /// 在prices中的索引
        /// </summary>
        private int ordinal = -1;

        private GDI.Pen pen;

        private void DrawTrendLine(float latestPrice, Boolean reDraw = false)
        {
            if (double.IsNaN(latestPrice))
            {
                return;
            }

           // ordinal = 200;

            if ((ordinal < prices.Length ) && !reDraw)
            {
                ordinal++;
            }               

            if ((ordinal == prices.Length ) && !reDraw)
            {
                prices.CopyTo(exchange, 0);
                for (int i = 0; i < prices.Length - 1; i++)
                {
                    prices[i] = exchange[i+1];
                }
                prices[prices.Length - 1] = latestPrice;
            }

            if ((ordinal < prices.Length ) && !reDraw)
            {
                prices[ordinal] = latestPrice;
            }                


            this.bitmap.Lock();

            using (GDI.Bitmap backBufferBitmap = new GDI.Bitmap(width, height,
                this.bitmap.BackBufferStride, GDI.Imaging.PixelFormat.Format24bppRgb,
                this.bitmap.BackBuffer))
            {
                using (GDI.Graphics backBufferGraphics = GDI.Graphics.FromImage(backBufferBitmap))
                {
                    backBufferGraphics.SmoothingMode = GDI.Drawing2D.SmoothingMode.HighSpeed;
                    backBufferGraphics.CompositingQuality = GDI.Drawing2D.CompositingQuality.HighSpeed;

                    float time_width = width / 25;
                    float vlot_height = height / 10;

                    backBufferGraphics.Clear(GDI.Color.White);                    

                    pen = new GDI.Pen(GDI.Color.Gray);
                    for (int i = 0; i < 26; i++)
                    {
                        backBufferGraphics.DrawLine(pen,
                            new GDI.PointF(i * time_width, 0),
                            new GDI.PointF(i * time_width, height));
                    }

                    for (int i = 0; i < 11; i++)
                    {
                        backBufferGraphics.DrawLine(pen,
                            new GDI.PointF(0, vlot_height * i),
                            new GDI.PointF(width, vlot_height * i));
                    }

                    pen = new GDI.Pen(GDI.Color.Red);
                    for (int i = 0; i < ordinal; i++)
                    {
                        if (i > 0)
                        {
                            /*float uv1 = (float)(4.578 * prices[i - 1]);
                             float uv2 = (float)(4.578 * prices[i]);
                            
                             float eachvolt = coordorigin / divider;
                             float y1 = (int)(coordorigin - eachvolt * uv1);
                             float y2 = (int)(coordorigin - eachvolt * uv2);*/

                           // float y1 = (int)prices[i - 1];
                           // float y2 = (int)prices[i];


                              backBufferGraphics.DrawLine(pen,
                                new GDI.PointF(((prices.Length - ordinal) * dx + (i-1) * dx), TransformY(prices[i - 1])),
                                 new GDI.PointF(((prices.Length - ordinal) * dx + i * dx), TransformY(prices[i])));

                           /* backBufferGraphics.DrawLine(pen,
                                    new GDI.PointF(((prices.Length - ordinal) * dx + (i - 1) * dx), TransformY(y1)),
                                     new GDI.PointF(((prices.Length - ordinal) * dx + i * dx), TransformY(y2)));*/
                        }

                    }



                   // ordinal = -1;

                    backBufferGraphics.Flush();
                }
            }
            this.bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            this.bitmap.Unlock();
        }

        public void SetRate(float rate)
        {
            this.rate = rate;
            this.DrawTrendLine(0, true);
        }

        private float TransformY(float y)
        {
            float half_hieht = height / 2;
            float vlot_num = half_hieht - y;
            float y_num = 0;

            if (vlot_num < 0)
            {
                y_num = half_hieht + Math.Abs(vlot_num) * rate;
            }
            else
            {
                y_num = half_hieht - vlot_num * rate;
            }

            return y_num;
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (bitmap == null)
            {
                this.width = (int)RenderSize.Width;
                this.height = (int)RenderSize.Height;
                this.bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr24, null);

                this.bitmap.Lock();
                using (GDI.Bitmap backBufferBitmap = new GDI.Bitmap(width, height,
               this.bitmap.BackBufferStride, GDI.Imaging.PixelFormat.Format24bppRgb,
               this.bitmap.BackBuffer))
                {
                    using (GDI.Graphics backBufferGraphics = GDI.Graphics.FromImage(backBufferBitmap))
                    {
                        backBufferGraphics.SmoothingMode = GDI.Drawing2D.SmoothingMode.HighSpeed;
                        backBufferGraphics.CompositingQuality = GDI.Drawing2D.CompositingQuality.HighSpeed;

                        backBufferGraphics.Clear(GDI.Color.White);

                        float time_width = width / 25;
                        float vlot_height = height / 10;

                        pen = new GDI.Pen(GDI.Color.Gray);
                        for (int i = 0; i < 26; i++)
                        {
                            backBufferGraphics.DrawLine(pen,
                                new GDI.PointF(i * time_width, 0),
                                new GDI.PointF(i * time_width, height));
                        }

                        for (int i = 0; i < 11; i++)
                        {
                            backBufferGraphics.DrawLine(pen,
                                new GDI.PointF(0, vlot_height * i),
                                new GDI.PointF(width, vlot_height * i));
                        }

                        backBufferGraphics.Flush();
                    }
                }
                this.bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
                this.bitmap.Unlock();

                prices = new float[(int)(this.width / this.dx)];
                this.exchange = new float[(int)(this.width / this.dx)];
            }
            dc.DrawImage(bitmap, new Rect(0, 0, RenderSize.Width, RenderSize.Height));
            base.OnRender(dc);
        }
    }
}

