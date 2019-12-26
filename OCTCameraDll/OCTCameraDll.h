// 下列 ifdef 块是创建使从 DLL 导出更简单的
// 宏的标准方法。此 DLL 中的所有文件都是用命令行上定义的 OCTCAMERADLL_EXPORTS
// 符号编译的。在使用此 DLL 的
// 任何其他项目上不应定义此符号。这样，源文件中包含此文件的任何其他项目都会将
// OCTCAMERADLL_API 函数视为是从 DLL 导入的，而此 DLL 则将用此宏定义的
// 符号视为是被导出的。
#pragma once
#include "CamCmosOctUsb3.h"
#ifdef OCTCAMERADLL_EXPORTS
#define OCTCAMERADLL_API __declspec(dllexport)
#else
#define OCTCAMERADLL_API __declspec(dllimport)
#endif

// 此类是从 OCTCameraDll.dll 导出的
class OCTCAMERADLL_API COCTCameraDll {
private:
	tCameraInfo CameraInfo;                  //相机信息
	void *hCamera;							//相机句柄
	tImageInfos ImageInfos;                //图片信息

	size_t iNbOfBuffer;               //图像缓存数目

	size_t picHight;                         //图像高度，图像默认横向像素点位2048
	size_t picWidth;                          //图像宽度

	size_t timeOut;                           //最长等待时间
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

