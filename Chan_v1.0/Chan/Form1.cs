using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Configuration;

namespace Chan
{
    public partial class Form1 : Form
    {
        private Pen grayPen = new Pen(Color.Gray, 1);
        private Pen lgrayPen = new Pen(Color.LightGray);
        private Pen bluePen = new Pen(Color.Blue, 1);
        private Pen lbluePen = new Pen(Color.LightBlue, 1);
        private Pen redPen = new Pen(Color.Red, 1);
        private Pen greenPen = new Pen(Color.Green, 1);
        private Pen magentaPen = new Pen(Color.Magenta, 1);

        private Int32 iKGrade;
        private Int32 iGraphiceGrade = 2;
        
        private KData kData;
        private Point[] PaintPointForK;
        private Point[] PaintPointForLine;
        private KBoxPoint[] PaintPointForBox;
        private MACDPoint[] PaintPointForMACD;

        private string strStockNumber;
        private string strDataFilePath;
        private string[] BlockStockNumbersArray;
        private string[] StockListArray;

        public Form1()
        {
            InitializeComponent();
            
            label2.Text = "";

            this.strDataFilePath = Properties.Settings.Default.OriginalDataPath;
            this.strStockNumber = Properties.Settings.Default.InitStockNumber;
            this.iKGrade = Convert.ToInt32(Properties.Settings.Default.KGrade);

            this.textBox1.KeyDown += new KeyEventHandler(this.OK);
            this.textBox1.KeyDown += new KeyEventHandler(this.ChangeGraphicGrade);
            this.textBox1.KeyDown += new KeyEventHandler(this.ChangeKGrade);
            this.textBox1.KeyDown += new KeyEventHandler(this.ChangeStock);

            //读取股票名列表
            string strListFileFullName = System.Environment.CurrentDirectory + "\\StockList.txt";
            if (true == File.Exists(strListFileFullName))
            {
                FileStream fs = new FileStream(strListFileFullName, FileMode.Open);
                StreamReader sr = new StreamReader(fs, Encoding.Default);

                char[] separator = new char[2];
                separator[0] = '\r';
                separator[1] = '\n';
                string strTemp = sr.ReadToEnd();
                this.StockListArray = strTemp.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                fs.Close();
                sr.Close();
            }

           
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Int32 iCount = 0;
            Graphics dc = e.Graphics;

            Point LineStart = new Point(0, this.ClientSize.Height - 26);
            Point LineEnd = new Point(this.ClientSize.Width, this.ClientSize.Height - 26);
            dc.DrawLine(grayPen, LineStart, LineEnd);

            
            
            if (null == kData)
                    return;

            //画K线
            if (null == PaintPointForK)
                return;

            for (iCount = 0; iCount < PaintPointForK.Length - 1; iCount += 2)
            {
                    dc.DrawLine(redPen, PaintPointForK[iCount], PaintPointForK[iCount + 1]);
            }

            if (null == PaintPointForLine)
                return;

            //画笔
            dc.DrawLines(grayPen, PaintPointForLine);

            //画中枢
            for (iCount = 0; iCount < PaintPointForBox.Length; iCount++)
            {
                dc.DrawRectangle(lbluePen, PaintPointForBox[iCount].RangeStart.X, PaintPointForBox[iCount].RangeStart.Y, PaintPointForBox[iCount].RangeWidth, PaintPointForBox[iCount].RangeHeight);
                dc.DrawRectangle(bluePen, PaintPointForBox[iCount].IntervalStart.X, PaintPointForBox[iCount].IntervalStart.Y, PaintPointForBox[iCount].IntervalWidth, PaintPointForBox[iCount].IntervalHeight);
            }

            LineStart = new Point(0, this.ClientSize.Height - 151);
            LineEnd = new Point(this.ClientSize.Width, this.ClientSize.Height - 151);
            dc.DrawLine(grayPen, LineStart, LineEnd);

            if (null == PaintPointForMACD)
                return;

            LineStart = new Point(0, this.PaintPointForMACD[0].Zero.Y);
            LineEnd = new Point(this.ClientSize.Width, this.PaintPointForMACD[0].Zero.Y);
            dc.DrawLine(lgrayPen, LineStart, LineEnd);

            //画MACD
            for (iCount = 0; iCount < PaintPointForMACD.Length; iCount++)
            {
                if (PaintPointForMACD[iCount].MACD.Y < PaintPointForMACD[iCount].Zero.Y)
                    dc.DrawLine(redPen, PaintPointForMACD[iCount].MACD, PaintPointForMACD[iCount].Zero);
                if (PaintPointForMACD[iCount].MACD.Y > PaintPointForMACD[iCount].Zero.Y)
                    dc.DrawLine(greenPen, PaintPointForMACD[iCount].MACD, PaintPointForMACD[iCount].Zero);
            
            }

            Point[] TempPointArray = new Point[PaintPointForMACD.Length];
            //画DIF
            for (iCount = 0; iCount < PaintPointForMACD.Length; iCount++)
                TempPointArray[iCount] = PaintPointForMACD[iCount].DIF;
            dc.DrawLines(bluePen, TempPointArray);

            //画DEA
            for (iCount = 0; iCount < PaintPointForMACD.Length; iCount++)
                TempPointArray[iCount] = PaintPointForMACD[iCount].DEA;
            dc.DrawLines(magentaPen, TempPointArray);
        }

        private void RefreshWindow(object sender, System.EventArgs e)
        {
            //重绘控件
            textBox1.Location = new Point(2, this.ClientSize.Height - 23);
            button1.Location = new Point(128, this.ClientSize.Height - 23);
            button2.Location = new Point(173, this.ClientSize.Height - 23);

            UpdatePaint();

            this.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            kData = new KData(strDataFilePath, strStockNumber);
            UpdatePaint();
            
            this.Refresh();
        }



