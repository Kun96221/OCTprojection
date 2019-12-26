// 这是主 DLL 文件。

#include "stdafx.h"

#include "ClassDll.h"
#include <math.h>

#pragma region OCT相关
AllClassDll::MyCamera::MyCamera()
{
	_camera = new COCTCameraDll();

	_line = 320;
	_pixel = 2048;
	_picNum = 320;
	_ks = new float[2048];
	_maxcorrectDATA = new float[1];
	_c1 = -0.1f;
	_c2 = (9.49f)*powf(10, -5.0f);
	_c3 = (0.98f)*powf(10, -8.0f);
	_c4 = (0.0f)*powf(10, -10.0f);


	TxtRead2("kss.txt", "ab+", _ks, 2048);
}

AllClassDll::MyCamera::~MyCamera()
{
	delete _camera;
	delete _ks;
	delete _maxcorrectDATA;
}

#pragma region OCT相机采集

bool AllClassDll::MyCamera::InitData()
{
	InitSsbcGPU(_pixel, _line, 1, _ks);			//初始化512垂直扫模式GPU空间
	InitFund(_pixel, _line, _picNum);

	_orinFloat = new float[_pixel*_line];
	_orin50Float = new float[_picNum * _pixel*_line];
	_dispersionUChar = new unsigned char[_pixel*_line / 2];
	_dispersionUCharTrans = new unsigned char[_pixel*_line / 2];
	_afterSSBC50UcharTrans = new unsigned char[_pixel*_line / 2 * _picNum];
	_OCTCastImgDst = new unsigned char[_picNum*_line];
	_castImgResize = new unsigned char[_line*_line];

	_count = 0;
	_ssbccount = 0;

	return true;
}

bool AllClassDll::MyCamera::InitCamera()
{
	return _camera->InitCamera();
}
bool AllClassDll::MyCamera::OpenCamera() {
	return _camera->OpenCamera();
}
bool AllClassDll::MyCamera::StartGetDataInit() {
	return _camera->StartGetDataInit(_line);
}
bool AllClassDll::MyCamera::FlushCamera()
{
	return _camera->FlushCamera();
}


IntPtr AllClassDll::MyCamera::GetData(int catchModel) {
	_origin = _camera->GetData();
	if (_origin != nullptr)
	{
		switch (catchModel)
		{
		case 1:
			for (int i = 0; i < _pixel * _line; i++)
			{
				_orinFloat[i] = *(((unsigned short int*)_origin + i));
			}

			return (IntPtr)_orinFloat;

		default:

			return (IntPtr)nullptr;
		}
	}
	else {
		return (IntPtr)nullptr;
	}

}

bool AllClassDll::MyCamera::StopCamera() {
	return _camera->StopCamera();
}
bool AllClassDll::MyCamera::CloseCamera() {
	return _camera->CloseCamera();
}

bool AllClassDll::MyCamera::DeleteGPU()
{
	FreeValSsbcGPU();
	Freefund();

	delete _orinFloat;
	delete _orin50Float;
	delete _dispersionUChar;
	delete _dispersionUCharTrans;
	delete _afterSSBC50UcharTrans;
	delete _OCTCastImgDst;
	delete _castImgResize;

	return true;
}

bool AllClassDll::MyCamera::ResetCount()
{
	_count = 0;
	_ssbccount = 0;
	return true;
}

IntPtr AllClassDll::MyCamera::GetOCTClu()//实时显示部分
{

	if (_origin != nullptr)
	{
		//无特殊情况外 所有实时显示都以下模式
		for (int i = 0; i < _line*_pixel; i++)
		{
			_orinFloat[i] = *(((unsigned short int*)_origin + i));
		}

		SXBF_WNSUCHAR(_orinFloat, _pixel, _line, 1, _c1, _c2, _c3, _c4, 6.0f, 0.0f, _dispersionUChar, (int)0, _maxcorrectDATA);

		for (int i = 0; i < _line; i++)
		{
			for (int j = 0; j < _pixel / 2; j++)
			{
				_dispersionUCharTrans[i + j * _line] = _dispersionUChar[j + i * _pixel / 2];
			}
		}

		return (IntPtr)_dispersionUCharTrans;


	}
	else {
		return (IntPtr)nullptr;
	}
}

