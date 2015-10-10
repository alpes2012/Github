#include "StdAfx.h"
#include "ChanModel.h"


CChanModel::CChanModel(void)
{
}


CChanModel::~CChanModel(void)
{
}

bool CChanModel::Init(OriginalData* odArray, int iArraySize)
{
	this->iODNumber = iArraySize;
	od = odArray;

	this->ProcessInclude();
	this->ProcessLine();
	this->ProcessSegment();

	return true;
}

bool CChanModel::ProcessLine()
{
	list<RealKData> lLineEndPoint;
	list<RealKData>::iterator itLineEndPoint;
	list<RealKData>::iterator itLineEndPointNext;
	list<RealKData>::iterator it;
	list<RealKData>::iterator itPrevious;
	list<RealKData>::iterator itNext;
	RealKData rkTemp;
	int iFirst;
	int iSecond;
	int iCount = 0;

	if (0 == lRealK.size())
		return false;

	//找到第一个分形
	itPrevious = lRealK.begin();
	itNext = itPrevious;
	itNext++;
	itNext++;
	for(it = (++lRealK.begin()); it != (--lRealK.end()); ++it)
	{
		if (true == IsShap((*itPrevious), (*it), (*itNext)))
			break;

		itPrevious++;
		itNext++;
	}

	this->CopyRealKData(rkTemp, (*it));

	it++;

	list<RealKData>::iterator itLine;
	for(; it != (--lRealK.end()); it++)
	{
		itPrevious++;
		itNext++;

		//不是分形
		if (false == IsShap((*itPrevious), (*it), (*itNext)))
			continue;

		//相同分形
		if (rkTemp.iState == it->iState)
		{
			if (STATE_UP == rkTemp.iState && it->iHigh >= rkTemp.iHigh)
				this->CopyRealKData(rkTemp, (*it));
			else if (STATE_DOWN == rkTemp.iState && it->iLow <= rkTemp.iLow)
				this->CopyRealKData(rkTemp, (*it));
		}
		else//不同分形
		{
			if (true == this->CountLine(rkTemp, (*it)))
			{
				lLineEndPoint.push_back(rkTemp);
				this->CopyRealKData(rkTemp, (*it));
			}
			else//如果不够顶底条件，要看与前前一个分形的值，如果同向值更高，则抛弃前一个和前前一个分形，压入当前分形
			{
				if (0 != lLineEndPoint.size())
				{
					itLine = (--lLineEndPoint.end());
					if ((STATE_UP == itLine->iState && itLine->iHigh <= it->iHigh) || (STATE_DOWN == itLine->iState && itLine->iLow >= it->iLow))
					{
						lLineEndPoint.pop_back();
						this->CopyRealKData(rkTemp, (*it));
					}
				}	
			}
		}
	}

	//处理最后一个分形
	it--;
	lLineEndPoint.push_back(*it);

	Line liTemp;
	itLineEndPointNext = lLineEndPoint.begin();
	itLineEndPointNext++;
	for (itLineEndPoint = lLineEndPoint.begin(); itLineEndPoint != (--lLineEndPoint.end()); itLineEndPoint++, itLineEndPointNext++)
	{
		liTemp.iStartNumnber = itLineEndPoint->iNumber;
		liTemp.iEndNumber = itLineEndPointNext->iNumber;

		if (itLineEndPoint->iHigh > itLineEndPointNext->iHigh)
		{
			liTemp.iState = STATE_DOWN;
			liTemp.iStartValue = itLineEndPoint->iHigh;
			liTemp.iEndValue = itLineEndPointNext->iLow;
		}
		else
		{
			liTemp.iState = STATE_UP;
			liTemp.iStartValue = itLineEndPoint->iLow;
			liTemp.iEndValue = itLineEndPointNext->iHigh;
		}

		this->lLine.push_back(liTemp);
	}

	return true;
}