        private void UpdatePaint()
        {
            if (null == kData)
                return;

            if (null == kData.by1FDataFileBuffer && null == kData.by5FDataFileBuffer && null == kData.byDayDataFileBuffer)
                return;

            PaintPointForLine = kData.GetPaintLinePointArrary(Convert.ToInt32(this.ClientSize.Height - 151), Convert.ToInt32(this.ClientSize.Width), this.iGraphiceGrade, this.iKGrade);
            PaintPointForK = kData.GetPaintKPointArrary(Convert.ToInt32(this.ClientSize.Height - 151), Convert.ToInt32(this.ClientSize.Width), this.iGraphiceGrade, this.iKGrade);
            PaintPointForBox = kData.GetBoxPointArrary(Convert.ToInt32(this.ClientSize.Height - 151), Convert.ToInt32(this.ClientSize.Width), this.iGraphiceGrade, this.iKGrade);
            PaintPointForMACD = kData.GetMACDPointArrary(new Point(0, ClientSize.Height - 151), 120, Convert.ToInt32(this.ClientSize.Width), this.iGraphiceGrade, this.iKGrade);

            label2.Text = this.strStockNumber;

            Int32 iCount = 0;
            for (iCount = 0; iCount < this.StockListArray.Length - 1; iCount++)
            {
                if (-1 == this.StockListArray[iCount].IndexOf(this.strStockNumber))
                    continue;

                label2.Text = Convert.ToString(this.StockListArray[iCount]);
                break;
            }

            switch (this.iKGrade)
            {
                case 1:
                    label2.Text += " 1分钟";
                    break;
                case 5:
                    label2.Text += " 5分钟";
                    break;
                case 30:
                    label2.Text += " 30分钟";
                    break;
                case 240:
                    label2.Text += " 日";
                    break;
                default:
                    break;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            strStockNumber = textBox1.Text;
        }

        private void ChangeKGrade(object sender, KeyEventArgs e)
        {
            if (37 != e.KeyValue && 39 != e.KeyValue)
                return;

            Int32[] iKGradeArray = new Int32[4]{1, 5, 30, 240};
            Int32 iCount = 0;

            for (iCount = 0; iCount < 4; iCount++)
            {
                if (this.iKGrade == iKGradeArray[iCount])
                {
                    if (37 == e.KeyValue) //Left
                    {
                        if (0 != iCount)
                            this.iKGrade = iKGradeArray[iCount - 1];
                    }
                    else //Right
                    {
                        if (3 != iCount)
                            this.iKGrade = iKGradeArray[iCount + 1];
                    }

                    break;
                }
            }

            UpdatePaint();

            this.Refresh();
            
        }

        private void ChangeGraphicGrade(object sender, KeyEventArgs e)
        {
            if (38 != e.KeyValue && 40 != e.KeyValue)
                return;

            Int32 iCount = 0;

            for (iCount = 1; iCount <= 10; iCount++)
            {
                if (this.iGraphiceGrade == iCount)
                {
                    if (38 == e.KeyValue) //Up
                    {
                        if (10 != iCount)
                            this.iGraphiceGrade = iCount + 1;
                    }
                    else //Down
                    {
                        if (1 != iCount)
                            this.iGraphiceGrade = iCount - 1;
                    }

                    break;
                }
            }

            UpdatePaint();

            this.Refresh();

        }

        private void OK(object sender, KeyEventArgs e)
        {
            if (13 != e.KeyValue) //Enter
                return;

            kData = new KData(strDataFilePath, strStockNumber);
            UpdatePaint();

            this.Refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = Properties.Settings.Default.BlockFilePath;
            fileDialog.Title = "选择文件";
            fileDialog.Filter = "blk files (*.blk)|*.blk";
            fileDialog.FilterIndex = 1;
            fileDialog.RestoreDirectory = true;

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            Properties.Settings.Default.BlockFilePath = fileDialog.FileName;
            Properties.Settings.Default.Save();
            String fileName = fileDialog.SafeFileName;
            button2.Text = "板块 " + fileName;
            // 使用文件名
            FileStream fs = new FileStream(fileDialog.FileName, FileMode.Open);
            StreamReader sr = new StreamReader(fs);

            char[] separator = new char[2];
            separator[0] = '\r';
            separator[1] = '\n';
            string strTemp = sr.ReadToEnd();
            this.BlockStockNumbersArray = strTemp.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            this.strStockNumber = this.BlockStockNumbersArray[0].Substring(1);
            kData = new KData(strDataFilePath, strStockNumber);
            UpdatePaint();
            this.Refresh();

            fs.Close();
            sr.Close();
        }

        private void ChangeStock(object sender, KeyEventArgs e)
        {
            if (Keys.PageDown != e.KeyData && Keys.PageUp != e.KeyData)
                return;

            Int32 iCount = 0;

            if (null == BlockStockNumbersArray)
                return;

            if (Keys.PageDown == e.KeyData)
            {
                for (iCount = 0; iCount < BlockStockNumbersArray.Length; iCount++)
                {
                    if (0 != this.BlockStockNumbersArray[iCount].Substring(1).CompareTo(strStockNumber))
                        continue;

                    if (iCount != BlockStockNumbersArray.Length - 1)
                    {
                        this.strStockNumber = this.BlockStockNumbersArray[iCount + 1].Substring(1);
                        break;
                    }

                    this.strStockNumber = this.BlockStockNumbersArray[0].Substring(1);
                    break;
                }
            }

            if (Keys.PageUp == e.KeyData)
            {
                for (iCount = 0; iCount < BlockStockNumbersArray.Length; iCount++)
                {
                    if (0 != this.BlockStockNumbersArray[iCount].Substring(1).CompareTo(strStockNumber))
                        continue;

                    if (iCount != 0)
                    {
                        this.strStockNumber = this.BlockStockNumbersArray[iCount - 1].Substring(1);
                        break;
                    }

                    this.strStockNumber = this.BlockStockNumbersArray[BlockStockNumbersArray.Length-1].Substring(1);
                    break;
                }
            }

            kData = new KData(strDataFilePath, strStockNumber);
            UpdatePaint();

            this.Refresh();
        }
    }

    public class KData
    {
        public SingalKData[] Singal;

        public SingalKData[] Array1F;
        public SingalKData[] Array5F;
        public SingalKData[] Array30F;
        public SingalKData[] ArrayDay;

        public Byte[] by1FDataFileBuffer;
        public Byte[] by5FDataFileBuffer;
        public Byte[] byDayDataFileBuffer;
        public Byte[] byRealTimeFileBuffer;

        public KData(String strDataFilePath, String strStockNumber)
        {
            GetOriginalData(strDataFilePath, strStockNumber);

            if (null != by1FDataFileBuffer)
            {
                Array1F = GetKDataArray(by1FDataFileBuffer.Length / 32, 1);
                CreateLine(Array1F);
            }

            if (null != by5FDataFileBuffer)
            {
                Array5F = GetKDataArray(by5FDataFileBuffer.Length/32, 5);
                CreateLine(Array5F);
            }

            if (null != by5FDataFileBuffer)
            {
                Array30F = GetKDataArray(by5FDataFileBuffer.Length/32/6, 30);
                CreateLine(Array30F);
            }

            if (null != byDayDataFileBuffer)
            {
                ArrayDay = GetKDataArray(byDayDataFileBuffer.Length/32, 240);
                CreateLine(ArrayDay);
            }

        }

        //获取原始数据
        public void GetOriginalData(String strDataFilePath, String strStockNumber)
        {
            FileStream fs;
            BinaryReader br;
            String str1FDataFileFullName;
            String str5FDataFileFullName;
            String strDayDataFileFullName;
            String strRealTimeDataFileFullName;
            char[] chBuf;

            //构造路径
            chBuf = strStockNumber.ToCharArray(0, 1);
            if ('6' <= chBuf[0])
            {
                str1FDataFileFullName = strDataFilePath + "\\sh\\minline\\sh" + strStockNumber + ".lc1";
                str5FDataFileFullName = strDataFilePath + "\\sh\\fzline\\sh" + strStockNumber + ".lc5";
                strDayDataFileFullName = strDataFilePath + "\\sh\\lday\\sh" + strStockNumber + ".day";
            }
            else
            {
                str1FDataFileFullName = strDataFilePath + "\\sz\\minline\\sz" + strStockNumber + ".lc1";
                str5FDataFileFullName = strDataFilePath + "\\sz\\fzline\\sz" + strStockNumber + ".lc5";
                strDayDataFileFullName = strDataFilePath + "\\sz\\lday\\sz" + strStockNumber + ".day";
            }

            strRealTimeDataFileFullName = strDataFilePath + "\\realtime\\" + strStockNumber + ".dat";

            //读1F文件
            if (false != File.Exists(str1FDataFileFullName))
            {
                fs = new FileStream(str1FDataFileFullName, FileMode.Open);
                br = new BinaryReader(fs);
                by1FDataFileBuffer = new Byte[fs.Length];
                br.Read(by1FDataFileBuffer, 0, Convert.ToInt32(fs.Length));

                br.Close();
                fs.Close();
            }

            //读5F文件
            if (false != File.Exists(str5FDataFileFullName))
            {
                fs = new FileStream(str5FDataFileFullName, FileMode.Open);
                br = new BinaryReader(fs);
                by5FDataFileBuffer = new Byte[fs.Length];
                br.Read(by5FDataFileBuffer, 0, Convert.ToInt32(fs.Length));

                br.Close();
                fs.Close();
            }

            //读day文件
            if (false != File.Exists(strDayDataFileFullName))
            {
                fs = new FileStream(strDayDataFileFullName, FileMode.Open);
                br = new BinaryReader(fs);
                byDayDataFileBuffer = new Byte[fs.Length];
                br.Read(byDayDataFileBuffer, 0, Convert.ToInt32(fs.Length));

                br.Close();
                fs.Close();
            }

            //读RealTime文件
            if (false != File.Exists(strRealTimeDataFileFullName))
            {
                fs = new FileStream(strRealTimeDataFileFullName, FileMode.Open);
                br = new BinaryReader(fs);
                byRealTimeFileBuffer = new Byte[fs.Length];
                br.Read(byRealTimeFileBuffer, 0, Convert.ToInt32(fs.Length));

                br.Close();
                fs.Close();
            }

        }

