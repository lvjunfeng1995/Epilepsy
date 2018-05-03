using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
using System.Xml;

namespace Epilepsy
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 


    using static Calculate;


    public partial class MainWindow : Window
    {
        private DispatcherTimer ShowTimer;
        // private int[,] Lbtn_num = new int[4,4];
        private Button[,] Lbtn_array = new Button[4, 4];
        // private int[,] Rbtn_num = new int[4, 4];
        private Button[,] Rbtn_array = new Button[4, 4];
        private Button[] btn_gain = new Button[4];
        private String[] channal = new String[4];
        private SerialPort ComDevice = new SerialPort();

        //define timer for send to serialport
        private Timer Eventtimer;
       
        //get therapy num
        private Timer Refreshtimer;

        private Timer DataRecTimer;

        private Timer DataSendTimer;

        private int therapy_num = -1;
        private List<CustomPoint> pointlist = new List<CustomPoint>();
        private List<Line> linelist = new List<Line>();
        private List<Point> list_point = new List<Point>();
        private const int HAVE_JOIN = 1;
        private const int NOT_JOIN = 0;
        Path myPath = new Path();
        private MinuteQuoteViewModel vm;
        private String FD_removed_str = "";
        private String SD_removed_str = "";
        private short[] Data_buf = new short[200];
        private int TimeFlag = 0;
        private int ErrNum = 0;
        private int ResendNum = 0;
        private Boolean isLive = false;
        private int KeepLive_Err_Ctr = 0;

        private Type.ConnectState CurState = Type.ConnectState.STATE_DISCONNECT;

        private byte[][] RequestData = new byte[Type.REQUEST_NUM][];
        private Type.Request PreReqIndex = Type.Request.REQ_CONNECT;
        private int[] SendTable = new int[Type.REQUEST_NUM];

        private static object obj = new object();

        public MainWindow()
        {
            InitializeComponent();
            getSystemTime();
            getLButtonArray();
            getRButtonArray();
            getComboboxData();
            getListData();
            setCmbEvent();
            setCmbData();
            setTimer();

            for (int i = 0; i < SendTable.Length; i++)
            {
                SendTable[i] = 0;
            }

            ComDevice.DataReceived += ComDevice_DataReceived;

            ShowTimer.Tick += new EventHandler(flag_detect);    //以ShowTimer这个时钟来监视检测状态flag，若有需要也可另起一个时钟

        }


        private void setTimer()
        {
            Eventtimer = new Timer();

            Eventtimer.Interval = 1000;
            Eventtimer.AutoReset = true;
            Eventtimer.Elapsed += Eventtimer_Elapsed;

            Eventtimer.Start();           

            Refreshtimer = new Timer();

            Refreshtimer.Interval = 20;
            Refreshtimer.AutoReset = true;
            Refreshtimer.Elapsed += Refreshtimer_Elapsed;

            DataRecTimer = new Timer();

            DataRecTimer.Interval = 50;
            DataRecTimer.AutoReset = false;
            DataRecTimer.Elapsed += DataRecTimer_Elapsed;

            DataSendTimer = new Timer();

            DataSendTimer.Interval = 300;
            DataSendTimer.AutoReset = false;
            DataSendTimer.Elapsed += DataSendTimer_Elapsed;
           // OpenSerialport();


        }

        private void DataSendTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ResendNum < 1)
            {
                ResendData(PreReqIndex);

                return;
            }

            isLive = false;

            ErrNum = 0;
            ResendNum = 0;

            KeepLive_Err_Ctr = 0;

            ReqState = Type.ReqState.SEND_REQ_CONNECT;

            CurState = Type.ConnectState.STATE_DISCONNECT;

            MessageBox.Show("断开连接!");
        }

        private void setCmbData() {
            for (int i = 0; i < 3; i++) {
                mode.Items.Add(i);
                //PA_frequency_min.Items.Add(i);
                //PA_amplitude.Items.Add(i);
                //PA_frequency_max.Items.Add(i);
                //PA_duration.Items.Add(i);
                //PA_trend_short.Items.Add(i);
                //PA_trend_long.Items.Add(i);
                //PA_threshold.Items.Add(i);
                PB_frequency_min.Items.Add(i);
                PB_amplitude.Items.Add(i);
                PB_frequency_max.Items.Add(i);
                PB_duration.Items.Add(i);
                PB_trend_short.Items.Add(i);
                PB_trend_long.Items.Add(i);
                PB_threshold.Items.Add(i);
                ecog_length.Items.Add(i);
                episode_length.Items.Add(i);
            }

            for (int i = 0; i < 24; i++) {
                start_time1.Items.Add(i + ":00");
                start_time2.Items.Add(i + ":00");
                start_time3.Items.Add(i + ":00");
                start_time4.Items.Add(i + ":00");
            }

            tpy1_bst1_cnt.Items.Add("1 mA");
            tpy1_bst1_fcy.Items.Add("200 Hz");
            tpy1_bst1_pse.Items.Add("5000 μs");
            tpy1_bst2_cnt.Items.Add("1 mA");
            tpy1_bst2_fcy.Items.Add("200 Hz");
            tpy1_bst2_pse.Items.Add("5000 μs");
            tpy2_bst1_cnt.Items.Add("1 mA");
            tpy2_bst1_fcy.Items.Add("200 Hz");
            tpy2_bst1_pse.Items.Add("5000 μs");
            tpy2_bst2_cnt.Items.Add("1 mA");
            tpy2_bst2_fcy.Items.Add("200 Hz");
            tpy2_bst2_pse.Items.Add("5000 μs");
            tpy3_bst1_cnt.Items.Add("1 mA");
            tpy3_bst1_fcy.Items.Add("200 Hz");
            tpy3_bst1_pse.Items.Add("5000 μs");
            tpy3_bst2_cnt.Items.Add("1 mA");
            tpy3_bst2_fcy.Items.Add("200 Hz");
            tpy3_bst2_pse.Items.Add("5000 μs");
            tpy4_bst1_cnt.Items.Add("1 mA");
            tpy4_bst1_fcy.Items.Add("200 Hz");
            tpy4_bst1_pse.Items.Add("5000 μs");
            tpy4_bst2_cnt.Items.Add("1 mA");
            tpy4_bst2_fcy.Items.Add("200 Hz");
            tpy4_bst2_pse.Items.Add("5000 μs");
            tpy5_bst1_cnt.Items.Add("1 mA");
            tpy5_bst1_fcy.Items.Add("200 Hz");
            tpy5_bst1_pse.Items.Add("5000 μs");
            tpy5_bst2_cnt.Items.Add("1 mA");
            tpy5_bst2_fcy.Items.Add("200 Hz");
            tpy5_bst2_pse.Items.Add("5000 μs");
            test_pse2.Items.Add("5000 μs");
            test_cnt1.Items.Add("500 μA");
            test_cnt1.Items.Add("1110 μA");
            test_cnt1.Items.Add("1590 μA");
            test_cnt1.Items.Add("2070 μA");
            test_cnt1.Items.Add("2550 μA");
            test_cnt2.Items.Add("1 mA");
            test_fcy1.Items.Add("500 Hz");
            test_fcy1.Items.Add("1000 Hz");
           // test_fcy1.Items.Add("2000 Hz");
           // test_fcy1.Items.Add("3000 Hz");
          //  test_fcy1.Items.Add("4000 Hz");
            test_fcy2.Items.Add("200 Hz");
            test_duration1.Items.Add("100 ms");
            test_duration1.Items.Add("500 ms");
            test_duration1.Items.Add("1000 ms");
            test_duration1.Items.Add("1500 ms");
            test_duration1.Items.Add("2000 ms");

            for (int i = 1; i < 6; i++)
            {
                tpy1_bst1_duration.Items.Add(i * 100 + " ms");
                tpy1_bst2_duration.Items.Add(i * 100 + " ms");
                tpy2_bst1_duration.Items.Add(i * 100 + " ms");
                tpy2_bst2_duration.Items.Add(i * 100 + " ms");
                tpy3_bst1_duration.Items.Add(i * 100 + " ms");
                tpy3_bst2_duration.Items.Add(i * 100 + " ms");
                tpy4_bst1_duration.Items.Add(i * 100 + " ms");
                tpy4_bst2_duration.Items.Add(i * 100 + " ms");
                tpy5_bst1_duration.Items.Add(i * 100 + " ms");
                tpy5_bst2_duration.Items.Add(i * 100 + " ms");
                test_duration2.Items.Add(i * 100 + " ms");
            }
        }

        private void setCmbEvent() {
            cmb1.SelectionChanged += cmb_SelectionChanged;
            cmb2.SelectionChanged += cmb_SelectionChanged;
            cmb3.SelectionChanged += cmb_SelectionChanged;
            cmb4.SelectionChanged += cmb_SelectionChanged;
            cmb5.SelectionChanged += cmb_SelectionChanged;
            cmb6.SelectionChanged += cmb_SelectionChanged;
            cmb7.SelectionChanged += cmb_SelectionChanged;
            cmb8.SelectionChanged += cmb_SelectionChanged;
        }

        public void getSystemTime() {
            ShowTimer = new System.Windows.Threading.DispatcherTimer();
            ShowTimer.Tick += new EventHandler(ShowCurTimer);//起个Timer一直获取当前时间
            ShowTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            ShowTimer.Start();
        }
        public void ShowCurTimer(object sender, EventArgs e)
        {
            //获得年月日
            this.date.Text = DateTime.Now.DayOfWeek.ToString() + " " + DateTime.Now.ToString("yyyy-MM-dd");   //yyyy年MM月dd日
            //获得时分秒
            this.Tt.Text = DateTime.Now.ToString("HH:mm:ss");
            this.time.Text = date.Text + " " + Tt.Text;
        }
        private void getLButtonArray() {
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    //   Lbtn_num[i, j] = 0;
                    Button button = new Button();
                    button.Width = 80;
                    button.Height = 20;
                    button.Content = "0";
                    button.Tag = i;
                    button.Click += new RoutedEventHandler(Lbtn_Click);
                    Lgrid_button_array.Children.Add(button);
                    button.SetValue(Grid.RowProperty, i);
                    button.SetValue(Grid.ColumnProperty, j);
                    Lbtn_array[i, j] = button;
                }
            }
        }
        private void getRButtonArray()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    //   Rbtn_num[i, j] = 0;
                    Button button = new Button();
                    button.Width = 80;
                    button.Height = 20;
                    button.Content = "0";
                    button.Tag = i;
                    button.Click += new RoutedEventHandler(Rbtn_Click);
                    Rgrid_button_array.Children.Add(button);
                    button.SetValue(Grid.RowProperty, i);
                    button.SetValue(Grid.ColumnProperty, j);
                    Rbtn_array[i, j] = button;
                }
            }
        }



        private void getComboboxData() {
            int Now_year = DateTime.Now.Year;
            for (int i = 1; i < 13; i++) {
                month.Items.Add(i);
            }
            for (int i = 1900; i < Now_year + 1; i++)
            {
                year.Items.Add(i);
            }
        }

        private void btn_gain_Click(object sender, RoutedEventArgs e) {
            Button button = sender as Button;
            if (button.Content.Equals("High")) {
                button.Content = "Low";
            }
            else
            {
                button.Content = "High";
            }
        }

        private void Lbtn_Click(object sender, RoutedEventArgs e) {
            Button button = sender as Button;
            int index_y = Convert.ToInt32(button.Tag);
            int index_x = 0;
            for (int i = 0; i < 4; i++)
            {
                if (button == Lbtn_array[index_y, i])
                {
                    index_x = i;
                }
            }
            Boolean isCanadd = Col_isCanAdd(index_y, index_x, Lbtn_array);
            Boolean isCansub = Col_isCanSub(index_y, index_x, Lbtn_array);
            if ((isCanadd && !isCanEnable_Add(Lbtn_array)) || (isCansub && !isCanEnable_Sub(Lbtn_array)))
                return;
            if (button.Content.Equals("0") && isCanadd) {
                button.Content = "+";
                for (int i = 0; i < 4; i++) {
                    if (i != index_x) {
                        Lbtn_array[index_y, i].IsEnabled = false;
                    }
                    if (!isCansub && Lbtn_array[i, index_x].Content.Equals("0")) {
                        Lbtn_array[i, index_x].IsEnabled = false;
                    }
                    Rbtn_array[i, index_x].IsEnabled = false;
                }
                setHipText("+", index_y, "L");
                getDChannels();
            }
            else if (!isCanadd && button.Content.Equals("0"))
            {
                button.Content = "-";
                for (int i = 0; i < 4; i++)
                {
                    if (i != index_x)
                    {
                        Lbtn_array[index_y, i].IsEnabled = false;
                    }
                    if (Lbtn_array[i, index_x].Content.Equals("0")) {
                        Lbtn_array[i, index_x].IsEnabled = false;
                    }
                    Rbtn_array[i, index_x].IsEnabled = false;
                }
                setHipText("-", index_y, "L");
                getDChannels();
            }
            else
            {
                button.Content = "0";
                for (int i = 0; i < 4; i++)
                {
                    if (Col_isCanAdd(index_y, i, Rbtn_array) && Col_isCanSub(index_y, i, Rbtn_array))
                        setEnable(index_y, i, Lbtn_array);
                    if (Col_isCanAdd(i, index_x, Rbtn_array) && Col_isCanSub(i, index_x, Rbtn_array))
                        setEnable(i, index_x, Lbtn_array);
                }
                if (Col_isCanAdd(index_y, index_x, Lbtn_array) && Col_isCanSub(index_y, index_x, Lbtn_array)) {
                    for (int i = 0; i < 4; i++) {
                        setEnable(i, index_x, Rbtn_array);
                    }
                }
                setHipText("0", index_y, "L");
                getDChannels();
            }
        }

        private void setHipText(String txt, int index, String str) {
            if (str.Equals("L")) {
                if (index == 0)
                    LHip1.Text = txt;
                else if (index == 1)
                {
                    LHip2.Text = txt;
                }
                else if (index == 2)
                {
                    LHip3.Text = txt;
                }
                else if (index == 3)
                    LHip4.Text = txt;
            }
            else if (str.Equals("R"))
            {
                if (index == 0)
                    RHip1.Text = txt;
                else if (index == 1)
                {
                    RHip2.Text = txt;
                }
                else if (index == 2)
                {
                    RHip3.Text = txt;
                }
                else if (index == 3)
                    RHip4.Text = txt;
            }
        }

        private void setEnable(int y, int x, Button[,] button) {
            if ((Col_isCanSub(y, x, button) || Col_isCanAdd(y, x, button)) && Row_isCanEnable(y, x, button))
                button[y, x].IsEnabled = true;
        }

        private Boolean isCanEnable_Add(Button[,] button) {
            Boolean isCanEnable = true;
            int add_num = 0;
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++)
                {
                    if (button[i, j].Content.Equals("+")) {
                        add_num++;
                        if (add_num > 1) {
                            isCanEnable = false;
                            break;
                        }
                    }
                }
            }
            return isCanEnable;
        }

        private Boolean isCanEnable_Sub(Button[,] button)
        {
            Boolean isCanEnable = true;
            int sub_num = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (button[i, j].Content.Equals("-"))
                    {
                        sub_num++;
                        if (sub_num > 1)
                        {
                            isCanEnable = false;
                            break;
                        }
                    }
                }
            }
            return isCanEnable;
        }

        private Boolean Col_isCanAdd(int y, int x, Button[,] button) {
            Boolean isCanadd = true;
            for (int i = 0; i < 4; i++) {
                if (button[i, x].Content.Equals("+"))
                {
                    isCanadd = false;
                    break;
                }
            }
            return isCanadd;
        }

        private Boolean Col_isCanSub(int y, int x, Button[,] button)
        {
            Boolean isCansub = true;
            for (int i = 0; i < 4; i++)
            {
                if (button[i, x].Content.Equals("-"))
                {
                    isCansub = false;
                    break;
                }
            }
            return isCansub;
        }

        private Boolean Row_isCanEnable(int y, int x, Button[,] button)
        {
            Boolean isCanEnable = true;
            for (int i = 0; i < 4; i++) {
                if (i != x && (button[y, i].Content.Equals("-") || button[y, i].Content.Equals("+"))) {
                    isCanEnable = false;
                    break;
                }
            }
            return isCanEnable;
        }

        private void Rbtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int index_y = Convert.ToInt32(button.Tag);
            int index_x = 0;
            for (int i = 0; i < 4; i++)
            {
                if (button == Rbtn_array[index_y, i])
                {
                    index_x = i;
                }
            }
            Boolean isCanadd = Col_isCanAdd(index_y, index_x, Rbtn_array);
            Boolean isCansub = Col_isCanSub(index_y, index_x, Rbtn_array);
            if ((isCanadd && !isCanEnable_Add(Rbtn_array)) || (isCansub && !isCanEnable_Sub(Rbtn_array)))
                return;
            if (button.Content.Equals("0") && isCanadd)
            {
                button.Content = "+";
                for (int i = 0; i < 4; i++)
                {
                    if (i != index_x)
                    {
                        Rbtn_array[index_y, i].IsEnabled = false;
                    }
                    if (!isCansub && Rbtn_array[i, index_x].Content.Equals("0"))
                    {
                        Rbtn_array[i, index_x].IsEnabled = false;
                    }
                    Lbtn_array[i, index_x].IsEnabled = false;
                }
                setHipText("+", index_y, "R");
                getDChannels();

            }
            else if (!isCanadd && button.Content.Equals("0"))
            {
                button.Content = "-";
                for (int i = 0; i < 4; i++)
                {
                    if (i != index_x)
                    {
                        Rbtn_array[index_y, i].IsEnabled = false;
                    }
                    if (Rbtn_array[i, index_x].Content.Equals("0"))
                    {
                        Rbtn_array[i, index_x].IsEnabled = false;
                    }
                    Lbtn_array[i, index_x].IsEnabled = false;
                }
                setHipText("-", index_y, "R");
                getDChannels();
            }
            else
            {
                button.Content = "0";
                for (int i = 0; i < 4; i++)
                {
                    if (Col_isCanAdd(index_y, i, Lbtn_array) && Col_isCanSub(index_y, i, Lbtn_array))
                        setEnable(index_y, i, Rbtn_array);
                    if (Col_isCanAdd(i, index_x, Lbtn_array) && Col_isCanSub(i, index_x, Lbtn_array))
                        setEnable(i, index_x, Rbtn_array);
                }
                if (Col_isCanAdd(index_y, index_x, Rbtn_array) && Col_isCanSub(index_y, index_x, Rbtn_array))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        setEnable(i, index_x, Lbtn_array);
                    }
                }
                setHipText("0", index_y, "R");
                getDChannels();
            }
        }

        private void month_Selected(object sender, RoutedEventArgs e)
        {
            int select_mon = Convert.ToInt32(month.SelectedItem);
            if (select_mon == 7)
            {
                getDay(31);
            }
            else if (select_mon == 2)
            {
                int select_year = Convert.ToInt32(year.SelectedItem);
                if (DateTime.IsLeapYear(select_year))
                {
                    getDay(29);
                }
                else
                {
                    getDay(28);
                }
            }
            else if ((select_mon % 2) == 0)
            {
                getDay(31);
            }
            else {
                getDay(30);
            }
        }

        private void year_Selected(object sender, RoutedEventArgs e)
        {
            int select_year = Convert.ToInt32(year.SelectedItem);
            int select_mon = Convert.ToInt32(month.SelectedItem);
            if (select_mon == 2 && DateTime.IsLeapYear(select_year))
            {
                getDay(29);
            }
            else if (select_mon == 2 && !DateTime.IsLeapYear(select_year))
            {
                getDay(28);
            }
        }

        private void getDay(int days) {
            day.Items.Clear();
            for (int i = 1; i < days + 1; i++)
            {
                day.Items.Add(i);
            }
        }

        private void getDChannels() {
            FDChannel.Items.Clear();
            SDChannel.Items.Clear();
            Array.Clear(channal, 0, 4);
            Dictionary<int, String> dictionary = new Dictionary<int, String>();
            int L_add_y, L_add_x, L_sub_y, R_add_y, R_add_x, R_sub_y = 0;
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    if (Lbtn_array[j, i].Content.Equals("+")) {
                        L_add_y = j;
                        L_add_x = i;
                        L_sub_y = getSubRow(L_add_x, Lbtn_array);
                        if (L_sub_y != -1) {
                            String str = "LHip" + (L_add_y + 1) + "-LHip" + (L_sub_y + 1);
                            dictionary.Add(L_add_x++, str);
                            channal[i] = "L" + L_add_y + "" + L_sub_y;
                        }
                    }
                    if (Rbtn_array[j, i].Content.Equals("+"))
                    {
                        R_add_y = j;
                        R_add_x = i;
                        R_sub_y = getSubRow(R_add_x, Rbtn_array);
                        if (R_sub_y != -1)
                        {
                            String str = "RHip" + (R_add_y + 1) + "-RHip" + (R_sub_y + 1);
                            dictionary.Add(R_add_x, str);
                            channal[i] = "R" + R_add_y + "" + R_sub_y;
                        }
                    }
                }
            }
            for (int i = 0; i < 4; i++) {
                if (dictionary.ContainsKey(i)) {
                    int num = i + 1;
                    FDChannel.Items.Add(num + "  " + dictionary[i]);
                    SDChannel.Items.Add(num + "  " + dictionary[i]);
                    setECoGChannels(num, dictionary[i]);
                    setSettingChannels(num, dictionary[i]);
                    setTestChannels(num, dictionary[i]);
                }
            }
            if (FDChannel.Items.Count < 4) {
                therapy1_status.Content = "Disable";
                detection_status.Content = "Disable";
            }
        }

        private void clearECoGChannels() {
            CH1_btn.IsEnabled = false;
            CH2_btn.IsEnabled = false;
            CH3_btn.IsEnabled = false;
            CH4_btn.IsEnabled = false;
            CH1_name.Text = "";
            CH2_name.Text = "";
            CH3_name.Text = "";
            CH4_name.Text = "";
        }

        private void setSettingChannels(int i, String str) {
            if (i == 1) {
                ch1_name.Text = str;
            }
            else if (i == 2)
            {
                ch2_name.Text = str;
            }
            else if (i == 3)
            {
                ch3_name.Text = str;
            }
            else if (i == 4)
            {
                ch4_name.Text = str;
            }
        }

        private void setTestChannels(int i, String str)
        {
            if (i == 1)
            {
                test_ch1.Text = str;
            }
            else if (i == 2)
            {
                // test_ch2.Text = str;
            }
            else if (i == 3)
            {
                // test_ch3.Text = str;
            }
            else if (i == 4)
            {
                //test_ch4.Text = str;
            }
        }

        private void setECoGChannels(int i, String str) {
            if (i == 1) {
                CH1_name.Text = str;
                CH1_btn.IsEnabled = true;
            }
            else if (i == 2) {
                CH2_name.Text = str;
                CH2_btn.IsEnabled = true;
            }
            else if (i == 3)
            {
                CH3_name.Text = str;
                CH3_btn.IsEnabled = true;
            }
            else if (i == 4)
            {
                CH4_name.Text = str;
                CH4_btn.IsEnabled = true;
            }
        }

        private int getSubRow(int x, Button[,] button) {
            int y = -1;
            for (int i = 0; i < 4; i++) {
                if (button[i, x].Content.Equals("-")) {
                    y = i;
                    break;
                }
            }
            return y;
        }

        private void getListData() {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("myXML.xml");
            XmlNode root = xmlDoc.SelectSingleNode("Root");
            XmlNodeList list = root.ChildNodes;
            for (int i = 0; i < list.Count; i++) {
                XmlElement element = (XmlElement)list[i];
                list_detector.Items.Add(new Detection { date = element.GetAttribute("time"), detection = element.Name });
            }

        }


        private void PAFbp_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                //PA_tabbp_grid.Visibility = Visibility.Visible;
                //PA_tabbp_header.Visibility = Visibility.Visible;
                setPASenable(false);
            }
            else if (button.Content.Equals("On")) {
                button.Content = "Off";
                //PA_tabbp_grid.Visibility = Visibility.Collapsed;
                //PA_tabbp_header.Visibility = Visibility.Collapsed;
                if (!isPAFSelected())
                    setPASenable(true);
            }
        }

        private Boolean isPAFSelected() {
            Boolean isSelected = false;
            if (PAFarea.Content.Equals("On"))
                isSelected = true;
            else if (PAFbp.Content.Equals("On"))
                isSelected = true;
            else if (PAFll.Content.Equals("On"))
                isSelected = true;
            return isSelected;
        }

        private Boolean isPASSelected()
        {
            Boolean isSelected = false;
            if (PASarea.Content.Equals("On"))
                isSelected = true;
            else if (PASbp.Content.Equals("On"))
                isSelected = true;
            else if (PASll.Content.Equals("On"))
                isSelected = true;
            return isSelected;
        }

        private void setPASenable(Boolean b) {
            PASarea.IsEnabled = b;
            PASbp.IsEnabled = b;
            PASll.IsEnabled = b;
        }

        private void setPAFenable(Boolean b)
        {
            PAFarea.IsEnabled = b;
            PAFbp.IsEnabled = b;
            PAFll.IsEnabled = b;
        }

        private Boolean isPBFSelected()
        {
            Boolean isSelected = false;
            if (PBFarea.Content.Equals("On"))
                isSelected = true;
            else if (PBFbp.Content.Equals("On"))
                isSelected = true;
            else if (PBFll.Content.Equals("On"))
                isSelected = true;
            return isSelected;
        }

        private Boolean isPBSSelected()
        {
            Boolean isSelected = false;
            if (PBSarea.Content.Equals("On"))
                isSelected = true;
            else if (PBSbp.Content.Equals("On"))
                isSelected = true;
            else if (PBSll.Content.Equals("On"))
                isSelected = true;
            return isSelected;
        }

        private void setPBSenable(Boolean b)
        {
            PBSarea.IsEnabled = b;
            PBSbp.IsEnabled = b;
            PBSll.IsEnabled = b;
        }

        private void setPBFenable(Boolean b)
        {
            PBFarea.IsEnabled = b;
            PBFbp.IsEnabled = b;
            PBFll.IsEnabled = b;
        }

        private void PAFarea_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                setPASenable(false);
            }
            else if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                if (!isPAFSelected())
                    setPASenable(true);
            }
        }

        private void PAFll_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                //PA_tabll_grid.Visibility = Visibility.Visible;
                //PA_tabll_header.Visibility = Visibility.Visible;
                setPASenable(false);
            }
            else if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                //PA_tabll_grid.Visibility = Visibility.Collapsed;
                //PA_tabll_header.Visibility = Visibility.Collapsed;
                if (!isPAFSelected())
                    setPASenable(true);
            }
        }

        private void PASbp_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                //PA_tabbp_grid.Visibility = Visibility.Visible;
                //PA_tabbp_header.Visibility = Visibility.Visible;
                setPAFenable(false);
            }
            else if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                //PA_tabbp_grid.Visibility = Visibility.Collapsed;
                //PA_tabbp_header.Visibility = Visibility.Collapsed;
                if (!isPASSelected())
                    setPAFenable(true);
            }
        }

        private void PASll_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                //PA_tabll_grid.Visibility = Visibility.Visible;
                //PA_tabll_header.Visibility = Visibility.Visible;
                setPAFenable(false);
            }
            else if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                //PA_tabll_grid.Visibility = Visibility.Collapsed;
                //PA_tabll_header.Visibility = Visibility.Collapsed;
                if (!isPASSelected())
                    setPAFenable(true);
            }
        }

        private void PASarea_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                setPAFenable(false);
            }
            else if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                if (!isPASSelected())
                    setPAFenable(true);
            }
        }

        private void PBFll_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                PB_tabll_grid.Visibility = Visibility.Visible;
                PB_tabll_header.Visibility = Visibility.Visible;
                setPBSenable(false);
            }
            else if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                PB_tabll_grid.Visibility = Visibility.Collapsed;
                PB_tabll_header.Visibility = Visibility.Collapsed;
                if (!isPBFSelected())
                    setPBSenable(true);
            }
        }

        private void PBFbp_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                PB_tabbp_grid.Visibility = Visibility.Visible;
                PB_tabbp_header.Visibility = Visibility.Visible;
                setPBSenable(false);
            }
            else if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                PB_tabbp_grid.Visibility = Visibility.Collapsed;
                PB_tabbp_header.Visibility = Visibility.Collapsed;
                if (!isPBFSelected())
                    setPBSenable(true);
            }
        }

        private void PBFarea_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                setPBSenable(false);
            }
            else if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                if (!isPBFSelected())
                    setPBSenable(true);
            }
        }

        private void PBSbp_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                PB_tabbp_grid.Visibility = Visibility.Visible;
                PB_tabbp_header.Visibility = Visibility.Visible;
                setPBFenable(false);
            }
            else if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                PB_tabbp_grid.Visibility = Visibility.Collapsed;
                PB_tabbp_header.Visibility = Visibility.Collapsed;
                if (!isPBSSelected())
                    setPBFenable(true);
            }
        }

        private void PBSll_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                PB_tabll_grid.Visibility = Visibility.Visible;
                PB_tabll_header.Visibility = Visibility.Visible;
                setPBFenable(false);
            }
            else if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                PB_tabll_grid.Visibility = Visibility.Collapsed;
                PB_tabll_header.Visibility = Visibility.Collapsed;
                if (!isPBSSelected())
                    setPBFenable(true);
            }
        }

        private void PBSarea_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                setPBFenable(false);
            }
            else if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                if (!isPBSSelected())
                    setPBFenable(true);
            }
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("Off"))
            {
                button.Content = "On";
            }
            else if (button.Content.Equals("On")) {
                button.Content = "Off";
            }
        }

        private void cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int cmb1_num, cmb2_num, cmb3_num, cmb4_num, cmb5_num, cmb6_num, cmb7_num, cmb8_num;
            cmb1_num = cmb1.SelectedIndex;
            cmb2_num = cmb2.SelectedIndex;
            cmb3_num = cmb3.SelectedIndex;
            cmb4_num = cmb4.SelectedIndex;
            cmb5_num = cmb5.SelectedIndex;
            cmb6_num = cmb6.SelectedIndex;
            cmb7_num = cmb7.SelectedIndex;
            cmb8_num = cmb8.SelectedIndex;
            txt_num.Text = Convert.ToString(cmb1_num + cmb2_num + cmb3_num + cmb4_num + cmb5_num + cmb6_num + cmb7_num + cmb8_num);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                cmb1.IsEnabled = false;
                cmb1.SelectedIndex = 0;
            }
            else if (button.Content.Equals("Off")) {
                button.Content = "On";
                cmb1.IsEnabled = true;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                cmb2.IsEnabled = false;
                cmb2.SelectedIndex = 0;
            }
            else if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                cmb2.IsEnabled = true;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                cmb3.IsEnabled = false;
                cmb3.SelectedIndex = 0;
            }
            else if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                cmb3.IsEnabled = true;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                cmb4.IsEnabled = false;
                cmb4.SelectedIndex = 0;
            }
            else if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                cmb4.IsEnabled = true;
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                cmb5.IsEnabled = false;
                cmb5.SelectedIndex = 0;
            }
            else if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                cmb5.IsEnabled = true;
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                cmb6.IsEnabled = false;
                cmb6.SelectedIndex = 0;
            }
            else if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                cmb6.IsEnabled = true;
            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                cmb7.IsEnabled = false;
                cmb7.SelectedIndex = 0;
            }
            else if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                cmb7.IsEnabled = true;
            }
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                cmb8.IsEnabled = false;
                cmb8.SelectedIndex = 0;
            }
            else if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                cmb8.IsEnabled = true;
            }
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            if (current.Text == "" || description.Text == "" || FDChannel.Items.Count != 4 || FDChannel.SelectedIndex < 0 ||
                SDChannel.SelectedIndex < 0) {
                MessageBox.Show("Incomplete Configuration！");
                return;
            }
            try {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("myXML.xml");
                XmlNode root = xmlDoc.SelectSingleNode("Root");
                if (root != null)
                {
                    XmlElement element = xmlDoc.CreateElement(current.Text);
                    element.SetAttribute("time", time.Text);
                    stamp_time.Text = time.Text;
                    for (int i = 0; i < 4; i++)
                    {
                        if (channal[i] != null)
                        {
                            XmlElement elec = xmlDoc.CreateElement("Elec" + i);
                            elec.InnerText = channal[i];
                            element.AppendChild(elec);
                        }
                    }
                    XmlElement gain1 = xmlDoc.CreateElement("Gain1");
                    gain1.InnerText = gain_btn1.Content.ToString();
                    XmlElement gain2 = xmlDoc.CreateElement("Gain2");
                    gain2.InnerText = gain_btn2.Content.ToString();
                    XmlElement gain3 = xmlDoc.CreateElement("Gain3");
                    gain3.InnerText = gain_btn3.Content.ToString();
                    XmlElement gain4 = xmlDoc.CreateElement("Gain4");
                    gain4.InnerText = gain_btn4.Content.ToString();
                    XmlElement description1 = xmlDoc.CreateElement("description");
                    description1.InnerText = description.Text;
                    XmlElement fdchannel = xmlDoc.CreateElement("FDChannel");
                    fdchannel.InnerText = FDChannel.SelectedIndex.ToString();
                    XmlElement sdchannel = xmlDoc.CreateElement("SDChannel");
                    sdchannel.InnerText = SDChannel.SelectedIndex.ToString();
                    XmlElement pafbp = xmlDoc.CreateElement("PAFbp");
                    pafbp.InnerText = PAFbp.Content.ToString();
                    XmlElement pafll = xmlDoc.CreateElement("PAFll");
                    pafll.InnerText = PAFll.Content.ToString();
                    XmlElement pafarea = xmlDoc.CreateElement("PAFarea");
                    pafarea.InnerText = PAFarea.Content.ToString();
                    XmlElement pasbp = xmlDoc.CreateElement("PASbp");
                    pasbp.InnerText = PASbp.Content.ToString();
                    XmlElement pasll = xmlDoc.CreateElement("PASll");
                    pasll.InnerText = PASll.Content.ToString();
                    XmlElement pasarea = xmlDoc.CreateElement("PASarea");
                    pasarea.InnerText = PASarea.Content.ToString();
                    XmlElement pbfbp = xmlDoc.CreateElement("PBFbp");
                    pbfbp.InnerText = PBFbp.Content.ToString();
                    XmlElement pbfll = xmlDoc.CreateElement("PBFll");
                    pbfll.InnerText = PBFll.Content.ToString();
                    XmlElement pbfarea = xmlDoc.CreateElement("PBFarea");
                    pbfarea.InnerText = PBFarea.Content.ToString();
                    XmlElement pbsbp = xmlDoc.CreateElement("PBSbp");
                    pbsbp.InnerText = PBSbp.Content.ToString();
                    XmlElement pbsll = xmlDoc.CreateElement("PBSll");
                    pbsll.InnerText = PBSll.Content.ToString();
                    XmlElement pbsarea = xmlDoc.CreateElement("PBSarea");
                    pbsarea.InnerText = PBSarea.Content.ToString();
                    element.AppendChild(gain1);
                    element.AppendChild(gain2);
                    element.AppendChild(gain3);
                    element.AppendChild(gain4);
                    element.AppendChild(description1);
                    element.AppendChild(fdchannel);
                    element.AppendChild(sdchannel);
                    element.AppendChild(pafbp);
                    element.AppendChild(pafll);
                    element.AppendChild(pafarea);
                    element.AppendChild(pasbp);
                    element.AppendChild(pasll);
                    element.AppendChild(pasarea);
                    element.AppendChild(pbfbp);
                    element.AppendChild(pbfll);
                    element.AppendChild(pbfarea);
                    element.AppendChild(pbsbp);
                    element.AppendChild(pbsll);
                    element.AppendChild(pbsarea);
                    root.AppendChild(element);
                    xmlDoc.Save("myXML.xml");
                }
                else
                {
                    XmlTextWriter writer = new XmlTextWriter("myXML.xml", System.Text.Encoding.UTF8);
                    //使用自动缩进便于阅读
                    writer.Formatting = Formatting.Indented;
                    //XML声明 
                    writer.WriteStartDocument();
                    //书写根元素 
                    writer.WriteStartElement("Root");
                    //开始一个元素 
                    writer.WriteStartElement(current.Text);
                    //向先前创建的元素中添加一个属性 
                    writer.WriteAttributeString("time", time.Text);
                    //添加子元素 
                    for (int i = 0; i < 4; i++)
                    {
                        if (channal[i] != null)
                        {
                            writer.WriteElementString("Elec" + i, channal[i]);
                        }
                    }
                    writer.WriteElementString("Gain1", gain_btn1.Content.ToString());
                    writer.WriteElementString("Gain2", gain_btn2.Content.ToString());
                    writer.WriteElementString("Gain3", gain_btn3.Content.ToString());
                    writer.WriteElementString("Gain4", gain_btn4.Content.ToString());
                    writer.WriteElementString("description", description.Text);
                    writer.WriteElementString("FDChannel", FDChannel.SelectedIndex.ToString());
                    writer.WriteElementString("SDChannel", SDChannel.SelectedIndex.ToString());
                    writer.WriteElementString("PAFbp", PAFbp.Content.ToString());
                    writer.WriteElementString("PAFarea", PAFarea.Content.ToString());
                    writer.WriteElementString("PAFll", PAFll.Content.ToString());
                    writer.WriteElementString("PASbp", PASbp.Content.ToString());
                    writer.WriteElementString("PASarea", PASarea.Content.ToString());
                    writer.WriteElementString("PASll", PASll.Content.ToString());
                    writer.WriteElementString("PBFbp", PBFbp.Content.ToString());
                    writer.WriteElementString("PBFarea", PBFarea.Content.ToString());
                    writer.WriteElementString("PBFll", PBFll.Content.ToString());
                    writer.WriteElementString("PBSbp", PBSbp.Content.ToString());
                    writer.WriteElementString("PBSarea", PBSarea.Content.ToString());
                    writer.WriteElementString("PBSll", PBSll.Content.ToString());
                    //关闭item元素 
                    writer.WriteEndElement(); // 关闭元素 
                                              //在节点间添加一些空
                    writer.Close();
                }
                list_detector.Items.Add(new Detection { date = time.Text, detection = current.Text });
            }
            catch (Exception str) {
                MessageBox.Show("读写文件出错" + str.ToString());
            }

        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {

        }

        private void CheckPattern() {
            if (PAFbp.Content.Equals("On") || PASbp.Content.Equals("On")) {
                //PA_tabbp_grid.Visibility = Visibility.Visible;
                //PA_tabbp_header.Visibility = Visibility.Visible;
            }
            if (PASll.Content.Equals("On") || PAFll.Content.Equals("On"))
            {
                //PA_tabll_grid.Visibility = Visibility.Visible;
                //PA_tabll_header.Visibility = Visibility.Visible;
            }
            if (PBFbp.Content.Equals("On") || PBSbp.Content.Equals("On"))
            {
                PB_tabbp_grid.Visibility = Visibility.Visible;
                PB_tabbp_header.Visibility = Visibility.Visible;
            }
            if (PBSll.Content.Equals("On") || PBFll.Content.Equals("On"))
            {
                PB_tabll_grid.Visibility = Visibility.Visible;
                PB_tabll_header.Visibility = Visibility.Visible;
            }
        }

        private void setAllbtndisable() {
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    Lbtn_array[i, j].IsEnabled = false;
                    Rbtn_array[i, j].IsEnabled = false;
                }
            }
        }

        private void setChannelbtn(String str, int num) {
            Char[] ch = str.ToCharArray();
            int add_y = Convert.ToInt32(ch[1]) - 48;
            int sub_y = Convert.ToInt32(ch[2]) - 48;
            if (ch[0] == 'L')
            {
                Lbtn_array[add_y, num].IsEnabled = true;
                Lbtn_array[add_y, num].Content = "+";
                Lbtn_array[sub_y, num].IsEnabled = true;
                Lbtn_array[sub_y, num].Content = "-";
                setHipText("+", add_y, "L");
                setHipText("-", sub_y, "L");
                num++;
                add_y++;
                sub_y++;
                String item = "LHip" + add_y + "-LHip" + sub_y;
                setECoGChannels(num, item);
                setSettingChannels(num, item);
                setTestChannels(num, item);
                FDChannel.Items.Add(num + "  " + item);
                SDChannel.Items.Add(num + "  " + item);
            }
            else if (ch[0] == 'R') {
                Rbtn_array[add_y, num].IsEnabled = true;
                Rbtn_array[add_y, num].Content = "+";
                Rbtn_array[sub_y, num].IsEnabled = true;
                Rbtn_array[sub_y, num].Content = "-";
                setHipText("+", add_y, "R");
                setHipText("-", sub_y, "R");
                num++;
                add_y++;
                sub_y++;
                String item = "RHip" + add_y + "-RHip" + sub_y;
                setECoGChannels(num, item);
                setSettingChannels(num, item);
                setTestChannels(num, item);
                FDChannel.Items.Add(num + "  " + item);
                SDChannel.Items.Add(num + "  " + item);
            }
        }

        private void list_detector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try {
                Detection detection = list_detector.SelectedItem as Detection;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("myXML.xml");

                XmlNodeList lis = xmlDoc.GetElementsByTagName(detection.detection);
                current.Text = detection.detection;
                stamp_time.Text = detection.date;
                setAllbtndisable();
                FDChannel.Items.Clear();
                SDChannel.Items.Clear();
                String elec0 = lis[0].SelectSingleNode("Elec0").InnerText;
                String elec1 = lis[0].SelectSingleNode("Elec1").InnerText;
                String elec2 = lis[0].SelectSingleNode("Elec2").InnerText;
                String elec3 = lis[0].SelectSingleNode("Elec3").InnerText;
                setChannelbtn(elec0, 0);
                setChannelbtn(elec1, 1);
                setChannelbtn(elec2, 2);
                setChannelbtn(elec3, 3);
                description.Text = lis[0].SelectSingleNode("description").InnerText;
                gain_btn1.Content = lis[0].SelectSingleNode("Gain1").InnerText;
                gain_btn2.Content = lis[0].SelectSingleNode("Gain2").InnerText;
                gain_btn3.Content = lis[0].SelectSingleNode("Gain3").InnerText;
                gain_btn4.Content = lis[0].SelectSingleNode("Gain4").InnerText;
                int fdindex = Convert.ToInt32(lis[0].SelectSingleNode("FDChannel").InnerText);
                int sdindex = Convert.ToInt32(lis[0].SelectSingleNode("SDChannel").InnerText);
                FDChannel.SelectedIndex = fdindex;
                SDChannel.SelectedIndex = sdindex;
                PAFbp.Content = lis[0].SelectSingleNode("PAFbp").InnerText;
                PAFll.Content = lis[0].SelectSingleNode("PAFll").InnerText;
                PAFarea.Content = lis[0].SelectSingleNode("PAFarea").InnerText;
                PASbp.Content = lis[0].SelectSingleNode("PASbp").InnerText;
                PASll.Content = lis[0].SelectSingleNode("PASll").InnerText;
                PASarea.Content = lis[0].SelectSingleNode("PASarea").InnerText;
                PBFbp.Content = lis[0].SelectSingleNode("PBFbp").InnerText;
                PBFll.Content = lis[0].SelectSingleNode("PBFll").InnerText;
                PBFarea.Content = lis[0].SelectSingleNode("PBFarea").InnerText;
                PBSbp.Content = lis[0].SelectSingleNode("PBSbp").InnerText;
                PBSll.Content = lis[0].SelectSingleNode("PBSll").InnerText;
                PBSarea.Content = lis[0].SelectSingleNode("PBSarea").InnerText;
                if (isPAFSelected())
                    setPASenable(false);
                else if (isPASSelected())
                    setPAFenable(false);
                if (isPBFSelected())
                    setPBSenable(false);
                else if (isPBSSelected())
                    setPBFenable(false);
                CheckPattern();
            }
            catch (Exception str) {
                MessageBox.Show("打开文件错误" + str.ToString());
            }
        }

        private void detection_status_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("Enable"))
            {
                button.Content = "Disable";
            }
            else if (current.Text == "" || description.Text == "" || FDChannel.Items.Count != 4 || FDChannel.SelectedIndex < 0 ||
                SDChannel.SelectedIndex < 0 || mode.SelectedIndex < 0)
            {
                MessageBox.Show("Incomplete Configuration！");
                return;
            }
            else {
                button.Content = "Enable";
            }

        }

        private void therapy_status_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("Enable"))
            {
                button.Content = "Disable";
            }
            else if (!ischecked())
            {
                MessageBox.Show("Incomplete Configuration！");
            }
            else {
                button.Content = "Enable";
            }
        }

        private Boolean ischecked()
        {
            Boolean boolean = true;
            if (tpy1_bst1_cnt.SelectedIndex < 0 || tpy1_bst1_duration.SelectedIndex < 0 || tpy1_bst1_fcy.SelectedIndex < 0 ||
                tpy1_bst1_pse.SelectedIndex < 0 || tpy1_bst2_cnt.SelectedIndex < 0 || tpy1_bst2_duration.SelectedIndex < 0 ||
                tpy1_bst2_fcy.SelectedIndex < 0 || tpy1_bst2_pse.SelectedIndex < 0 || FDChannel.Items.Count < 4) {
                boolean = false;
            }
            return boolean;
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            if (ComDevice.IsOpen)
            {
                ComDevice.Close();
                MessageBox.Show("已关闭");
            }
            else {
                ComDevice.PortName = "COM5";
                ComDevice.BaudRate = 19200;
                ComDevice.Parity = (Parity)0;
                ComDevice.DataBits = 8;
                ComDevice.StopBits = (StopBits)1;
                try
                {
                    ComDevice.Open();
                    MessageBox.Show("串口打开成功!");
                }
                catch (Exception str)
                {
                    MessageBox.Show(str.ToString());
                }
            }
        }
        public bool SendData(byte[] data)
        {
            
                if (ComDevice.IsOpen)
                {
                    try
                    {
                        ComDevice.Write(data, 0, data.Length);//发送数据
                        
                        return true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("串口未打开");
                }
                return false;
                          
        }

        private byte[] strToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2).Replace(" ", ""), 16);
            return returnBytes;
        }

        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            String[] portname = SerialPort.GetPortNames();
            String name = "";
            foreach (String str in portname) {
                name += str + " ";
            }
            MessageBox.Show(name);
        }

        /*   private byte[] GetAppFormat(byte[] data, int typeNum) {
               int count = data.Length;
               byte[] appbyte = new byte[7 + count];
               appbyte[0] = (byte)seq;
               appbyte[1] = (byte)typeNum;
               appbyte[2] = 0;
               appbyte[3] = 0;
               appbyte[4] = (byte)count;
               appbyte[3] = (byte)(count >> 8);
               data.CopyTo(appbyte, 5);
               appbyte[5 + count] = 0;
               appbyte[6 + count] = 0;
               return appbyte;
           }

           private byte[] GetTransportsFormat(byte[] data, int snum, int dnum) {
               int count = data.Length;
               byte[] tsbyte = new byte[3 + count];
               tsbyte[0] = (byte)snum;
               tsbyte[1] = (byte)dnum;
               data.CopyTo(tsbyte, 2);
               tsbyte[2 + count] = 0;
               return tsbyte;
           }

           private byte[] GetIntworkFormat(byte[] data, int snum, int dnum) {
               int count = data.Length;
               byte[] intbyte = new byte[3 + count];
               intbyte[0] = (byte)snum;
               intbyte[1] = (byte)dnum;
               data.CopyTo(intbyte, 2);
               intbyte[2 + count] = 0;
               return intbyte;
           }

           private byte[] GetDataLinkFormat(byte[] data, Boolean addMac) {
               byte[] newbyte = ModifyData(data, 0);
               int count = newbyte.Length;
               byte[] dlbyte;
               if (addMac)
               {
                   dlbyte = new byte[10 + count];
                   dlbyte[0] = 0xFF;
                   dlbyte[1] = 0;
                   dlbyte[2] = 0;
                   dlbyte[3] = 1;
                   dlbyte[4] = 2;
                   dlbyte[5] = 3;
                   dlbyte[6] = 4;
                   dlbyte[7] = 5;
                   newbyte.CopyTo(dlbyte, 8);
                   dlbyte[8 + count] = 0;
                   dlbyte[9 + count] = 0xFF;
               }
               else
               {
                   dlbyte = new byte[4 + count];
                   dlbyte[0] = 0xFF;
                   dlbyte[1] = 1;
                   newbyte.CopyTo(dlbyte, 2);
                   dlbyte[2 + count] = 0;
                   dlbyte[3 + count] = 0xFF;
               }
               return dlbyte;
           } */

        private byte[] PackData(byte[] data,Type.Request request)
        {
            int length = data.Length;

            byte[] datapack = new byte[length+2];

            datapack[0] = (byte)request;

            for (int i = 0; i < length; i++)
            {
                datapack[i + 1] = data[i];
            }

            byte sum = 0;

            for (int i = 0; i < length + 1; i++)
            {
                sum += datapack[i];
            }

            datapack[length + 1] = sum;

            byte[] modifydata = ModifyData(datapack);

            int mod_length = modifydata.Length;

            byte[] senddata = new byte[mod_length + 2];

            senddata[0] = 0xAA;

            for (int i = 0; i < mod_length; i++)
            {
                senddata[i + 1] = modifydata[i];
            }

            senddata[mod_length + 1] = 0xBB;

            return senddata;
        }

        private byte[] ModifyData(byte[] data)
        {
            int length = data.Length;

            byte[] modifydata = new byte[length*2-1];

            modifydata[0] = data[0];

            for (int i = 1; i < length; i++)
            {
                byte num = data[i];

                modifydata[i * 2 - 1] = (byte)(0x30 | (num >> 4));
                modifydata[i * 2] = (byte)( 0x30 |(0x0F & num));
            }

            return modifydata;
           /* for (int i = index; i < data.Length; i++) {
                if (data[i] == 0xFF) {
                    byte[] newbyte = new byte[data.Length + 1];
                    Array.Copy(data, newbyte, i);
                    newbyte[i] = 0xFE;
                    newbyte[i + 1] = 0x0D;
                    i++;
                    for (int j = i; j < data.Length; j++) {
                        newbyte[j + 1] = data[j];
                    }
                    return ModifyData(newbyte, i + 1);
                }
                if (data[i] == 0xFE)
                {
                    byte[] newbyte = new byte[data.Length + 1];
                    Array.Copy(data, newbyte, i);
                    newbyte[i] = 0xFE;
                    newbyte[i + 1] = 0x0E;
                    i++;
                    for (int j = i; j < data.Length; j++)
                    {
                        newbyte[j + 1] = data[j];
                    }
                    return ModifyData(newbyte, i + 1);
                }
            }
            return data; */
        }

        private void Button_Click_12(object sender, RoutedEventArgs e)
        {
            
        }

        private void DataRecTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
             byte[] data = new byte[receive_len];

             for (int i = 0; i < receive_len; i++)
             {
                 data[i] = Receive_buf[i];
             }

             receive_len = 0;

             AnalysisData(data);          
        }

        byte[] Receive_buf = new byte[500];
        int receive_len = 0;
        Boolean buf_flag = false;
        private void ComDevice_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            DataRecTimer.Stop();

            byte[] Redata = new byte[ComDevice.BytesToRead];
            int head_buf = receive_len;
            ComDevice.Read(Redata, 0, Redata.Length);
            receive_len += Redata.Length;

            if (Redata.Length == 0 || receive_len > 500)
            {
                DataRecTimer.Start();

                return;
            }

            int j = 0;

            for (int i = head_buf; i < receive_len; i++)
            {
                Receive_buf[i] = Redata[j];

                j++;
            }

            DataRecTimer.Start();

            /*    int j = 0;

                for (int i = head_buf; i < receive_len; i++)
                {
                    Receive_buf[i] = Redata[j];

                    j++;
                }

                if (receive_len > 299)
                {
                    byte[] data = new byte[receive_len];

                    for (int i = 0; i < receive_len; i++)
                    {
                        data[i] = Receive_buf[i];
                    }

                    receive_len = 0;

                    AnalysisData(data);
                } */

            //AnalysisData(Redata);
          /*  if (!buf_flag && Redata.Length ==1)
                 {
                     if (Redata[0] == 0xAA)
                     {
                         head_buf = 0;
                         receive_len = 0;

                         receive_len += Redata.Length;

                         Receive_buf[0] = Redata[0];

                         buf_flag = true;
                     }
                 }
                 else if (Redata.Length>1 && Redata[0] == 0xAA && Redata[Redata.Length - 1] == 0xBB)
                 {
                     receive_len = 0;
                     buf_flag = false;

                     AnalysisData(Redata);
                 }
                 else if (!buf_flag && Redata[0] == 0xAA)
                 {
                     int j = 0;
                     head_buf = 0;
                     receive_len = 0;

                     receive_len +=Redata.Length ;

                     for (int i = head_buf; i < receive_len; i++)
                     {
                         Receive_buf[i] = Redata[j];

                         j++;
                     }

                     buf_flag = true;
                 }
                 else if (buf_flag && Redata[Redata.Length - 1] == 0xBB)
                 {
                     if (receive_len > 1024)
                     {
                         receive_len = 0;
                         buf_flag = false;
                     }

                     int j = 0;

                     for (int i = head_buf; i < receive_len; i++)
                     {
                         Receive_buf[i] = Redata[j];

                         j++;
                     }

                     byte[] data = new byte[receive_len];

                     for (int i = 0; i < receive_len; i++)
                     {
                         data[i] = Receive_buf[i];
                     }

                     receive_len = 0;
                     buf_flag = false;

                     AnalysisData(data);
                 }
                 else if (buf_flag)
                 {
                     if (receive_len > 1024)
                     {
                         receive_len = 0;
                         buf_flag = false;
                     }

                     int j = 0;

                     for (int i = head_buf; i < receive_len; i++)
                     {
                         Receive_buf[i] = Redata[j];

                         j++;
                     }
                 }*/
        }
       
        int index = 0;
        int Max_num = -10000000;
        int Min_num = 10000000;
        int eegnum = 0;
        private void AnalysisData(byte[] data) {
            int length = data.Length;       

            if (length <3)
                return;

           // byte[] restore = RestoreData(data);

            //int res_length = restore.Length;

            if (0xAA == data[0] && 0xBB == data[length - 1])
            {
                byte sum = 0;
                int i = 1;

                for (; i < length - 2; i++)
                {
                    sum += data[i];
                }

                if (sum == data[length - 2])
                {
                    Type.Reply reply = (Type.Reply)data[1];

                    switch (reply)
                    {
                        case Type.Reply.REP_CONFIRM:

                            ErrNum = 0;
                            ResendNum = 0;

                            ReqState = Type.ReqState.SEND_KEEP_LIVE;

                            isLive = true;

                            CurState = Type.ConnectState.STATE_CONNECT;

                            //CheckisSend();

                            MessageBox.Show("连接成功！");

                            break;

                        case Type.Reply.REP_EEGDATA:
                            eegnum++;
                            ReplyEEgData(data);

                            break;

                        case Type.Reply.REP_STIMULATE:

                            ErrNum = 0;
                            ResendNum = 0;

                            CheckisSend();

                            MessageBox.Show("刺激成功!");

                            break;

                        case Type.Reply.REP_ALIVE:

                            ErrNum = 0;
                            ResendNum = 0;

                            CheckisSend();

                            break;

                        case Type.Reply.REP_RESEND:

                            ReplyResend(data[2]);

                            break;

                        case Type.Reply.REP_CHARGE:

                            ReplyCharge(data[2]);

                            break;

                        case Type.Reply.REP_FLASHEEGEND:

                            ErrNum = 0;
                            ResendNum = 0;

                            ReqState = Type.ReqState.SEND_KEEP_LIVE;

                            isLive = true;

                            MessageBox.Show("FLASH EEG 读取完成！");

                            break;
                    }
                }
                else
                {
                    ReqResend();
                }
            }
            else
            {
                ReqResend();
            }

            /*   for (int i = 0;i < data.Length - 3;i++)
               {
                   int num = -1;

                   if ((data[i] & 0xC0) == 0xC0)
                   {
                       num++;

                       int sign = 0;

                       char j = (char)(data[i] & 0x3F);

                    //   if ((j & 0x20) > 0)
                    //   {
                   //        j = (char)(j & 0x1F);

                  //         sign = -1;
                  //     }

                       num += j * 4096;

                       j = (char)(data[i + 1] & 0x3F);

                       num += j * 64;

                       j = (char)(data[i + 2] & 0x3F);

                       num += j;

                       //num *= 64;

                       //   if (sign < 0)
                       //   {
                       //       num = 0 - num;
                       //  }
                       num = num - 131072;
                   }


                   if (num != -1)
                   {
                       WriteableBitmapTrendLine.prices[index] = (float)num;

                       if (num > Max_num)
                       {
                           Max_num = num;
                       }

                       if (num < Min_num)
                       {
                           Min_num = num;
                       }

                       index++;

                       if (index == 250)
                       {                     

                           ReceiveVoltageData(1);

                           index = 0;
                       }

                   }

               } */
         /*   if (data[0] == 0xFF && data[length - 1] == 0xFF) {
               // try
             //   {
                    byte[] newbyte = new byte[length - 2];
                    for (int i = 1; i < length - 1; i++)
                    {
                        newbyte[i - 1] = data[i];
                    }                  
                  

                    byte[] newdata = RestoreData(newbyte, 0);

                    //Check device whether is joined
                       switch (Convert.ToInt32(newbyte[0]))
                       {
                           case HAVE_JOIN:
                               ReadData(newdata);
                               break;
                           case NOT_JOIN:
                               Responsive(newdata);
                               break;
                       }
                    //int datalength = newbyte[13] * 255 + newbyte[14];
                    //String datastr = "";
                    //for (int i = 0; i < datalength; i++)
                    //{
                    //    datastr += newbyte[15 + i] + "\n";
                    //}
                    //String str = "MAC地址:" + newdata[0] + " " + newdata[1] + " " + newdata[2] + " " +
                    //            newdata[3] + " " + newdata[4] + newbyte[5] + "\n短源地址:" + newbyte[6] +
                    //            "\n短目的地址:" + newbyte[7] + "\n源端口:" + newbyte[8] + "\n目的端口:" + newbyte[9] +
                    //            "\nseq号:" + newbyte[10] + "\n包类型:" + newbyte[11] + "\n重试次数:" + newbyte[12] +
                    //            "\n数据长度:" + datalength + "\n数据:" + datastr;
                    //MessageBox.Show(str);
                    // MessageBox.Show(newdata[0]+" ");
         //       }
         //       catch (Exception ex) {
                   // MessageBox.Show(ex.ToString());
           //     }              
                
            } */
        }

        private void CheckisSend()
        {
            lock (obj)
            {
                isLive = true;

                for (int i = 0; i < Type.REQUEST_NUM; i++)
                {
                    if (1 == SendTable[i])
                    {
                        DataSendTimer.Stop();

                        PreReqIndex = (Type.Request)i;

                        SendTable[i] = 0;

                        byte[] senddata = RequestData[i];

                        SendData(senddata);

                        isLive = false;

                        return;
                    }
                }

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,

                   new Action(() =>

                   {
                       Charge_btn.IsEnabled = true;
                   }));
            }           
        }

        private byte[] RestoreData(byte[] data)
        {          
            int length = data.Length;

            byte[] restore = new byte[(length - 3)/2+3];

            restore[0] = data[0];
            restore[1] = data[1];

            int j = 2;

            for (int i = 2; i < length - 2; i++)
            {
                byte high = data[i];
                i++;

                byte low = data[i];

                restore[j] = (byte)((high << 4) | (0x0F & low));
                j++;
            }

            restore[j] = data[length - 1];

            return restore;
        }
        int j = 0;
        private void ReplyCharge(byte data)
        {
            ResendNum = 0;
            ErrNum = 0;

            j++;
            CheckisSend();

            if (Type.ChARGING == data)
            {
                MessageBox.Show("充电中!");
            }
            else if (Type.DISCHARGING == data)
            {             
                MessageBox.Show("停止充电!");
            }
        }

        private void ReplyResend(byte data)
        {
            if (ResendNum > 2)
            {
                ResendNum = 0;
                ErrNum = 0;

                CheckisSend();

                return;
            }

            if (Type.REPLY_PRE == data)
            {
                ResendData(PreReqIndex);
            }
           /* else
            {
                Type.Request request = (Type.Request)data;

                switch (request)
                {
                    case Type.Request.REQ_CONNECT:

                        ResendData(Type.Request.REQ_CONNECT);

                        break;

                    case Type.Request.REQ_EEGDATA:

                        ResendData(Type.Request.REQ_EEGDATA);

                        break;

                    case Type.Request.REQ_RESEND:

                        ResendData(Type.Request.REQ_RESEND);

                        break;

                    case Type.Request.REQ_STIMULATE:

                        ResendData(Type.Request.REQ_STIMULATE);

                        break;

                    case Type.Request.KEEP_LIVE:

                        ResendData(Type.Request.KEEP_LIVE);

                        break;

                    case Type.Request.REQ_CHARGE:

                        ResendData(Type.Request.REQ_CHARGE);

                        break;
                }
            }*/
        }

        private void ResendData(Type.Request request)
        {
            byte[] senddata = RequestData[(int)request];          

            PreReqIndex = Type.Request.REQ_RESEND;
            RequestData[(int)Type.Request.REQ_RESEND] = senddata;

            ResendNum ++;
            ErrNum = 0;

            SendData(senddata);
        }

        private void ReplyEEgData(byte[] data)
        {

            for (int i = 0; i < Data_buf.Length; i++)
            {
                short high = data[3 + i*2];
                short low = data[2 + i*2];

                short num = (short)(((short)(high << 8)) | low);

                Data_buf[i] = num;
            }

            TimeFlag = Data_buf.Length - 1;

            ErrNum = 0;
            ResendNum = 0;

            CheckisSend();

            Refreshtimer.Start();
        }

       /* private void ReadData(byte[] data)
        {
            if (checkAddress(data[1],data[2]))
            {
                try
                {
                    //int datalength = data[8] * 255 + data[9];
                    //String datastr = "";
                    //for (int i = 0; i < datalength; i++)
                    //{
                    //    datastr += data[10 + i] + "\n";
                    //}
                    //String str = "源端口:" + data[3] + "\n目的端口:" + data[4] + "\n包类型:" + data[6]
                    //           + datastr;
                    //MessageBox.Show(str);

                    //check port number
                    switch (Convert.ToInt32(data[4]))
                    {
                        case 3:

                           /* int j = 0;

                              for (int i = 0; i < Data_buf.Length; i++)
                              {
                                  short high = (short)data[11+j];
                                  short low = (short)data[10+j];

                                  short num = (short)(((short)(high << 8)) | low);

                                 j += 2;

                                  Data_buf[i] = num;
                              }

                              TimeFlag = Data_buf.Length - 1;

                              Refreshtimer.Start();*/

                          /*  short high = (short)data[11];
                            short low = (short)data[10];
                            short num = (short)(((short)(high << 8)) | low);


                            ReceiveVoltageData(num);


                            break;
                    }
                   // MessageBox.Show(data[4] + " ");
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.ToString());
                }              
            }
        }*/

        /*  private Boolean checkAddress(byte source,byte destination)
          {
              if ((Convert.ToInt32(source) == 1) && (Convert.ToInt32(destination) == 0))
              {
                  return true;
              }

              MessageBox.Show("地址错误:"+Convert.ToInt32(source)+"&"+ Convert.ToInt32(destination));

              return false;
          }*/

        private void ReqResend()
        {
            if (ErrNum < 3)
            {
                byte[] data = new byte[3];

                data[0] = 0x01;
                data[1] = 0x02;
                data[2] = 0x03;

                byte[] senddata = PackData(data, Type.Request.REQ_RESEND);

                //PreReqIndex = Type.Request.REQ_RESEND;
                RequestData[(int)Type.Request.REQ_RESEND] = senddata;

                SynchroSendData(Type.Request.REQ_RESEND, senddata);

                //isLive = false;

                ErrNum++;
                ResendNum = 0;
            }
            else
            {
                ErrNum = 0;
                ResendNum = 0;

                CheckisSend();
            }
        }
        int k = 0;
        private void ReplyFinish(Type.Request request)
        {
            switch (request)
            {
                case Type.Request.REQ_STIMULATE:

                    ErrNum = 0;
                    ResendNum = 0;

                    CheckisSend();

                    MessageBox.Show("刺激成功!");

                    break;

                case Type.Request.KEEP_LIVE:

                    ErrNum = 0;
                    ResendNum = 0;
                    k++;
                    CheckisSend();

                    break;
            }
        }

        /*    private Boolean checkMac()
            {
                return true;
            }

            private Boolean checkType(byte data)
            {
                if (Convert.ToInt32(data) == Type.JOIN_REQUEST)
                {
                    return true;
                }

              //  MessageBox.Show("包类型错误:"+ Convert.ToInt32(data));

                return false;
            }

            private Boolean checkSN(byte datah,byte datal)
            {
                if ((Convert.ToInt32(datah) == 0xAB) && (Convert.ToInt32(datal) == 0xCD))
                {
                    return true;
                }

                MessageBox.Show("SN号错误:"+ Convert.ToInt32(datah)+"&"+ Convert.ToInt32(datal));

                return false;
            }

            private byte[] RestoreData(byte[] data,int index) {
                for (int i = index; i < data.Length; i++) {
                    if (data[i] == 0xFE&&data[i+1]==0x0D) {
                        byte[] newbyte = new byte[data.Length - 1];
                        Array.Copy(data,newbyte,i);
                        newbyte[i] = 0xFF;
                        i++;
                        for (int j = i; j < data.Length - 1; j++) {
                            newbyte[j] = data[j + 1];
                        }
                        return RestoreData(newbyte, i);
                    }
                    if (data[i] == 0xFE && data[i + 1] == 0x0E)
                    {
                        byte[] newbyte = new byte[data.Length - 1];
                        Array.Copy(data, newbyte, i);
                        newbyte[i] = 0xFE;
                        i++;
                        for (int j = i; j < data.Length - 1; j++)
                        {
                            newbyte[j] = data[j + 1];
                        }
                        return RestoreData(newbyte, i);
                    }
                }
                return data;
            }*/
        int reqeegnum = 0;
        Type.ReqState ReqState = Type.ReqState.SEND_REQ_CONNECT;
        private void Eventtimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ComDevice.IsOpen)
            {
                switch (ReqState)
                {
                    case Type.ReqState.SEND_REQ_CONNECT:

                        SendReqConnect();

                        break;

                    case Type.ReqState.SEND_KEEP_LIVE:

                        SendKeepLive();

                        break;

                    case Type.ReqState.SEND_REQ_EEGDATA:
                        reqeegnum++;
                        SendReqEEG();

                        break;

                    case Type.ReqState.SEND_REQ_FLASHEEG:

                        SendReqFlashEEG();

                        break;
                }
            }
            else
            {
                if (isLive)
                {
                    isLive = false;
                                    
                    ErrNum = 0;
                    ResendNum = 0;

                    KeepLive_Err_Ctr = 0;

                    ReqState = Type.ReqState.SEND_REQ_CONNECT;

                    MessageBox.Show("断开连接!");
                }

                OpenSerialport();
            }
         
        }

        private void SendReqFlashEEG()
        {
            byte[] data = new byte[5];

            data[0] = 0x01;
            data[1] = 0x02;
            data[2] = 0x03;
            data[3] = 0x04;
            data[4] = 0x05;

            byte[] senddata = PackData(data, Type.Request.REQ_FLASHEEG);

            RequestData[(int)Type.Request.REQ_FLASHEEG] = senddata;

            SynchroSendData(Type.Request.REQ_FLASHEEG, senddata);
        }

        private void checkislive()
        {
            if (!isLive)
            {
                KeepLive_Err_Ctr++;

                if (KeepLive_Err_Ctr > 1)
                {
                    ErrNum = 0;
                    ResendNum = 0;

                    KeepLive_Err_Ctr = 0;

                    ReqState = Type.ReqState.SEND_REQ_CONNECT;

                    MessageBox.Show("断开连接");
                }
            }
            else
            {
                KeepLive_Err_Ctr = 0;
            }
        }

        private void SendReqConnect()
        {           
            byte[] data = new byte[0];

            byte[] senddata = PackData(data, Type.Request.REQ_CONNECT);

            PreReqIndex = Type.Request.REQ_CONNECT;
            RequestData[(int)Type.Request.REQ_CONNECT] = senddata;         

            SendData(senddata);

            isLive = false;
        }

        private void SendReqEEG()
        {
            //checkislive();

            byte[] data = new byte[2];

            data[0] = 0x01;
            data[1] = 0x02;

            byte[] senddata = PackData(data, Type.Request.REQ_EEGDATA);

            RequestData[(int)Type.Request.REQ_EEGDATA] = senddata;

            SynchroSendData(Type.Request.REQ_EEGDATA, senddata);
        }
        int m = 0;
        private void SendKeepLive()
        {
            // checkislive();
            m++;
            byte[] data = new byte[4];

            data[0] = 0x01;
            data[1] = 0x02;
            data[2] = 0x03;
            data[3] = 0x04;

            byte[] senddata = PackData(data, Type.Request.KEEP_LIVE);
           
            RequestData[(int)Type.Request.KEEP_LIVE] = senddata;

            SynchroSendData(Type.Request.KEEP_LIVE, senddata);
        }

        int CurPortLength = 0;
        int MaxPortLength = 0;
        private Boolean OpenSerialport()
        {
            String[] portName = SerialPort.GetPortNames();

            MaxPortLength = portName.Length;

            if (MaxPortLength < 1)
            {
                return false;
            }
            else
            {
                if (CurPortLength > MaxPortLength - 1)
                {
                    CurPortLength = 0;
                }

                ComDevice.PortName = portName[CurPortLength];
                ComDevice.BaudRate = 19200;
                ComDevice.Parity = (Parity)0;
                ComDevice.DataBits = 8;
                ComDevice.StopBits = (StopBits)1;
                try
                {
                    ComDevice.Open();
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;
        }

        private void Button_Click_13(object sender, RoutedEventArgs e)
        {
            /*int current = 1;
            int frequency = 20;
            int pwper = 1000;
            int duration = 500;
            int flag = 1;

            byte[] newdata = new byte[14];
            newdata[0] = (byte)current;
            newdata[1] = (byte)frequency;
            newdata[2] = (byte)(frequency >> 8);
            newdata[3] = (byte)(frequency >> 16);
            newdata[4] = (byte)(frequency >> 24);
            newdata[5] = (byte)pwper;
            newdata[6] = (byte)(pwper >> 8);
            newdata[7] = (byte)(pwper>>16);
            newdata[8] = (byte)(pwper >> 24);
            newdata[9] = (byte)duration;
            newdata[10] = (byte)(duration >> 8);
            newdata[11] = (byte)(duration >> 16);
            newdata[12] = (byte)(duration >> 24);
            newdata[13] = (byte)(flag);*/

            Button button = sender as Button;

            if (button.Content.Equals("Program"))
            {
                button.Content = "1111";

                DataRecTimer.Start();
            }
            else if (button.Content.Equals("1111"))
            {
                button.Content = "Program";

                DataRecTimer.Stop();
            }
           
        }

        private void Button_Click_14(object sender, RoutedEventArgs e)
        {
            if (!checkDataisEmpty(0))
            {
                test_Lp1.Content = tpy1_Lp1.Content;
                test_Lp2.Content = tpy1_Lp2.Content;
                test_Lp3.Content = tpy1_Lp3.Content;
                test_Lp4.Content = tpy1_Lp4.Content;
                test_Rp1.Content = tpy1_Rp1.Content;
                test_Rp2.Content = tpy1_Rp2.Content;
                test_Rp3.Content = tpy1_Rp3.Content;
                test_Rp4.Content = tpy1_Rp4.Content;
                test_cnt1.SelectedIndex = tpy1_bst1_cnt.SelectedIndex;
                test_cnt2.SelectedIndex = tpy1_bst2_cnt.SelectedIndex;
                test_duration1.SelectedIndex = tpy1_bst1_duration.SelectedIndex;
                test_duration2.SelectedIndex = tpy1_bst2_duration.SelectedIndex;
                test_fcy1.SelectedIndex = tpy1_bst1_fcy.SelectedIndex;
                test_fcy2.SelectedIndex = tpy1_bst2_fcy.SelectedIndex;
                test_pse1.SelectedIndex = tpy1_bst1_pse.SelectedIndex;
                test_pse2.SelectedIndex = tpy1_bst2_pse.SelectedIndex;

                test_burst1.Content = "Enable";
                test_burst2.Content = "Enable";

                therapy_num = 0;

                MainTab.SelectedIndex = 5;
            }           
        }

        private Boolean checkDataisEmpty(int therapy_num)
        {
            ComboBox[,] cnt = new ComboBox[6, 2] { { tpy1_bst1_cnt,tpy1_bst2_cnt},{ tpy2_bst1_cnt,tpy2_bst2_cnt}
                ,{tpy3_bst1_cnt,tpy3_bst2_cnt },{tpy4_bst1_cnt,tpy4_bst2_cnt },{tpy5_bst1_cnt,tpy5_bst2_cnt }
            ,{ test_cnt1,test_cnt2} };
            ComboBox[,] duration = new ComboBox[6, 2] { { tpy1_bst1_duration,tpy1_bst2_duration},{ tpy2_bst1_duration,tpy2_bst2_duration}
                ,{tpy3_bst1_duration,tpy3_bst2_duration },{tpy4_bst1_duration,tpy4_bst2_duration },{tpy5_bst1_duration,tpy5_bst2_duration }
            ,{ test_duration1,test_duration2} };
            ComboBox[,] fcy = new ComboBox[6, 2] { { tpy1_bst1_fcy,tpy1_bst2_fcy},{ tpy2_bst1_fcy,tpy2_bst2_fcy}
                ,{tpy3_bst1_fcy,tpy3_bst2_fcy },{tpy4_bst1_fcy,tpy4_bst2_fcy },{tpy5_bst1_fcy,tpy5_bst2_fcy }
            ,{ test_fcy1,test_fcy2} };
            ComboBox[,] pse = new ComboBox[6, 2] { { tpy1_bst1_pse,tpy1_bst2_pse},{ tpy2_bst1_pse,tpy2_bst2_pse}
                ,{tpy3_bst1_pse,tpy3_bst2_pse },{tpy4_bst1_pse,tpy4_bst2_pse },{tpy5_bst1_pse,tpy5_bst2_pse }
            ,{ test_pse1,test_pse2} };

            /* if (FDChannel.Items.Count < 4|| cnt[therapy_num,0].SelectedIndex < 0 || duration[therapy_num,0].SelectedIndex < 0 
                 || fcy[therapy_num,0].SelectedIndex < 0|| pse[therapy_num,0].SelectedIndex < 0 || cnt[therapy_num,1].SelectedIndex < 0 
                 || duration[therapy_num,1].SelectedIndex < 0|| fcy[therapy_num,1].SelectedIndex < 0 || pse[therapy_num,1].SelectedIndex < 0)
             {
                 return true;
             }*/

            if (test_cnt1.SelectedIndex < 0 || test_fcy1.SelectedIndex < 0 || test_pse1.SelectedIndex < 0
                || test_duration1.SelectedIndex < 0)
            {
                return true;
            }

            return false;
        }

        //Deliver
        private void Button_Click_15(object sender, RoutedEventArgs e)
        {
           // while (busy) ;
            if (checkDataisEmpty(5))
            {
                return;
            }

            String cnt = test_cnt1.SelectedItem.ToString();
            String fcy = test_fcy1.SelectedItem.ToString();
            String pse = test_pse1.SelectedItem.ToString();
            String dura = test_duration1.SelectedItem.ToString();

            int current = GetStr2Int(cnt)/10;
            int frequency = GetStr2Int(fcy);
            int pwper = GetStr2Int(pse)/100;
            int duration = GetStr2Int(dura);

            byte[] newdata = new byte[13];
            
            newdata[0] = (byte)frequency;
            newdata[1] = (byte)(frequency >> 8);
            newdata[2] = (byte)(frequency >> 16);
            newdata[3] = (byte)(frequency >> 24);
            newdata[4] = (byte)pwper;
            newdata[5] = (byte)(pwper >> 8);
            newdata[6] = (byte)(pwper >> 16);
            newdata[7] = (byte)(pwper >> 24);
            newdata[8] = (byte)duration;
            newdata[9] = (byte)(duration >> 8);
            newdata[10] = (byte)(duration >> 16);
            newdata[11] = (byte)(duration >> 24);
            newdata[12] = (byte)current;

            byte[] senddata = PackData(newdata, Type.Request.REQ_STIMULATE);
           
            RequestData[(int)Type.Request.REQ_STIMULATE] = senddata;

            SynchroSendData(Type.Request.REQ_STIMULATE, senddata);
        }

        private int GetStr2Int(String str)
        {
            String[] newstr = str.Split(' ');
            int num = Convert.ToInt32(newstr[0]);
            return num;
        }

        //receive voltage graph 
        private void Button_Click_16(object sender, RoutedEventArgs e)
        {
            if (CurState == Type.ConnectState.STATE_CONNECT)
            {
                ReqState = Type.ReqState.SEND_REQ_EEGDATA;

                Eeg_Start_btn.IsEnabled = false;
                Eeg_Stop_btn.IsEnabled = true;
            }            
        }
        

        private void ReceiveVoltageData(int voltage)
        {
           // double mv = (double)((voltage-512) * (-19.23));
            double uv = 0.195 * ((ushort )voltage - 32768);
           // double uv = 4.578 * voltage;

            double coordorigin = bitmap_ch1.ActualHeight / 2;
            double eachvolt = coordorigin / 10000;
            double y = coordorigin - eachvolt * uv;
            //double x = bd_canvas.ActualWidth; 
           // WriteableBitmapTrendLine.coordorigin = (float)(bitmap_ch1.ActualHeight / 2);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,

               new Action(() =>

               {
                   vm = new MinuteQuoteViewModel();
                   vm.LastPx = y;
                   this.bitmap_ch1.LatestQuote = vm;

                 //  Min_Num.Text = Min_num + " ";
                //   Max_Num.Text = Max_num + " ";

               /*    int Dvalue = Max_num - Min_num;

                   if (Dvalue < 22)
                   {
                       zero.Text = "0μv";
                       PstNum1.Text = "20μv ";
                       PstNum2.Text = "40μv ";
                       NegNum1.Text = "-20μv";
                       NegNum2.Text = "-40μv";

                       WriteableBitmapTrendLine.divider = 50;
                   }
                   else if (Dvalue < 220)
                   {
                       zero.Text = "0μv";
                       PstNum1.Text = "200μv ";
                       PstNum2.Text = "400μv ";
                       NegNum1.Text = "-200μv";
                       NegNum2.Text = "-400μv";

                       WriteableBitmapTrendLine.divider = 500;
                   }
                   else if (Dvalue < 2200)
                   {
                       zero.Text = "0mv";
                       PstNum1.Text = "2mv ";
                       PstNum2.Text = "4mv ";
                       NegNum1.Text = "-2mv";
                       NegNum2.Text = "-4mv";

                       WriteableBitmapTrendLine.divider = 5000;
                   }
                   else if (Dvalue < 22000)
                   {
                       zero.Text = "0mv";
                       PstNum1.Text = "20mv ";
                       PstNum2.Text = "40mv ";
                       NegNum1.Text = "-20mv";
                       NegNum2.Text = "-40mv";

                       WriteableBitmapTrendLine.divider = 50000;
                   }
                   else
                   {
                       zero.Text = "0 mv";
                       PstNum1.Text = "200mv ";
                       PstNum2.Text = "400mv ";
                       NegNum1.Text = "-200mv";
                       NegNum2.Text = "-400mv";

                       WriteableBitmapTrendLine.divider = 500000;
                   }

                   Min_num = 10000000;
                   Max_num = -10000000; */

                               

                   //CustomPoint point = new CustomPoint(x, y);

                   //ch1_canvas.Children.Add(point);
                   //foreach (CustomPoint p in pointlist)
                   //{
                   //    Point oldpoint = p.Point;
                   //    p.Point = new Point(oldpoint.X - 1, oldpoint.Y);
                   //}
                   //foreach (Line l in linelist)
                   //{
                   //    l.X1 = l.X1 - 1;
                   //    l.X2 = l.X2 - 1;
                   //}

                   //if (pointlist.Count != 0)
                   //{
                   //    Line line = new Line();
                   //    line.X1 = pointlist.Last<CustomPoint>().Point.X;
                   //    line.Y1 = pointlist.Last<CustomPoint>().Point.Y;
                   //    line.X2 = point.Point.X;
                   //    line.Y2 = point.Point.Y;
                   //    line.StrokeThickness = 1;
                   //    line.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
                   //    linelist.Add(line);

                   //    ch1_canvas.Children.Add(line);


                   //}

                   //pointlist.Add(point);

                   //if (pointlist.Count > 1000)
                   //{
                   //    CustomPoint custom = pointlist.First<CustomPoint>();
                   //    pointlist.RemoveAt(0);
                   //    custom.Close();
                   //}

                   //if (linelist.Count > 1000)
                   //{
                   //    Line l = linelist.First<Line>();
                   //    linelist.RemoveAt(0);
                   //    l = null;
                   //    GC.Collect();
                   //}


               }));
            //MessageBox.Show(x + "&" + y+"&"+voltage);

        }

        private void Refreshtimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ReceiveVoltageData(Data_buf[TimeFlag]);

            TimeFlag--;

            if (TimeFlag == 0)
            {
                Refreshtimer.Stop();
            }
        }



        private void Button_Click_17(object sender, RoutedEventArgs e)
        {
            if (CurState == Type.ConnectState.STATE_CONNECT)
            {
                ReqState = Type.ReqState.SEND_KEEP_LIVE;

                Refreshtimer.Stop();

                Eeg_Stop_btn.IsEnabled = false;
                Eeg_Start_btn.IsEnabled = true;
            }
        }

        private void Backgroundtimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,

              new Action(() =>

              {
                  vm = new MinuteQuoteViewModel();
                  vm.LastPx = 0;
                  this.bitmap_ch1.LatestQuote = vm;
              }));           
        }

        private void Button_Click_18(object sender, RoutedEventArgs e)
        {
            String userName = user.Text.Trim();
            String pword = password.Password.Trim();
            if (userName =="" && pword =="")
            {
                return;
            }
            if (userName.Equals("admin") && pword.Equals("123456"))
            {
                item_configure.IsEnabled = true;
                item_data.IsEnabled = true;
                item_product.IsEnabled = true;
                item_program.IsEnabled = true;
                item_test.IsEnabled = true;
                user.Visibility =Visibility.Hidden;
                password.Visibility = Visibility.Hidden;
                land.Visibility = Visibility.Hidden;
            }
            else
            {
                MessageBox.Show("login incorrect");
            }
        }

        private void Button_Click_19(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button.Content.Equals("Disable"))
            {
                button.Content = "Enable";
            }
            else
            {
                button.Content = "Disable";
            }
        }

        private void Button_Click_20(object sender, RoutedEventArgs e)
        {
            tpy2_bst1_cnt.SelectedIndex = tpy1_bst1_cnt.SelectedIndex;
            tpy2_bst1_fcy.SelectedIndex = tpy1_bst1_fcy.SelectedIndex;
            tpy2_bst1_pse.SelectedIndex = tpy1_bst1_pse.SelectedIndex;
            tpy2_bst1_duration.SelectedIndex = tpy1_bst1_duration.SelectedIndex;
            tpy2_bst2_cnt.SelectedIndex = tpy1_bst2_cnt.SelectedIndex;
            tpy2_bst2_fcy.SelectedIndex = tpy1_bst2_fcy.SelectedIndex;
            tpy2_bst2_pse.SelectedIndex = tpy1_bst2_pse.SelectedIndex;
            tpy2_bst2_duration.SelectedIndex = tpy1_bst2_duration.SelectedIndex;

            therapyTab.SelectedIndex = 2; 
        }

        private void Button_Click_21(object sender, RoutedEventArgs e)
        {
            tpy3_bst1_cnt.SelectedIndex = tpy2_bst1_cnt.SelectedIndex;
            tpy3_bst1_fcy.SelectedIndex = tpy2_bst1_fcy.SelectedIndex;
            tpy3_bst1_pse.SelectedIndex = tpy2_bst1_pse.SelectedIndex;
            tpy3_bst1_duration.SelectedIndex = tpy2_bst1_duration.SelectedIndex;
            tpy3_bst2_cnt.SelectedIndex = tpy2_bst2_cnt.SelectedIndex;
            tpy3_bst2_fcy.SelectedIndex = tpy2_bst2_fcy.SelectedIndex;
            tpy3_bst2_pse.SelectedIndex = tpy2_bst2_pse.SelectedIndex;
            tpy3_bst2_duration.SelectedIndex = tpy2_bst2_duration.SelectedIndex;

            therapyTab.SelectedIndex = 3;
        }

        private void Button_Click_22(object sender, RoutedEventArgs e)
        {
            tpy4_bst1_cnt.SelectedIndex = tpy3_bst1_cnt.SelectedIndex;
            tpy4_bst1_fcy.SelectedIndex = tpy3_bst1_fcy.SelectedIndex;
            tpy4_bst1_pse.SelectedIndex = tpy3_bst1_pse.SelectedIndex;
            tpy4_bst1_duration.SelectedIndex = tpy3_bst1_duration.SelectedIndex;
            tpy4_bst2_cnt.SelectedIndex = tpy3_bst2_cnt.SelectedIndex;
            tpy4_bst2_fcy.SelectedIndex = tpy3_bst2_fcy.SelectedIndex;
            tpy4_bst2_pse.SelectedIndex = tpy3_bst2_pse.SelectedIndex;
            tpy4_bst2_duration.SelectedIndex = tpy3_bst2_duration.SelectedIndex;

            therapyTab.SelectedIndex = 4;
        }

        private void Button_Click_23(object sender, RoutedEventArgs e)
        {
            tpy5_bst1_cnt.SelectedIndex = tpy4_bst1_cnt.SelectedIndex;
            tpy5_bst1_fcy.SelectedIndex = tpy4_bst1_fcy.SelectedIndex;
            tpy5_bst1_pse.SelectedIndex = tpy4_bst1_pse.SelectedIndex;
            tpy5_bst1_duration.SelectedIndex = tpy4_bst1_duration.SelectedIndex;
            tpy5_bst2_cnt.SelectedIndex = tpy4_bst2_cnt.SelectedIndex;
            tpy5_bst2_fcy.SelectedIndex = tpy4_bst2_fcy.SelectedIndex;
            tpy5_bst2_pse.SelectedIndex = tpy4_bst2_pse.SelectedIndex;
            tpy5_bst2_duration.SelectedIndex = tpy4_bst2_duration.SelectedIndex;
          
            therapyTab.SelectedIndex = 5;
        }

        private void Button_Click_24(object sender, RoutedEventArgs e)
        {
            tpy1_bst1_cnt.SelectedIndex = tpy5_bst1_cnt.SelectedIndex;
            tpy1_bst1_fcy.SelectedIndex = tpy5_bst1_fcy.SelectedIndex;
            tpy1_bst1_pse.SelectedIndex = tpy5_bst1_pse.SelectedIndex;
            tpy1_bst1_duration.SelectedIndex = tpy5_bst1_duration.SelectedIndex;
            tpy1_bst2_cnt.SelectedIndex = tpy5_bst2_cnt.SelectedIndex;
            tpy1_bst2_fcy.SelectedIndex = tpy5_bst2_fcy.SelectedIndex;
            tpy1_bst2_pse.SelectedIndex = tpy5_bst2_pse.SelectedIndex;
            tpy1_bst2_duration.SelectedIndex = tpy5_bst2_duration.SelectedIndex;

            therapyTab.SelectedIndex = 1;
        }

        private void Button_Click_25(object sender, RoutedEventArgs e)
        {
            if (!checkDataisEmpty(1))
            {
                test_Lp1.Content = tpy2_Lp1.Content;
                test_Lp2.Content = tpy2_Lp2.Content;
                test_Lp3.Content = tpy2_Lp3.Content;
                test_Lp4.Content = tpy2_Lp4.Content;
                test_Rp1.Content = tpy2_Rp1.Content;
                test_Rp2.Content = tpy2_Rp2.Content;
                test_Rp3.Content = tpy2_Rp3.Content;
                test_Rp4.Content = tpy2_Rp4.Content;
                test_cnt1.SelectedIndex = tpy2_bst1_cnt.SelectedIndex;
                test_cnt2.SelectedIndex = tpy2_bst2_cnt.SelectedIndex;
                test_duration1.SelectedIndex = tpy2_bst1_duration.SelectedIndex;
                test_duration2.SelectedIndex = tpy2_bst2_duration.SelectedIndex;
                test_fcy1.SelectedIndex = tpy2_bst1_fcy.SelectedIndex;
                test_fcy2.SelectedIndex = tpy2_bst2_fcy.SelectedIndex;
                test_pse1.SelectedIndex = tpy2_bst1_pse.SelectedIndex;
                test_pse2.SelectedIndex = tpy2_bst2_pse.SelectedIndex;

                test_burst1.Content = "Enable";
                test_burst2.Content = "Enable";

                therapy_num = 1;

                MainTab.SelectedIndex = 5;
            }
        }

        private void Button_Click_26(object sender, RoutedEventArgs e)
        {
            if (!checkDataisEmpty(2))
            {
                test_Lp1.Content = tpy3_Lp1.Content;
                test_Lp2.Content = tpy3_Lp2.Content;
                test_Lp3.Content = tpy3_Lp3.Content;
                test_Lp4.Content = tpy3_Lp4.Content;
                test_Rp1.Content = tpy3_Rp1.Content;
                test_Rp2.Content = tpy3_Rp2.Content;
                test_Rp3.Content = tpy3_Rp3.Content;
                test_Rp4.Content = tpy3_Rp4.Content;
                test_cnt1.SelectedIndex = tpy3_bst1_cnt.SelectedIndex;
                test_cnt2.SelectedIndex = tpy3_bst2_cnt.SelectedIndex;
                test_duration1.SelectedIndex = tpy3_bst1_duration.SelectedIndex;
                test_duration2.SelectedIndex = tpy3_bst2_duration.SelectedIndex;
                test_fcy1.SelectedIndex = tpy3_bst1_fcy.SelectedIndex;
                test_fcy2.SelectedIndex = tpy3_bst2_fcy.SelectedIndex;
                test_pse1.SelectedIndex = tpy3_bst1_pse.SelectedIndex;
                test_pse2.SelectedIndex = tpy3_bst2_pse.SelectedIndex;

                test_burst1.Content = "Enable";
                test_burst2.Content = "Enable";

                therapy_num = 2;

                MainTab.SelectedIndex = 5;
            }
        }

        private void Button_Click_27(object sender, RoutedEventArgs e)
        {
            if (!checkDataisEmpty(3))
            {
                test_Lp1.Content = tpy4_Lp1.Content;
                test_Lp2.Content = tpy4_Lp2.Content;
                test_Lp3.Content = tpy4_Lp3.Content;
                test_Lp4.Content = tpy4_Lp4.Content;
                test_Rp1.Content = tpy4_Rp1.Content;
                test_Rp2.Content = tpy4_Rp2.Content;
                test_Rp3.Content = tpy4_Rp3.Content;
                test_Rp4.Content = tpy4_Rp4.Content;
                test_cnt1.SelectedIndex = tpy4_bst1_cnt.SelectedIndex;
                test_cnt2.SelectedIndex = tpy4_bst2_cnt.SelectedIndex;
                test_duration1.SelectedIndex = tpy4_bst1_duration.SelectedIndex;
                test_duration2.SelectedIndex = tpy4_bst2_duration.SelectedIndex;
                test_fcy1.SelectedIndex = tpy4_bst1_fcy.SelectedIndex;
                test_fcy2.SelectedIndex = tpy4_bst2_fcy.SelectedIndex;
                test_pse1.SelectedIndex = tpy4_bst1_pse.SelectedIndex;
                test_pse2.SelectedIndex = tpy4_bst2_pse.SelectedIndex;

                test_burst1.Content = "Enable";
                test_burst2.Content = "Enable";

                therapy_num = 3;

                MainTab.SelectedIndex = 5;
            }
        }

        private void Button_Click_28(object sender, RoutedEventArgs e)
        {
            if (!checkDataisEmpty(4))
            {
                test_Lp1.Content = tpy5_Lp1.Content;
                test_Lp2.Content = tpy5_Lp2.Content;
                test_Lp3.Content = tpy5_Lp3.Content;
                test_Lp4.Content = tpy5_Lp4.Content;
                test_Rp1.Content = tpy5_Rp1.Content;
                test_Rp2.Content = tpy5_Rp2.Content;
                test_Rp3.Content = tpy5_Rp3.Content;
                test_Rp4.Content = tpy5_Rp4.Content;
                test_cnt1.SelectedIndex = tpy5_bst1_cnt.SelectedIndex;
                test_cnt2.SelectedIndex = tpy5_bst2_cnt.SelectedIndex;
                test_duration1.SelectedIndex = tpy5_bst1_duration.SelectedIndex;
                test_duration2.SelectedIndex = tpy5_bst2_duration.SelectedIndex;
                test_fcy1.SelectedIndex = tpy5_bst1_fcy.SelectedIndex;
                test_fcy2.SelectedIndex = tpy5_bst2_fcy.SelectedIndex;
                test_pse1.SelectedIndex = tpy5_bst1_pse.SelectedIndex;
                test_pse2.SelectedIndex = tpy5_bst2_pse.SelectedIndex;

                test_burst1.Content = "Enable";
                test_burst2.Content = "Enable";

                therapy_num = 4;

                MainTab.SelectedIndex = 5;

            }
        }

        private void Button_Click_29(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button.Content.Equals("Enable"))
            {
                test_cnt1.IsEnabled = false;
                test_duration1.IsEnabled = false;
                test_fcy1.IsEnabled = false;
                test_pse1.IsEnabled = false;
                button.Content = "Disable";
            }
            else if(button.Content.Equals("Disable"))
            {
                test_cnt1.IsEnabled = true;
                test_duration1.IsEnabled = true;
                test_fcy1.IsEnabled = true;
                test_pse1.IsEnabled = true;
                button.Content = "Enable";
            }

        }

        private void Button_Click_30(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button.Content.Equals("Disable"))
            {
                test_cnt2.IsEnabled = true;
                test_duration2.IsEnabled = true;
                test_fcy2.IsEnabled = true;
                test_pse2.IsEnabled = true;
                button.Content = "Enable";
            }         
        }

        private void Button_Click_31(object sender, RoutedEventArgs e)
        {
            if (!checkDataisEmpty(5))
            {
                test_cnt1.IsEnabled = false;
                test_duration1.IsEnabled = false;
                test_fcy1.IsEnabled = false;
                test_pse1.IsEnabled = false;
                test_cnt2.IsEnabled = false;
                test_duration2.IsEnabled = false;
                test_fcy2.IsEnabled = false;
                test_pse2.IsEnabled = false;

                test_burst1.Content = "Enable";
                test_burst2.Content = "Enable";
            }
        }

        private void Button_Click_32(object sender, RoutedEventArgs e)
        {
            if (therapy_num == -1)
            {
                return;
            }

            Cancelchange();
        }

        private void Cancelchange()
        {
            ComboBox[,] cnt = new ComboBox[6, 2] { { tpy1_bst1_cnt,tpy1_bst2_cnt},{ tpy2_bst1_cnt,tpy2_bst2_cnt}
                ,{tpy3_bst1_cnt,tpy3_bst2_cnt },{tpy4_bst1_cnt,tpy4_bst2_cnt },{tpy5_bst1_cnt,tpy5_bst2_cnt }
            ,{ test_cnt1,test_cnt2} };
            ComboBox[,] duration = new ComboBox[6, 2] { { tpy1_bst1_duration,tpy1_bst2_duration},{ tpy2_bst1_duration,tpy2_bst2_duration}
                ,{tpy3_bst1_duration,tpy3_bst2_duration },{tpy4_bst1_duration,tpy4_bst2_duration },{tpy5_bst1_duration,tpy5_bst2_duration }
            ,{ test_duration1,test_duration2} };
            ComboBox[,] fcy = new ComboBox[6, 2] { { tpy1_bst1_fcy,tpy1_bst2_fcy},{ tpy2_bst1_fcy,tpy2_bst2_fcy}
                ,{tpy3_bst1_fcy,tpy3_bst2_fcy },{tpy4_bst1_fcy,tpy4_bst2_fcy },{tpy5_bst1_fcy,tpy5_bst2_fcy }
            ,{ test_fcy1,test_fcy2} };
            ComboBox[,] pse = new ComboBox[6, 2] { { tpy1_bst1_pse,tpy1_bst2_pse},{ tpy2_bst1_pse,tpy2_bst2_pse}
                ,{tpy3_bst1_pse,tpy3_bst2_pse },{tpy4_bst1_pse,tpy4_bst2_pse },{tpy5_bst1_pse,tpy5_bst2_pse }
            ,{ test_pse1,test_pse2} };

            cnt[5, 0].SelectedIndex = cnt[therapy_num, 0].SelectedIndex;
            cnt[5, 1].SelectedIndex = cnt[therapy_num, 1].SelectedIndex;
            duration[5, 0].SelectedIndex = duration[therapy_num, 0].SelectedIndex;
            duration[5, 1].SelectedIndex = duration[therapy_num, 1].SelectedIndex;
            fcy[5, 0].SelectedIndex = fcy[therapy_num, 0].SelectedIndex;
            fcy[5, 1].SelectedIndex = fcy[therapy_num, 1].SelectedIndex;
            pse[5, 0].SelectedIndex = pse[therapy_num, 0].SelectedIndex;
            pse[5, 1].SelectedIndex = pse[therapy_num, 1].SelectedIndex;

            cnt[5, 0].IsEnabled = false;
            cnt[5, 1].IsEnabled = false;
            duration[5, 0].IsEnabled = false;
            duration[5, 1].IsEnabled = false;
            fcy[5, 0].IsEnabled = false;
            fcy[5, 1].IsEnabled = false;
            pse[5, 0].IsEnabled = false;
            pse[5, 1].IsEnabled = false;

            test_burst1.Content = "Enable";
            test_burst2.Content = "Enable";

        }

        private void FDChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SD_removed_str != "")
            {
                SDChannel.Items.Add(SD_removed_str);              
            }

            if (FDChannel.SelectedItem != null)
            {
                SD_removed_str = FDChannel.SelectedItem.ToString();
                SDChannel.Items.Remove(SD_removed_str);
            }            

            SetFDenable(true);
        }

        private void SetFDenable(Boolean enable)
        {
            PAFarea.IsEnabled = enable;
            PAFbp.IsEnabled = enable;
            PAFll.IsEnabled = enable;
            PBFarea.IsEnabled = enable;
            PBFbp.IsEnabled = enable;
            PBFll.IsEnabled = enable;
        }

        private void SDChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FD_removed_str != "")
            {
                FDChannel.Items.Add(FD_removed_str);             
            }

            if (SDChannel.SelectedItem != null)
            {
                FD_removed_str = SDChannel.SelectedItem.ToString();
                FDChannel.Items.Remove(FD_removed_str);
            }           

            SetSDenable(true);
        }

        private void SetSDenable(Boolean enable)
        {
            PASarea.IsEnabled = enable;
            PASbp.IsEnabled = enable;
            PASll.IsEnabled = enable;
            PBSarea.IsEnabled = enable;
            PBSbp.IsEnabled = enable;
            PBSll.IsEnabled = enable;
        }

        private void Button_Click_33(object sender, RoutedEventArgs e)
        {
            //this.bitmap_ch1.SetRate(2.0f);        

            if (CurState == Type.ConnectState.STATE_CONNECT)
            {
                ReqState = Type.ReqState.SEND_REQ_FLASHEEG;
            }
        }

        private void Button_Click_34(object sender, RoutedEventArgs e)
        {
            this.bitmap_ch1.SetRate(0.5f);
        }

        private void Button_Click_35(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button.Content.Equals("+"))
            {
                row1.Height = new GridLength(0, GridUnitType.Star);
                row2.Height = new GridLength(0, GridUnitType.Star);
                button.Content = "-";
            }
            else
            {
                row1.Height = new GridLength(21, GridUnitType.Star);
                row2.Height = new GridLength(3, GridUnitType.Star);
                button.Content = "+";
            }
        }

        private void test_fcy1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            String str = comboBox.SelectedValue.ToString();

            String[] sArray = str.Split(' ');

            int value = Convert.ToInt32(sArray[0]);

            int num = (1000000 / (value * 2));

            int num_size = num / 5;

            test_pse1.Items.Clear();

            for (int i = 1; i < 5; i++)
            {
                test_pse1.Items.Add(i*num_size+" μs");
            }
        }

        private void Button_Click_36(object sender, RoutedEventArgs e)
        {
            Eventtimer.Start();
        }

        private void Button_Click_37(object sender, RoutedEventArgs e)
        {
            /*  Button button = sender as Button;

              if (button.Content.Equals("Discharging"))
              {
                  button.Content = "Charging";
              }
              else if (button.Content.Equals("Charging"))
              {
                  button.Content = "Discharging";
              }

              byte[] data = new byte[1];
              data[0] = 1;
              byte[] appdata = GetAppFormat(data, Type.ASK_DATA_REQUEST);
              byte[] tsdata = GetTransportsFormat(appdata, 4, 4);
              byte[] intdata = GetIntworkFormat(tsdata, 0, 1);
              byte[] dldata = GetDataLinkFormat(intdata, false);
              SendData(dldata);*/

            byte[] data = new byte[1];
            data[0] = 1;

            byte []senddata = PackData(data, Type.Request.REQ_CHARGE);
           
            RequestData[(int)Type.Request.REQ_CHARGE] = senddata;         

            SynchroSendData(Type.Request.REQ_CHARGE, senddata);

            Charge_btn.IsEnabled = false;
        }

        private void SynchroSendData(Type.Request request,byte[] senddata)
        {
            lock (obj)
            {
                if (!isLive)
                {

                    SendTable[(int)request] = 1;

                    DataSendTimer.Start();

                }
                else if (isLive)
                {
                    SendData(senddata);

                    PreReqIndex = request;

                    isLive = false;
                }
            }
        }




        
        private void Stimulate()
        {
            int curr1, frequency1, pwper1, duration1;

            //刺激参数取默认值
            curr1 = 1;
            frequency1 = 20;
            pwper1 = 1000;
            duration1 = 500;
            //flag1 = 1;

            //将刺激参数存储为17个字节
            byte[] newdata = new byte[17];
            newdata[0] = 0xAA;                          //首字节
            newdata[1] = 1;                             //数据为刺激标志      
            newdata[2] = (byte)frequency1;               //刺激参数
            newdata[3] = (byte)(frequency1 >> 8);
            newdata[4] = (byte)(frequency1 >> 16);
            newdata[5] = (byte)(frequency1 >> 24);
            newdata[6] = (byte)pwper1;
            newdata[7] = (byte)(pwper1 >> 8);
            newdata[8] = (byte)(pwper1 >> 16);
            newdata[9] = (byte)(pwper1 >> 24);
            newdata[10] = (byte)duration1;
            newdata[11] = (byte)(duration1 >> 8);
            newdata[12] = (byte)(duration1 >> 16);
            newdata[13] = (byte)(duration1 >> 24);
            newdata[14] = (byte)curr1;
            newdata[15] = 0;                           //第newdata[1]到newdata[14]位的和，用作和校验，接下来赋值
            newdata[16] = 0xBB;

            for (int i = 1; i < 15; i++)
            {
                newdata[15] += newdata[i];
            }

            byte[] data = new byte[31];               //将newdata[2]到newdata[15]的数据高4位和低4位分别保存在一个字节中
            data[0] = newdata[0];
            data[1] = newdata[1];
            for (int i = 2; i < 16; i++)
            {
                byte num = newdata[i];
                data[(i - 1) * 2] = (byte)(0x30 | (num >> 4));
                data[(i - 1) * 2 + 1] = (byte)(0x30 | (0x0F & num));
            }
            data[30] = newdata[16];

            ComDevice.Write(data, 0, data.Length);      //data写入串口
        }

        private void flag_detect(object sender, EventArgs e)
        {
            if (flag != 0)
            {
                Stimulate();
                MessageBox.Show("发出刺激！");
                flag = 0;
            }
        }


    }



}
