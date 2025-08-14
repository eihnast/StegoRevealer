# StegoRevealer

[![Stego Revealer Pipeline](https://github.com/eihnast/StegoRevealer/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/eihnast/StegoRevealer/actions/workflows/dotnet-desktop.yml)

---

### Программа комплексного стегоаналитического детектирования "StegoRevealer"
_ПО [зарегистрировано](https://new.fips.ru/registers-doc-view/fips_servlet?DB=EVM&DocNumber=2025614424&TypeFile=html) Федеральным институтом промышленной собственности Федеральной службы по интеллектуальной собственности Министерства экономического развития Российской Федерации 21.02.2025, №2025614424._

## Описание

<img align="right" width="100" src="Docs/AppLogo.png">
StegoRevealer - стегоаналитический детектор, предназначенный для обнаружения стеганографического встраивания данных в файлы изображений.

#### Поддерживаемые форматы анализируемых изображений:
* PNG
* BMP

#### StegoRevealer позволяет выполнить стегоанализ следующими методами:
* CSA (Chi-Square Attack): Метод оценки по критерию Хи-квадрат;
* RS (Regular-Singular);
* SPA (Sample Pair Analysis);
* FAN (Fast Additive Noise / HCF-COM);
* CKZhA (Consecutive Koch-Zhao Attack): Атака на последовательное встраивание по методу КохаЖао (метод Белима и Вильховского);
* ZCA (Zhilkin Compression Analysis): Метод анализа поведения при сжатии данных (метод Жилкина, Меленцовой и Рябко);
* Метод комплексного статистического стегоанализа (бинарный классификатор, формирующий вывод на основе вычисления оценок CSA, RS, CKZhA, безэталонных статистических характеристик изображения (шум, резкость, размытость, контраст, энтропия Шеннона, энтропия Реньи) и размера изображения)

#### Поддерживаемые ОС:
* Windows 10 / Windows 11 (.exe, протестирован в: Windows 11 24H2)
* Linux (ELF, протестирован в: Ubuntu 24.04, Kubuntu 25.04, Mint 22.1, Alt Workstation 10.04, RedOS 8)

#### Логирование и конфигурация:

* По умолчанию в программе включено логирование операций. Логи и настройки хранятся во временной папке пользователя. Для Windows: ``c:\Users\user\AppData\Local\StegoRevealer``
* Файл конфигурации создаётся при первом запуске программы автоматически.
* Настройки десктопного приложения могут быть установлены через графичесекий интерфейс программы в соответствующей вкладке.
* Переход к временной папке с логами и настройками можно осуществить через соответствующую кнопку в меню параметров.

## Графический интерфейс

Настройки десктопной версии программы хранятся в файле ``StegoRevealerSettings.json``.

StegoRevealer предоставляет графический пользовательский интерфейс для осуществления ручного стегоанализа специалистом-аналитиком.

<img align="center" src="Docs/StegoRevealerExample.gif">

При включении и успешном завершении работы методов CSA и RS в ходе стегоанализа доступна кнопка "Совместный вывод". С её помощью можно оценить результаты согласно методике совместного формирования стегоаналитического вывода (https://doi.org/10.21683/1729-2646-2021-21-3-39-46).

Пример окна совместного вывода:

<img align="center" src="Docs/JointConclusionWindow.jpg">

## API

<img align="right" width="100" src="Docs/APILogo.png">

Программа выпускается также в форме сервера API. Конфигурация сервера API хранится в файле ``StegoRevealerApiSettings.json``.

По умолчанию сервер запускается на порте 11038 (HTTP) и 11040 (HTTPS).

### Методы API

| Метод         | Тип  | Путь                  | Тело запроса / параметры           | Назначение анализа                     |
| ------------- | ---- | --------------------- | ---------------------------------- | -------------------------------------- |
| `GetDecision` | GET  | `/api/sa/getDecision` | `path`, `verboseResult` (optional) | Упрощённый вызов `ComplexSsaAsync`     |
| `GetDecision` | POST | `/api/sa/getDecision` | JSON: `ComplexSsaRequest`          | Упрощённый вызов `ComplexSsaAsync`     |
| `ComplexSsa`  | GET  | `/api/sa/complexSsa`  | `path`, `verboseResult` (optional) | Комплексный анализ (SSA)               |
| `ComplexSsa`  | POST | `/api/sa/complexSsa`  | JSON: `ComplexSsaRequest`          | Комплексный анализ (SSA)               |
| `Full`        | GET  | `/api/sa/full`        | `path`                             | Упрощённый вызов `FullAnalysis`        |
| `Full`        | POST | `/api/sa/full`        | JSON: `FullAnalysisRequest`        | Упрощённый вызов `FullAnalysis`        |
| `FullAnalysis`| GET  | `/api/sa/fullAnalysis`| `path`                             | Объединённый анализ (все методы)       |
| `FullAnalysis`| POST | `/api/sa/fullAnalysis`| JSON: `FullAnalysisRequest`        | Объединённый анализ (все методы)       |
| `Csa`         | GET  | `/api/sa/csa`         | `path`                             | Chi-Square Attack (CSA)                |
| `Csa`         | POST | `/api/sa/csa`         | JSON: `CsaRequest`                 | Chi-Square Attack (CSA)                |
| `Rs`          | GET  | `/api/sa/rs`          | `path`                             | Regular-Singular (RS)                  |
| `Rs`          | POST | `/api/sa/rs`          | JSON: `RsRequest`                  | Regular-Singular (RS)                  |
| `Spa`         | GET  | `/api/sa/spa`         | `path`                             | Sample Pair Analysis (SPA)             |
| `Spa`         | POST | `/api/sa/spa`         | JSON: `SpaRequest`                 | Sample Pair Analysis (SPA)             |
| `Fan`         | GET  | `/api/sa/fan`         | `path`                             | Fast Additive Noise (FAN) / HCF-COM    |
| `Fan`         | POST | `/api/sa/fan`         | JSON: `FanRequest`                 | Fast Additive Noise (FAN) / HCF-COM    |
| `Ckzha`       | GET  | `/api/sa/ckzha`       | `path`                             | Стегоанализ скрытия по Коха-Жао (CKZhA)|
| `Ckzha`       | POST | `/api/sa/ckzha`       | JSON: `CkzhaRequest`               | Стегоанализ скрытия по Коха-Жао (CKZhA)|
| `Zca`         | GET  | `/api/sa/zca`         | `path`                             | Анализ поведения при сжатии (ZCA)      |
| `Zca`         | POST | `/api/sa/zca`         | JSON: `ZcaRequest`                 | Анализ поведения при сжатии (ZCA)      |
| `Statm`       | GET  | `/api/sa/statm`       | `path`                             | Расчёт оценок качества изображения     |
| `Statm`       | POST | `/api/sa/statm`       | JSON: `StatmRequest`               | Расчёт оценок качества изображения     |

Во всех случаях:
* `path` - это путь к файлу изображения, который необходимо проанализировать.
* `verboseResult` - возвращает все результаты работы задействованных методов стегоанализа, а не только вывод методики комплексного статистического стегоанализа.
* POST-методы принимают JSON-запросы, которые должны включать указание:
  * либо `ImageUrl` - пути к файлу изображения;
  * либо `ImageData` - изображение, закодированное в Base64 (без префиксов с указанием типа).

## Параметры методов стегоанализа

Все указанные параметры могут быть переданы в теле соответствующих POST-запросов в формате JSON.

Также данные параметры настраиваются в графическом интерфейсе десктопного приложения.

Общие типы:
* `TraverseType`: `Horizontal` / `Vertical` - тип обхода матрицы пикселей;
* `Channels`: `Red`, `Green`, `Blue` - цветовые каналы.

### CSA

| Имя параметра                  | Тип              | Описание                                                          | Значение по умолчанию  |
| ------------------------------ | ---------------- | ----------------------------------------------------------------- | ---------------------  |
| `Visualize`                    | `bool`           | Визуализировать подозрительную область                            | `false`                |
| `TraverseType`                 | Тип обхода       | Тип обхода матрицы пикселей                                       | `Horizontal`           |
| `UseSeparateChannelsCalc`      | `bool`           | Применять ли алгоритм по отдельности для каждого канала           | `true`                 |
| `UseUnitedCnum`                | `bool`           | Считать ли общее количество интенсивности цветов без учёта канала | `true`                 |
| `UsePreviousCnums`             | `bool`           | Использовать ли режим подсчёта с накоплением                      | `true`                 |
| `ExcludeZeroPairs`             | `bool`           | Исключать ли из анализа пары, где ожидаемая частота цвета = 0     | `true`                 |
| `UseUnifiedCathegories`        | `bool`           | Объединять ли низкочастотные категории                            | `true`                 |
| `UnifyingCathegoriesThreshold` | `int`            | Верхний порог частот для объединения                              | 4                      |
| `Threshold`                    | `double`         | Порог значения p-value                                            | 0.95                   |
| `Channels`                     | Массив каналов   | Анализируемые каналы                                              | `Red`, `Green`, `Blue` |
| `BlockWidth`                   | `int`            | Ширина анализируемого блока                                       | Ширина изображения     |
| `BlockHeight`                  | `int`            | Высота анализируемого блока                                       | 1                      |

### RS

| Имя параметра                  | Тип              | Описание                    | Значение по умолчанию  |
| ------------------------------ | ---------------- | --------------------------- | ---------------------- |
| `TraverseType`                 | Тип обхода       | Тип обхода матрицы пикселей | `Horizontal`           |
| `Channels`                     | Массив каналов   | Анализируемые каналы        | `Red`, `Green`, `Blue` |
| `BlockWidth`                   | `int`            | Ширина анализируемого блока | 4                      |
| `BlockHeight`                  | `int`            | Высота анализируемого блока | 1                      |

### SPA

| Имя параметра                  | Тип              | Описание                                                                                | Значение по умолчанию  |
| ------------------------------ | ---------------- | --------------------------------------------------------------------------------------- | ---------------------- |
| `MethodVersion`                | Версия метода    | Версия метода                                                                           | `Original`             |
| `Direction`                    | Направление      | Направление анализа пар пикселей (если не включён UseDoubleDirection)                   | `Horizontal`           |
| `Channels`                     | Массив каналов   | Анализируемые каналы                                                                    | `Red`, `Green`, `Blue` |
| `UseDoubleDirection`           | `bool`           | Выполнять двухпроходный алгоритм (с горизонтальным и вертикальным направлением анализа) | `true`                 |

* `MethodVersion`: `Original` / `StegExpose` - версия метода SPA (из оригинальной статьи или из реализации в StegExpose);
* `Direction`: `Horizontal` / `Vertical` / `Diagonal` - направление выбора пар пикселей.

### FAN

| Имя параметра                  | Тип              | Описание                  | Значение по умолчанию  |
| ------------------------------ | ---------------- | ------------------------- | ---------------------- |
| `Threshold`                    | `double`         | Пороговое значение        | 3.401714170610843      |

### CKZhA

| Имя параметра                  | Тип                       | Описание                                                                                  | Значение по умолчанию        |
| ------------------------------ | ------------------------- | ----------------------------------------------------------------------------------------- | ---------------------------- |
| `Threshold`                    | `double`                  | Минимальный порог разницы коэффициентов, превышение служит сигналом о наличии встраивания | 20                           |
| `CutCoefficient`               | `double`                  | Порог отсечки для массива значений разности между dct-коэффициентами                      | 0.35                         |
| `TraverseType`                 | Тип обхода                | Тип обхода матрицы пикселей                                                               | `Horizontal`                 |
| `Channels`                     | Массив каналов            | Анализируемые каналы                                                                      | `Blue`                       |
| `AnalysisCoeffs`               | Массив пар коэффициентов  | Анализируемые пары коэффициентов матриц ДКП                                               | `(2, 3)`, `(2, 4)`, `(3, 4)` |
| `TryToExtract`                 | `bool`                    | Пробовать извлечь информацию автоматически                                                | `true`                       |
| `LoggingCSequences`            | `bool`                    | Включить логирование полных последовательностей cSequence                                 | `false`                      |

* `AnalysisCoeffs`: массив кортежей типа `(int, int)`, указывающих на индексы коэффициентов матриц ДКП (левый верхний коэффициент имеет индексы `(0, 0)`).

### ZCA

| Имя параметра                  | Тип              | Описание                                                          | Значение по умолчанию        |
| ------------------------------ | ---------------- | ----------------------------------------------------------------- | ---------------------------- |
| `TraverseType`                 | Тип обхода       | Тип обхода матрицы пикселей                                       | `Horizontal`                 |
| `Channels`                     | Массив каналов   | Анализируемые каналы                                              | `Red`, `Green`, `Blue`       |
| `CompressingAlgorithm`         | Метод сжатия     | Используемый метод сжатия                                         | `ZIP`                        |
| `RatioThreshold`               | `double`         | Порог средней разности степени сжатия для определения встраивания | 0.008                        |
| `UseOverallCompression`        | `bool`           | Использовать ли сжатие всего изображения (а не поканально)        | `true`                       |
| `BlockWidth`                   | `int`            | Ширина анализируемого блока                                       | 16                           |
| `BlockHeight`                  | `int`            | Высота анализируемого блока                                       | 16                           |

* `CompressingAlgorithm`: `ZIP` / `BZIP2` / `GZIP` - используемый метод сжатия.

### Метод комплексного статистического стегоанализа

Специфические параметры методы отсутствуют

### Примеры запросов и ответов API

GET-запрос метода комплексного статистического стегоанализа с подробным ответом
```
GET http://localhost:11038/api/sa/getDecision?path=e:\img1.png&verboseResult=true
```
Ответ:
```json
{
    "isHidingDetected": false,
    "steganalysisResult": {
        "chiSquareHorizontalResult": {
            "messageRelativeVolume": 0.03756906077348066,
            "messageRelativeVolumesByChannels": {
                "Red": 0.03756906077348066,
                "Green": 0.03756906077348066,
                "Blue": 0.03756906077348066
            },
            "elapsedTime": 1171,
            "hasErrors": false,
            "methodSuccessful": true
        },
        "chiSquareVerticalResult": {
            "messageRelativeVolume": 0.03756906077348066,
            "messageRelativeVolumesByChannels": {
                "Red": 0.03756906077348066,
                "Green": 0.03756906077348066,
                "Blue": 0.03756906077348066
            },
            "elapsedTime": 1207,
            "hasErrors": false,
            "methodSuccessful": true
        },
        "rsResult": {
            "messageRelativeVolume": 0.04891949547753832,
            "messageRelativeVolumesByChannels": {
                "Red": 0.04931201359356092,
                "Green": 0.0629869137533115,
                "Blue": 0.03445955908574254
            },
            "elapsedTime": 1174,
            "hasErrors": false,
            "methodSuccessful": true
        },
        "kzhaHorizontalResult": {
            "suspiciousIntervalIsFound": false,
            "threshold": 0,
            "coefficients": {
                "firstIndex": 2,
                "secondIndex": 3,
                "firstValue": 2,
                "secondValue": 3
            },
            "messageBitsVolume": 0,
            "extractedData": null,
            "suspiciousInterval": null,
            "elapsedTime": 986,
            "hasErrors": false,
            "methodSuccessful": true
        },
        "kzhaVerticalResult": {
            "suspiciousIntervalIsFound": false,
            "threshold": 0,
            "coefficients": {
                "firstIndex": 2,
                "secondIndex": 3,
                "firstValue": 2,
                "secondValue": 3
            },
            "messageBitsVolume": 0,
            "extractedData": null,
            "suspiciousInterval": null,
            "elapsedTime": 986,
            "hasErrors": false,
            "methodSuccessful": true
        },
        "statmResult": {
            "noiseValue": 24.422111332221387,
            "sharpnessValue": 82.5,
            "blurValue": 0.43435297405369355,
            "contrastValue": 0.7423836637778976,
            "entropyValues": {
                "shennon": 7.58329440639484,
                "vaida": 0,
                "tsallis": 0,
                "renyi": 7.553087210270699,
                "havard": 0
            },
            "elapsedTime": 1350,
            "hasErrors": false,
            "methodSuccessful": true
        },
        "pixelsNum": 814500,
        "isHidingDetected": false,
        "decisionProbability": 0.3095238208770752,
        "elapsedTime": 1379,
        "hasErrors": false,
        "methodSuccessful": true
    }
}
```

Пример POST-запроса `getDecision` с указанием пути файла:
```
POST http://localhost:11038/api/sa/getDecision
```
```json
{
    "ImageUrl": "https://some-domain.com/img1.png"
}
```
Пример POST-запроса `getDecision` с указанием данных изображения:
```
POST http://localhost:11038/api/sa/getDecision
```
```json
{
    "ImageData": "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"
}
```