bool CChanModel::ProcessSegment()
{
	if (0 == this->lLine.size())
		return false;

	list<Line>::iterator itLine;
	list<Line>::iterator itNext;
	list<Line>::iterator itNextNext;
	list<Line>::iterator itPrevious;
	list<Line>::iterator itEnd;
	itEnd = lLine.end();
	itEnd--;
	itEnd--;
	Line lTempStart;
	lTempStart.iState = -1;
	Segment sgTemp;

	for (itLine = lLine.begin(); itLine != itEnd; itLine ++)
	{
		itNext = itLine;
		itNext++;
		itNextNext = itLine;
		itNextNext++;
		itNextNext++;

		if (false == this->IsSegment((*itLine), (*itNext), (*itNextNext)))
			continue;

		//第一个
		if (-1 == lTempStart.iState)
		{
			this->CopyLineData(lTempStart, (*itLine));
			continue;
		}

		//如果是通向的则抛弃
		if (itLine->iState == lTempStart.iState)
			continue;

		itPrevious = itLine;
		itPrevious--;

		sgTemp.iState = lTempStart.iState;
		sgTemp.iStartNumnber = lTempStart.iStartNumnber;
		sgTemp.iStartValue = lTempStart.iStartValue;
		sgTemp.iEndNumber = itPrevious->iEndNumber;
		sgTemp.iEndValue = itPrevious->iEndValue;

		this->CopyLineData(lTempStart, (*itLine));

		this->lSegment.push_back(sgTemp);
	}

	return true;
}

bool CChanModel::IsSegment(Line& l1, Line& l2, Line& l3)
{
	if (STATE_UP == l1.iState)//上升线段
	{
		if (l1.iStartValue >= l2.iEndValue)
			return false;

		if (l3.iEndValue <= l2.iStartValue)
			return false;
	}
	else//下降线段
	{
		if (l1.iStartValue <= l2.iEndValue)
			return false;

		if (l3.iEndValue >= l2.iStartValue)
			return false;
	}

	return true;
}

void CChanModel::CopyRealKData(RealKData& rkDest, RealKData& rkSour)
{
	rkDest.iState = rkSour.iState;
	rkDest.iNumber = rkSour.iNumber;
	rkDest.iHigh = rkSour.iHigh;
	rkDest.iLow = rkSour.iLow;
}

bool CChanModel::CountLine(RealKData& rkPrevious, RealKData& rkBack)
{
	//判断分形区间是否成立
	//1 区间不能有包含
	if (true == this->IsInclude(rkPrevious.iHigh, rkPrevious.iLow, rkBack.iHigh, rkBack.iLow))
		return false;
	//2 不能顶底倒置
	if (STATE_UP == rkPrevious.iState && rkBack.iHigh > rkPrevious.iHigh)
		return false;
	if (STATE_DOWN == rkPrevious.iState && rkBack.iHigh < rkPrevious.iHigh)
		return false;

	//分形之间K数目大于4，成立
	if (rkBack.iNumber -rkPrevious.iNumber >= 4)
		return true;

	//分形之间K数目<=3，需要找出正真的分形的最高或最低K
	int iTemp1 = rkPrevious.iNumber;
	int iTemp2 = rkBack.iNumber;
	int iCount = 0;
	//前一个
	for (iCount = rkPrevious.iNumber; iCount >= 0; iCount--)
	{
		if (STATE_UP == rkPrevious.iState && od[iCount].iHigh == rkPrevious.iHigh)
		{
			iTemp1 = iCount;
			break;
		}
		else if (STATE_DOWN == rkPrevious.iState && od[iCount].iLow == rkPrevious.iLow)
		{
			iTemp1 = iCount;
			break;
		}
	}
	//后一个
	for (iCount = rkBack.iNumber; iCount >= 0; iCount--)
	{
		if (STATE_UP == rkBack.iState && od[iCount].iHigh == rkBack.iHigh)
		{
			iTemp2 = iCount;
			break;
		}
		else if (STATE_DOWN == rkBack.iState && od[iCount].iLow == rkBack.iLow)
		{
			iTemp2 = iCount;
			break;
		}
	}


	if (iTemp2 - iTemp1 >= 4)
		return true;

	//计算跳空
	iCount = iTemp2 - iTemp1;
	for (int i = iTemp1 + 1; i <= iTemp2; i++)
	{
		if (od[i].iHigh < od[i-1].iLow || od[i].iLow > od[i-1].iHigh)
			iCount ++;
		if (iCount >= 4)
			return true;
	}

	return false;
}