        //根据给定的K线个数和K线级别（以分钟为单位），返回K线数据数组
        public SingalKData[] GetKDataArray(Int32 iKNumber, Int32 iKGrade)
        {
            SingalKData[] KDataArray = new SingalKData[iKNumber];
            Byte[] byTempBuffer;

            DateTime dt = DateTime.Now;
            Int32 iTodayKNumber = 0;
            Int32 iOffset;
            Int32 iCount;
            Int32 iCount2;
            Int32 iRecord = 0;
            Int32 iCurrentTime = 0;

            switch (iKGrade)
            {
                case 1:
                    byTempBuffer = new Byte[by1FDataFileBuffer.Length];
                    by1FDataFileBuffer.CopyTo(byTempBuffer, 0);
                    

                    //确定需要从RealTime文件中构造多少个K线数据
                    iTodayKNumber = 0;
                    if (null != byRealTimeFileBuffer)
                    {
                        if (570 <= (dt.Hour * 60 + dt.Minute) && 690 > (dt.Hour * 60 + dt.Minute))
                            iTodayKNumber = dt.Hour * 60 + dt.Minute - 570 + 1;
                        else if (660 <= (dt.Hour * 60 + dt.Minute) && 900 > (dt.Hour * 60 + dt.Minute))
                            iTodayKNumber = dt.Hour * 60 + dt.Minute - 660 + 120 + 1;
                    }

                    iOffset = byTempBuffer.Length - 32 * (iKNumber - iTodayKNumber);

                    //从K线文件中填充数据
                    for (iCount = 0; iCount < iKNumber - iTodayKNumber; iCount++)
                    {
                        KDataArray[iCount].iOpen = Convert.ToInt32(BitConverter.ToSingle(byTempBuffer, 4 + iOffset + 32 * iCount) * 100);
                        KDataArray[iCount].iHigh = Convert.ToInt32(BitConverter.ToSingle(byTempBuffer, 8 + iOffset + 32 * iCount) * 100);
                        KDataArray[iCount].iLow = Convert.ToInt32(BitConverter.ToSingle(byTempBuffer, 12 + iOffset + 32 * iCount) * 100);
                        KDataArray[iCount].iClose = Convert.ToInt32(BitConverter.ToSingle(byTempBuffer, 16 + iOffset + 32 * iCount) * 100);
                    }

                    //从RealTime文件中构造K线数据
                    for (iCount = iKNumber - iTodayKNumber; iCount < iKNumber; iCount++)
                    {
                        KDataArray[iCount].iOpen = 0;
                        KDataArray[iCount].iHigh = 0;
                        KDataArray[iCount].iLow = 0;
                        KDataArray[iCount].iClose = 0;

                        iCurrentTime = Convert.ToInt32(byRealTimeFileBuffer[2 + 9 * iRecord]);
                        KDataArray[iCount].iOpen = Convert.ToInt32(BitConverter.ToSingle(byRealTimeFileBuffer, 5 + 9 * iRecord) * 100);
                        while (iCurrentTime == Convert.ToInt32(byRealTimeFileBuffer[2 + 9 * iRecord]) || byRealTimeFileBuffer.Length == 9 * iRecord)
                        {
                            if (KDataArray[iCount].iHigh < Convert.ToInt32(BitConverter.ToSingle(byRealTimeFileBuffer, 5 + 9 * iRecord) * 100))
                                KDataArray[iCount].iHigh = Convert.ToInt32(BitConverter.ToSingle(byRealTimeFileBuffer, 5 + 9 * iRecord) * 100);

                            if (KDataArray[iCount].iLow > Convert.ToInt32(BitConverter.ToSingle(byRealTimeFileBuffer, 5 + 9 * iRecord) * 100))
                                KDataArray[iCount].iLow = Convert.ToInt32(BitConverter.ToSingle(byRealTimeFileBuffer, 5 + 9 * iRecord) * 100);

                            iRecord++;
                        }
                        KDataArray[iCount].iClose = Convert.ToInt32(BitConverter.ToSingle(byRealTimeFileBuffer, 5 + 9 * (iRecord - 1)) * 100);
                    }

                    break;

                case 5:
                    byTempBuffer = new Byte[by5FDataFileBuffer.Length];
                    by5FDataFileBuffer.CopyTo(byTempBuffer, 0);

                    //确定需要从RealTime文件中构造多少个K线数据
                    iTodayKNumber = 0;
                    if (null != byRealTimeFileBuffer)
                    {
                        if (570 <= (dt.Hour * 60 + dt.Minute) && 690 > (dt.Hour * 60 + dt.Minute))
                            iTodayKNumber = (dt.Hour * 60 + dt.Minute - 570 + 1)/5;
                        else if (660 <= (dt.Hour * 60 + dt.Minute) && 900 > (dt.Hour * 60 + dt.Minute))
                            iTodayKNumber = (dt.Hour * 60 + dt.Minute - 660 + 120 + 1)/5;
                    }

                    iOffset = byTempBuffer.Length - 32 * (iKNumber - iTodayKNumber);

                    //从K线文件中填充数据
                    for (iCount = 0; iCount < iKNumber - iTodayKNumber; iCount++)
                    {
                        KDataArray[iCount].iOpen = Convert.ToInt32(BitConverter.ToSingle(byTempBuffer, 4 + iOffset + 32 * iCount) * 100);
                        KDataArray[iCount].iHigh = Convert.ToInt32(BitConverter.ToSingle(byTempBuffer, 8 + iOffset + 32 * iCount) * 100);
                        KDataArray[iCount].iLow = Convert.ToInt32(BitConverter.ToSingle(byTempBuffer, 12 + iOffset + 32 * iCount) * 100);
                        KDataArray[iCount].iClose = Convert.ToInt32(BitConverter.ToSingle(byTempBuffer, 16 + iOffset + 32 * iCount) * 100);
                    }

                    //从RealTime文件中构造K线数据
                    //for (iCount = iKNumber - iTodayKNumber; iCount < iKNumber; iCount++)
                    //{
                    //    KDataArray[iCount].iOpen = 0;
                    //    KDataArray[iCount].iHigh = 0;
                    //    KDataArray[iCount].iLow = 0;
                    //    KDataArray[iCount].iClose = 0;

                    //    iCurrentTime = Convert.ToInt32(byRealTimeFileBuffer[2 + 9 * iRecord]);
                    //    KDataArray[iCount].iOpen = Convert.ToInt32(BitConverter.ToSingle(byRealTimeFileBuffer, 5 + 9 * iRecord) * 100);
                    //    while (iCurrentTime == Convert.ToInt32(byRealTimeFileBuffer[2 + 9 * iRecord]) || byRealTimeFileBuffer.Length == 9 * iRecord)
                    //    {
                    //        if (KDataArray[iCount].iHigh < Convert.ToInt32(BitConverter.ToSingle(byRealTimeFileBuffer, 5 + 9 * iRecord) * 100))
                    //            KDataArray[iCount].iHigh = Convert.ToInt32(BitConverter.ToSingle(byRealTimeFileBuffer, 5 + 9 * iRecord) * 100);

                    //        if (KDataArray[iCount].iLow > Convert.ToInt32(BitConverter.ToSingle(byRealTimeFileBuffer, 5 + 9 * iRecord) * 100))
                    //            KDataArray[iCount].iLow = Convert.ToInt32(BitConverter.ToSingle(byRealTimeFileBuffer, 5 + 9 * iRecord) * 100);

                    //        iRecord++;
                    //    }
                    //    KDataArray[iCount].iClose = Convert.ToInt32(BitConverter.ToSingle(byRealTimeFileBuffer, 5 + 9 * (iRecord - 1)) * 100);
                    //}

                    break;

                case 30:
                    byTempBuffer = new Byte[by5FDataFileBuffer.Length];
                    by5FDataFileBuffer.CopyTo(byTempBuffer, 0);

                    //iKNumber *= 6;

                    //确定需要从RealTime文件中构造多少个K线数据
                    iTodayKNumber = 0;
                    if (null != byRealTimeFileBuffer)
                    {
                        if (570 <= (dt.Hour * 60 + dt.Minute) && 690 > (dt.Hour * 60 + dt.Minute))
                            iTodayKNumber = dt.Hour * 60 + dt.Minute - 570 + 1;
                        else if (660 <= (dt.Hour * 60 + dt.Minute) && 900 > (dt.Hour * 60 + dt.Minute))
                            iTodayKNumber = dt.Hour * 60 + dt.Minute - 660 + 120 + 1;
                    }

                    iOffset = byTempBuffer.Length - 32 * (iKNumber - iTodayKNumber)  *6;

                    //从K线文件中填充数据
                    for (iCount = 0; iCount < iKNumber - iTodayKNumber; iCount++)
                    {
                        KDataArray[iCount].iOpen = Convert.ToInt32(BitConverter.ToSingle(byTempBuffer, 4 + iOffset + 32 * iCount * 6) * 100);
                        KDataArray[iCount].iClose = Convert.ToInt32(BitConverter.ToSingle(byTempBuffer, 16 + iOffset + 32 * (iCount * 6 + 5)) * 100);

                        KDataArray[iCount].iHigh = Convert.ToInt32(BitConverter.ToSingle(byTempBuffer, 8 + iOffset + 32 * iCount * 6) * 100);
                        KDataArray[iCount].iLow = Convert.ToInt32(BitConverter.ToSingle(byTempBuffer, 12 + iOffset + 32 * iCount * 6) * 100);

                        for (iCount2 = 1; iCount2 < 6; iCount2++)
                        {
                            if (KDataArray[iCount].iHigh < Convert.ToInt32(BitConverter.ToSingle(byTempBuffer, 8 + iOffset + 32 * (iCount * 6 + iCount2)) * 100))
                                KDataArray[iCount].iHigh = Convert.ToInt32(BitConverter.ToSingle(byTempBuffer, 8 + iOffset + 32 * (iCount * 6 + iCount2)) * 100);
                            if (KDataArray[iCount].iLow > Convert.ToInt32(BitConverter.ToSingle(byTempBuffer, 12 + iOffset + 32 * (iCount * 6 + iCount2)) * 100))
                                KDataArray[iCount].iLow = Convert.ToInt32(BitConverter.ToSingle(byTempBuffer, 12 + iOffset + 32 * (iCount * 6 + iCount2)) * 100);
                        }
                    }
                    break;
                case 240:
                    byTempBuffer = new Byte[byDayDataFileBuffer.Length];
                    byDayDataFileBuffer.CopyTo(byTempBuffer, 0);

                    iOffset = byTempBuffer.Length - 32 * iKNumber;

                    //从K线文件中填充数据
                    for (iCount = 0; iCount < iKNumber; iCount++)
                    {
                        KDataArray[iCount].iOpen = BitConverter.ToInt32(byTempBuffer, 4 + iOffset + 32 * iCount);
                        KDataArray[iCount].iHigh = BitConverter.ToInt32(byTempBuffer, 8 + iOffset + 32 * iCount);
                        KDataArray[iCount].iLow = BitConverter.ToInt32(byTempBuffer, 12 + iOffset + 32 * iCount);
                        KDataArray[iCount].iClose = BitConverter.ToInt32(byTempBuffer, 16 + iOffset + 32 * iCount);
                    }

                    break;
                default:
                    return null;
            }
            
            //初始化
            for (iCount = 0; iCount < iKNumber; iCount++)
            {
                KDataArray[iCount].iOriginalHigh = KDataArray[iCount].iHigh;
                KDataArray[iCount].iOriginalLow = KDataArray[iCount].iLow;
                KDataArray[iCount].iBoxStatus = 0;
                KDataArray[iCount].iRangeHigh = 0;
                KDataArray[iCount].iRangeLow = 0;
                KDataArray[iCount].iIntervalHigh = 0;
                KDataArray[iCount].iIntervalLow = 0;
            }

            return KDataArray;
        }

