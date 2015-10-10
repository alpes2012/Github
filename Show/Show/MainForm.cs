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
using System.Text.RegularExpressions;

namespace Show
{
    public partial class MainForm : Form
    {

        private Pen grayPen = new Pen(Color.Gray, 1);
        private Pen lgrayPen = new Pen(Color.LightGray);
        private Pen bluePen = new Pen(Color.Blue, 1);
        private Pen lbluePen = new Pen(Color.LightBlue, 1);
        private Pen redPen = new Pen(Color.Red, 1);
        private Pen greenPen = new Pen(Color.Green, 1);
        private Pen magentaPen = new Pen(Color.Magenta, 1);

        private Int32 iKGrade = 0;

        private KPoint[] PaintPointForK;
        private KData[] KDataArray;

        private LineData[] LineDataArray;
        private Point[] PaintPointForLine;

        private SegmentData[] SegmentDataArray;
        private Point[] PointForSegment;

        private string strOriginalDataFileName;
        private string strLineDataFileName;
        private string strSegmentDataFileName;
        private ProcessDrawPoint drawK;

        int iPreMinute = 0;

        public MainForm()
        {
            InitializeComponent();

            this.lb_KInfo.Location = new Point(0, this.ClientSize.Height - 12);
            this.MouseMove += new MouseEventHandler(MainForm_MouseMove);


            iKGrade = 10;
            this.SizeChanged += new EventHandler(MainForm_SizeChanged);
            this.KeyDown += new KeyEventHandler(MainForm_KeyDown);
            strOriginalDataFileName = System.Environment.CurrentDirectory + "\\K.txt";
            strLineDataFileName = @"D:\快盘\Code\ChanClass\ChanClass\L.txt";
            strSegmentDataFileName = @"D:\快盘\Code\ChanClass\ChanClass\S.txt";
            //strOriginalDataFileName = @"D:\new_tdx\vipdoc\sh\minline\sh999999.lc1";
            this.UpdateData();

            drawK = new ProcessDrawPoint(0, 0, this.ClientSize.Width, this.ClientSize.Height - 12);
            drawK.UpdateKData(this.KDataArray);
            drawK.UpdateLineData(this.LineDataArray);
            drawK.UpdateSegmentData(this.SegmentDataArray);
            drawK.UpdateGrade(this.iKGrade);
        }

        //跟随鼠标显示K信息
        void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            KData kd = drawK.GetKDataInfo(e.X / (iKGrade + 1));
            this.lb_KInfo.Text = String.Format("{3} {4}点{5}分 H:{0} L:{1} {2} {6}/{7}", kd.iHigh, kd.iLow, drawK.iShowStartNumber, kd.iDate, kd.iMinute / 60, kd.iMinute % 60, drawK.iShowStartNumber + e.X / (iKGrade + 1), KDataArray.Length);
            this.lb_KInfo.Refresh();
        }

        void MainForm_SizeChanged(object sender, EventArgs e)
        {
            drawK.ResetRange(0, 0, this.ClientSize.Width, this.ClientSize.Height - 12);
            this.lb_KInfo.Location = new Point(0, this.ClientSize.Height - 12);
            this.UpdateData();
            this.Refresh();
        }

        void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            //回车键，刷新
            if (13 == e.KeyValue)
            {
                this.UpdateData();
            }

            //左键，左移
            if (37 == e.KeyValue)
            {
                drawK.MoveBack(20);
            }

            //右键，右移
            if (39 == e.KeyValue)
            {
                drawK.MoveAhead(20);
            }

            //上键，显示级别+1
            if (38 == e.KeyValue)
            {
                this.iKGrade = iKGrade + 1 > 10 ? 10 : iKGrade + 1;
                drawK.UpdateGrade(this.iKGrade);
            }

