using Accord;

namespace StegoRevealer.StegoCore.ScMath;

/// <summary>
/// Apache-реализация оценки по критерию Хи-квадрат
/// </summary>
public static class ChiSquareApache
{

    // Предопределённые константы

    private static List<double> _lanczos = new()
    {
        0.99999999999999709182,
        57.156235665862923517,
        -59.597960355475491248,
        14.136097974741747174,
        -0.49191381609762019978,
        0.33994649984811888699e-4,
        0.46523628927048575665e-4,
        -0.98374475304879564677e-4,
        0.15808870322491248884e-3,
        -0.021026444172410488319e-3,
        0.21743961811521264320e-3,
        -0.16431810653676389022e-3,
        0.84418223983852743293e-4,
        -0.26190838401581408670e-4,
        0.36899182659531622704e-5
    };

    private static double _halfLog2Pi = 0.5 * Math.Log(2.0 * Math.PI);
    private static double _defaultEpsilon = 10e-15;


    // Функции расчёта

    private static double? LogGamma(double x)
    {
        if (x <= 0)
            return null;

        var sum = _lanczos[0];
        for (int i = 1; i < _lanczos.Count; i++)
            sum += (_lanczos[i] / (x + 1));

        var tmp = x + 0.5 + (607 / 128);
        return ((x + 0.5) * Math.Log(tmp)) - tmp + _halfLog2Pi + Math.Log(sum / x);
    }

    private static double? RegularizedGammaP(double a, double x, double epsilon, long maxIterations)
    {
        if (a <= 0.0 || x < 0)
            return null;
        if (x.Equals(0.0))
            return 0.0;
        if (x >= a + 1)
            return 1.0 - RegularizedGammaQ(a, x, epsilon, maxIterations);

        double n = 0.0;
        double an = 1 / a;
        double sum = an;

        while (Math.Abs(an / sum) > epsilon && n < maxIterations && sum < double.PositiveInfinity)
        {
            n = n + 1.0;
            an = an * (x / (a + n));
            sum = sum + an;
        }

        if (double.IsInfinity(sum))
            return 1.0;

        var logGammaA = LogGamma(a);
        if (!logGammaA.HasValue)
            throw new ArithmeticException("logGamma returns null");

        var res = Math.Exp(-x + (a * Math.Log(x)) - logGammaA.Value) * sum;
        return res;
    }

    private static double? RegularizedGammaP(double a, double x)
    {
        return RegularizedGammaP(a, x, _defaultEpsilon, (long)Math.Pow(2, 32) - 1);
    }

    private static bool Isfinite(double val)
    {
        return double.IsFinite(val);
    }

    private static double? RegularizedGammaQ(double a, double x, double epsilon, long maxIterations)
    {
        if (a <= 0.0 || x < 0)
            return null;
        if (x.Equals(0.0))
            return 1;
        if (x < a + 1.0)
            return 1 - RegularizedGammaP(a, x, epsilon, maxIterations);

        double? ret = 1.0 / ContinuedFraction(x, epsilon, maxIterations,
            (n, x) => ((2.0 * n) + 1.0) - a + x, (n) => n * (a - n));

        if (!ret.HasValue)
            throw new ArithmeticException("ret is null");

        var logGammaA = LogGamma(a);
        if (!logGammaA.HasValue)
            throw new ArithmeticException("logGamma returns null");
        return Math.Exp(-x + (a * Math.Log(x)) - logGammaA.Value) * ret;
    }

    private static double? CumulativeProbability(double x, int degreesOfFreedom)
    {
        if (x <= 0.0)
            return 0.0;
        return RegularizedGammaP((double)degreesOfFreedom / 2, x / 2);
    }

    private static void CheckPositive(IEnumerable<double> arr)
    {
        foreach (var value in arr)
            if (value <= 0.0)
                throw new ArgumentException($"arr contains not positive value: {value}");
    }

