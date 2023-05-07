using ConsoleTables;

Console.WriteLine("Введите количество неизвестных");
var n = int.Parse(Console.ReadLine());
Console.WriteLine("Введите количество итераций");
var k = int.Parse(Console.ReadLine());

var results = new double[k + 1][];
var strings = new string[n];
Console.WriteLine("Введите СЛАУ");
for (var i = 0; i < n; i++)
    strings[i] = Console.ReadLine();

Console.WriteLine("Введите начальное приближение");
results[0] = Console.ReadLine().Split(' ').Select(double.Parse).ToArray();
if (results[0].Length != n)
    throw new Exception("Неверное начальное приближение");

Console.WriteLine("\n--- термы ---");
var terms = new string[n][];
for (var i = 0; i < n; i++)
{
    var x = ParseTerms(strings[i].Replace(" ", "")).ToArray();
    terms[i] = x;
    Console.WriteLine($"({string.Join(" ", x)})");
}

Console.WriteLine("\n--- диагональное преобладание ---");
var elements = new double[n][];
for (var i = 0; i < n; i++)
{
    var x = ParseElements(terms[i]).ToArray();
    elements[i] = x;
}

DiagonalElements(elements);
for (var i = 0; i < n; i++)
    Console.WriteLine($"[{string.Join(" ", elements[i])}]");

Console.WriteLine("\n--- вычисление ---");
for (var i = 1; i <= k; i++)
    results[i] = Iterate(n, elements, results[i - 1]);

var columns = new[] {"i"}.Concat(Enumerable.Range(1, n).Select(x => $"x_{x}")).ToArray();
var table = new ConsoleTable(columns);
for (var i = 0; i <= k; i++)
{
    var row = new[] {(object) i}.Concat(results[i].Select(x => Math.Round(x, 4)).Cast<object>()).ToArray();
    table.AddRow(row);
}

table.Write(Format.Minimal);

static double[] Iterate(int n, double[][] elements, double[] pervious)
{
    var c = new double[n][];
    var d = new double[n];
    for (var i = 0; i < n; i++)
    {
        d[i] = elements[i][^1] / elements[i][i];
        c[i] = new double[n];
        for (var j = 0; j < c[i].Length; j++)
        {
            c[i][j] = i == j ? 0 : -(elements[i][j] / elements[i][i]);
            if (c[i][j] > 1)
            {
                Console.WriteLine(
                    "Метод Зейделя не сходится для указанной СЛАУ. Приведите матрицу к диагональному преобладанию.");
                Environment.Exit(0);
            }
        }
    }

    var result = new double[n];
    for (var i = 0; i < n; i++)
    {
        var sum = d[i];
        for (var j = 0; j < i; j++)
            sum += c[i][j] * result[j];
        for (var j = i; j < n; j++)
            sum += c[i][j] * pervious[j];
        result[i] = sum;
    }

    return result;
}

static void DiagonalElements(double[][] elements)
{
    for (var i = 0; i < elements.Length; i++)
    {
        var maxRowIndex = i;
        for (var j = i + 1; j < elements.Length; j++)
            if (Math.Abs(elements[j][i]) > Math.Abs(elements[maxRowIndex][i]))
                maxRowIndex = j;

        if (maxRowIndex != i)
            for (var j = 0; j < elements[0].Length; j++)
            {
                var temp = elements[i][j];
                elements[i][j] = elements[maxRowIndex][j];
                elements[maxRowIndex][j] = temp;
            }

        for (var j = i + 1; j < elements.Length; j++)
        {
            var coeff = elements[j][i] / elements[i][i];
            for (var k = i; k < elements[j].Length; k++)
                elements[j][k] -= coeff * elements[i][k];
        }
    }
}

static IEnumerable<double> ParseElements(IEnumerable<string> terms)
{
    foreach (var term in terms)
    {
        var index = term.IndexOf('x');
        var numString = index > 0 ? term.Substring(0, index) : term;
        yield return numString == "+" ? 1 : numString == "-" ? -1 : double.Parse(numString);
    }
}

static IEnumerable<string> ParseTerms(string s)
{
    var normalizeTerm = (string s) => s.StartsWith('+') || s.StartsWith('-') ? s : $"+{s}";

    var start = 0;
    for (var i = 0; i < s.Length; i++)
    {
        var c = s[i];
        if (c is '+' or '-' && i > 0)
        {
            yield return normalizeTerm(s.Substring(start, i - start));
            start = i;
        }

        if (c is '=')
        {
            yield return normalizeTerm(s.Substring(start, i - start));
            yield return normalizeTerm(s.Substring(i + 1));
            break;
        }
    }
}