            //下键，显示级别-1
            if (40 == e.KeyValue)
            {
                this.iKGrade = iKGrade - 1 < 0 ? 0 : iKGrade - 1;
                drawK.UpdateGrade(this.iKGrade);
            }

            
            this.Refresh();
        }

        private void Init()
        {

        }

        private void UpdateData()
        {
            this.ReadOriginalData();
            this.ReadLineData();
            this.ReadSegmentData();
        }

        private void ReadSegmentData()
        {
            FileStream fs;
            BinaryReader br;

            fs = new FileStream(strSegmentDataFileName, FileMode.Open);
            br = new BinaryReader(fs);
            Byte[] bySegmentDataFileBuffer = new Byte[fs.Length];
            br.Read(bySegmentDataFileBuffer, 0, Convert.ToInt32(fs.Length));

            br.Close();
            fs.Close();

            if (0 != bySegmentDataFileBuffer.Length % 20 || 20 > bySegmentDataFileBuffer.Length)
                return;

            this.SegmentDataArray = new SegmentData[bySegmentDataFileBuffer.Length / 20];

            for (int i = 0; i < bySegmentDataFileBuffer.Length / 20; i++)
            {
                this.SegmentDataArray[i].iState = BitConverter.ToInt32(bySegmentDataFileBuffer, 0 + 20 * i);
                this.SegmentDataArray[i].iStartNumber = BitConverter.ToInt32(bySegmentDataFileBuffer, 4 + 20 * i);
                this.SegmentDataArray[i].iStartValue = BitConverter.ToInt32(bySegmentDataFileBuffer, 8 + 20 * i);
                this.SegmentDataArray[i].iEndNumber = BitConverter.ToInt32(bySegmentDataFileBuffer, 12 + 20 * i);
                this.SegmentDataArray[i].iEndValue = BitConverter.ToInt32(bySegmentDataFileBuffer, 16 + 20 * i);
            }
        }

        private void ReadOriginalData()
        {
            FileStream fs;
            BinaryReader br;

            fs = new FileStream(strOriginalDataFileName, FileMode.Open);
            br = new BinaryReader(fs);
            Byte[] by1FDataFileBuffer = new Byte[fs.Length];
            br.Read(by1FDataFileBuffer, 0, Convert.ToInt32(fs.Length));

            br.Close();
            fs.Close();

            if (0 != by1FDataFileBuffer.Length % 32 || 32 > by1FDataFileBuffer.Length)
                return;

            KDataArray = new KData[by1FDataFileBuffer.Length / 32];

            for (int i = 0; i < by1FDataFileBuffer.Length / 32; i++)
            {
                KDataArray[i].iOpen = Convert.ToInt32((BitConverter.ToSingle(by1FDataFileBuffer, 4 + 32 * i) * 1000));
                KDataArray[i].iHigh = Convert.ToInt32((BitConverter.ToSingle(by1FDataFileBuffer, 8 + 32 * i) * 1000));
                KDataArray[i].iLow = Convert.ToInt32((BitConverter.ToSingle(by1FDataFileBuffer, 12 + 32 * i) * 1000));
                KDataArray[i].iClose = Convert.ToInt32((BitConverter.ToSingle(by1FDataFileBuffer, 16 + 32 * i) * 1000));

                KDataArray[i].iDate = BitConverter.ToInt32(by1FDataFileBuffer, 32 * i) % 65536;
                KDataArray[i].iDate = this.TDX1FData_DateConverter(KDataArray[i].iDate);
                KDataArray[i].iMinute = BitConverter.ToInt32(by1FDataFileBuffer, 32 * i) / 65536;

            }

        }

        private Int32 TDX1FData_DateConverter(int iOriginalData)
        {
            Int32 iYear = iOriginalData / 2048 + 2004;
            Int32 iMonth = (iOriginalData % 2048) / 100;
            Int32 iDay = iOriginalData % 2048 % 100;
            return iYear * 10000 + iMonth * 100 + iDay;
        }

        private void ReadLineData()
        {
            FileStream fs;
            BinaryReader br;

            fs = new FileStream(strLineDataFileName, FileMode.Open);
            br = new BinaryReader(fs);
            Byte[] byLineDataFileBuffer = new Byte[fs.Length];
            br.Read(byLineDataFileBuffer, 0, Convert.ToInt32(fs.Length));

            br.Close();
            fs.Close();

            if (0 != byLineDataFileBuffer.Length % 20 || 20 > byLineDataFileBuffer.Length)
                return;

            this.LineDataArray = new LineData[byLineDataFileBuffer.Length / 20];

            for (int i = 0; i < byLineDataFileBuffer.Length / 20; i++)
            {
                this.LineDataArray[i].iState = BitConverter.ToInt32(byLineDataFileBuffer, 0 + 20 * i);
                this.LineDataArray[i].iStartNumber = BitConverter.ToInt32(byLineDataFileBuffer, 4 + 20 * i);
                this.LineDataArray[i].iStartValue = BitConverter.ToInt32(byLineDataFileBuffer, 8 + 20 * i);
                this.LineDataArray[i].iEndNumber = BitConverter.ToInt32(byLineDataFileBuffer, 12 + 20 * i);
                this.LineDataArray[i].iEndValue = BitConverter.ToInt32(byLineDataFileBuffer, 16 + 20 * i);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Int32 iCount = 0;
            Graphics dc = e.Graphics;

            if (null == KDataArray || 0 == KDataArray.Length)
                return;

            drawK.Refresh();
            PaintPointForK = drawK.GetKPoint();
            PaintPointForLine = drawK.GetLinePoint();
            PointForSegment = drawK.GetSegmentPoint();

            //////////////////////////////////////////////////////////////////////////
            //画刻度
            Point p1;
            Point p2;
            for (iCount = (iKGrade + 1) * 10; iCount < this.ClientSize.Width; iCount += (iKGrade + 1) * 10)
            {
                p1 = new Point(iCount, 0);
                p2 = new Point(iCount, this.ClientSize.Height);
                dc.DrawLine(lbluePen, p1, p2);
                dc.DrawString(Convert.ToString(iCount / ((iKGrade + 1) * 10)), new Font("Verdana", 5), new SolidBrush(Color.LightBlue), iCount - (iKGrade + 1) * 10, 0);
            }

            //写起始点
            //dc.DrawString(Convert.ToString(drawK.iShowStartNumber), new Font("Verdana", 5), new SolidBrush(Color.Blue), 0, this.ClientSize.Height - 10);
            //////////////////////////////////////////////////////////////////////////
            //画K线
            if (null == PaintPointForK)
                return;

            for (iCount = 0; iCount < PaintPointForK.Length; iCount++ )
            {
                dc.DrawLine(redPen, PaintPointForK[iCount].Top, PaintPointForK[iCount].Button);
                //dc.DrawString(Convert.ToString(iCount), new Font("Verdana", 7), new SolidBrush(Color.LightBlue), PaintPointForK[iCount].Top.X, PaintPointForK[iCount].Top.Y);
            }

            //画笔
            if (1 < PaintPointForLine.Length)
                dc.DrawLines(grayPen, PaintPointForLine);

            //画线段
            if (1 < PointForSegment.Length)
                dc.DrawLines(greenPen, PointForSegment);
        }

        private void UpdatePaint()
        {

        }
    }

    //K线数据结构
    public struct KData
    {
        public Int32 iHigh;
        public Int32 iOpen;
        public Int32 iLow;
        public Int32 iClose;

        public Int32 iDate;
        public Int32 iMinute;
    }

    

    //笔数据结构
    public struct LineData
    {
        public Int32 iState;
        public Int32 iStartNumber;
        public Int32 iStartValue;
        public Int32 iEndNumber;
        public Int32 iEndValue;
    }

    //线段数据结构
    public struct SegmentData
    {
        public Int32 iState;
        public Int32 iStartNumber;
        public Int32 iStartValue;
        public Int32 iEndNumber;
        public Int32 iEndValue;
    }
}
