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

	//���ֻ�������
	//��
	bool ProcessLine();
	//�߶�
	bool ProcessSegment();
	//����
	bool ProcessBox();

	//�������
	bool ProcessInclude();
	//����RealKData����
	void CopyRealKData(RealKData& rkDest, RealKData& rkSour);
	//����������֮����ЧK��Ŀ�����ر��Ƿ����
	bool CountLine(RealKData& rkPrevious, RealKData& rkBack);
	//�ж���ʲô���Σ����أ�1 �����Σ�0 ���Ƿ��Σ�-1 �׷���
	bool IsShap(RealKData& rkPrevious, RealKData& rkCurrent, RealKData& rkNext);
	//�ж��Ƿ����
	bool IsInclude(int iPreHigh, int iPreLow, int iBakHigh, int iBakLow);
	//�ж������Ƿ񹹳��߶�
	bool IsSegment(Line& l1, Line& l2, Line& l3);
	//���Ʊ�����
	void CopyLineData(Line& liDest, Line& liSour);
	
};

