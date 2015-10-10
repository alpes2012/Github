#pragma once

using namespace std;

class CChanModel
{
public:
	CChanModel(void);
	~CChanModel(void);

	char* pszErrorInfo;

	bool Init(OriginalData* odArray, int iArraySize);
	Line* GetLineArray();
	int GetLineCount();
	Segment* GetSegmentArray();
	int GetSegmentCount();
	Box GetBoxArray();
	int GetBoxCount();

private:
	list<RealKData> lRealK;
	list<Line> lLine;
	list<Segment> lSegment;
	list<Box> lBox;

	int iODNumber;
	OriginalData* od;

	//三种基本构形
	//笔
	bool ProcessLine();
	//线段
	bool ProcessSegment();
	//中枢
	bool ProcessBox();

	//处理包含
	bool ProcessInclude();
	//复制RealKData数据
	void CopyRealKData(RealKData& rkDest, RealKData& rkSour);
	//计算两分形之间有效K数目，返回笔是否成立
	bool CountLine(RealKData& rkPrevious, RealKData& rkBack);
	//判断是什么分形，返回：1 顶分形，0 不是分形，-1 底分形
	bool IsShap(RealKData& rkPrevious, RealKData& rkCurrent, RealKData& rkNext);
	//判断是否包含
	bool IsInclude(int iPreHigh, int iPreLow, int iBakHigh, int iBakLow);
	//判断三笔是否构成线段
	bool IsSegment(Line& l1, Line& l2, Line& l3);
	//复制笔数据
	void CopyLineData(Line& liDest, Line& liSour);
	
};