        //填充K线信息：顶底、包含、笔、中枢
        public void CreateLine(SingalKData[] InKData)
        {
            ProcessLine(InKData);
            ProcessBox(InKData);
            ProcessMACD(InKData);
        }

        //计算笔
        public void ProcessLine(SingalKData[] InKData)
        {
            Int32 iCount = 0;
            Int32 iKNumber = InKData.Length;
            Int32 iLastIndependentK = 0;

            //处理包含
            InKData[0].iHigh = InKData[1].iHigh - 1;
            InKData[0].iLow = InKData[1].iLow - 1;
            InKData[0].bIsButtom = false;
            InKData[0].bIsInclude = false;
            InKData[0].bIsTop = false;

            InKData[1].bIsButtom = false;
            InKData[1].bIsInclude = true;
            InKData[1].bIsTop = false;

            for (iCount = 0; iCount < iKNumber - 1; iCount++)//找到第一个不包含的K线开始
            {
                if ((InKData[iCount + 1].iHigh > InKData[iCount].iHigh && InKData[iCount + 1].iLow > InKData[iCount].iLow)
                    || (InKData[iCount + 1].iHigh < InKData[iCount].iHigh && InKData[iCount + 1].iLow < InKData[iCount].iLow))
                {
                    iLastIndependentK = iCount;
                    break;
                }
            }

            for (iCount += 2; iCount < iKNumber; iCount++)
            {
                InKData[iCount].bIsInclude = false;

                //有包含
                if ((InKData[iCount].iHigh <= InKData[iCount - 1].iHigh && InKData[iCount].iLow >= InKData[iCount - 1].iLow)
                    || (InKData[iCount - 1].iHigh <= InKData[iCount].iHigh && InKData[iCount - 1].iLow >= InKData[iCount].iLow))
                {
                    InKData[iCount].bIsInclude = true;
                    InKData[iCount - 1].bIsInclude = true;
                    if (InKData[iCount - 1].iHigh > InKData[iLastIndependentK].iHigh)//向上包含
                    {
                        if (InKData[iCount].iHigh < InKData[iCount - 1].iHigh)
                            InKData[iCount].iHigh = InKData[iCount - 1].iHigh;

                        if (InKData[iCount].iLow < InKData[iCount - 1].iLow)
                            InKData[iCount].iLow = InKData[iCount - 1].iLow;
                    }
                    else//向下包含
                    {
                        if (InKData[iCount].iHigh > InKData[iCount - 1].iHigh)
                            InKData[iCount].iHigh = InKData[iCount - 1].iHigh;

                        if (InKData[iCount].iLow > InKData[iCount - 1].iLow)
                            InKData[iCount].iLow = InKData[iCount - 1].iLow;
                    }
                }
                else
                {
                    InKData[iCount - 1].bIsInclude = false;
                    iLastIndependentK = iCount - 1;
                }

            }

            //标出顶底
            Int32 iBefore = 0;
            Int32 iAfter = 0;
            Int32 iMiddle = 0;

            for (iCount = 1; iCount < iKNumber; iCount++)
            {
                InKData[iCount].bIsButtom = false;
                InKData[iCount].bIsTop = false;

                if (true == InKData[iCount].bIsInclude)
                    continue;

                iBefore = iMiddle;
                iMiddle = iAfter;
                iAfter = iCount;

                if (0 == iBefore || 0 == iMiddle || 0 == iAfter)
                    continue;

                if (InKData[iBefore].iHigh < InKData[iMiddle].iHigh && InKData[iAfter].iHigh < InKData[iMiddle].iHigh)
                    InKData[iMiddle].bIsTop = true;

                if (InKData[iBefore].iLow > InKData[iMiddle].iLow && InKData[iAfter].iLow > InKData[iMiddle].iLow)
                    InKData[iMiddle].bIsButtom = true;

            }

            

            //去掉假顶底
            Int32 iKCount = 0;
            Int32 iCount2 = 0;
            SingalKData[] KTemp = new SingalKData[4];
            Int32 iLast = 0;

            //找到第一个顶或底
            for (iCount = 0; iCount < iKNumber; iCount++)
            {
                if (InKData[iCount].bIsButtom != InKData[iCount].bIsTop)
                {
                    iBefore = iCount;
                    break;
                }
            }

            for (iCount = iBefore + 1; iCount < iKNumber; iCount++)
            {
 
                //不是独立K线
                if (true == InKData[iCount].bIsInclude)
                    continue;

                //是独立K线且不为顶底
                if (InKData[iCount].bIsButtom == InKData[iCount].bIsTop)
                {
                    iKCount++;
                    continue;
                }

                //同为顶或底
                if (InKData[iCount].bIsTop == InKData[iBefore].bIsTop && InKData[iCount].bIsButtom == InKData[iBefore].bIsButtom)
                {
                    //同为顶
                    if (true == InKData[iCount].bIsTop)
                    {
                        if (InKData[iCount].iHigh > InKData[iBefore].iHigh)
                        {
                            InKData[iBefore].bIsTop = false;
                            iBefore = iCount;
                            iKCount = 0;
                        }
                        else
                        {
                            InKData[iCount].bIsTop = false;
                            iKCount++;
                        }
                    }
                    else if (true == InKData[iCount].bIsButtom)//同为底
                    {
                        if (InKData[iCount].iLow < InKData[iBefore].iLow)
                        {
                            InKData[iBefore].bIsButtom = false;
                            iBefore = iCount;
                            iKCount = 0;
                        }
                        else
                        {
                            InKData[iCount].bIsButtom = false;
                            iKCount++;
                        }
                    }

                    continue;
                }

                //类型不同

                //区间问题
                if (true == InKData[iBefore].bIsTop && InKData[iBefore].iHigh <= InKData[iCount].iHigh)
                {
                    InKData[iCount].bIsButtom = false;
                    continue;
                }

                if (true == InKData[iBefore].bIsButtom && InKData[iBefore].iLow >= InKData[iCount].iLow)
                {
                    InKData[iCount].bIsTop = false;
                    continue;
                }

                //独立K线多于三根成立
                if (3 <= iKCount)
                {
                    iBefore = iCount;
                    iKCount = 0;
                    continue;
                }

                //没有独立K线不成立
                if (0 == iKCount)
                {
                    InKData[iCount].bIsTop = false;
                    InKData[iCount].bIsButtom = false;
                    iKCount++;
                    continue;
                }

                //对于有一根或两个独立K线的情况计算跳空和独立K线包含
                iLast = iBefore;
                for (iCount2 = iBefore + 1; iCount2 < iCount + 1; iCount2++)
                {
                    if (true == InKData[iCount2].bIsInclude)
                        continue;

                    //有包含
                    if (false == InKData[iCount2].bIsInclude && true == InKData[iCount2 - 1].bIsInclude && iCount2 != iCount)
                        iKCount++;

                    //有跳空
                    if (false == InKData[iCount2].bIsInclude && (InKData[iLast].iLow > InKData[iCount2].iHigh || InKData[iCount2].iLow > InKData[iLast].iHigh))
                        iKCount++;

                    iLast = iCount2;
                }

                if (3 <= iKCount)
                {
                    iBefore = iCount;
                    iKCount = 0;
                    continue;
                }
                else
                {
                    InKData[iCount].bIsTop = false;
                    InKData[iCount].bIsButtom = false;
                    iKCount++;
                    continue;
                }

            }

            //处理最后一根线

            if (InKData[InKData.Length - 1].bIsButtom != InKData[InKData.Length - 1].bIsTop)
                return;

            for (iCount = InKData.Length - 1; iCount >= 0; iCount--)
            {
                if (InKData[iCount].bIsButtom == InKData[iCount].bIsTop)
                    continue;

                if (true == InKData[iCount].bIsButtom)
                {
                    if (InKData[iCount].iLow >= InKData[InKData.Length - 1].iHigh)
                    {
                        InKData[InKData.Length - 1].bIsButtom = true;
                        InKData[iCount].bIsButtom = false;
                    }
                    else
                        InKData[InKData.Length - 1].bIsTop = true;
                    //InKData[InKData.Length - 1].bIsButtom = true;
                    //InKData[iCount].bIsButtom = false;
                }
                else
                {
                    if (InKData[iCount].iHigh <= InKData[InKData.Length - 1].iLow)
                    {
                        InKData[InKData.Length - 1].bIsTop = true;
                        InKData[iCount].bIsTop = false;
                    }
                    else
                        InKData[InKData.Length - 1].bIsButtom = true;

                    //InKData[InKData.Length - 1].bIsTop = true;
                    //InKData[iCount].bIsTop = false;
                }

                break;
            }

        }