    private static void CheckNonNegative(IEnumerable<double> arr)
    {
        foreach (var value in arr)
            if (value < 0.0)
                throw new ArgumentException($"arr contains not non-negative value: {value}");
    }

    private static double? ContinuedFraction(double x, double epsilon, long maxIterations,
        Func<double, double, double> getA, Func<double, double> getB)
    {
        double p0 = 1.0;
        double p1 = getA(0.0, x);
        double q0 = 0.0;
        double q1 = 1.0;
        double c = p1 / q1;
        double n = 0.0;
        double relativeError = (double)long.MaxValue;

        while (n < maxIterations && relativeError > epsilon)
        {
            n++;
            var a = getA(n, x);
            var b = getB(n);
            var p2 = a * p1 + b * p0;
            var q2 = a * q1 + b * q0;
            bool infinite = false;

            if (!Isfinite(p2) || !Isfinite(q2))
            {
                double scaleFactor = 1.0;
                double lastScaleFactor = 1.0;
                int maxPower = 5;

                double scale = Math.Max(a, b);

                if (scale <= 0.0)
                    throw new ArithmeticException("scale is less than zero");

                infinite = true;

                for (int i = 0; i < maxPower; i++)
                {
                    lastScaleFactor = scaleFactor;
                    scaleFactor *= scale;
                    if (!a.Equals(0.0) && a > b)
                    {
                        p2 = p1 / lastScaleFactor + (b / scaleFactor * p0);
                        q2 = q1 / lastScaleFactor + (b / scaleFactor * q0);
                    }
                    else if (!b.Equals(0.0))
                    {
                        p2 = (a / scaleFactor * p1) + p0 / lastScaleFactor;
                        q2 = (a / scaleFactor * q1) + q0 / lastScaleFactor;
                    }

                    infinite = !Isfinite(p2) || !Isfinite(q2);
                    if (!infinite)
                        break;
                }
            }

            if (infinite)
                throw new ArithmeticException("Can't scale");
            if (q2.Equals(0.0))
                throw new ArithmeticException("q2 is zero, can't divide");

            var r = p2 / q2;

            relativeError = Math.Abs(r / c - 1.0);

            c = p2 / q2;
            p0 = p1;
            p1 = p2;
            q0 = q1;
            q1 = q2;
        }

        if (n >= maxIterations)
            throw new ArithmeticException("Non convergent");

        return c;
    }

    private static double? ChiSquare(List<double> expected, List<double> observed)
    {
        if (expected.Count < 2)
            throw new ArgumentException("Length of expected array should be grather than 1");
        if (expected.Count != observed.Count)
            throw new ArgumentException("Length of expected and observed arrays should be equal");

        CheckPositive(expected);
        CheckNonNegative(observed);

        double sumExpected = 0.0;
        double sumObserved = 0.0;

        for (int i = 0; i < observed.Count; i++)
        {
            sumExpected += expected[i];
            sumObserved += observed[i];
        }

        double ratio = 1.0;
        bool rescale = false;

        if (Math.Abs(sumExpected - sumObserved) > 10E-6)
        {
            ratio = sumObserved / sumExpected;
            rescale = true;
        }

        double sumSq = 0.0;

        for (int i = 0; i < observed.Count; i++)
        {
            if (rescale)
            {
                double dev = observed[i] - ratio * expected[i];
                sumSq += dev * dev / (ratio * expected[i]);
            }
            else
            {
                double dev = observed[i] - expected[i];
                sumSq += dev * dev / expected[i];
            }
        }

        return sumSq;
    }

    public static (double Statistic, double PValue) ChiSquareTest(List<double> expected, List<double> observed)
    {
        double? chi2 = ChiSquare(expected, observed);
        if (!chi2.HasValue)
            throw new ArithmeticException("Chi-Square statistic can't be calculate");

        var cp = CumulativeProbability(chi2.Value, expected.Count - 1);
        double? p = 1 - cp;
        if (!p.HasValue)
            throw new ArithmeticException("Error while calculating PValue");

        return (chi2.Value, p.Value);
    }
}
