using StegoRevealer.Utils.Common.Entities;

namespace StegoRevealer.Utils.Common.Lib;

public static class StartParametersHelper
{
    public static double? TryGetDoubleParameter(string[] args, string parameterName)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(parameterName, StringComparison.OrdinalIgnoreCase))
            {
                double.TryParse(args[i + 1], out var value);
                return value;
            }
        }

        return null;
    }

    public static string? GetSpecifiedParameter(string[] args, IEnumerable<string> avaliableParameterNames) =>
        args.FirstOrDefault(arg => avaliableParameterNames.Contains(arg));

    public static bool IsParameterSpecified(string[] args, IEnumerable<string> avaliableParameterNames) =>
        !string.IsNullOrEmpty(GetSpecifiedParameter(args, avaliableParameterNames));

    public static string? GetSpecifiedParameter(string[] args, InputParameter inputParameter) =>
        args.FirstOrDefault(arg => inputParameter.AvailableNames.Contains(arg));

    public static bool IsParameterSpecified(string[] args, InputParameter inputParameter) =>
        !string.IsNullOrEmpty(GetSpecifiedParameter(args, inputParameter));

    public static bool IsParametersValid(string[] args, params InputParameter[] inputParameters)
    {
        var inputParametersList = inputParameters.ToList();

        int i = 0;
        while (i < args.Length)
        {
            // Проверяем, что этот параметр есть в каком-нибудь списке параметров
            var inputParameter = inputParametersList.FirstOrDefault(ip => ip.AvailableNames.Contains(args[i]));
            if (inputParameter is null)
                return false;

            // Если нашли список, где есть этот параметр, убираем список: дважды один параметр указан быть не должен
            inputParametersList.Remove(inputParameter);

            // Если параметр со значением, просто пропускаем следующий, корректность значения не валидируем (как и не проверяем, что значение параметра пропущено)
            if (inputParameter.HasValue)
                i += 2;
            else
                i++;
        }

        return true;
    }
}
