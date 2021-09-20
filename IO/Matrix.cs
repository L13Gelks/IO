using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace IO
{
    public struct Fraction
    {
        public Fraction(int n, int d)
        {
            N = n;
            D = d;
        }

        public int N { get; private set; }
        public int D { get; private set; }
    }
    class Matrix
    {
        int tiempoEspera = 10000;
        private double[,] Matriz;
        private double[,] MatrizIdentidad;
        private double[,] copiaMatriz;
        private int cols;
        private int rows;
        private string[] strPunteroOriginal;
        private string[] strPunteroCopia;
        private string[] strPunteroIdentidad;
        private TextBox filasTextBox;
        private TextBox columnasTextBox;
        private TextBox informationTextBox;
        private Panel panel1;
        private int pasos = 0;
        private double maxRelativeError = 0.0001;
        void Seek(ref int a, ref int b, int ainc, int binc, Func<int, int, bool> f)
        {
            a += ainc;
            b += binc;

            if (f(a, b))
            {
                int weight = 1;

                do
                {
                    weight *= 2;
                    a += ainc * weight;
                    b += binc * weight;
                }
                while (f(a, b));

                do
                {
                    weight /= 2;

                    int adec = ainc * weight;
                    int bdec = binc * weight;

                    if (!f(a - adec, b - bdec))
                    {
                        a -= adec;
                        b -= bdec;
                    }
                }
                while (weight > 1);
            }
        }
        public void deleteAllProgressBars() {
            foreach (Control control in Application.OpenForms["Form1"].Controls)
            {
                if (control is ProgressBar)
                {
                    Application.OpenForms["Form1"].Controls.Remove(control);
                }
            }
        }
        public Fraction RealToFraction(double value, double accuracy)
        {
            if (accuracy <= 0.0 || accuracy >= 1.0)
            {
                throw new ArgumentOutOfRangeException("accuracy", "Must be > 0 and < 1.");
            }

            int sign = Math.Sign(value);

            if (sign == -1)
            {
                value = Math.Abs(value);
            }

            // Accuracy is the maximum relative error; convert to absolute maxError
            double maxError = sign == 0 ? accuracy : value * accuracy;

            int n = (int)Math.Floor(value);
            value -= n;

            if (value < maxError)
            {
                return new Fraction(sign * n, 1);
            }

            if (1 - maxError < value)
            {
                return new Fraction(sign * (n + 1), 1);
            }

            // The lower fraction is 0/1
            int lower_n = 0;
            int lower_d = 1;

            // The upper fraction is 1/1
            int upper_n = 1;
            int upper_d = 1;

            while (true)
            {
                // The middle fraction is (lower_n + upper_n) / (lower_d + upper_d)
                int middle_n = lower_n + upper_n;
                int middle_d = lower_d + upper_d;

                if (middle_d * (value + maxError) < middle_n)
                {
                    // real + error < middle : middle is our new upper
                    Seek(ref upper_n, ref upper_d, lower_n, lower_d, (un, ud) => (lower_d + ud) * (value + maxError) < (lower_n + un));
                }
                else if (middle_n < (value - maxError) * middle_d)
                {
                    // middle < real - error : middle is our new lower
                    Seek(ref lower_n, ref lower_d, upper_n, upper_d, (ln, ld) => (ln + upper_n) < (value - maxError) * (ld + upper_d));
                }
                else
                {
                    // Middle is our best fraction
                    return new Fraction((n * middle_d + middle_n) * sign, middle_d);
                }
            }
        }

        public Matrix(int rows, int cols) {
            filasTextBox = Application.OpenForms["Form1"].Controls["filasTextBox"] as TextBox;
            columnasTextBox = Application.OpenForms["Form1"].Controls["columnasTextBox"] as TextBox;
            informationTextBox = Application.OpenForms["Form1"].Controls["informationTextBox"] as TextBox;
            panel1 = Application.OpenForms["Form1"].Controls["panel1"] as Panel;
            this.rows = rows;
            this.cols = cols;
            Matriz = new double[rows, cols];
            MatrizIdentidad = new double[rows, cols];
            copiaMatriz = new double[rows, cols];
        }
        public int getRows()
        {
            return rows;
        }

        public int getCols()
        {
            return cols;
        }

        public int getSize()
        {
            return cols * rows;
        }

        public int stepsCount() {
            return pasos;
        }

        public void imprimirMatriz(string matrix)
        {
            double[,] m = null;
            switch (matrix) {
                case "Original":
                    m = Matriz;
                    break;
                case "Copia":
                    m = copiaMatriz;
                    break;
                case "Identidad":
                    m = MatrizIdentidad;
                    break;
            }
            int count = 1;
            foreach (double element in m)
            {
                Console.Write(element);
                Console.Write("  ");
                if (count == Matriz.GetLength(1)) { Console.WriteLine('\n'); count = 0; }
                count++;
            }
            Console.Write("-----------------------\n");
        }

        public void llenarMatriz()
        {
            int count = 0;

            double[] vector = new double[rows * cols];
            foreach (var control in panel1.Controls)
            {
                var textBox = control as TextBox;
                if (textBox != null && strPunteroOriginal.Contains(textBox.Name))
                {
                    vector[count] = Double.Parse(textBox.Text);
                    count++;
                }
            }
            count = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    MatrizIdentidad[i, j] = 0;
                    if (i == j)
                    {
                        MatrizIdentidad[i, j] = 1;
                    }
                    Matriz[j, i] = vector[count];
                    count++;
                }
            }
            copiaMatriz = (double[,])Matriz.Clone();
        }

        private void convertirDiagonalesEnUnos(double[,] copiaMatriz, double[,] MatrizIdentidad, int x, int y)
        {
            //convertir numeros diagonales en 1
            double numerito = copiaMatriz[y, x];

            if (numerito == -1)
            {
                for (int f = 0; f < copiaMatriz.GetLength(1); f++)
                {
                    //multiplicar toda la fila para obtener 1 en y,x
                    copiaMatriz[y, f] *= -1;
                    MatrizIdentidad[y, f] *= -1;
                }
                informationTextBox.AppendText(" -1f" + (y + 1).ToString());
            }
            else
            {
                numerito = 1 / numerito;
                for (int f = 0; f < copiaMatriz.GetLength(1); f++)
                {
                    //multiplicar toda la fila para obtener 1 en y,x
                    copiaMatriz[y, f] = copiaMatriz[y, f] * (numerito);
                    MatrizIdentidad[y, f] = MatrizIdentidad[y, f] * (numerito);
                }

                if (numerito % 1 != 0) {
                    Fraction fractionA = RealToFraction(numerito, maxRelativeError);
                    informationTextBox.AppendText(fractionA.N.ToString() + "/" + fractionA.D.ToString() + "f" + (y + 1).ToString());
                }
                else
                {
                    informationTextBox.AppendText(numerito.ToString() + "f" + (y + 1).ToString());
                }
            }

            Console.Write("Matrix\n");
            imprimirMatriz("Copia");
            Console.Write("Matriz identidad\n");
            imprimirMatriz("Identidad");
        }

        private void convertirCamposEnCeros(string tipo, ref double[,] copiaMatriz, ref double[,] MatrizIdentidad, ref int x, ref int y, ref int pivoteDiagonal)
        {
            int valorCambiante = 0;
            int valorCambiante2 = 0;
            bool saltar = true;
            switch (tipo) {
                case "UnoDiagonal":
                    saltar = false;
                    break;
                case "IniciaCero":
                    int filaValida = 0;
                    for (int d = 0; d < copiaMatriz.GetLength(1); d++)
                    {
                        if (copiaMatriz[d, x] != 0)
                        {
                            filaValida = d;
                        }
                    }
                    valorCambiante = filaValida;
                    valorCambiante2 = filaValida;
                    break;
                case "CeroArriba":
                case "CeroAbajo":
                    valorCambiante = pivoteDiagonal - 1;
                    valorCambiante2 = y;
                    break;
            }
            if (saltar) {
                double[] vector = new double[copiaMatriz.GetLength(1)];
                double[] vectorIdentidad = new double[copiaMatriz.GetLength(1)];

                double valor = copiaMatriz[valorCambiante2, x] * -1;

                informationTextBox.AppendText(" " + valor.ToString() + "f" + (valorCambiante + 1).ToString() + " + " + "f" + (y + 1).ToString());

                for (int f = 0; f < copiaMatriz.GetLength(1); f++)
                {
                    vector[f] = copiaMatriz[valorCambiante, f] * valor;
                    vectorIdentidad[f] = MatrizIdentidad[valorCambiante, f] * valor;
                }

                for (int f = 0; f < copiaMatriz.GetLength(1); f++)
                {
                    copiaMatriz[y, f] = copiaMatriz[y, f] + vector[f];
                    MatrizIdentidad[y, f] = MatrizIdentidad[y, f] + vectorIdentidad[f];
                }
            }
            if (tipo == "UnoDiagonal" || tipo == "IniciaCero")
            {
                convertirDiagonalesEnUnos(copiaMatriz, MatrizIdentidad, x, y);
                if (pivoteDiagonal != 0)
                {
                    y = -1;
                }
                pivoteDiagonal++;
            }

            imprimirMatriz("Copia");
            imprimirMatriz("Identidad");
        }

        public async Task<bool> calcularInveras()
        {
            ProgressBar pBar = new ProgressBar();
            pBar.Location = new System.Drawing.Point(20, 250);
            pBar.Name = "progressBar1";
            pBar.Width = 600;
            pBar.Height = 10;
            pBar.Minimum = 0;
            pBar.Maximum = 100;
            pBar.Value = 0;
            Application.OpenForms["Form1"].Controls.Add(pBar);

            int x = 0;
            int y = 0;
            int pivoteDiagonal = 0;
            while (true)
            {
                pBar.Value += 6;
                Console.Write("Paso: ");
                Console.Write(pasos++);
                Console.Write("\n");
                informationTextBox.Text = "Paso: " + pasos.ToString() + Environment.NewLine;

                if (x == y && y == pivoteDiagonal)
                {
                    if (copiaMatriz[y, x] == 0)
                    {
                        convertirCamposEnCeros("IniciaCero", ref copiaMatriz, ref MatrizIdentidad, ref x, ref y, ref pivoteDiagonal);
                    }
                    else if (copiaMatriz[y, x] == 1) {
                        informationTextBox.AppendText(" Sin paso, valor 1 en (" + (x + 1).ToString() + "," + (y + 1).ToString() + ") ya existe");
                        pivoteDiagonal++;
                        y = -1;
                    }
                    else
                    {
                        convertirCamposEnCeros("UnoDiagonal", ref copiaMatriz, ref MatrizIdentidad, ref x, ref y, ref pivoteDiagonal);
                    }
                }
                else if (copiaMatriz[y, x] != 0 && y >= pivoteDiagonal && y != x)
                {
                    convertirCamposEnCeros("CeroArriba", ref copiaMatriz, ref MatrizIdentidad, ref x, ref y, ref pivoteDiagonal);
                }
                else if (copiaMatriz[y, x] != 0 && y <= pivoteDiagonal && y != x)
                {
                    convertirCamposEnCeros("CeroAbajo", ref copiaMatriz, ref MatrizIdentidad, ref x, ref y, ref pivoteDiagonal);
                } else if (copiaMatriz[y, x] == 0) {
                    informationTextBox.AppendText(" Sin paso, valor 0 en (" + (x + 1).ToString() + "," + (y + 1).ToString() + ") ya existe");
                }

                //avanzar por la matriz
                y++;
                if (y == copiaMatriz.GetLength(1))
                {
                    y = pivoteDiagonal;
                    x++;
                }
                else if (pivoteDiagonal != 0 && y == pivoteDiagonal - 1 && y < copiaMatriz.GetLength(1))
                {
                    y++;
                }

                drawMatrix();

                await PutTaskDelay();

                if (y >= copiaMatriz.GetLength(1))
                {
                    pBar.Value = 100;
                    return true;
                }
            }
        }
        public double[,] calcularInversa(double[,] matrix)
        {
            int x = 0;
            int y = 0;
            int pivoteDiagonal = 0;
            while (true)
            {
                if (x == y && y == pivoteDiagonal)
                {
                    if (matrix[y, x] == 0)
                    {
                        convertirCamposEnCeros("IniciaCero", ref matrix, ref matrix, ref x, ref y, ref pivoteDiagonal);
                    }
                    else if (matrix[y, x] == 1)
                    {
                        pivoteDiagonal++;
                        y = -1;
                    }
                    else
                    {
                        convertirCamposEnCeros("UnoDiagonal", ref matrix, ref matrix, ref x, ref y, ref pivoteDiagonal);
                    }
                }
                else if (matrix[y, x] != 0 && y >= pivoteDiagonal && y != x)
                {
                    convertirCamposEnCeros("CeroArriba", ref matrix, ref matrix, ref x, ref y, ref pivoteDiagonal);
                }
                else if (matrix[y, x] != 0 && y <= pivoteDiagonal && y != x)
                {
                    convertirCamposEnCeros("CeroAbajo", ref matrix, ref matrix, ref x, ref y, ref pivoteDiagonal);
                }

                //avanzar por la matriz
                y++;
                if (y == matrix.GetLength(1))
                {
                    y = pivoteDiagonal;
                    x++;
                }
                else if (pivoteDiagonal != 0 && y == pivoteDiagonal - 1 && y < matrix.GetLength(1))
                {
                    y++;
                }

                if (y >= matrix.GetLength(1))
                {
                    break;
                }
            }
            return matrix;
        }
        async Task PutTaskDelay()
        {
            await Task.Delay(tiempoEspera);
        }
        private void drawMatrix() {
            int count = 0;
            string[] vector = new string[rows * cols];
            string[] vector2 = new string[rows * cols];

            double num = 0.0;
            Fraction fraction;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    num = MatrizIdentidad[j, i];
                    if (num % 1 == 0)
                    {
                        vector[count] = num.ToString();
                    }
                    else
                    {
                        fraction = RealToFraction(MatrizIdentidad[j, i], maxRelativeError);
                        vector[count] = fraction.N.ToString() + "/" + fraction.D.ToString();
                    }

                    num = copiaMatriz[j, i];
                    if (num % 1 == 0)
                    {
                        vector2[count] = num.ToString();
                    }
                    else
                    {
                        fraction = RealToFraction(copiaMatriz[j, i], maxRelativeError);
                        vector2[count] = fraction.N.ToString() + "/" + fraction.D.ToString();
                    }

                    count++;
                }
            }
            count = 0;
            foreach (var control in panel1.Controls)
            {
                var textBox = control as TextBox;
                if (count == rows * cols) {
                    count = 0;
                }
                if (textBox != null && strPunteroCopia.Contains(textBox.Name))
                {
                    textBox.Text = vector2[count++];
                }
                else if (textBox != null && strPunteroIdentidad.Contains(textBox.Name))
                {
                    textBox.Text = vector[count++];
                }
            }
        }
        public void initMatrix()
        {
            int matrixSize = Int32.Parse(filasTextBox.Text) * Int32.Parse(columnasTextBox.Text);
            strPunteroOriginal = new string[matrixSize];
            strPunteroCopia = new string[matrixSize];
            strPunteroIdentidad = new string[matrixSize];
            //double[] vec = new double[9] {0,3,1,1,1,4,1,1,3};
            //double[] vec = new double[9] { 1, 0, 5, 2, 1, 6, 3, 4, 0 };
            //double[] vec = new double[9] { 1, 1, 0, 1, 0, 1, 0, 1, 0 };
            //double[] vec = new double[9] { 1, 0, 2, -1, 1, 0, 0, 0, 1 };
            double[] vec = new double[9] { 2, 1, 3, 0, 1, 7, 1, -4, -3 };

            try
            {
                int cellSize = 80;
                int pointX = 1;
                int pointY = cellSize;
                panel1.Controls.Clear();
                int count = 1;

                for (int i = 0; i < matrixSize * 3; i++)
                {
                    cellSize = 80;
                    TextBox a = new TextBox();

                    if (i < matrixSize) {
                        strPunteroOriginal[i] = "Original" + (i + 1).ToString();
                        a.Name = strPunteroOriginal[i];
                        a.Text = vec[i].ToString();
                        cellSize = 25;
                    } else if (i < matrixSize * 2) {
                        if (i == matrixSize) {
                            pointX += cellSize;
                        }
                        strPunteroCopia[i - matrixSize] = "Copia" + (i + 1).ToString();
                        a.Name = strPunteroCopia[i - matrixSize];
                    }
                    else if (i < matrixSize * 3) {
                        if (i == matrixSize * 2)
                        {
                            pointX += 30 / 2;
                            Pen blackpen = new Pen(Color.Black, 2);
                            Graphics g = panel1.CreateGraphics();
                            g.DrawLine(blackpen, pointX, 10, pointX, 150);
                            g.Dispose();
                            pointX += 30 / 2;
                        }
                        strPunteroIdentidad[i - matrixSize * 2] = "Identidad" + (i + 1).ToString();
                        a.Name = strPunteroIdentidad[i - matrixSize * 2];
                    }

                    a.Location = new Point(pointX, pointY);
                    a.Height = cellSize;
                    a.Width = cellSize;
                    panel1.Controls.Add(a);
                    panel1.Show();
                    pointY += 30;

                    if (count == Int32.Parse(filasTextBox.Text))
                    {
                        pointX += cellSize + 5;
                        pointY -= 30 * cols;
                        count = 0;
                    }
                    count++;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public double[,] matrixMult(double[,] A, double[,] B) {
            double[,] matrix;
            //para verificar que una matriz sea multiplicable A.X == B.Y ej [2,2] * [2,1] donde [y,x] filas * columnas en ese orden
            //el resultado es una matrix de los valres restantes ej [2,2] * [2,1] la nueva matriz seria [2,1]
            //Nota: esta matriz no se puede multiplicar B * A
            int a = A.GetLength(1);
            int b = B.GetLength(0);
            if (A.GetLength(1) == B.GetLength(0)) {
                int aX = A.GetLength(0);
                int aY = A.GetLength(1);
                int bX = B.GetLength(0);
                int bY = B.GetLength(1);
                matrix = new double[aX, bY];

                double temp = 0;
                for (int i = 0; i < aX; i++)
                {
                    for (int j = 0; j < bY; j++)
                    {
                        temp = 0;
                        for (int k = 0; k < aY; k++)
                        {
                            temp += A[i, k] * B[k, j];
                        }
                        matrix[i, j] = temp;
                    }
                }
            }
            else
            {
                return null;
            }

            return matrix;
        }
        public double[,] matrixMult(double[,] A, double[] B)
        {
            double[,] matrix = new double[B.Length,1];
            int i = 0;
            foreach (double value in B) {
                matrix[i++,0] = value;
            }
            
            return matrixMult(A,matrix);
        }
        public double[,] matrixMult(double[] A, double[,] B)
        {
            double[,] matrix = new double[1, A.Length];
            int i = 0;
            foreach (double value in A)
            {
                matrix[0, i++] = value;
            }

            return matrixMult(matrix, B);
        }
    }
}