        //计算中枢
        public void ProcessBox(SingalKData[] InKData)
        {
            Int32 iCount;
            Int32 iCount2;
            List<KLineData> lKLineList = new List<KLineData>();
            List<KBoxPoint> lKBoxList = new List<KBoxPoint>();

            KLineData KLineDataTemp = new KLineData();

            //构造线段序列
            for (iCount = 0; iCount < InKData.Length; iCount++)
            {
                if (true == InKData[iCount].bIsButtom || true == InKData[iCount].bIsTop)
                    break;
            }

            if (InKData.Length - 1 == iCount)
                return;

            KLineDataTemp.iStartSerialNumber = iCount;
            if (true == InKData[iCount].bIsButtom)//为底
                KLineDataTemp.iStartPrice = InKData[iCount].iLow;
            else//为顶
                KLineDataTemp.iStartPrice = InKData[iCount].iHigh;
            KLineDataTemp.iSerialNumber = 0;

            for (iCount += 1; iCount < InKData.Length; iCount++)
            {
                if (InKData[iCount].bIsButtom == InKData[iCount].bIsTop)
                    continue;

                KLineDataTemp.iEndSerialNumber = iCount;
                if (true == InKData[iCount].bIsButtom)
                    KLineDataTemp.iEndPrice = InKData[iCount].iLow;
                else
                    KLineDataTemp.iEndPrice = InKData[iCount].iHigh;

                lKLineList.Add(KLineDataTemp);

                KLineDataTemp.iStartPrice = KLineDataTemp.iEndPrice;
                KLineDataTemp.iStartSerialNumber = KLineDataTemp.iEndSerialNumber;
                KLineDataTemp.iSerialNumber++;
            }

            //计算中枢
            KLineData[] KLineDataArray = new KLineData[lKLineList.Count];
            lKLineList.CopyTo(KLineDataArray);

            Int32 iTempIntervalLowPrice = 0;
            Int32 iTempIntervalHighPrice = 0;

            Int32 iIntervalLowPrice = 0;
            Int32 iIntervalHighPrice = 0;

            Int32 iRangeLowPrice = 0;
            Int32 iRangeHighPrice = 0;

            KLineData BoxStartLine;
            KLineData BoxEndLine;

            if (3 > KLineDataArray.Length)
                return;

            BoxStartLine = KLineDataArray[1];
            //中枢区间初始化
            if (Math.Abs(KLineDataArray[0].iStartPrice - KLineDataArray[0].iEndPrice) < Math.Abs(KLineDataArray[1].iStartPrice - KLineDataArray[1].iEndPrice))
            {
                if (KLineDataArray[0].iStartPrice < KLineDataArray[0].iEndPrice)
                {
                    iIntervalHighPrice = KLineDataArray[0].iEndPrice;
                    iIntervalLowPrice = KLineDataArray[0].iStartPrice;
                }
                else
                {
                    iIntervalHighPrice = KLineDataArray[0].iStartPrice;
                    iIntervalLowPrice = KLineDataArray[0].iEndPrice;
                }
            }
            else
            {
                if (KLineDataArray[1].iStartPrice < KLineDataArray[1].iEndPrice)
                {
                    iIntervalHighPrice = KLineDataArray[1].iEndPrice;
                    iIntervalLowPrice = KLineDataArray[1].iStartPrice;
                }
                else
                {
                    iIntervalHighPrice = KLineDataArray[1].iStartPrice;
                    iIntervalLowPrice = KLineDataArray[1].iEndPrice;
                }
            }

            for (iCount = 2; iCount < KLineDataArray.Length; iCount++)
            {
                if (Math.Abs(KLineDataArray[iCount - 1].iStartPrice - KLineDataArray[iCount - 1].iEndPrice) < Math.Abs(KLineDataArray[iCount].iStartPrice - KLineDataArray[iCount].iEndPrice))
                {
                    if (KLineDataArray[iCount - 1].iStartPrice < KLineDataArray[iCount - 1].iEndPrice)
                    {
                        iTempIntervalHighPrice = KLineDataArray[iCount - 1].iEndPrice;
                        iTempIntervalLowPrice = KLineDataArray[iCount - 1].iStartPrice;
                    }
                    else
                    {
                        iTempIntervalHighPrice = KLineDataArray[iCount - 1].iStartPrice;
                        iTempIntervalLowPrice = KLineDataArray[iCount - 1].iEndPrice;
                    }
                }
                else
                {
                    if (KLineDataArray[iCount].iStartPrice < KLineDataArray[iCount].iEndPrice)
                    {
                        iTempIntervalHighPrice = KLineDataArray[iCount].iEndPrice;
                        iTempIntervalLowPrice = KLineDataArray[iCount].iStartPrice;
                    }
                    else
                    {
                        iTempIntervalHighPrice = KLineDataArray[iCount].iStartPrice;
                        iTempIntervalLowPrice = KLineDataArray[iCount].iEndPrice;
                    }
                }

                //判断区间是否有交合
                if ((iTempIntervalLowPrice > iIntervalHighPrice || iTempIntervalHighPrice < iIntervalLowPrice) || iCount == KLineDataArray.Length - 1) //没有交合
                {

                    BoxEndLine = KLineDataArray[iCount - 2];

                    if (0 < BoxEndLine.iSerialNumber - BoxStartLine.iSerialNumber)//中枢成立
                    {
                        //计算范围
                        iRangeHighPrice = BoxStartLine.iStartPrice;
                        iRangeLowPrice = BoxStartLine.iStartPrice;
                        for (iCount2 = BoxStartLine.iSerialNumber; iCount2 <= BoxEndLine.iSerialNumber; iCount2++)
                        {
                            if (iRangeHighPrice < KLineDataArray[iCount2].iStartPrice)
                                iRangeHighPrice = KLineDataArray[iCount2].iStartPrice;
                            if (iRangeHighPrice < KLineDataArray[iCount2].iEndPrice)
                                iRangeHighPrice = KLineDataArray[iCount2].iEndPrice;

                            if (iRangeLowPrice > KLineDataArray[iCount2].iStartPrice)
                                iRangeLowPrice = KLineDataArray[iCount2].iStartPrice;
                            if (iRangeLowPrice > KLineDataArray[iCount2].iEndPrice)
                                iRangeLowPrice = KLineDataArray[iCount2].iEndPrice;
                        }

                        for (iCount2 = BoxStartLine.iStartSerialNumber; iCount2 < BoxEndLine.iEndSerialNumber; iCount2++)
                        {
                            InKData[iCount2].iBoxStatus = 1;//在中枢内
                            InKData[iCount2].iRangeHigh = iRangeHighPrice;
                            InKData[iCount2].iRangeLow = iRangeLowPrice;
                            InKData[iCount2].iIntervalHigh = iIntervalHighPrice;
                            InKData[iCount2].iIntervalLow = iIntervalLowPrice;
                        }
                        InKData[iCount2].iBoxStatus = 2;//中枢内结束
                    }

                    BoxStartLine = KLineDataArray[iCount];

                    iIntervalHighPrice = iTempIntervalHighPrice;
                    iIntervalLowPrice = iTempIntervalLowPrice;
                }
                else //有交合
                {
                    if (iIntervalHighPrice > iTempIntervalHighPrice)
                        iIntervalHighPrice = iTempIntervalHighPrice;
                    if (iIntervalLowPrice < iTempIntervalLowPrice)
                        iIntervalLowPrice = iTempIntervalLowPrice;
                }
            }
        }

