// ChanClass.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"

int TDX1FData_DateConverter(int iOriginalData)
{
	int iYear = iOriginalData / 2048 + 2004;
	int iMonth = (iOriginalData % 2048) /100;
	int iDay = iOriginalData % 2048 %100;
	return iYear * 10000 + iMonth * 100 + iDay;
}

int _tmain(int argc, _TCHAR* argv[])
{
	FILE* fpIn = fopen("K.txt", "rb");
	if (nullptr == fpIn)
		return -1;

	fseek(fpIn, 0, FILE_END);
	int iFileSize = ftell(fpIn);
	fseek(fpIn, 0, FILE_BEGIN);

	byte* byFileBuffer = new byte[iFileSize];
	fread(byFileBuffer, 1, iFileSize, fpIn);
	fclose(fpIn);

	OriginalData* pod = new OriginalData[iFileSize / 32];
	OriginalData* podTemp = pod;
	float fTemp[4] = {0.0};
	for (int i = 0; i < iFileSize / 32; i++)
	{
		pod->iDate = byFileBuffer[i*32] + byFileBuffer[i*32+1]*256;
		pod->iDate = TDX1FData_DateConverter(pod->iDate);
		pod->iMinute = byFileBuffer[i*32+2] + byFileBuffer[i*32+3]*256;
		
		//printf("%d %d:%d\n", pod->iDate, pod->iMinute / 60, pod->iMinute % 60);

		memcpy((byte*)fTemp, byFileBuffer + i * 32 + 4, 16);
		pod->iOpen = (int)(fTemp[0]*1000 + 0.5);
		pod->iHigh = (int)(fTemp[1]*1000 + 0.5);
		pod->iLow = (int)(fTemp[2]*1000 + 0.5);
		pod->iClose = (int)(fTemp[3]*1000 + 0.5);
		pod++;

		
	}

	CChanModel* ccm = new CChanModel();
	ccm->Init(podTemp, iFileSize / 32);
	int iLineCount = ccm->GetLineCount();
	Line* pp = ccm->GetLineArray();
	int iSegmentCount = ccm->GetSegmentCount();
	Segment* ss = ccm->GetSegmentArray();

	FILE* fpOut = fopen("L.txt", "wb");
	fwrite((byte*)pp, 1, iLineCount*sizeof(Line), fpOut);
	fclose(fpOut);

	fpOut = fopen("S.txt", "wb");
	fwrite((byte*)ss, 1, iLineCount*sizeof(Segment), fpOut);
	fclose(fpOut);

	delete byFileBuffer;
	delete podTemp;
	delete pp;
	delete ss;

	return 0;
}

