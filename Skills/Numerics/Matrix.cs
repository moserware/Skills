using System;
using System.Collections.Generic;
using System.Linq;

namespace Moserware.Numerics
{
    /// <summary>
    /// Represents an MxN matrix with double precision values.
    /// </summary>    
    internal class Matrix
    {
        // Anything smaller than this will be assumed to be rounding error in terms of equality matching
        private const int FractionalDigitsToRoundTo = 10;
        private static readonly double ErrorTolerance = Math.Pow(0.1, FractionalDigitsToRoundTo); // e.g. 1/10^10

        protected double[][] _MatrixRowValues;
        // Note: some properties like Determinant, Inverse, etc are properties instead
        // of methods to make the syntax look nicer even though this sort of goes against
        // Framework Design Guidelines that properties should be "cheap" since it could take
        // a long time to compute these properties if the matrices are "big."

        protected Matrix()
        {
        }

        public Matrix(int rows, int columns, params double[] allRowValues)
        {
            Rows = rows;
            Columns = columns;

            _MatrixRowValues = new double[rows][];

            int currentIndex = 0;
            for (int currentRow = 0; currentRow < Rows; currentRow++)
            {
                _MatrixRowValues[currentRow] = new double[Columns];

                for (int currentColumn = 0; currentColumn < Columns; currentColumn++)
                {
                    if ((allRowValues != null) && (currentIndex < allRowValues.Length))
                    {
                        _MatrixRowValues[currentRow][currentColumn] = allRowValues[currentIndex++];
                    }
                }
            }
        }

        public Matrix(double[][] rowValues)
        {
            if (!rowValues.All(row => row.Length == rowValues[0].Length))
            {
                throw new ArgumentException("All rows must be the same length!");
            }

            Rows = rowValues.Length;
            Columns = rowValues[0].Length;
            _MatrixRowValues = rowValues;
        }

        protected Matrix(int rows, int columns, double[][] matrixRowValues)
        {
            Rows = rows;
            Columns = columns;
            _MatrixRowValues = matrixRowValues;
        }

        public Matrix(int rows, int columns, IEnumerable<IEnumerable<double>> columnValues)
            : this(rows, columns)
        {
            int columnIndex = 0;

            foreach (var currentColumn in columnValues)
            {
                int rowIndex = 0;
                foreach (double currentColumnValue in currentColumn)
                {
                    _MatrixRowValues[rowIndex++][columnIndex] = currentColumnValue;
                }
                columnIndex++;
            }
        }

        public int Rows { get; protected set; }
        public int Columns { get; protected set; }

        public double this[int row, int column]
        {
            get { return _MatrixRowValues[row][column]; }
        }

        public Matrix Transpose
        {
            get
            {
                // Just flip everything 
                var transposeMatrix = new double[Columns][];
                for (int currentRowTransposeMatrix = 0;
                     currentRowTransposeMatrix < Columns;
                     currentRowTransposeMatrix++)
                {
                    var transposeMatrixCurrentRowColumnValues = new double[Rows];
                    transposeMatrix[currentRowTransposeMatrix] = transposeMatrixCurrentRowColumnValues;

                    for (int currentColumnTransposeMatrix = 0;
                         currentColumnTransposeMatrix < Rows;
                         currentColumnTransposeMatrix++)
                    {
                        transposeMatrixCurrentRowColumnValues[currentColumnTransposeMatrix] =
                            _MatrixRowValues[currentColumnTransposeMatrix][currentRowTransposeMatrix];
                    }
                }

                return new Matrix(Columns, Rows, transposeMatrix);
            }
        }

        private bool IsSquare
        {
            get { return (Rows == Columns) && Rows > 0; }
        }

        public double Determinant
        {
            get
            {
                // Basic argument checking
                if (!IsSquare)
                {
                    throw new NotSupportedException("Matrix must be square!");
                }

                if (Rows == 1)
                {
                    // Really happy path :)
                    return _MatrixRowValues[0][0];
                }

                if (Rows == 2)
                {
                    // Happy path!
                    // Given:
                    // | a b |
                    // | c d |
                    // The determinant is ad - bc
                    double a = _MatrixRowValues[0][0];
                    double b = _MatrixRowValues[0][1];
                    double c = _MatrixRowValues[1][0];
                    double d = _MatrixRowValues[1][1];
                    return a*d - b*c;
                }

                // I use the Laplace expansion here since it's straightforward to implement.
                // It's O(n^2) and my implementation is especially poor performing, but the
                // core idea is there. Perhaps I should replace it with a better algorithm
                // later.
                // See http://en.wikipedia.org/wiki/Laplace_expansion for details

                double result = 0.0;

                // I expand along the first row
                for (int currentColumn = 0; currentColumn < Columns; currentColumn++)
                {
                    double firstRowColValue = _MatrixRowValues[0][currentColumn];
                    double cofactor = GetCofactor(0, currentColumn);
                    double itemToAdd = firstRowColValue*cofactor;
                    result += itemToAdd;
                }

                return result;
            }
        }

