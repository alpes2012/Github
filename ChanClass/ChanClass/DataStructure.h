#define STATE_UP 0
#define STATE_DOWN 1

#define SHARP_TOP 1
#define SHARP_NONE 0
#define SHARP_BOTTON -1

//K
struct OriginalData
{
	int iOpen;
	int iHigh;
	int iLow;
	int iClose;

	int iDate;
	int iMinute;
};

//笔
struct Line 
{
	int iState; //Up or Down
	int iStartNumnber;
	int iStartValue;
	int iEndNumber;
	int iEndValue;
};

//线段
struct Segment
{
	int iState; //Up or Down
	int iStartNumnber;
	int iStartValue;
	int iEndNumber;
	int iEndValue;
};

//中枢
struct Box
{
	int iGrade; //级别，从1开始
	int iStartNumnber;
	int iEndNumber;
	int iHigh;
	int iLow;
};

//包含处理后的数据
struct RealKData
{
	int iState;
	int iNumber;
	int iHigh;
	int iLow;
};