        //计算MACD
        public void ProcessMACD(SingalKData[] InKData)
        {
            float[] EMA12 = new float[InKData.Length];
            float[] EMA26 = new float[InKData.Length];
            EMA12[0] = Convert.ToSingle(InKData[0].iClose);
            EMA26[0] = Convert.ToSingle(InKData[0].iClose);
            InKData[0].Macd.DIF = EMA12[0] - EMA26[0];
            InKData[0].Macd.DEA = InKData[0].Macd.DIF;
            InKData[0].Macd.MACD = 2 * (InKData[0].Macd.DIF - InKData[0].Macd.DEA);
            
            Int32 iCount = 0;
  
            for (iCount = 1; iCount < InKData.Length; iCount++)
            {
                EMA12[iCount] = (2 * Convert.ToSingle(InKData[iCount].iClose) + EMA12[iCount - 1] * 11) / 13;
                EMA26[iCount] = (2 * Convert.ToSingle(InKData[iCount].iClose) + EMA26[iCount - 1] * 25) / 27;
                InKData[iCount].Macd.DIF = EMA12[iCount] - EMA26[iCount];
                InKData[iCount].Macd.DEA = (2 * InKData[iCount].Macd.DIF + InKData[iCount - 1].Macd.DEA * 8) / 10;
                InKData[iCount].Macd.MACD = 2 * (InKData[iCount].Macd.DIF - InKData[iCount].Macd.DEA);
            }
        }

        //根据画图区域的长宽、单位周期K线所占宽度以及K线级别，返回画出笔的点序列
        public Point[] GetPaintLinePointArrary(Int32 iHeight, Int32 iWidth, Int32 iGraphicGrade, Int32 iKGrade)
        {
            if (0 == iHeight || 0 == iWidth)
                return null;

            switch (iKGrade)
            {
                case 1:
                    if (null == by1FDataFileBuffer)
                        return null;
                    break;
                case 5:
                    if (null == by5FDataFileBuffer)
                        return null;
                    break;
                case 30:
                    if (null == by5FDataFileBuffer)
                        return null;
                    break;
                case 240:
                    if (null == byDayDataFileBuffer)
                        return null;
                    break;
                default:
                    return null;
            }

            Point[] PointArray;
            SingalKData[] KDataTempBuffer;
            Int32 iHighestPrice = 0;
            Int32 iLowestPrice = 0;
            Int32 iKNumber = 0;
            float fPixelPerFen = 0;
            Int32 iPointNumber = 0;
            Int32 iPointNumberCount = 0;

            Int32 iCount = 0;

            //选择K线数据
            switch (iKGrade)
            {
                case 1:
                    KDataTempBuffer = new SingalKData[Array1F.Length];
                    Array1F.CopyTo(KDataTempBuffer, 0);
                    break;
                case 5:
                    KDataTempBuffer = new SingalKData[Array5F.Length];
                    Array5F.CopyTo(KDataTempBuffer, 0);
                    break;
                case 30:
                    KDataTempBuffer = new SingalKData[Array30F.Length];
                    Array30F.CopyTo(KDataTempBuffer, 0);
                    break;
                case 240:
                    KDataTempBuffer = new SingalKData[ArrayDay.Length];
                    ArrayDay.CopyTo(KDataTempBuffer, 0);
                    break;
                default :
                    return null;
            }
            

            //计算K线个数
            iKNumber = iWidth / iGraphicGrade;
            if (0 == iKNumber)
                return null;

            if (iKNumber > KDataTempBuffer.Length)
                iKNumber = KDataTempBuffer.Length;

            //找出最高价和最低价
            iLowestPrice = KDataTempBuffer[KDataTempBuffer.Length - 1].iLow;

            for (iCount = KDataTempBuffer.Length - 1; iCount >= KDataTempBuffer.Length - iKNumber; iCount--)
            {
                if (iHighestPrice < KDataTempBuffer[iCount].iHigh)
                    iHighestPrice = KDataTempBuffer[iCount].iHigh;

                if (iLowestPrice > KDataTempBuffer[iCount].iLow)
                    iLowestPrice = KDataTempBuffer[iCount].iLow;
            }

            //计算 像素/分
            fPixelPerFen = Convert.ToSingle(iHeight) / Convert.ToSingle(iHighestPrice - iLowestPrice);
            if (0 == fPixelPerFen)
                return null;

            //统计点的个数
            for (iCount = KDataTempBuffer.Length - 1; iCount >= KDataTempBuffer.Length - iKNumber; iCount--)
            {
                if (KDataTempBuffer[iCount].bIsButtom != KDataTempBuffer[iCount].bIsTop)
                    iPointNumber++;
            }

            if (KDataTempBuffer[KDataTempBuffer.Length - 1].bIsTop == KDataTempBuffer[KDataTempBuffer.Length - 1].bIsButtom)
                iPointNumber++;

            if (KDataTempBuffer[KDataTempBuffer.Length - iKNumber].bIsTop == KDataTempBuffer[KDataTempBuffer.Length - iKNumber].bIsButtom)
                iPointNumber++;

            PointArray = new Point[iPointNumber];

            //构造点序列
            for (iCount = KDataTempBuffer.Length - iKNumber; iCount < KDataTempBuffer.Length; iCount++)
            {
                if (iCount == KDataTempBuffer.Length - 1 && KDataTempBuffer[iCount].bIsButtom == KDataTempBuffer[iCount].bIsTop)
                {
                    PointArray[iPointNumberCount] = new Point(iGraphicGrade * (iCount + iKNumber - KDataTempBuffer.Length), Convert.ToInt32(Convert.ToSingle(iHighestPrice - KDataTempBuffer[iCount].iHigh) * fPixelPerFen));
                    iPointNumberCount++;
                    continue;
                }

                if (iCount == KDataTempBuffer.Length - iKNumber && KDataTempBuffer[iCount].bIsButtom == KDataTempBuffer[iCount].bIsTop)
                {
                    PointArray[iPointNumberCount] = new Point(iGraphicGrade * (iCount + iKNumber - KDataTempBuffer.Length), Convert.ToInt32(Convert.ToSingle(iHighestPrice - KDataTempBuffer[iCount].iHigh) * fPixelPerFen));
                    iPointNumberCount++;
                    continue;
                }

                if (KDataTempBuffer[iCount].bIsButtom == KDataTempBuffer[iCount].bIsTop)
                    continue;

                if (true == KDataTempBuffer[iCount].bIsButtom)
                {
                    PointArray[iPointNumberCount] = new Point(iGraphicGrade * (iCount + iKNumber - KDataTempBuffer.Length), Convert.ToInt32(Convert.ToSingle(iHighestPrice - KDataTempBuffer[iCount].iLow) * fPixelPerFen));
                    iPointNumberCount++;
                    continue;
                }
                else
                {
                    PointArray[iPointNumberCount] = new Point(iGraphicGrade * (iCount + iKNumber - KDataTempBuffer.Length), Convert.ToInt32(Convert.ToSingle(iHighestPrice - KDataTempBuffer[iCount].iHigh) * fPixelPerFen));
                    iPointNumberCount++;
                    continue;
                }
            }

            return PointArray;

        }

