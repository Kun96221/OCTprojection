// OCTCameraDll.cpp : ���� DLL Ӧ�ó���ĵ���������
//

#include "stdafx.h"
#include "OCTCameraDll.h"


COCTCameraDll::COCTCameraDll()
{
	hCamera = NULL;
	iNbOfBuffer = 128;
	timeOut = 1000;
	picHight = 512;
	picWidth = 2048;
	MAX = 15;
	LP = 893;		//���ٶ�
	model = 2;
}
COCTCameraDll::~COCTCameraDll()
{
	delete hCamera;
}
bool COCTCameraDll::InitCamera()
{
	int nError;
	unsigned long ulNbCameras;
	nError = USB3_InitializeLibrary();
	if (nError != CAM_ERR_SUCCESS)                                            //��ʼ�������
		return false;
	else {
		nError = USB3_UpdateCameraList(&ulNbCameras);
		if (nError != CAM_ERR_SUCCESS)                                        //Ѱ������������ҵ����������һ���б���
			return false;
		else {
			for (unsigned long ulIndex = 0; ulIndex < ulNbCameras; ulIndex++)
			{
				nError = USB3_GetCameraInfo(ulIndex, &CameraInfo);
				if (nError != CAM_ERR_SUCCESS)                                //��������б��������Ϣ
				{
					return false;
				}
			}
			return true;
		}
	}
}

bool COCTCameraDll::OpenCamera()
{
	int nError;
	nError = USB3_OpenCamera(&CameraInfo, &hCamera);
	if (nError != CAM_ERR_SUCCESS)
	{
		return false;
	}
	else {
		int ETMax = 0, ETMin = 0;
		size_t size = 4;
		//Line_period = 2000;//20΢��
		USB3_WriteRegister(hCamera, 0x12100, &LP, &size);//���ٶ�
		USB3_WriteRegister(hCamera, 0x1210C, &model, &size);//����ģʽ,3�ⲿ�ߴ���														//USB3_ReadRegister(hCamera, 0x12108, &ET, &size);														//Exposure_time = ET / 100;
		USB3_WriteRegister(hCamera, 0x12114, &MAX, &size);
		return true;
	}
}

bool COCTCameraDll::StartGetDataInit(int height)
{
	int nError;
	picHight = height;
	nError = USB3_SetImageParameters(hCamera, picHight, iNbOfBuffer);
	if (nError != CAM_ERR_SUCCESS)
		return false;

	nError = USB3_StartAcquisition(hCamera);
	if (nError != CAM_ERR_SUCCESS)
		return false;

	return true;
}

void* COCTCameraDll::GetData()
{
	int nError;
	nError = USB3_GetBuffer(hCamera, &ImageInfos, timeOut);
	if (nError != CAM_ERR_SUCCESS)
	{
		if (nError == CAM_ERR_TIMEOUT)
		{
			return NULL;
		}
		else {
			return NULL;
		}
	}

	if (nError == CAM_ERR_SUCCESS)
	{
		nError = USB3_RequeueBuffer(hCamera, ImageInfos.hBuffer);
		if (nError != CAM_ERR_SUCCESS)
		{
			return NULL;
		}
	}
	return ImageInfos.pDatas;
}

bool COCTCameraDll::StopCamera()
{
	int nError;
	nError = USB3_StopAcquisition(hCamera);
	nError = USB3_FlushBuffers(hCamera);
	if (nError != CAM_ERR_SUCCESS)
	{
		return false;
	}
	else {
		return true;
	}
}

bool COCTCameraDll::CloseCamera()
{
	int nError;
	nError = USB3_CloseCamera(hCamera);
	if (nError != CAM_ERR_SUCCESS)
	{
		return false;
	}
	else {
		nError = USB3_TerminateLibrary();
		if (nError != CAM_ERR_SUCCESS)
		{
			return false;
		}
		else {
			return true;
		}
	}
}

bool COCTCameraDll::FlushCamera()
{
	int nError;

	nError = USB3_FlushBuffers(hCamera);
	if (nError == CAM_ERR_SUCCESS)
	{
		return true;
	}
	else
		return true;

}