        public Matrix Adjugate
        {
            get
            {
                if (!IsSquare)
                {
                    throw new ArgumentException("Matrix must be square!");
                }

                // See http://en.wikipedia.org/wiki/Adjugate_matrix
                if (Rows == 2)
                {
                    // Happy path!
                    // Adjugate of:
                    // | a b |
                    // | c d |
                    // is
                    // | d -b |
                    // | -c a |

                    double a = _MatrixRowValues[0][0];
                    double b = _MatrixRowValues[0][1];
                    double c = _MatrixRowValues[1][0];
                    double d = _MatrixRowValues[1][1];

                    return new SquareMatrix(d, -b,
                                            -c, a);
                }

                // The idea is that it's the transpose of the cofactors                
                var result = new double[Columns][];

                for (int currentColumn = 0; currentColumn < Columns; currentColumn++)
                {
                    result[currentColumn] = new double[Rows];

                    for (int currentRow = 0; currentRow < Rows; currentRow++)
                    {
                        result[currentColumn][currentRow] = GetCofactor(currentRow, currentColumn);
                    }
                }

                return new Matrix(result);
            }
        }

        public Matrix Inverse
        {
            get
            {
                if ((Rows == 1) && (Columns == 1))
                {
                    return new SquareMatrix(1.0/_MatrixRowValues[0][0]);
                }

                // Take the simple approach:
                // http://en.wikipedia.org/wiki/Cramer%27s_rule#Finding_inverse_matrix
                return (1.0/Determinant)*Adjugate;
            }
        }

        public static Matrix operator *(double scalarValue, Matrix matrix)
        {
            int rows = matrix.Rows;
            int columns = matrix.Columns;
            var newValues = new double[rows][];

            for (int currentRow = 0; currentRow < rows; currentRow++)
            {
                var newRowColumnValues = new double[columns];
                newValues[currentRow] = newRowColumnValues;

                for (int currentColumn = 0; currentColumn < columns; currentColumn++)
                {
                    newRowColumnValues[currentColumn] = scalarValue*matrix._MatrixRowValues[currentRow][currentColumn];
                }
            }

            return new Matrix(rows, columns, newValues);
        }

        public static Matrix operator +(Matrix left, Matrix right)
        {
            if ((left.Rows != right.Rows) || (left.Columns != right.Columns))
            {
                throw new ArgumentException("Matrices must be of the same size");
            }

            // simple addition of each item

            var resultMatrix = new double[left.Rows][];

            for (int currentRow = 0; currentRow < left.Rows; currentRow++)
            {
                var rowColumnValues = new double[right.Columns];
                resultMatrix[currentRow] = rowColumnValues;
                for (int currentColumn = 0; currentColumn < right.Columns; currentColumn++)
                {
                    rowColumnValues[currentColumn] = left._MatrixRowValues[currentRow][currentColumn]
                                                     +
                                                     right._MatrixRowValues[currentRow][currentColumn];
                }
            }

            return new Matrix(left.Rows, right.Columns, resultMatrix);
        }

        public static Matrix operator *(Matrix left, Matrix right)
        {
            // Just your standard matrix multiplication.
            // See http://en.wikipedia.org/wiki/Matrix_multiplication for details

            if (left.Columns != right.Rows)
            {
                throw new ArgumentException("The width of the left matrix must match the height of the right matrix",
                                            "right");
            }

            int resultRows = left.Rows;
            int resultColumns = right.Columns;

            var resultMatrix = new double[resultRows][];

            for (int currentRow = 0; currentRow < resultRows; currentRow++)
            {
                resultMatrix[currentRow] = new double[resultColumns];

                for (int currentColumn = 0; currentColumn < resultColumns; currentColumn++)
                {
                    double productValue = 0;

                    for (int vectorIndex = 0; vectorIndex < left.Columns; vectorIndex++)
                    {
                        double leftValue = left._MatrixRowValues[currentRow][vectorIndex];
                        double rightValue = right._MatrixRowValues[vectorIndex][currentColumn];
                        double vectorIndexProduct = leftValue*rightValue;
                        productValue += vectorIndexProduct;
                    }

                    resultMatrix[currentRow][currentColumn] = productValue;
                }
            }

            return new Matrix(resultRows, resultColumns, resultMatrix);
        }

