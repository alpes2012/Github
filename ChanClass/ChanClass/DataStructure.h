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

//��
struct Line 
{
	int iState; //Up or Down
	int iStartNumnber;
	int iStartValue;
	int iEndNumber;
	int iEndValue;
};

//�߶�
struct Segment
{
	int iState; //Up or Down
	int iStartNumnber;
	int iStartValue;
	int iEndNumber;
	int iEndValue;
};

//����
struct Box
{
	int iGrade; //���𣬴�1��ʼ
	int iStartNumnber;
	int iEndNumber;
	int iHigh;
	int iLow;
};

//��������������
struct RealKData
{
	int iState;
	int iNumber;
	int iHigh;
	int iLow;
};