        //根据画图区域的长宽、单位周期K线所占宽度以及K线级别，返回画K线的点序列
        public Point[] GetPaintKPointArrary(Int32 iHeight, Int32 iWidth, Int32 iGraphicGrade, Int32 iKGrade)
        {
            if (0 == iHeight || 0 == iWidth)
                return null;

            switch (iKGrade)
            {
                case 1:
                    if (null == by1FDataFileBuffer)
                        return null;
                    break;
                case 5:
                    if (null == by5FDataFileBuffer)
                        return null;
                    break;
                case 30:
                    if (null == by5FDataFileBuffer)
                        return null;
                    break;
                case 240:
                    if (null == byDayDataFileBuffer)
                        return null;
                    break;
                default:
                    return null;
            }

            Point[] PointArray;
            SingalKData[] KDataTempBuffer;
            Int32 iHighestPrice = 0;
            Int32 iLowestPrice = 0;
            Int32 iKNumber = 0;
            float fPixelPerFen = 0;

            Int32 iCount = 0;

            //选择K线数据
            switch (iKGrade)
            {
                case 1:
                    KDataTempBuffer = new SingalKData[Array1F.Length];
                    Array1F.CopyTo(KDataTempBuffer, 0);
                    break;
                case 5:
                    KDataTempBuffer = new SingalKData[Array5F.Length];
                    Array5F.CopyTo(KDataTempBuffer, 0);
                    break;
                case 30:
                    KDataTempBuffer = new SingalKData[Array30F.Length];
                    Array30F.CopyTo(KDataTempBuffer, 0);
                    break;
                case 240:
                    KDataTempBuffer = new SingalKData[ArrayDay.Length];
                    ArrayDay.CopyTo(KDataTempBuffer, 0);
                    break;
                default:
                    return null;
            }

            //计算K线个数
            iKNumber = iWidth / iGraphicGrade;
            if (0 == iKNumber)
                return null;

            if (iKNumber > KDataTempBuffer.Length)
                iKNumber = KDataTempBuffer.Length;

            //找出最高价和最低价
            iLowestPrice = KDataTempBuffer[KDataTempBuffer.Length - 1].iLow;

            for (iCount = KDataTempBuffer.Length - 1; iCount >= KDataTempBuffer.Length - iKNumber; iCount--)
            {
                if (iHighestPrice < KDataTempBuffer[iCount].iHigh)
                    iHighestPrice = KDataTempBuffer[iCount].iHigh;

                if (iLowestPrice > KDataTempBuffer[iCount].iLow)
                    iLowestPrice = KDataTempBuffer[iCount].iLow;
            }

            //计算 像素/分
            fPixelPerFen = Convert.ToSingle(iHeight) / Convert.ToSingle(iHighestPrice - iLowestPrice);
            if (0 == fPixelPerFen)
                return null;

            PointArray = new Point[iKNumber * 2];

            //构造点序列
            for (iCount = KDataTempBuffer.Length - iKNumber; iCount < KDataTempBuffer.Length; iCount++)
            {
                PointArray[(iCount + iKNumber - KDataTempBuffer.Length) * 2] = new Point(iGraphicGrade * (iCount + iKNumber - KDataTempBuffer.Length), Convert.ToInt32(Convert.ToSingle(iHighestPrice - KDataTempBuffer[iCount].iOriginalHigh) * fPixelPerFen));
                PointArray[(iCount + iKNumber - KDataTempBuffer.Length) * 2 + 1] = new Point(iGraphicGrade * (iCount + iKNumber - KDataTempBuffer.Length), Convert.ToInt32(Convert.ToSingle(iHighestPrice - KDataTempBuffer[iCount].iOriginalLow) * fPixelPerFen));
                if (PointArray[(iCount + iKNumber - KDataTempBuffer.Length) * 2].Y == PointArray[(iCount + iKNumber - KDataTempBuffer.Length) * 2 + 1].Y)
                    PointArray[(iCount + iKNumber - KDataTempBuffer.Length) * 2 + 1].Y ++;
            }

            return PointArray;

        }
    
         //根据画图区域的长宽、单位周期K线所占宽度以及K线级别，返回画中枢的点序列
        public KBoxPoint[] GetBoxPointArrary(Int32 iHeight, Int32 iWidth, Int32 iGraphicGrade, Int32 iKGrade)
        {
            if (0 == iHeight || 0 == iWidth)
                return null;

            SingalKData[] KDataTempBuffer;
            Int32 iHighestPrice = 0;
            Int32 iLowestPrice = 0;
            Int32 iKNumber = 0;
            float fPixelPerFen = 0;

            Int32 iCount = 0;

            //选择K线数据
            switch (iKGrade)
            {
                case 1:
                    KDataTempBuffer = new SingalKData[Array1F.Length];
                    Array1F.CopyTo(KDataTempBuffer, 0);
                    break;
                case 5:
                    KDataTempBuffer = new SingalKData[Array5F.Length];
                    Array5F.CopyTo(KDataTempBuffer, 0);
                    break;
                case 30:
                    KDataTempBuffer = new SingalKData[Array30F.Length];
                    Array30F.CopyTo(KDataTempBuffer, 0);
                    break;
                case 240:
                    KDataTempBuffer = new SingalKData[ArrayDay.Length];
                    ArrayDay.CopyTo(KDataTempBuffer, 0);
                    break;
                default:
                    return null;
            }

            //计算K线个数
            iKNumber = iWidth / iGraphicGrade;
            if (0 == iKNumber)
                return null;

            if (iKNumber > KDataTempBuffer.Length)
                iKNumber = KDataTempBuffer.Length;

            //找出最高价和最低价
            iLowestPrice = KDataTempBuffer[KDataTempBuffer.Length - 1].iLow;

            for (iCount = KDataTempBuffer.Length - 1; iCount >= KDataTempBuffer.Length - iKNumber; iCount--)
            {
                if (iHighestPrice < KDataTempBuffer[iCount].iHigh)
                    iHighestPrice = KDataTempBuffer[iCount].iHigh;

                if (iLowestPrice > KDataTempBuffer[iCount].iLow)
                    iLowestPrice = KDataTempBuffer[iCount].iLow;
            }

            //计算 像素/分
            fPixelPerFen = Convert.ToSingle(iHeight) / Convert.ToSingle(iHighestPrice - iLowestPrice);
            if (0 == fPixelPerFen)
                return null;

            List<KLineData> lKLineList = new List<KLineData>();
            List<KBoxPoint> lKBoxList = new List<KBoxPoint>();

            KBoxPoint KBoxTemp = new KBoxPoint();
            Int32 iKBoxStartK = 0;
            Int32 iKBoxEndK = 0;

            //计算中枢
            for (iCount = KDataTempBuffer.Length -iKNumber; iCount < KDataTempBuffer.Length; iCount++)
            {
                if (0 == KDataTempBuffer[iCount].iBoxStatus)
                    continue;
                //寻找中枢开始和结束
                if (1 == KDataTempBuffer[iCount].iBoxStatus)
                {
                    iKBoxStartK = iCount;
                    for (iCount++; iCount < KDataTempBuffer.Length; iCount++)
                    {
                        if (1 == KDataTempBuffer[iCount].iBoxStatus)
                            continue;

                        iKBoxEndK = iCount;
                        break;
                    }
                }

                //计算中枢的点
                KBoxTemp.RangeStart = new Point((iKBoxStartK - KDataTempBuffer.Length + iKNumber) * iGraphicGrade, Convert.ToInt32(Convert.ToSingle(iHighestPrice - KDataTempBuffer[iKBoxStartK].iRangeHigh) * fPixelPerFen));
                KBoxTemp.RangeWidth = (iKBoxEndK - iKBoxStartK + 1) * iGraphicGrade;
                KBoxTemp.RangeHeight = Convert.ToInt32(Convert.ToSingle(KDataTempBuffer[iKBoxStartK].iRangeHigh - KDataTempBuffer[iKBoxStartK].iRangeLow) * fPixelPerFen);

                KBoxTemp.IntervalStart = new Point((iKBoxStartK - KDataTempBuffer.Length + iKNumber) * iGraphicGrade, Convert.ToInt32(Convert.ToSingle(iHighestPrice - KDataTempBuffer[iKBoxStartK].iIntervalHigh) * fPixelPerFen));
                KBoxTemp.IntervalWidth = KBoxTemp.RangeWidth;
                KBoxTemp.IntervalHeight = Convert.ToInt32(Convert.ToSingle(KDataTempBuffer[iKBoxStartK].iIntervalHigh - KDataTempBuffer[iKBoxStartK].iIntervalLow) * fPixelPerFen);
                if (0 == KBoxTemp.IntervalHeight)
                    KBoxTemp.IntervalHeight++;

                lKBoxList.Add(KBoxTemp);


            }

            KBoxPoint[] KBoxPointArray = new KBoxPoint[lKBoxList.Count];
            lKBoxList.CopyTo(KBoxPointArray);

            return KBoxPointArray;
        }
        
