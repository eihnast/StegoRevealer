# StegoRevealer

[![Stego Revealer Pipeline](https://github.com/eihnast/StegoRevealer/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/eihnast/StegoRevealer/actions/workflows/dotnet-desktop.yml)

---

### ��������� ������������ ������������������� �������������� "StegoRevealer"
_�� [����������������](https://new.fips.ru/registers-doc-view/fips_servlet?DB=EVM&DocNumber=2025614424&TypeFile=html) ����������� ���������� ������������ ������������� ����������� ������ �� ���������������� ������������� ������������ �������������� �������� ���������� ��������� 21.02.2025, �2025614424._

## ��������

<img align="right" width="100" src="Docs/AppLogo.png">
StegoRevealer - ������������������ ��������, ��������������� ��� ����������� ������������������� ����������� ������ � ����� �����������.

�������������� ������� ������������� �����������:
* PNG
* BMP

StegoRevealer ���������� ����� ������������ ������������ ��� ������������������� ������������ ������ � ������� ���� ���������� ���������� ���������� � �����������. �������������� ��������� ������ ����� ���� ��������� ��������. � �� �����:
* ����� ������ �� �������� ��-�������;
* ����� Regular-Singular;
* ����������� ����������� ����������������� �������, ������������� �� ������ ����-���;
* ��������� �������� ������������ �������������� ������������� ����������� (���, ��������, ����������, ��������, �������� �������, �������� �����).

## ����������� ���������

StegoRevealer ������������� ����������� ���������������� ��������� ��� ������������� ������� ������������ ������������-����������.

������� ���� ���������:
<img align="center" src="Docs/GUIScreen_Main.jpg">

������ ����������� ������������:
<img align="center" src="Docs/GUIScreen_Analyzed.jpg">

������� ���� ��������������� (�������� ���������� ���������� ������):
<img align="center" src="Docs/GUIScreen_Extractor.jpg">

������ ����������� ���������� ������:
<img align="center" src="Docs/GUIScreen_Extracted.jpg">

## ������ API (Beta)

<img align="right" width="100" src="Docs/APILogo.png">
��������� ����������� ����� � ����� API-�������. �� ������ ������ �� �� ����� ������������ � ����������� �� ������ http://localhost:5000.

������� ������ � ����������� ������� ��� ������������� ������������ �������� ��������� �������:
```
http://localhost:5000/api/sa/getDecision?path=IMG_PATH&verboseResult=true
```
``verboseResult`` �������� ����� ��������� ������ � ���� ����������� ���������� ��������� ���������� � �������� ������������ ������������.

����� ������� ������������� ������������������ ���������� � ������� JSON. ��������:
```json
{
  "isHidingDetected": true,
  "steganalysisResult": {
    "chiSquareHorizontalVolume": 0.14794921875,
    "chiSquareVerticalVolume": 0.14794921875,
    "rsVolume": 0.098581594474544,
    "kzhaHorizontalThreshold": 0,
    "kzhaHorizontalMessageBitVolume": 0,
    "kzhaVerticalThreshold": 0,
    "kzhaVerticalMessageBitVolume": 0,
    "noiseValue": 1.80602136215222,
    "sharpnessValue": 125.157900270019,
    "blurValue": 2.10231001762825,
    "contrastValue": 0.312726282286162,
    "entropyShennonValue": 6.64421417564485,
    "entropyRenyiValue": 6.5442823668356,
    "pixelsNumber": 4194304
  }
}
```
