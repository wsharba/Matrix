using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

class MatrixProcessor
{
    private int _size = 100;
    private DenseMatrix? _matrixA;
    private DenseMatrix? _matrixB;
    private DenseMatrix? _result;
    private bool _matricesGenerated = false;
    private bool _multiplied = false;

    public void Run()
    {
        while (true)
        {
            ShowMenu();
            Console.Write("\nEnter your choice (1-8): ");
            string? input = Console.ReadLine();

            if (!int.TryParse(input, out int choice) || choice < 1 || choice > 8)
            {
                Console.WriteLine("❌ Invalid choice. Please enter a number between 1 and 8.");
                continue;
            }

            try
            {
                switch (choice)
                {
                    case 1: InputSize(); break;
                    case 2: GenerateMatrices(); break;
                    case 3: MultiplyMatrices(); break;
                    case 4: InvertMatrix(); break;
                    case 5: CalculateDeterminant(); break;
                    case 6: DisplayEigenvalues(); break;
                    case 7: MakeDiagonal(); break;
                    case 8:
                        Console.WriteLine("👋 Goodbye!");
                        return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private void ShowMenu()
    {
        Console.WriteLine("=".PadRight(50, '='));
        Console.WriteLine("MATRIX OPERATIONS MENU".PadLeft(32));
        Console.WriteLine("=".PadRight(50, '='));
        Console.WriteLine($"Current size: {_size}x{_size}");
        Console.WriteLine("=".PadRight(50, '='));
        Console.WriteLine("(1) Input Size");
        Console.WriteLine("(2) Generate Random Matrices");
        Console.WriteLine("(3) Multiply Matrices");
        Console.WriteLine("(4) Invert Result Matrix");
        Console.WriteLine("(5) Calculate Determinant");
        Console.WriteLine("(6) Display Eigenvalues");
        Console.WriteLine("(7) Make Diagonal Matrix");
        Console.WriteLine("(8) Exit");
        Console.WriteLine("=".PadRight(50, '='));
    }

    private void InputSize()
    {
        Console.Write($"Enter new matrix size (current: {_size}): ");
        if (int.TryParse(Console.ReadLine(), out int newSize) && newSize > 0 && newSize <= 500)
        {
            _size = newSize;
            _matricesGenerated = false;
            _multiplied = false;
            _matrixA = null;
            _matrixB = null;
            _result = null;
            Console.WriteLine($"✅ Matrix size set to {_size}x{_size}");
        }
        else
        {
            Console.WriteLine("❌ Invalid size. Must be between 1 and 500.");
        }
    }

    private void GenerateMatrices()
    {
        Console.WriteLine("Generating random matrices...");
        var sw = Stopwatch.StartNew();

        var random = new Random();
        _matrixA = DenseMatrix.Create(_size, _size, (i, j) =>
            Math.Round(random.NextDouble() * 20 - 10, 2)); // Range [-10, 10]
        _matrixB = DenseMatrix.Create(_size, _size, (i, j) =>
            Math.Round(random.NextDouble() * 20 - 10, 2));

        _matricesGenerated = true;
        _multiplied = false;
        _result = null;
        sw.Stop();
        Console.WriteLine($"✅ Matrices generated in {sw.ElapsedMilliseconds} ms");
    }

    private void MultiplyMatrices()
    {
        if (!_matricesGenerated)
        {
            Console.WriteLine("❌ Generate matrices first (Option 2)!");
            return;
        }

        Console.WriteLine("Multiplying matrices...");
        var sw = Stopwatch.StartNew();
        _result = _matrixA! * _matrixB!;
        _multiplied = true;
        sw.Stop();
        Console.WriteLine($"✅ Multiplication completed in {sw.ElapsedMilliseconds} ms");
    }

    private void InvertMatrix()
    {
        if (!_multiplied)
        {
            Console.WriteLine("❌ Multiply matrices first (Option 3)!");
            return;
        }

        Console.WriteLine("Inverting result matrix...");
        var sw = Stopwatch.StartNew();
        try
        {
            // .Inverse() returns Matrix<double>, so cast to DenseMatrix
            var inverse = _result!.Inverse() as DenseMatrix;
            if (inverse == null)
            {
                throw new InvalidOperationException("Inverse operation did not return a DenseMatrix.");
            }

            _result = inverse;
            sw.Stop();
            Console.WriteLine($"✅ Inversion completed in {sw.ElapsedMilliseconds} ms");
            DisplayMatrixSection("Inverted Matrix", _result);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Matrix is singular (non-invertible) or ill-conditioned.", ex);
        }
    }

    private void CalculateDeterminant()
    {
        var matrix = GetTargetMatrix();
        if (matrix == null) return;

        Console.WriteLine("Calculating determinant...");
        var sw = Stopwatch.StartNew();
        double det = matrix.Determinant();
        sw.Stop();
        Console.WriteLine($"✅ Determinant = {det:E6} (computed in {sw.ElapsedMilliseconds} ms)");
    }

    private void DisplayEigenvalues()
    {
        var matrix = GetTargetMatrix();
        if (matrix == null) return;

        if (_size > 50)
        {
            Console.WriteLine($"⚠️ Warning: Eigenvalue computation for {_size}x{_size} matrix may take several seconds.");
            Console.Write("Continue? (y/n): ");
            if (Console.ReadLine()?.ToLower() != "y") return;
        }

        Console.WriteLine("Computing eigenvalues...");
        var sw = Stopwatch.StartNew();
        try
        {
            var evd = matrix.Evd();
            var eigenvalues = evd.EigenValues.ToArray();
            sw.Stop();

            Console.WriteLine($"✅ Computed {_size} eigenvalues in {sw.ElapsedMilliseconds} ms");
            Console.WriteLine("\nFirst 10 eigenvalues (real parts):");
            Console.WriteLine("-".PadRight(40, '-'));

            for (int i = 0; i < Math.Min(10, eigenvalues.Length); i++)
            {
                var ev = eigenvalues[i];
                string imagPart = Math.Abs(ev.Imaginary) > 1e-10
                    ? $" + {ev.Imaginary:F4}i"
                    : "";
                Console.WriteLine($"λ{i + 1,2}: {ev.Real,10:F4}{imagPart}");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Eigenvalue computation failed (matrix may be defective).", ex);
        }
    }

    private void MakeDiagonal()
    {
        var matrix = GetTargetMatrix();
        if (matrix == null) return;

        Console.WriteLine("Creating diagonal matrix from result...");
        var sw = Stopwatch.StartNew();

        // Create diagonal matrix directly as DenseMatrix
        var diagonalMatrix = DenseMatrix.CreateDiagonal(_size, _size, i => matrix[i, i]);

        _result = diagonalMatrix;
        _multiplied = true;
        sw.Stop();
        Console.WriteLine($"✅ Diagonal matrix created in {sw.ElapsedMilliseconds} ms");
        DisplayMatrixSection("Diagonal Matrix", _result);
    }

    private DenseMatrix? GetTargetMatrix()
    {
        if (!_multiplied)
        {
            Console.WriteLine("❌ Multiply matrices first (Option 3)!");
            return null;
        }
        return _result;
    }

    private void DisplayMatrixSection(string title, DenseMatrix matrix, int size = 5)
    {
        int showSize = Math.Min(size, _size);
        Console.WriteLine($"\n{title} (first {showSize}x{showSize} section):");
        Console.WriteLine("-".PadRight(60, '-'));

        for (int i = 0; i < showSize; i++)
        {
            for (int j = 0; j < showSize; j++)
            {
                Console.Write($"{matrix[i, j],10:F2} ");
            }
            Console.WriteLine();
        }
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Starting Matrix Processor...");
        Console.WriteLine("Note: For large matrices (>50), some operations may take time.\n");

        var processor = new MatrixProcessor();
        processor.Run();
    }
}