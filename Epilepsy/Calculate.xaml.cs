using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Epilepsy
{
    /// <summary>
    /// Calculate.xaml 的交互逻辑
    /// </summary>
    public partial class Calculate : UserControl
    {
        private ObservableDataSource<Point> dataSource = new ObservableDataSource<Point>();
        private ObservableDataSource<Point> dataSource1 = new ObservableDataSource<Point>();
        private ObservableDataSource<Point> dataSource2 = new ObservableDataSource<Point>();
        private ObservableDataSource<Point> dataSource3 = new ObservableDataSource<Point>();
        private LineGraph graph = new LineGraph();
        private LineAndMarker<MarkerPointsGraph> graph1 = new LineAndMarker<MarkerPointsGraph>();
        private LineAndMarker<MarkerPointsGraph> graph2 = new LineAndMarker<MarkerPointsGraph>();
        private LineAndMarker<MarkerPointsGraph> graph3 = new LineAndMarker<MarkerPointsGraph>();
        private DispatcherTimer timer = new DispatcherTimer();
        public double ArgShort, ArgLong, ArgDetect;
        private double[,] data;
        double[][] y;
        int sfreq = 250;   //采样频率
        int i = 0, detecflag = 0;   //detecflag  1:线长检测 2：面积检测 3：带通检测
        int row;

        int shortNum, longNum, start;        //线长与面积的输入参数
        double tmin, tmax, minAmpPer, durBp, nerror = 2; //带通的参数
        private int initCount;

        double xaxis = 0;      //窗口显示的x起始坐标
        //double ymax;
        double yaxis=120;            //窗口显示的y起始坐标
        double group = 5;//默认组距 


        public static int aaa = 222;

        public static int flag = 0;


        public Calculate()
        {
            InitializeComponent();

            initCount = plotter.Children.Count;
        }

        private double[,] LoadData()                       //加载数据
        {            
            //选择的EEG记录编号
            ComboBoxItem item = Select_EEG_Record.SelectedItem as ComboBoxItem;
            string str = item.Content.ToString();

            //读入记录
            string datafile = str + ".mat";                   
            string path = "../../../EEG/";
            Matrix<double> filt_data = MathNet.Numerics.Data.Matlab.MatlabReader.Read<double>(path + datafile, "record");
            y = filt_data.ToRowArrays();  //将每行生成一个数组

            //默认以EEG的第6通道作为测试数据
            row = y[5].Length;
            double[,] p1 = new double[row, 2]; //数组
            for (int i = 0; i < row; i++)
            {
                p1[i, 0] = (double)i / sfreq;
                p1[i, 1] = y[5][i];
            }
            
            return p1;
        }

        private void AnimatedPlot(object sender, EventArgs e)
        {
            
            Point point = new Point();
            point = new Point(data[i, 0], data[i, 1]);
            dataSource.AppendAsync(base.Dispatcher, point);


            if (i> longNum && i < row)
            {
                switch(detecflag)
                {
                    case 0:
                        break;
                    case 1:
                        LineDetection();
                        break;
                    case 2:
                        AreaDetection();
                        break;
                    case 3:
                        BandPassDetection();
                        break;
                }
                
            }            
            i++;


            if (i == row)
            {               
                timer.IsEnabled = false;
            }

            if (data[i, 0] - group > 0)
            {
                xaxis = data[i, 0] - group;
            }
            else
            {
                xaxis = 0;
            }

            plotter.Viewport.Visible = new System.Windows.Rect(xaxis, -yaxis, group, 2 * yaxis);



            int m = dataSource.Collection.Count;

            if (m / 2000 > 0)                                       //保持dataSource中仅有2000个点
            {

                for (int ii = (m - 2000); ii >= 0; ii--)
                {
                    dataSource.Collection.RemoveAt(ii);
                }
            }


            int m3 = dataSource3.Collection.Count;

            if (m3 / 200 > 0)                                       //保持dataSource中仅有200个点
            {

                for (int ii = (m3 - 2000); ii >= 0; ii--)
                {
                    dataSource3.Collection.RemoveAt(ii);
                }
            }



        }


        private void BtnLineLengthStart_Click(object sender, RoutedEventArgs e)            //线长检测开始
        {
            plotter.Children.Remove(graph);
            data = LoadData();
            ArgShort = Convert.ToDouble(textBoxShort.Text);     //短趋势时间参数
            ArgLong = Convert.ToDouble(textBoxLong.Text);       //长趋势时间参数
            ArgDetect = Convert.ToDouble(textBoxDetect.Text);    //阈值

            double inter = data[1, 0] - data[0, 0];
            shortNum = (int)(ArgShort / inter);
            longNum = (int)(ArgLong / inter);
            start = longNum;

            detecflag = 1;                                    //检测方式标志置1

            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += new EventHandler(AnimatedPlot);    //连续执行AnimatedPlot 事件实时绘制新坐标点
            timer.IsEnabled = true;
            timer.Start();

            graph = plotter.AddLineGraph(dataSource, Colors.Green, 2, "data curves");           
            graph1 = plotter.AddLineGraph(dataSource1,
              new Pen(Brushes.Red, 0),
              new CirclePointMarker { Size = 3.0, Fill = Brushes.Red },
              new PenDescription("LineLength"));          
            plotter.Legend.Remove();
            plotter.Viewport.FitToView();
            
        }

        

        private void BtnAreaStart_Click(object sender, RoutedEventArgs e)               //面积检测开始
        {
            plotter.Children.Remove(graph);
            data = LoadData();
            ArgShort = Convert.ToDouble(textBoxShortA.Text);     //短趋势时间参数
            ArgLong = Convert.ToDouble(textBoxLongA.Text);       //长趋势时间参数
            ArgDetect = Convert.ToDouble(textBoxDetectA.Text);    //阈值

            double inter = data[1, 0] - data[0, 0];
            shortNum = (int)(ArgShort / inter);
            longNum = (int)(ArgLong / inter);
            start = longNum;

            detecflag = 2;                                    //检测方式标志置2

            timer.Interval = TimeSpan.FromMilliseconds(1);

            timer.Tick += new EventHandler(AnimatedPlot);    //连续执行AnimatedPlot 事件实时绘制新坐标点
            graph = plotter.AddLineGraph(dataSource, Colors.Green, 2, "data curves");



            graph2 = plotter.AddLineGraph(dataSource2,
              new Pen(Brushes.Blue, 0),
              new CirclePointMarker { Size = 3.0, Fill = Brushes.Blue },
              new PenDescription("Area"));

            timer.IsEnabled = true;

            plotter.Legend.Remove();

            plotter.Viewport.FitToView();
            timer.Start();
        }

        private void BtnBandPassStart_Click(object sender, RoutedEventArgs e)       //带通检测开始
        {
            plotter.Children.Remove(graph);
            data = LoadData();
            //读取文本框参数
            
            tmin = 1 / Convert.ToDouble(textBoxMaxFre.Text);  //半波的最小周期，对应最大频率Maximum Frequency
            tmax = 1 / Convert.ToDouble(textBoxMinFre.Text);  //半波的最大周期，对应最小频率Minimum Frequency
            minAmpPer = Convert.ToDouble(textBoxMinAmp.Text) / 100;  //最小的有效幅值百分比，对应Minimum Amplitude(% of full scale)
            durBp = Convert.ToDouble(textMiniDurBp.Text);     //计算带通的时长，对应Minimum Duration

            double inter = data[1, 0] - data[0, 0];
            longNum = (int)(durBp / inter);
            shortNum = longNum / 2;

            detecflag = 3;                                    //检测方式标志置3

            timer.Interval = TimeSpan.FromMilliseconds(1);

            timer.Tick += new EventHandler(AnimatedPlot);    //连续执行AnimatedPlot 事件实时绘制新坐标点
            graph = plotter.AddLineGraph(dataSource, Colors.Green, 2, "data curves");


            graph3 = plotter.AddLineGraph(dataSource3,
              new Pen(Brushes.Yellow, 0),
              new CirclePointMarker { Size = 3.0, Fill = Brushes.Yellow },
              new PenDescription("BandPass"));

            timer.IsEnabled = true;

            plotter.Legend.Remove();

            plotter.Viewport.FitToView();
            timer.Start();

        }





        private void LineDetection()
        {            
            double shortSum = 0, longSum = 0, shortAve,longAve;
            for (int j = i; j > (i - shortNum + 1); j--)
            {
                shortSum += System.Math.Abs(data[j, 1] - data[j - 1, 1]);
            }
            for (int j = i; j > (i - longNum + 1); j--)
            {
                longSum += System.Math.Abs(data[j, 1] - data[j - 1, 1]);
            }
            shortAve = shortSum / shortNum;
            longAve = longSum / longNum;

            if (Math.Abs((shortAve - longAve) / longAve) > ArgDetect)
            {
                Point point = new Point();
                point = new Point(data[i, 0], data[i, 1]);
                dataSource1.AppendAsync(base.Dispatcher, point);
                flag = 1;
            }
        }

        

        private void AreaDetection()
        {

            double shortSum = 0, longSum = 0, shortAve, longAve;
            for (int j = i; j > (i - shortNum + 1); j--)
            {
                double slope = (data[j, 1] - data[j - 1, 1]) / (data[j, 0] - data[j - 1, 0]);
                double intercept = data[j, 1] - slope * data[j, 0];
                var result1 = Integrate.OnClosedInterval(x => slope * x + intercept, data[j - 1, 0], (data[j, 0]));

                shortSum += Math.Abs(result1);
            }
            for (int j = i; j > (i - longNum + 1); j--)
            {
                double slope = (data[j, 1] - data[j - 1, 1]) / (data[j, 0] - data[j - 1, 0]);
                double intercept = data[j, 1] - slope * data[j, 0];
                var result2 = Integrate.OnClosedInterval(x => slope * x + intercept, data[j - 1, 0], (data[j, 0]));

                longSum += Math.Abs(result2);
            }
            shortAve = shortSum / shortNum;
            longAve = longSum / longNum;

            if (Math.Abs((shortAve - longAve) / longAve) > ArgDetect)
            {
                Point point = new Point();
                point = new Point(data[i, 0], data[i, 1]);
                dataSource2.AppendAsync(base.Dispatcher, point);
                flag = 2;
            }
        }

        private void BandPassDetection()
        {
            double max = 0, min = 0, Amp;   //求现有数据的最大幅值
            int Count = 0, Count2 = 0;
            for (int j = i; j > (i - longNum + 1); j--)
            {
                if (data[j, 1] > max)
                {
                    max = data[j, 1];
                }
                if (data[j, 1] < min)
                {
                    min = data[j, 1];
                }
            }
            Amp = max - min;
            double minAmp = minAmpPer * Amp;  //最小的有效幅值

            ArrayList pole = new ArrayList();   //保存极点位置是第几个点
            for (int j = i; j > (i - longNum + 1); j--)    //1 求极点
            {
                double a1 = data[j, 1] - data[j - 1, 1];
                double a2 = data[j - 1, 1] - data[j-2, 1];

                if (a1 * a2 < 0 && Math.Abs(a1) > minAmp && Math.Abs(a2) > minAmp)
                {
                    pole.Add(j-1);
                }
            }

            int num = pole.Count;

            for (int i = 1; i < num; i++)                        //求半波的时间，判断超出频率范围的异常点
            {
                double tHaldWave = Math.Abs(data[(int)pole[i], 0] - data[(int)pole[i - 1], 0]);

                if (tHaldWave > tmin && tHaldWave < tmax)
                {
                    Count++;
                }
            }
            //短时间周期内异常点数 

            ArrayList pole2 = new ArrayList();   //保存极点位置是第几个点
            for (int j = i; j > (i - shortNum + 1); j--)    //1 求极点
            {
                double a1 = data[j, 1] - data[j - 1, 1];
                double a2 = data[j - 1, 1] - data[j - 2, 1];

                if (a1 * a2 < 0 && Math.Abs(a1) > minAmp && Math.Abs(a2) > minAmp)
                {
                    pole2.Add(j - 1);
                }
            }
            int num2 = pole2.Count;
            for (int i = 1; i < num2; i++)                
            {
                double tHaldWave = Math.Abs(data[(int)pole2[i], 0] - data[(int)pole2[i - 1], 0]);

                if (tHaldWave > tmin && tHaldWave < tmax)
                {
                    Count2++;
                }
            }




            //double bpe = Math.Abs((double)(Count-2*Count2)/ Count);

            double bpe2 = Math.Abs((double)(num - 2 * num2) / (num-num2));

            if (num !=0)
            {
                if (bpe2 > nerror)                          //异常点的占比超过nerror
                {
                    Point point = new Point();
                    point = new Point(data[i, 0], data[i, 1]);
                    dataSource3.AppendAsync(base.Dispatcher, point);
                    flag = 3;
                }
            }
            
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();

        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            int k;

            plotter.Children.Remove(graph);

            int count = plotter.Children.Count;

            for (k = count - 1; k >= initCount; k--)
            {
                plotter.Children.RemoveAt(k);
            }

            dataSource = new ObservableDataSource<Point>();
            dataSource1 = new ObservableDataSource<Point>();
            dataSource2 = new ObservableDataSource<Point>();
            dataSource3 = new ObservableDataSource<Point>();
            i = 0;
            plotter.Viewport.Visible = new System.Windows.Rect(xaxis, -yaxis, group, 2 * yaxis);
        }





    }
}