        //计算画MACD的点序列
        public MACDPoint[] GetMACDPointArrary(Point StartPoint, Int32 iHeight, Int32 iWidth, Int32 iGraphicGrade, Int32 iKGrade)
        {
            if (0 == iHeight || 0 == iWidth)
                return null;

            SingalKData[] KDataTempBuffer;
            float iHighestValue = 0;
            float iLowestValue = 0;
            Int32 iKNumber = 0;
            float fPixelPerFen = 0;

            Int32 iCount = 0;

            //选择K线数据
            switch (iKGrade)
            {
                case 1:
                    KDataTempBuffer = new SingalKData[Array1F.Length];
                    Array1F.CopyTo(KDataTempBuffer, 0);
                    break;
                case 5:
                    KDataTempBuffer = new SingalKData[Array5F.Length];
                    Array5F.CopyTo(KDataTempBuffer, 0);
                    break;
                case 30:
                    KDataTempBuffer = new SingalKData[Array30F.Length];
                    Array30F.CopyTo(KDataTempBuffer, 0);
                    break;
                case 240:
                    KDataTempBuffer = new SingalKData[ArrayDay.Length];
                    ArrayDay.CopyTo(KDataTempBuffer, 0);
                    break;
                default:
                    return null;
            }

            //计算K线个数
            iKNumber = iWidth / iGraphicGrade;
            if (0 == iKNumber)
                return null;

            if (iKNumber > KDataTempBuffer.Length)
                iKNumber = KDataTempBuffer.Length;

            //找出最高值和最低值

            for (iCount = KDataTempBuffer.Length - 1; iCount >= KDataTempBuffer.Length - iKNumber; iCount--)
            {
                if (iLowestValue > KDataTempBuffer[iCount].Macd.DEA)
                    iLowestValue = KDataTempBuffer[iCount].Macd.DEA;
                if (iLowestValue > KDataTempBuffer[iCount].Macd.DIF)
                    iLowestValue = KDataTempBuffer[iCount].Macd.DIF;
                if (iLowestValue > KDataTempBuffer[iCount].Macd.MACD)
                    iLowestValue = KDataTempBuffer[iCount].Macd.MACD;

                if (iHighestValue < KDataTempBuffer[iCount].Macd.DEA)
                    iHighestValue = KDataTempBuffer[iCount].Macd.DEA;
                if (iHighestValue < KDataTempBuffer[iCount].Macd.DIF)
                    iHighestValue = KDataTempBuffer[iCount].Macd.DIF;
                if (iHighestValue < KDataTempBuffer[iCount].Macd.MACD)
                    iHighestValue = KDataTempBuffer[iCount].Macd.MACD;
            }

            if (0 == iHighestValue || 0 == iLowestValue)
                return null;

            //计算 像素/单位
            //Int32  iBigestValue = Math.Abs(iHighestValue) > Math.Abs(iLowestValue) ? Math.Abs(iHighestValue) : Math.Abs(iLowestValue);
            fPixelPerFen = Convert.ToSingle(iHeight) / Convert.ToSingle(Math.Abs(iHighestValue) + Math.Abs(iLowestValue));
            if (0 == fPixelPerFen)
                return null;

            

            MACDPoint[] MACDPointArray = new MACDPoint[KDataTempBuffer.Length];

            //计算DIF点
            for (iCount = KDataTempBuffer.Length-iKNumber; iCount < KDataTempBuffer.Length; iCount++)
            {
                //DIF
                MACDPointArray[iCount].DIF = new Point(iGraphicGrade * (iCount + iKNumber - KDataTempBuffer.Length), Convert.ToInt32((iHighestValue - KDataTempBuffer[iCount].Macd.DIF) * fPixelPerFen) + StartPoint.Y);
                //DEA
                MACDPointArray[iCount].DEA = new Point(iGraphicGrade * (iCount + iKNumber - KDataTempBuffer.Length), Convert.ToInt32((iHighestValue - KDataTempBuffer[iCount].Macd.DEA) * fPixelPerFen) + StartPoint.Y);
                //MACD
                MACDPointArray[iCount].MACD = new Point(iGraphicGrade * (iCount + iKNumber - KDataTempBuffer.Length), Convert.ToInt32((iHighestValue - KDataTempBuffer[iCount].Macd.MACD) * fPixelPerFen) + StartPoint.Y);
                //Zero
                MACDPointArray[iCount].Zero = new Point(iGraphicGrade * (iCount + iKNumber - KDataTempBuffer.Length), Convert.ToInt32((iHighestValue) * fPixelPerFen) + StartPoint.Y);
            }

            return MACDPointArray;
        }
 

    }

    //K线数据结构
    public struct SingalKData
    {
        public Int32 iHigh;
        public Int32 iOpen;
        public Int32 iLow;
        public Int32 iClose;

        public Int32 iOriginalHigh;
        public Int32 iOriginalLow;

        public Int32 iBoxStatus;
        public Int32 iRangeHigh;
        public Int32 iRangeLow;
        public Int32 iIntervalHigh;
        public Int32 iIntervalLow;

        public bool bIsInclude;
        public bool bIsButtom;
        public bool bIsTop;

        public MACDInfo Macd;
        
    }

    //笔数据结构
    public struct KLineData
    {
        public Int32 iStartPrice;
        public Int32 iEndPrice;

        public Int32 iStartSerialNumber;
        public Int32 iEndSerialNumber;

        public Int32 iSerialNumber;
    }

    //中枢点结构
    public struct KBoxPoint
    {
        //范围
        public Point RangeStart;
        public Int32 RangeWidth;
        public Int32 RangeHeight;

        //区间
        public Point IntervalStart;
        public Int32 IntervalWidth;
        public Int32 IntervalHeight;

    }

    //MACD信息结构
    public struct MACDInfo
    {
        public float DIF;
        public float DEA;
        public float MACD;
    }

    //MACD点结构
    public struct MACDPoint
    {
        public Point DIF;
        public Point DEA;
        public Point MACD;
        public Point Zero;
    }
}


