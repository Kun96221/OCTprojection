// ���� ifdef ���Ǵ���ʹ�� DLL �������򵥵�
// ��ı�׼�������� DLL �е������ļ��������������϶���� OCTCAMERADLL_EXPORTS
// ���ű���ġ���ʹ�ô� DLL ��
// �κ�������Ŀ�ϲ�Ӧ����˷��š�������Դ�ļ��а������ļ����κ�������Ŀ���Ὣ
// OCTCAMERADLL_API ������Ϊ�Ǵ� DLL ����ģ����� DLL ���ô˺궨���
// ������Ϊ�Ǳ������ġ�
#pragma once
#include "CamCmosOctUsb3.h"
#ifdef OCTCAMERADLL_EXPORTS
#define OCTCAMERADLL_API __declspec(dllexport)
#else
#define OCTCAMERADLL_API __declspec(dllimport)
#endif

// �����Ǵ� OCTCameraDll.dll ������
class OCTCAMERADLL_API COCTCameraDll {
private:
	tCameraInfo CameraInfo;                  //�����Ϣ
	void *hCamera;							//������
	tImageInfos ImageInfos;                //ͼƬ��Ϣ

	size_t iNbOfBuffer;               //ͼ�񻺴���Ŀ

	size_t picHight;                         //ͼ��߶ȣ�ͼ��Ĭ�Ϻ������ص�λ2048
	size_t picWidth;                          //ͼ����

	size_t timeOut;                           //��ȴ�ʱ��
	double MAX;
	int LP;
	int model;
public:
	COCTCameraDll();
	~COCTCameraDll();
	bool InitCamera();
	bool OpenCamera();
	bool StartGetDataInit(int height);
	void* GetData();
	bool StopCamera();
	bool CloseCamera();
	bool ChangeConfig();
	bool FlushCamera();
};

