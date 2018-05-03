using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Xml;

namespace Epilepsy
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer ShowTimer;
       // private int[,] Lbtn_num = new int[4,4];
        private Button[,] Lbtn_array = new Button[4, 4];
       // private int[,] Rbtn_num = new int[4, 4];
        private Button[,] Rbtn_array = new Button[4, 4];
        private Button[] btn_gain = new Button[4];
        private String[] channal = new String[4];
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
        }

        private void setCmbData() {
            for (int i = 0; i < 3; i++) {
                mode.Items.Add(i);
                PA_frequency_min.Items.Add(i);
                PA_amplitude.Items.Add(i);
                PA_frequency_max.Items.Add(i);
                PA_duration.Items.Add(i);
                PA_trend_short.Items.Add(i);
                PA_trend_long.Items.Add(i);
                PA_threshold.Items.Add(i);
                PB_frequency_min.Items.Add(i);
                PB_amplitude.Items.Add(i);
                PB_frequency_max.Items.Add(i);
                PB_duration.Items.Add(i);
                PB_trend_short.Items.Add(i);
                PB_trend_long.Items.Add(i);
                PB_threshold.Items.Add(i);
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
            this.date.Text = DateTime.Now.DayOfWeek.ToString()+" "+DateTime.Now.ToString("yyyy-MM-dd");   //yyyy年MM月dd日
            //获得时分秒
            this.Tt.Text = DateTime.Now.ToString("HH:mm:ss");
            this.time.Text = date.Text +" "+Tt.Text;
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
                    button.SetValue(Grid.RowProperty,i);
                    button.SetValue(Grid.ColumnProperty,j);
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
            for (int i = 1900; i < Now_year+1; i++)
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
            int index_x=0;
            for (int i = 0; i < 4; i++)
            {
                if (button == Lbtn_array[index_y, i])
                {
                    index_x = i;
                }
            }
            Boolean isCanadd = Col_isCanAdd(index_y,index_x, Lbtn_array);
            Boolean isCansub = Col_isCanSub(index_y, index_x, Lbtn_array);
            if ((isCanadd && !isCanEnable_Add(Lbtn_array))||(isCansub && !isCanEnable_Sub(Lbtn_array)))
                return;
            if (button.Content.Equals("0")&&isCanadd) {
                button.Content = "+";                
                for (int i = 0; i < 4; i++) {
                    if (i!=index_x) {
                        Lbtn_array[index_y, i].IsEnabled = false;
                    }
                    if (!isCansub && Lbtn_array[i, index_x].Content.Equals("0")) {
                        Lbtn_array[i, index_x].IsEnabled = false;
                    }
                    Rbtn_array[i,index_x].IsEnabled = false;
                }
                setHipText("+",index_y,"L");
                getDChannels();
            }
            else if (!isCanadd&& button.Content.Equals("0"))
            {
                button.Content = "-";
                for (int i = 0; i < 4; i++)
                {
                    if (i != index_x)
                    {
                        Lbtn_array[index_y, i].IsEnabled = false;
                    }
                    if (Lbtn_array[i,index_x].Content.Equals("0")) {
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

        private void setHipText(String txt,int index,String str) {
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
                else if(index==3)
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
            if ((Col_isCanSub(y, x, button) || Col_isCanAdd(y, x, button)) && Row_isCanEnable(y,x, button))
                button[y, x].IsEnabled = true;
        }

        private Boolean isCanEnable_Add(Button[,] button) {
            Boolean isCanEnable = true;
            int add_num = 0;
            for (int i = 0; i < 4; i++) {
                for(int j = 0; j < 4; j++)
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

        private Boolean Col_isCanAdd(int y,int x,Button[,] button) {
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
                if ( button[i, x].Content.Equals("-"))
                {
                    isCansub = false;
                    break;
                }
            }
            return isCansub;
        }

        private Boolean Row_isCanEnable(int y,int x, Button[,] button)
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
            if (select_mon==2&&DateTime.IsLeapYear(select_year))
            {
                getDay(29);
            }
            else if(select_mon == 2 && !DateTime.IsLeapYear(select_year))
            {
                getDay(28);
            }
        }

        private void getDay(int days) {
            day.Items.Clear();
            for (int i = 1; i < days+1; i++)
            {
                day.Items.Add(i);
            }
        }

        private void getDChannels() {
            FDChannel.Items.Clear();
            SDChannel.Items.Clear();
            Array.Clear(channal,0,4);
            Dictionary<int, String> dictionary = new Dictionary<int, String>();
            int L_add_y, L_add_x, L_sub_y, R_add_y, R_add_x, R_sub_y = 0;           
            for(int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    if (Lbtn_array[j, i].Content.Equals("+")) {
                        L_add_y = j;
                        L_add_x = i;
                        L_sub_y = getSubRow(L_add_x, Lbtn_array);
                        if (L_sub_y != -1) {                          
                            String str = "LHip" + (L_add_y+1) + "-LHip" + (L_sub_y+1);
                            dictionary.Add(L_add_x++, str);
                            channal[i] = "L"+L_add_y + "" + L_sub_y;
                        }
                    }
                    if (Rbtn_array[j, i].Content.Equals("+"))
                    {
                        R_add_y = j;
                        R_add_x = i;
                        R_sub_y = getSubRow(R_add_x, Rbtn_array);
                        if (R_sub_y != -1)
                        {
                            String str = "RHip" + (R_add_y+1) + "-RHip" + (R_sub_y+1);
                            dictionary.Add(R_add_x, str);
                            channal[i] = "R"+R_add_y + "" + R_sub_y;
                        }
                    }
                }
            }           
            for (int i = 0; i < 4; i++) {
                if (dictionary.ContainsKey(i)) {
                    int num = i+1;
                    FDChannel.Items.Add(num +"  "+dictionary[i]);
                    SDChannel.Items.Add(num + "  " + dictionary[i]);
                    setECoGChannels(num,dictionary[i]);
                    setSettingChannels(num,dictionary[i]);                   
                }
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

        private void setSettingChannels(int i,String str) {
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

        private void setECoGChannels(int i,String str) {
            if (i == 1) {
                CH1_name.Text = str;
                CH1_btn.IsEnabled = true;
            }
            else if (i==2) {
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

        private int getSubRow(int x,Button[,] button) {
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
                list_detector.Items.Add(new Detection { date=element.GetAttribute("time"),detection=element.Name});
            }
            
        }
     

        private void PAFbp_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("Off"))
            {
                button.Content = "On";
                PA_tabbp_grid.Visibility = Visibility.Visible;
                PA_tabbp_header.Visibility = Visibility.Visible;
                setPASenable(false);              
            }
            else if (button.Content.Equals("On")) {
                button.Content = "Off";
                PA_tabbp_grid.Visibility = Visibility.Collapsed;
                PA_tabbp_header.Visibility = Visibility.Collapsed;
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
                PA_tabll_grid.Visibility = Visibility.Visible;
                PA_tabll_header.Visibility = Visibility.Visible;
                setPASenable(false);
            }
            else if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                PA_tabll_grid.Visibility = Visibility.Collapsed;
                PA_tabll_header.Visibility = Visibility.Collapsed;
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
                PA_tabbp_grid.Visibility = Visibility.Visible;
                PA_tabbp_header.Visibility = Visibility.Visible;
                setPAFenable(false);
            }
            else if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                PA_tabbp_grid.Visibility = Visibility.Collapsed;
                PA_tabbp_header.Visibility = Visibility.Collapsed;
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
                PA_tabll_grid.Visibility = Visibility.Visible;
                PA_tabll_header.Visibility = Visibility.Visible;
                setPAFenable(false);
            }
            else if (button.Content.Equals("On"))
            {
                button.Content = "Off";
                PA_tabll_grid.Visibility = Visibility.Collapsed;
                PA_tabll_header.Visibility = Visibility.Collapsed;
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
            int cmb1_num, cmb2_num, cmb3_num, cmb4_num, cmb5_num, cmb6_num, cmb7_num, cmb8_num ;
            cmb1_num = cmb1.SelectedIndex;
            cmb2_num = cmb2.SelectedIndex;
            cmb3_num = cmb3.SelectedIndex;
            cmb4_num = cmb4.SelectedIndex;
            cmb5_num = cmb5.SelectedIndex;
            cmb6_num = cmb6.SelectedIndex;
            cmb7_num = cmb7.SelectedIndex;
            cmb8_num = cmb8.SelectedIndex;
            txt_num.Text = Convert.ToString(cmb1_num+cmb2_num + cmb3_num + cmb4_num + cmb5_num + cmb6_num + cmb7_num + cmb8_num);
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
            try {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("myXML.xml");
                XmlNode root = xmlDoc.SelectSingleNode("Root");
                if (root != null)
                {
                    XmlElement element = xmlDoc.CreateElement(current.Text);
                    element.SetAttribute("time",time.Text);
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
            catch (Exception  str) {
                MessageBox.Show("读写文件出错" + str.ToString());
            }
            
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {           
           
        }

        private void CheckPattern() {
            if (PAFbp.Content.Equals("On") || PASbp.Content.Equals("On")) {
                PA_tabbp_grid.Visibility = Visibility.Visible;
                PA_tabbp_header.Visibility = Visibility.Visible;
            }
            if (PASll.Content.Equals("On") || PAFll.Content.Equals("On"))
            {
                PA_tabll_grid.Visibility = Visibility.Visible;
                PA_tabll_header.Visibility = Visibility.Visible;
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

        private void setChannelbtn(String str,int num) {
            Char[] ch = str.ToCharArray();
            int add_y = Convert.ToInt32(ch[1])-48;
            int sub_y = Convert.ToInt32(ch[2])-48;
            if (ch[0] == 'L')
            {
                Lbtn_array[add_y, num].IsEnabled = true;
                Lbtn_array[add_y, num].Content = "+";
                Lbtn_array[sub_y, num].IsEnabled = true;
                Lbtn_array[sub_y, num].Content = "-";
                setHipText("+", add_y, "L");
                setHipText("-",sub_y,"L");
                num++;
                add_y++;
                sub_y++;
                String item = "LHip" + add_y + "-LHip" + sub_y;
                setECoGChannels(num, item);
                setSettingChannels(num, item);
                FDChannel.Items.Add(num+"  " + item);
                SDChannel.Items.Add(num + "  "+item);
            }
            else if (ch[0] == 'R') {
                Rbtn_array[add_y, num].IsEnabled = true;
                Rbtn_array[add_y, num].Content = "+";
                Rbtn_array[sub_y, num].IsEnabled = true;
                Rbtn_array[sub_y, num].Content = "-";
                setHipText("+",add_y,"R");
                setHipText("-",sub_y,"R");
                num++;
                add_y++;
                sub_y++;
                String item =  "RHip" + add_y + "-RHip" + sub_y;
                setECoGChannels(num,item);
                setSettingChannels(num, item);
                FDChannel.Items.Add(num +"  " +item);
                SDChannel.Items.Add(num + "  "+item);
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
                else
                    setPAFenable(false);
                if (isPBFSelected())
                    setPBSenable(false);
                else
                    setPBFenable(false);
                CheckPattern();
            }
            catch (Exception str) {
                MessageBox.Show("打开文件错误"+str.ToString());
            }
        }
    }
   
}