#pragma endregion

#pragma region  图像处理方法
//色散补偿
IntPtr AllClassDll::MyCamera::Ssbc(IntPtr srcImg) {

	SXBF_WNSUCHAR((float*)srcImg.ToPointer(), _pixel, _line, 1, _c1, _c2, _c3, _c4, 6.0f, 0.0f, _dispersionUChar, (int)0, _maxcorrectDATA);

	for (int i = 0; i < _line; i++)
	{
		for (int j = 0; j < _pixel / 2; j++)
		{
			_dispersionUCharTrans[ i + j * _line] = _dispersionUChar[j + i * _pixel / 2];
		}
	}

	_ssbccount++;
	return (IntPtr)_dispersionUCharTrans;
}

IntPtr AllClassDll::MyCamera::CastCompute(int pixel, int line, int Nframe, float gamma)
{
	Fundtee(_afterSSBC50UcharTrans, _OCTCastImgDst, pixel, line, Nframe, gamma);

	OpencvResize(_OCTCastImgDst, Nframe, line, line, line, _castImgResize);

	return (IntPtr)_castImgResize;
}
#pragma endregion

#pragma region 保存数据 float/unsigned char

void AllClassDll::MyCamera::SaveByteData(IntPtr address, IntPtr data, int num)
{
	//parameter 1：保存路径			parameter 2：创建文件模式 具体可百度fopen函数   
	//parameter 3：传入保存数据     parameter 4：数据长度		
	SaveImgs((char*)address.ToPointer(), "ab+", (unsigned char*)data.ToPointer(), num);
}

void AllClassDll::MyCamera::SaveFloatData(IntPtr address, IntPtr data, int num)
{
	SaveImg((char*)address.ToPointer(), "ab+", (float*)data.ToPointer(), num);
}

#pragma endregion

#pragma endregion

#pragma region LSO图像计算

AllClassDll::LSOCompute::LSOCompute()
{

}
AllClassDll::LSOCompute::~LSOCompute()
{
}

IntPtr AllClassDll::LSOCompute::LSOExcutex(IntPtr srcImg, IntPtr beijing, int pixel, int line)
{
	return (IntPtr)Excute((unsigned char*)srcImg.ToPointer(), (unsigned char*)beijing.ToPointer(), pixel, line);
}

void AllClassDll::LSOCompute::LSOInitExcu(int pixel, int line, float High, float Low, float C)
{
	InitExc(pixel, line, High, Low, C);
}

void AllClassDll::LSOCompute::LSOFeeExcu()
{
	FreeExc();
}
#pragma endregion

#pragma region 三维显示 Resize

void AllClassDll::ReSize::ReSize3D(IntPtr srcimgdata, IntPtr dstimgdata, int Column, int Row, int PhotoNum)
{
	Img128To3D((unsigned char*)srcimgdata.ToPointer(), (unsigned char *)dstimgdata.ToPointer(), Column, Row, PhotoNum);
}

void AllClassDll::ReSize::ReSize2D(IntPtr Src, int Row, int Column, int NewRow, int NewColumn, IntPtr Dst)
{
	OpencvResize((unsigned char*)Src.ToPointer(), Row, Column, NewRow, NewColumn, (unsigned char*)Dst.ToPointer());
}

#pragma endregion

#pragma region OCT图投影计算

AllClassDll::OCTCastCompute::OCTCastCompute()
{

}
AllClassDll::OCTCastCompute::~OCTCastCompute()
{
}

void AllClassDll::OCTCastCompute::InitCast(int pixel, int line, int Nframe)
{
	InitFund(pixel, line, Nframe);
}

void AllClassDll::OCTCastCompute::CastCompute(IntPtr pDatas, IntPtr h_Fundus, int pixel, int line, int Nframe, float gamma)
{
	Fundtee((unsigned char*)pDatas.ToPointer(), (unsigned char*)h_Fundus.ToPointer(), pixel, line, Nframe, gamma);
}

void AllClassDll::OCTCastCompute::FreeCast()
{
	Freefund();
}
#pragma endregion

