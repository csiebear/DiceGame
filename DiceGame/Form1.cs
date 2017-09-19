using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiceGame
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        int Run_Time_Num = 0;
        int Final_TH_Num = 0;
        int DiceTotal = 0;
        int[] WIP_Before_Workstation = new int[] { 0, 0, 0, 0, 0 };
        int[] TempDiceVal = new int[] { 0, 0, 0, 0, 0 };
        double[] AvaliableCap = new double[] { 0.0, 0.0, 0.0, 0.0, 0.0 };//骰子骰出的點數
        double[] TrueCap = new double[] { 0.0, 0.0, 0.0, 0.0, 0.0};//實際產出(會因為與前站依存關係而有所波動)
     
        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Run_Time_Num++;
            Run_Time.Text = Run_Time_Num.ToString();
            Final_TH.Text = Final_TH_Num.ToString();
            for (int count = 0;count < 5 ;count++)
            {
                TempDiceVal[count] = Dice();
                DiceTotal= DiceTotal+TempDiceVal[count];
            }
            //由於假設無限供應原料，所以首站(Process_A)的點數與產出相同的
            //而當站完成加工之後，當站的數量直接加入站後的在製品(WIP)
            DiceNum_A.Text = "本期點數:"+ TempDiceVal[0].ToString();
            TH1.Text= "本期產出:"+TempDiceVal[0].ToString();
            TrueCap[0] = TrueCap[0] + TempDiceVal[0];
            REAL_TH1.Text = "實際產出:" + (TrueCap[0] / Run_Time_Num).ToString("0.0");
            AvaliableCap[0] = AvaliableCap[0]+ TempDiceVal[0];
            AVA_TH1.Text= "可用產能:" + (AvaliableCap[0] / Run_Time_Num).ToString("0.0");
            WIP_Before_Workstation[0] = WIP_Before_Workstation[0] + TempDiceVal[0];
            //將骰子點數率先更新
            DiceNum_B.Text = "本期點數:" + TempDiceVal[1].ToString();
            AvaliableCap[1] = AvaliableCap[1] + TempDiceVal[1];
            DiceNum_C.Text = "本期點數:" + TempDiceVal[2].ToString();
            AvaliableCap[2] = AvaliableCap[2] + TempDiceVal[2];
            DiceNum_D.Text = "本期點數:" + TempDiceVal[3].ToString();
            AvaliableCap[3] = AvaliableCap[3] + TempDiceVal[3];
            DiceNum_E.Text = "本期點數:" + TempDiceVal[4].ToString();
            AvaliableCap[4] = AvaliableCap[4] + TempDiceVal[4];
            FollowStation();
            AVG_Label.Text=Average(DiceTotal, Run_Time_Num).ToString("0.0");
        }
        private int Dice()
        {
            //避免重複性
            //http://mogerwu.pixnet.net/blog/post/28856448-c%23%E4%BA%82%E6%95%B8%E7%94%A2%E7%94%9F
            Random rndnum = new Random(Guid.NewGuid().GetHashCode());
            //random.Next方法解說https://msdn.microsoft.com/zh-tw/library/2dx6wyd4(v=vs.110).aspx
            //會回傳大於等於最小值 和 "小於"最大值，所以此處最小值是1最大值是7這樣就會產出1~6的亂數
            int rnd_result = rndnum.Next(1,7);
            return rnd_result;
        }

        private void FollowStation()
        {
            Label[] TH_Labels = new Label[] { this.TH1, this.TH2, this.TH3, this.TH4, this.TH5 };
            Label[] WIP_Labels = new Label[] { this.WIP1, this.WIP2, this.WIP3, this.WIP4 };
            Label[] AVATH_Labels = new Label[] { this.AVA_TH2, this.AVA_TH3, this.AVA_TH4, this.AVA_TH5 };
            Label[] REALTH_Labels = new Label[] { this.REAL_TH2, this.REAL_TH3, this.REAL_TH4, this.REAL_TH5 };
            for (int j = 1; j <= 4; j++) { 
                //後續幾站都要根據前一站產出的在製品來判斷其產出
                if (TempDiceVal[j] >= WIP_Before_Workstation[j-1])
                {
                    TempDiceVal[j] = WIP_Before_Workstation[j-1];
                    TrueCap[j] = TrueCap[j]+TempDiceVal[j];
                    WIP_Before_Workstation[j-1] = WIP_Before_Workstation[j-1] - WIP_Before_Workstation[j-1];
                }
                else
                {
                    TrueCap[j] = TrueCap[j]+TempDiceVal[j];
                    WIP_Before_Workstation[j-1] = WIP_Before_Workstation[j-1] - TempDiceVal[j];
                }
                //產出後更新工作站後的在製品數量
                WIP_Before_Workstation[j] = WIP_Before_Workstation[j]+TempDiceVal[j];
                TH_Labels[j].Text = "本期產出:" + TempDiceVal[j].ToString();
                
                AVATH_Labels[j - 1].Text = "可用產能:" + (AvaliableCap[j] / Run_Time_Num).ToString("0.0");
                REALTH_Labels[j - 1].Text = "實際產出:" + (TrueCap[j] / Run_Time_Num).ToString("0.0");
                WIP_Labels[j-1].Text = WIP_Before_Workstation[j-1].ToString();
                if (j == 4)
                {
                    //4工作站都生產完畢之後，進行期末的盤點(將當期成品存貨更新到上方)
                    Final_TH_Num = WIP_Before_Workstation[j];
                    Final_TH.Text = Final_TH_Num.ToString();
                }
            }//end for
            Result_Label.Text = (100 *(double)Final_TH_Num/Run_Time_Num).ToString("0.0");
        }

        private void RestartBtn_Click(object sender, EventArgs e)
        {
            AvaliableCap =new double[] { 0.0, 0.0, 0.0, 0.0, 0.0 };
            TrueCap = new double[] { 0.0, 0.0, 0.0, 0.0, 0.0 };
            //Reset all control
            Label[] Dice_Labels = new Label[] { this.DiceNum_A, this.DiceNum_B, this.DiceNum_C, this.DiceNum_D, this.DiceNum_E };
            Label[] TH_Labels = new Label[] { this.TH1, this.TH2, this.TH3, this.TH4, this.TH5 };
            Label[] WIP_Labels = new Label[] { this.WIP1, this.WIP2, this.WIP3, this.WIP4 };
            Label[] AVATH_Labels = new Label[] { this.AVA_TH1, this.AVA_TH2, this.AVA_TH3, this.AVA_TH4, this.AVA_TH5 };
            Label[] REALTH_Labels = new Label[] { this.REAL_TH1, this.REAL_TH2, this.REAL_TH3, this.REAL_TH4, this.REAL_TH5 };
            Run_Time_Num = 0;
            Run_Time.Text = Run_Time_Num.ToString();
            Final_TH_Num = 0;
            Final_TH.Text = Final_TH_Num.ToString();
            WIP_Before_Workstation = new int[] { 0, 0, 0, 0, 0};
            TempDiceVal = new int[] { 0, 0, 0, 0, 0};
            DiceTotal = 0;
            AVG_Label.Text = "0";
            Result_Label.Text = "0.0";
            for (int k = 0; k < 5; k++) {
                Dice_Labels[k].Text = "本期點數";
                TH_Labels[k].Text = "本期產出:";
                if(k!=4)
                    WIP_Labels[k].Text = 0.ToString();
                AVATH_Labels[k].Text = "可用產能:";
                REALTH_Labels[k].Text = "實際產出:";
            }
        }

        private float Average(int Cap, int Times)
        {
            float CapAverage = (float)Cap / Times/5;
            return CapAverage;
        }

        private void Dice34_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int times = 0; times < 100; times++)
            {
                Run_Time_Num++;
                Run_Time.Text = Run_Time_Num.ToString();
                Final_TH.Text = Final_TH_Num.ToString();
                for (int count = 0; count < 5; count++)
                {
                    TempDiceVal[count] = Dice();
                    DiceTotal = DiceTotal + TempDiceVal[count];
                }
                //由於假設無限供應原料，所以首站(Process_A)的點數與產出相同的
                //而當站完成加工之後，當站的數量直接加入站後的在製品(WIP)
                DiceNum_A.Text = "本期點數:" + TempDiceVal[0].ToString();
                TH1.Text = "本期產出:" + TempDiceVal[0].ToString();
                TrueCap[0] = TrueCap[0] + TempDiceVal[0];
                REAL_TH1.Text = "實際產出:" + (TrueCap[0] / Run_Time_Num).ToString("0.0");
                AvaliableCap[0] = AvaliableCap[0] + TempDiceVal[0];
                AVA_TH1.Text = "可用產能:" + (AvaliableCap[0] / Run_Time_Num).ToString("0.0");
                WIP_Before_Workstation[0] = WIP_Before_Workstation[0] + TempDiceVal[0];
                //將骰子點數率先更新
                DiceNum_B.Text = "本期點數:" + TempDiceVal[1].ToString();
                AvaliableCap[1] = AvaliableCap[1] + TempDiceVal[1];
                DiceNum_C.Text = "本期點數:" + TempDiceVal[2].ToString();
                AvaliableCap[2] = AvaliableCap[2] + TempDiceVal[2];
                DiceNum_D.Text = "本期點數:" + TempDiceVal[3].ToString();
                AvaliableCap[3] = AvaliableCap[3] + TempDiceVal[3];
                DiceNum_E.Text = "本期點數:" + TempDiceVal[4].ToString();
                AvaliableCap[4] = AvaliableCap[4] + TempDiceVal[4];
                FollowStation();
                AVG_Label.Text = Average(DiceTotal, Run_Time_Num).ToString("0.0");
            }
        }
    }
}
