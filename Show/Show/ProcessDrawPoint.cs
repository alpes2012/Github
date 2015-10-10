using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Show
{
    class ProcessDrawPoint
    {
        private int iTopX;
        private int iTopY;
        private int iButtonX;
        private int iButtonY;

        public int iShowStartNumber = -1;
        public int iKCount = 0;
        private int iShowGrade = 0;
        private float fPixel = 0;
        private int iHighest = 0;
        private int iLowest = 0;

        private KData[] KDataArray = null;
        private LineData[] LineDataArray = null;
        private SegmentData[] SegmentDataArray = null;

        private List<KPoint> lKPoint = new List<KPoint>();
        private List<Point> lLinePoint = new List<Point>();
        private List<Point> lSegmentPoint = new List<Point>();

        public KPoint[] KPointArray = null;
        public Point[] LinePointArray = null;

        public ProcessDrawPoint(int iInTopX, int iInTopY, int iInButtonX, int iInButtonY)
        {
            iTopX = iInTopX;
            iTopY = iInTopY;
            iButtonX = iInButtonX;
            iButtonY = iInButtonY;
        }

        public void UpdateGrade(int iIn)
        {
            this.iShowGrade = iIn;
        }

        public void UpdateKData(KData[] kdIn)
        {
            this.KDataArray = kdIn;
        }

        public void UpdateLineData(LineData[] ldIn)
        {
            this.LineDataArray = ldIn;
        }

        public void UpdateSegmentData(SegmentData[] sdIn)
        {
            this.SegmentDataArray = sdIn;
        }

        public void Refresh()
        {
            lKPoint.Clear();
            lLinePoint.Clear();
            lSegmentPoint.Clear();
            this.ProcessKPoint();
            this.ProcessLinePoint();
            this.ProcessSegmentPoint();
        }

        public KPoint[] GetKPoint()
        {
            if (null == lKPoint || 0 == lKPoint.Count)
                return null;

            KPoint[] kpArray = new KPoint[lKPoint.Count];
            lKPoint.CopyTo(kpArray);
            return kpArray;
        }

        public Point[] GetLinePoint()
        {
            if (null == lLinePoint || 0 == lLinePoint.Count)
                return null;

            Point[] pLineArray = new Point[lLinePoint.Count];
            lLinePoint.CopyTo(pLineArray);
            return pLineArray;
        }

        public Point[] GetSegmentPoint()
        {
            if (null == lSegmentPoint || 0 == lSegmentPoint.Count)
                return null;

            Point[] pSegmentArray = new Point[lSegmentPoint.Count];
            lSegmentPoint.CopyTo(pSegmentArray);
            return pSegmentArray;
        }

        private void ProcessKPoint()
        {
            if (null == this.KDataArray)
                return;

            //显示的K线个数
            iKCount = (iButtonX - iTopX - 2) / (this.iShowGrade + 1);
            if (iKCount > this.KDataArray.Length)
                iKCount = this.KDataArray.Length;
            this.KPointArray = new KPoint[iKCount];

            //第一次从尾部开始显示
            if (-1 == this.iShowStartNumber)
            {
                //if (this.iKCount < this.KDataArray.Length)
                //    this.iShowStartNumber = this.KDataArray.Length - iKCount;
                //else
                    this.iShowStartNumber = 0;
            }
            else if (this.KDataArray.Length - this.iShowStartNumber < this.iKCount)
            {
                this.iKCount = this.KDataArray.Length - this.iShowStartNumber;
            }



            //计算最高和最低
            iHighest = 0;
            iLowest = this.KDataArray[this.iShowStartNumber].iLow;

            for (int i = this.iShowStartNumber; i < this.iShowStartNumber + iKCount; i++)
            {
                if (this.KDataArray[i].iLow < iLowest)
                    iLowest = this.KDataArray[i].iLow;

                if (this.KDataArray[i].iHigh > iHighest)
                    iHighest = this.KDataArray[i].iHigh;
            }

            iLowest--;//避免相减为0的情况

            //计算 像素/单位
            fPixel = Convert.ToSingle(iButtonY - iTopY) / Convert.ToSingle(iHighest - iLowest);

            //构造Point
            int iPointTopX;
            int iPointTopY;
            int iPointButtonX;
            int iPointButtonY;
            KPoint kpTemp;
            for (int i = 0; i < iKCount; i++)
            {
                iPointTopX = iPointButtonX = iTopX + i * (this.iShowGrade + 1) + this.iShowGrade;
                iPointButtonY = iButtonY - Convert.ToInt32(Convert.ToSingle(this.KDataArray[i + this.iShowStartNumber].iLow - iLowest) * fPixel);
                if (this.KDataArray[i + this.iShowStartNumber].iHigh == this.KDataArray[i + this.iShowStartNumber].iLow)
                    iPointTopY = iPointButtonY - Convert.ToInt32(Convert.ToSingle(1) * fPixel);
                else
                    iPointTopY = iPointButtonY - Convert.ToInt32(Convert.ToSingle(this.KDataArray[i + this.iShowStartNumber].iHigh - this.KDataArray[i + this.iShowStartNumber].iLow) * fPixel);

                kpTemp.Top = new Point(iPointTopX, iPointTopY);
                kpTemp.Button = new Point(iPointButtonX, iPointButtonY);

                lKPoint.Add(kpTemp);
            }

        }

        private void ProcessLinePoint()
        {
            if (null == this.LineDataArray || 0 == this.fPixel)
                return;

            KPoint[] kp = this.GetKPoint();
            if (null == kp)
                return;

            

            int iShowEndNumber = this.iShowStartNumber + this.iKCount - 1;
            int iLineCount = 0;
            int iTempX = 0;
            int iTempY = 0;
            foreach (LineData ld in this.LineDataArray)
            {
                iLineCount++;

                if (ld.iEndNumber > iShowEndNumber && ld.iStartNumber >= iShowEndNumber)
                    break;

                if (ld.iStartNumber < this.iShowStartNumber)
                    continue;

                iTempX = this.iTopX + (ld.iStartNumber - this.iShowStartNumber) * (this.iShowGrade + 1) + this.iShowGrade;
                iTempY = this.iTopY + Convert.ToInt32(Convert.ToSingle(this.iHighest - ld.iStartValue) * this.fPixel);

                this.lLinePoint.Add(new Point(iTempX, iTempY));

                //if (0 == ld.iState)//上升
                //    this.lLinePoint.Add(kp[ld.iStartNumber - this.iShowStartNumber].Button);
                //else
                //    this.lLinePoint.Add(kp[ld.iStartNumber - this.iShowStartNumber].Top);
                
            }

            iLineCount--;
            if (this.LineDataArray[iLineCount].iEndNumber > iShowEndNumber)
                return;

            //显示最后一条的后一个点

            iTempX = this.iTopX + (LineDataArray[LineDataArray.Length - 1].iEndNumber - this.iShowStartNumber) * (this.iShowGrade + 1) + this.iShowGrade;
            iTempY = this.iTopY + Convert.ToInt32(Convert.ToSingle(this.iHighest - LineDataArray[LineDataArray.Length - 1].iEndValue) * this.fPixel);

            this.lLinePoint.Add(new Point(iTempX, iTempY));

            //if (0 == this.LineDataArray[iLineCount].iState)//上升
            //    this.lLinePoint.Add(kp[this.LineDataArray[iLineCount].iEndNumber - this.iShowStartNumber].Top);
            //else
            //    this.lLinePoint.Add(kp[this.LineDataArray[iLineCount].iEndNumber - this.iShowStartNumber].Button);
        }

        private void ProcessSegmentPoint()
        {
            if (null == this.SegmentDataArray || 0 == this.fPixel)
                return;

            KPoint[] kp = this.GetKPoint();
            if (null == kp)
                return;

            int iShowEndNumber = this.iShowStartNumber + this.iKCount - 1;
            int iSegmentCount = 0;
            int iTempX = 0;
            int iTempY = 0;
            foreach (SegmentData ld in this.SegmentDataArray)
            {
                iSegmentCount++;

                if (ld.iEndNumber > iShowEndNumber && ld.iStartNumber >= iShowEndNumber)
                    break;

                if (ld.iStartNumber < this.iShowStartNumber)
                    continue;

                iTempX = this.iTopX + (ld.iStartNumber - this.iShowStartNumber) * (this.iShowGrade + 1) + this.iShowGrade;
                iTempY = this.iTopY + Convert.ToInt32(Convert.ToSingle(this.iHighest - ld.iStartValue) * this.fPixel);

                this.lSegmentPoint.Add(new Point(iTempX, iTempY));

                //if (0 == ld.iState)//上升
                //    this.lSegmentPoint.Add(kp[ld.iStartNumber - this.iShowStartNumber].Button);
                //else
                //    this.lSegmentPoint.Add(kp[ld.iStartNumber - this.iShowStartNumber].Top);

            }

            iSegmentCount--;
            if (this.SegmentDataArray[iSegmentCount].iEndNumber > iShowEndNumber)
                return;

            //显示最后一条的后一个点

            iTempX = this.iTopX + (SegmentDataArray[SegmentDataArray.Length - 1].iEndNumber - this.iShowStartNumber) * (this.iShowGrade + 1) + this.iShowGrade;
            iTempY = this.iTopY + Convert.ToInt32(Convert.ToSingle(this.iHighest - SegmentDataArray[SegmentDataArray.Length - 1].iEndValue) * this.fPixel);

            this.lSegmentPoint.Add(new Point(iTempX, iTempY));

            //if (0 == this.SegmentDataArray[iSegmentCount].iState)//上升
            //    this.lSegmentPoint.Add(kp[this.SegmentDataArray[iSegmentCount].iEndNumber - this.iShowStartNumber].Top);
            //else
            //    this.lSegmentPoint.Add(kp[this.SegmentDataArray[iSegmentCount].iEndNumber - this.iShowStartNumber].Button);
        }

        public void MoveAhead(int iSteps)
        {
            if (this.KDataArray.Length - 1 > this.iShowStartNumber + iSteps)
                this.iShowStartNumber +=  iSteps;
        }

        public void MoveBack(int iSteps)
        {
            if (this.iShowStartNumber - iSteps < 0)
                this.iShowStartNumber = 0;
            else
                this.iShowStartNumber -= iSteps;
        }

        public void ResetRange(int iInTopX, int iInTopY, int iInButtonX, int iInButtonY)
        {
            iTopX = iInTopX;
            iTopY = iInTopY;
            iButtonX = iInButtonX;
            iButtonY = iInButtonY;
        }

        //根据指定的位置的K信息
        public KData GetKDataInfo(Int32 iIndex)
        {
            if (0 == iKCount)
                return this.KDataArray[0];

            if (0 > iIndex || iKCount <= iIndex)
                return this.KDataArray[0];
            else
                return this.KDataArray[this.iShowStartNumber + iIndex];
        }
         
    }

    //K线绘图点结构
    public struct KPoint
    {
        public Point Top;
        public Point Button;
    }

    //笔绘图点结构
    public struct LinePoint
    {
        public Point Top;
        public Point Button;
    }
}
