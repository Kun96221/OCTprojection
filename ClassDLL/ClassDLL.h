// AllClassDll.h

#pragma once

using namespace System;

namespace AllClassDll {


	public ref class MyCamera
	{
		// TODO:  �ڴ˴���Ӵ���ķ�����
	public:
		COCTCameraDll* _camera;

		int _pixel;
		int _line;
		int _model;
		int _picNum;
#pragma region ɫɢ�������ϵ��
		float _c1;
		float _c2;
		float _c3;
		float _c4;
		float* _ks;
		float* _maxcorrectDATA;
#pragma endregion

		int _count;																		//��ģʽ�ɼ�ͬһλ�ö���ʱ����
		int _ssbccount;

		void* _origin;																	//���ԭʼ���� ָ��
		float* _orinFloat;																//ԭʼ����float����
		float* _orin50Float;
		unsigned char* _afterSSBC50UcharTrans;
		unsigned char* _dispersionUChar;												//ɫɢ�������������� uchar����
		unsigned char* _dispersionUCharTrans;											//ɫɢ�������������� ��ת�� uchar����
		unsigned char* _OCTCastImgDst;
		unsigned char* _castImgResize;

		MyCamera();
		~MyCamera();

#pragma region OCT������Ʒ���
		//������Ϣ��鿴OCTCameraDll��Ŀ
		bool InitCamera();																//��ʼ�����
		bool OpenCamera();																//�����
		bool StartGetDataInit();														//��ʼ��ȡ����
		bool StopCamera();																//ֹͣץȡ���� ��ˢ�»�����
		bool CloseCamera();																//�ر����
		bool FlushCamera();																//��ʱ����
#pragma endregion
		
		bool InitData();												//��ģʽ��Ҫ���ô�DLL����ʱ�ĳ�ʼ������									
		bool DeleteGPU();																//�ͷŸ�ģʽ����ʱ��������GPU�ռ� 
																	//bool CatchModelInitData(int line, int model);									//��ʵʱ��ʾģʽת���ɼ�ģʽʱ �ͷ�GPU�ռ� �����¿����µ�GPU�ռ�
		bool ResetCount();
		
		IntPtr GetOCTClu();												//��ģʽʵʱ��ʾʱ���÷���
		IntPtr GetData(int catchModel);													//��ģʽ���вɼ�ͼ������ʱ����
		IntPtr Ssbc(IntPtr srcImg);										//��ģʽɫɢ����
		IntPtr CastCompute(int pixel, int line, int Nframe, float gamma);
#pragma region ��������
		void SaveByteData(IntPtr address, IntPtr data, int num);										//����byte��������	 �����ɼ�ʵ��������
		void SaveFloatData(IntPtr address, IntPtr data, int num);										//����float��������
#pragma endregion


	};
	public ref class LSOCompute
	{
	private:

	public:
		LSOCompute();
		~LSOCompute();
		void LSOInitExcu(int pixel, int line, float High, float Low, float C);
		IntPtr LSOExcutex(IntPtr srcImg, IntPtr beijing, int pixel, int line);
		void LSOFeeExcu();

	};

	public ref class ReSize
	{
	public:

		static void ReSize3D(IntPtr srcimgdata, IntPtr dstimgdata, int Column, int Row, int PhotoNum);
		static void ReSize2D(IntPtr Src, int Row, int Column, int NewRow, int NewColumn, IntPtr Dst);
	};

	public ref class OCTCastCompute
	{
	private:

	public:
		OCTCastCompute();
		~OCTCastCompute();
		void InitCast(int pixel, int line, int Nframe);
		void CastCompute(IntPtr pDatas, IntPtr h_Fundus, int pixel, int line, int Nframe, float gamma);
		void FreeCast();
	};

}