bool CChanModel::ProcessInclude()
{
	RealKData rk;
	int iCount = 0;

	//找到第一个不包含的K
	for (iCount = 1; iCount < iODNumber; iCount++)
	{
		if (true == this->IsInclude(this->od[iCount - 1].iHigh, this->od[iCount - 1].iLow, this->od[iCount].iHigh, this->od[iCount].iLow))
			continue;

		//将前一根压入
		rk.iNumber = iCount - 1;
		rk.iHigh = this->od[iCount - 1].iHigh;
		rk.iLow = this->od[iCount - 1].iLow;

		if (this->od[iCount - 1].iHigh > this->od[iCount].iHigh)
			rk.iState = STATE_DOWN;
		else
			rk.iState = STATE_UP;

		lRealK.push_back(rk);

		rk.iNumber = iCount;
		rk.iHigh = this->od[iCount].iHigh;
		rk.iLow = this->od[iCount].iLow;
	
		break;
	}
	iCount ++;

	for(;iCount < iODNumber; iCount++)
	{
		//无包含
		if (false == this->IsInclude(rk.iHigh, rk.iLow, this->od[iCount].iHigh, this->od[iCount].iLow))
		{
			lRealK.push_back(rk);

			if (rk.iHigh > this->od[iCount].iHigh)
				rk.iState = STATE_DOWN;
			else
				rk.iState = STATE_UP;

			rk.iNumber = iCount;
			rk.iHigh = this->od[iCount].iHigh;
			rk.iLow = this->od[iCount].iLow;
			
		}
		else
		{
			rk.iNumber = iCount;
			if (STATE_UP == rk.iState)
			{
				rk.iHigh = (rk.iHigh > this->od[iCount].iHigh) ? rk.iHigh : this->od[iCount].iHigh;
				rk.iLow = (rk.iLow > this->od[iCount].iLow) ? rk.iLow : this->od[iCount].iLow;
			}
			else
			{
				rk.iHigh = (rk.iHigh < this->od[iCount].iHigh) ? rk.iHigh : this->od[iCount].iHigh;
				rk.iLow = (rk.iLow < this->od[iCount].iLow) ? rk.iLow : this->od[iCount].iLow;
			}
		}
	}

	//添加最后一根
	lRealK.push_back(rk);

	return true;
}

int CChanModel::GetLineCount()
{
	return lLine.size();
}

Line* CChanModel::GetLineArray()
{
	Line* pLineArray = new Line[lLine.size()];
	Line* pTemp = pLineArray;
	list<Line>::iterator it;
	for (it = lLine.begin();it != lLine.end();it++)
	{
		pLineArray->iState = it->iState;
		pLineArray->iStartNumnber = it->iStartNumnber;
		pLineArray->iEndNumber = it->iEndNumber;
		pLineArray->iStartValue = it->iStartValue;
		pLineArray->iEndValue = it->iEndValue;
		pLineArray++;
	}

	return pTemp;
}

int CChanModel::GetSegmentCount()
{
	return this->lSegment.size();
}

Segment* CChanModel::GetSegmentArray()
{
	Segment* pSegmentArray = new Segment[lSegment.size()];
	Segment* pTemp = pSegmentArray;
	list<Segment>::iterator it;
	for (it = lSegment.begin();it != lSegment.end();it++)
	{
		pSegmentArray->iState = it->iState;
		pSegmentArray->iStartNumnber = it->iStartNumnber;
		pSegmentArray->iEndNumber = it->iEndNumber;
		pSegmentArray->iStartValue = it->iStartValue;
		pSegmentArray->iEndValue = it->iEndValue;
		pSegmentArray++;
	}

	return pTemp;
}

bool CChanModel::IsShap(RealKData& rkPrevious, RealKData& rkCurrent, RealKData& rkNext)
{
	if ((rkCurrent.iHigh > rkPrevious.iHigh && rkCurrent.iHigh  < rkNext.iHigh) || (rkCurrent.iHigh < rkPrevious.iHigh && rkCurrent.iHigh  > rkNext.iHigh))
		return false;
	else
		return true;
}

bool CChanModel::IsInclude(int iPreHigh, int iPreLow, int iBakHigh, int iBakLow)
{
	if ((iPreHigh <= iBakHigh && iPreLow >= iBakLow) || (iPreHigh >= iBakHigh && iPreLow <= iBakLow))
		return true;
	else
		return false;
}

void CChanModel::CopyLineData(Line& liDest, Line& liSour)
{
	liDest.iState = liSour.iState;
	liDest.iStartNumnber = liSour.iStartNumnber;
	liDest.iStartValue = liSour.iStartValue;
	liDest.iEndValue = liSour.iEndValue;
	liDest.iEndNumber = liSour.iEndNumber;
}

