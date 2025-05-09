﻿using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Entities;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;

/// <summary>
/// Параметры оценки статистических метрик
/// </summary>
public class StatmParameters
{
    /// <summary>
    /// Изображение
    /// </summary>
    public ImageHandler Image { get; set; }


    // Параметры подсчёта шума

    /// <summary>Шаг выбора строк для подсчёта минимальных дисперсий (Метод 2)</summary>
    public int NoiseCalcSteps { get; set; } = 50;  // h

    /// <summary>Делитель высоты изображения, если с заданным шагом такого количества строк не выбрать (Метод 2)</summary>
    public int NoiseCalcStepsDivider { get; set; } = 8;

    /// <summary>Число интервалов, на которые разбивается строка (Метод 2)</summary>
    public int NoiseCalcIntervalNumber { get; set; } = 4;

    /// <summary>Число блоков по длинной стороне, на которые разбивается изображение (Метод 1)</summary>
    public int NoiseCalcBlocksNumber { get; set; } = 16;

    /// <summary>Количество фиксируемых блоков с минимальной дисперсией (Метод 1)</summary>
    public int NoiseCalcFixedBlocksCount { get; set; } = 5;

    /// <summary>Число строк, которые фиксируются в блоке (Метод 1)</summary>
    public int NoiseCalcRowsInBlock { get; set; } = 3;


    // Параметры подсчёта резкости

    /// <summary>Значение "слабого" пикселя</summary>
    public byte SharpnessCalcWeakPixel { get; set; } = 25;

    /// <summary>Значение "сильного" пикселя</summary>
    public byte SharpnessCalcStrongPixel { get; set; } = 255;

    /// <summary>Размер ядра сглаживания по Гауссу</summary>
    public int SharpnessCalcGuassianKernelSize { get; set; } = 5;

    /// <summary>Сигма ядра сглаживания по Гауссу</summary>
    public double SharpnessCalcGuassianKernelSigma { get; set; } = 1.0;

    /// <summary>Использовать ли оператор Щарра вместо оригинального оператора Собеля</summary>
    public bool SharpnessCalcUseScharrOperator { get; set; } = false;

    /// <summary>Использовать ли усреднение для вычисления grayscale байта (если false, будет использован BT-709 (HDTV))</summary>
    public bool SharpnessCalcUseAveragedGrayscale { get; set; } = false;  // https://onlinejpgtools.com/convert-jpg-to-grayscale

    /// <summary>Размер окрестности, в которой ищутся экстремумы относительно краевого пикселя</summary>
    public int SharpnessCalcExtremumsNeighborhoodSize { get; set; } = 3;

    /// <summary>Верхний порог двойной пороговой фильтрации Канни</summary>
    public double SharpnessCalcCannyUpThreshold { get; set; } = 0.5;

    /// <summary>Нижний порог двойной пороговой фильтрации Канни</summary>
    public double SharpnessCalcCannyDownThreshold { get; set; } = 0.4;


    // Параметры подсчёта размытости

    /// <summary>k1</summary>
    public int BlurCalcFilterSizeK1 { get; set; } = 5;

    /// <summary>k2 > k1</summary>
    public int BlurCalcFilterSizeK2 { get; set; } = 7;

    /// <summary>Использовать ли усреднение для вычисления grayscale байта (если false, будет использован BT-709 (HDTV))</summary>
    public bool BlurCalcUseAveragedGrayscale { get; set; } = false;  // https://onlinejpgtools.com/convert-jpg-to-grayscale


    // Параметры подсчёта контраста

    /// <summary>Размер центра окна вычисления локальной оценки контраста</summary>
    public int ContrastCalcWindowCenterSize { get; set; } = 3;

    /// <summary>Использовать ли усреднение для вычисления grayscale байта (если false, будет использован BT-709 (HDTV))</summary>
    public bool ContrastCalcUseAveragedGrayscale { get; set; } = false;  // https://onlinejpgtools.com/convert-jpg-to-grayscale


    // Параметры подсчёта энтропии

    /// <summary>Методы, используемые для подсчёта энтропии</summary>
    public EntropyMethods EntropyMethods { get; set; } = EntropyMethods.Shennon | EntropyMethods.Renyi;

    /// <summary>Параметр чувствительности, не стоит задавать >3</summary>
    public double EntropyCalcSensitivity { get; set; } = 1.1;  // 2.5

    /// <summary>Использовать ли усреднение для вычисления grayscale байта (если false, будет использован BT-709 (HDTV))</summary>
    public bool EntropyCalcUseAveragedGrayscale { get; set; } = false;  // https://onlinejpgtools.com/convert-jpg-to-grayscale



    public StatmParameters(ImageHandler image)
    {
        Image = image;
    }
}
