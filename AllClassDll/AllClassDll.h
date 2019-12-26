// AllClassDll.h

#pragma once

using namespace System;

namespace AllClassDll {


	public ref class MyCamera
	{
		// TODO:  在此处添加此类的方法。
	public:
		COCTCameraDll* _camera;

		int _pixel;
		int _line;
		int _model;
		int _picNum;
#pragma region 色散补偿相关系数
		float _c1;
		float _c2;
		float _c3;
		float _c4;
		float* _ks;
		float* _maxcorrectDATA;
#pragma endregion

		int _count;																		//各模式采集同一位置多张时计数
		int _ssbccount;

		void* _origin;																	//相机原始数据 指针
		float* _orinFloat;																//原始数据float类型
		float* _orin50Float;
		unsigned char* _afterSSBC50UcharTrans;
		unsigned char* _dispersionUChar;												//色散补偿出来的数据 uchar类型
		unsigned char* _dispersionUCharTrans;											//色散补偿出来的数据 的转置 uchar类型
		unsigned char* _OCTCastImgDst;
		unsigned char* _castImgResize;

		MyCamera();
		~MyCamera();

#pragma region OCT相机控制方法
		//具体信息请查看OCTCameraDll项目
		bool InitCamera();																//初始化相机
		bool OpenCamera();																//打开相机
		bool StartGetDataInit();														//开始获取数据
		bool StopCamera();																//停止抓取数据 并刷新缓存区
		bool CloseCamera();																//关闭相机
		bool FlushCamera();																//暂时无用
#pragma endregion

		bool InitData();												//各模式需要调用此DLL函数时的初始化变量									
		bool DeleteGPU();																//释放各模式运行时所创建的GPU空间 
																						//bool CatchModelInitData(int line, int model);									//从实时显示模式转到采集模式时 释放GPU空间 并重新开辟新的GPU空间
		IntPtr GetOCTClu();												//各模式实时显示时调用方法
		IntPtr GetData();													//各模式进行采集图像数据时调用
		IntPtr Ssbc();										//各模式色散补偿
		IntPtr CastCompute(int pixel, int line, int Nframe, float gamma);
#pragma region 保存数据
		void SaveByteData(IntPtr address, IntPtr data, int num);										//保存byte类型数据	 用来采集实验数据用
		void SaveFloatData(IntPtr address, IntPtr data, int num);										//保存float类型数据
#pragma endregion


	};


}