        private Matrix GetMinorMatrix(int rowToRemove, int columnToRemove)
        {
            // See http://en.wikipedia.org/wiki/Minor_(linear_algebra)

            // I'm going to use a horribly naïve algorithm... because I can :)
            var result = new double[Rows - 1][];
            int resultRow = 0;

            for (int currentRow = 0; currentRow < Rows; currentRow++)
            {
                if (currentRow == rowToRemove)
                {
                    continue;
                }

                result[resultRow] = new double[Columns - 1];

                int resultColumn = 0;

                for (int currentColumn = 0; currentColumn < Columns; currentColumn++)
                {
                    if (currentColumn == columnToRemove)
                    {
                        continue;
                    }

                    result[resultRow][resultColumn] = _MatrixRowValues[currentRow][currentColumn];
                    resultColumn++;
                }

                resultRow++;
            }

            return new Matrix(Rows - 1, Columns - 1, result);
        }

        private double GetCofactor(int rowToRemove, int columnToRemove)
        {
            // See http://en.wikipedia.org/wiki/Cofactor_(linear_algebra) for details
            // REVIEW: should things be reversed since I'm 0 indexed?
            int sum = rowToRemove + columnToRemove;
            bool isEven = (sum%2 == 0);

            if (isEven)
            {
                return GetMinorMatrix(rowToRemove, columnToRemove).Determinant;
            }
            else
            {
                return -1.0*GetMinorMatrix(rowToRemove, columnToRemove).Determinant;
            }
        }

        // Equality stuff
        // See http://msdn.microsoft.com/en-us/library/ms173147.aspx

        public static bool operator ==(Matrix a, Matrix b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object) a == null) || ((object) b == null))
            {
                return false;
            }

            if ((a.Rows != b.Rows) || (a.Columns != b.Columns))
            {
                return false;
            }
            
            for (int currentRow = 0; currentRow < a.Rows; currentRow++)
            {
                for (int currentColumn = 0; currentColumn < a.Columns; currentColumn++)
                {
                    double delta =
                        Math.Abs(a._MatrixRowValues[currentRow][currentColumn] -
                                 b._MatrixRowValues[currentRow][currentColumn]);

                    if (delta > ErrorTolerance)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool operator !=(Matrix a, Matrix b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            double result = Rows;
            result += 2*Columns;

            unchecked
            {
                for (int currentRow = 0; currentRow < Rows; currentRow++)
                {
                    bool eventRow = (currentRow%2) == 0;
                    double multiplier = eventRow ? 1.0 : 2.0;

                    for (int currentColumn = 0; currentColumn < Columns; currentColumn++)
                    {
                        double cellValue = _MatrixRowValues[currentRow][currentColumn];
                        double roundedValue = Math.Round(cellValue, FractionalDigitsToRoundTo);
                        result += multiplier*roundedValue;
                    }
                }
            }

            // Ok, now convert that double to an int
            byte[] resultBytes = BitConverter.GetBytes(result);

            var finalBytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                finalBytes[i] = (byte) (resultBytes[i] ^ resultBytes[i + 4]);
            }

            int hashCode = BitConverter.ToInt32(finalBytes, 0);
            return hashCode;
        }
        
        public override bool Equals(object obj)
        {
            var other = obj as Matrix;
            if (other == null)
            {
                return base.Equals(obj);
            }

            return this == other;
        }
    }

    internal class DiagonalMatrix : Matrix
    {
        public DiagonalMatrix(IList<double> diagonalValues)
            : base(diagonalValues.Count, diagonalValues.Count)
        {
            for (int i = 0; i < diagonalValues.Count; i++)
            {
                _MatrixRowValues[i][i] = diagonalValues[i];
            }
        }
    }

    internal class Vector : Matrix
    {
        public Vector(IList<double> vectorValues)
            : base(vectorValues.Count, 1, new IEnumerable<double>[] {vectorValues})
        {
        }
    }

    internal class SquareMatrix : Matrix
    {
        public SquareMatrix(params double[] allValues)
        {
            Rows = (int) Math.Sqrt(allValues.Length);
            Columns = Rows;

            int allValuesIndex = 0;

            _MatrixRowValues = new double[Rows][];
            for (int currentRow = 0; currentRow < Rows; currentRow++)
            {
                var currentRowValues = new double[Columns];
                _MatrixRowValues[currentRow] = currentRowValues;

                for (int currentColumn = 0; currentColumn < Columns; currentColumn++)
                {
                    currentRowValues[currentColumn] = allValues[allValuesIndex++];
                }
            }
        }
    }

    internal class IdentityMatrix : DiagonalMatrix
    {
        public IdentityMatrix(int rows)
            : base(CreateDiagonal(rows))
        {
        }

        private static double[] CreateDiagonal(int rows)
        {
            var result = new double[rows];
            for (int i = 0; i < rows; i++)
            {
                result[i] = 1.0;
            }

            return result;
        }
    }
}