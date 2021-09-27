using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace IO
{
    class SimplexMethod
    {
        //Canonical Form
        private int zSize;
        private int restrictionsSize;
        private double[] Z;
        private string[] Zstr;
        private string[] restrictionsStr;
        private double[,] restrictions;
        //Standard Form
        private int zEstandarSize;
        private int restrictionsEstandarSize;
        private double[] zEstandar;
        private double[,] restrictionsEstandar;
        //Variables
        private double[,] A;
        private double[,] B;
        private double[] b;
        private double[] Cj;
        private double[,] Yj;
        private double[] Zj;
        //Rows Considerated
        private double[] considerations;
        private double[] excludedConsiderations;
        //
        private int lowestRow = -1;
        private int secondLowestRow = -1;
        //
        double[] cB;
        int containedRows = 0;
        Matrix matrix = new Matrix(0, 0);
        double[,] Binverse;
        double[,] xB;
        double[,] vectorA;
        double[,] zRara;
        bool isMaximun;
        double lowestValue;
        double[] solucion;
        int EDUARDO = 0;
        bool donuts = false;

        public void init(int zSize, int restrictionsSize)
        {
            this.zSize = zSize;
            this.restrictionsSize = restrictionsSize;

            Z = new double[this.zSize];
            restrictions = new double[this.zSize + 1, this.restrictionsSize];

            zEstandarSize = this.zSize + this.restrictionsSize;
            zEstandar = new double[zEstandarSize];
            restrictionsEstandarSize = (zEstandarSize + 1) * this.restrictionsSize;
            restrictionsEstandar = new double[zEstandarSize + 1, this.restrictionsSize];

            A = new double[this.zSize + this.restrictionsSize, this.restrictionsSize];
            B = new double[this.restrictionsSize, this.restrictionsSize];
            b = new double[this.restrictionsSize];
            Cj = new double[this.zSize + this.restrictionsSize];
            //Yj = new double[this.zSize + this.restrictionsSize];

            considerations = new double[this.restrictionsSize];
            excludedConsiderations = new double[this.zSize + this.restrictionsSize];
            for (int i = 0; i < excludedConsiderations.Length; i++) {
                excludedConsiderations[i] = -1;
            }
        }
        public void Print() {
            Application.OpenForms["Form1"].Controls.Clear();
            Panel panelSimplex = new Panel();
            panelSimplex.Height = 200;
            panelSimplex.Width = 500;
            panelSimplex.Location = new System.Drawing.Point(10, 10);
            panelSimplex.Name = "panelSimplex";
            Application.OpenForms["Form1"].Controls.Add(panelSimplex);

            Button botonPanelSimplex = new Button();
            botonPanelSimplex.Name = "botonPanelSimplex";
            botonPanelSimplex.Text = "Empezar";
            botonPanelSimplex.Location = new System.Drawing.Point(10, 300);
            botonPanelSimplex.Click += (s, e) => { fillCampos(); };
            Application.OpenForms["Form1"].Controls.Add(botonPanelSimplex);

            TextBox cantidadRestriccionesTextBox = new TextBox();
            TextBox functionSizeTextBox = new TextBox();
            cantidadRestriccionesTextBox.Location = new System.Drawing.Point(10, 250);
            cantidadRestriccionesTextBox.Text = "Restricciones";
            functionSizeTextBox.Location = new System.Drawing.Point(200, 250);
            functionSizeTextBox.Text = "Funcion";

            Application.OpenForms["Form1"].Controls.Add(cantidadRestriccionesTextBox);
            Application.OpenForms["Form1"].Controls.Add(functionSizeTextBox);
        }
        private void fillCampos() {
            int cellSize = 20;
            int pointX = cellSize;
            int pointY = cellSize;
            //int[]vec = new int[8]{ 5,3,3,5,15,5,2,10 };
            //int[]vec = new int[8]{ 2,1,3,4,6,6,1,3 };
            int[] vec = new int[11] {1, 2, 1, 0, 5, 0, 1, 6, 1, 1, 8 };
            try
            {
                Zstr = new string[zSize];

                for (int i = 0; i < zSize; i++)
                {
                    Zstr[i] = "Zstr" + (i + 1).ToString();

                    TextBox a = new TextBox();
                    a.Name = Zstr[i];
                    a.Text = vec[i].ToString();
                    a.Location = new Point(pointX, pointY);
                    a.Height = cellSize;
                    a.Width = cellSize;
                    Application.OpenForms["Form1"].Controls["panelSimplex"].Controls.Add(a);

                    Label lb = new Label();
                    lb.Text = "x" + (i + 1) ;
                    lb.Location = new Point(pointX + cellSize, pointY);
                    lb.Height = cellSize;
                    lb.Width = cellSize;
                    Application.OpenForms["Form1"].Controls["panelSimplex"].Controls.Add(lb);

                    Application.OpenForms["Form1"].Show();
                    pointX += cellSize * 2;
                }
                pointX = cellSize;
                pointY += 50;
                int count = 0;
                restrictionsStr = new string[(zSize + 1) * restrictionsSize];
                for (int i = 0; i < restrictionsStr.Length; i++)
                {
                    restrictionsStr[i] = "restriccionesStr" + (i + 1).ToString();
                    if (count == zSize + 1)
                    {
                        pointY += cellSize + 5;
                        pointX -= 30 * (zSize + 1);
                        count = 0;
                    }
                    TextBox b = new TextBox();
                    b.Name = restrictionsStr[i];
                    b.Text = vec[i+2].ToString();
                    b.Location = new Point(pointX, pointY);
                    b.Height = cellSize;
                    b.Width = cellSize;
                    Application.OpenForms["Form1"].Controls["panelSimplex"].Controls.Add(b);

                    pointX += 30;

                    Application.OpenForms["Form1"].Show();

                    count++;
                }
                pointX = cellSize;
                pointY += cellSize;
                Button botonCalcularEstandar = new Button();
                botonCalcularEstandar.Name = "botonPanelSimplex";
                botonCalcularEstandar.Text = "Empezar";
                botonCalcularEstandar.Location = new System.Drawing.Point(pointX, pointY);
                botonCalcularEstandar.Click += (s, e) => { crearFormaCanonica(); crearFormaEstandar(); };
                Application.OpenForms["Form1"].Controls["panelSimplex"].Controls.Add(botonCalcularEstandar);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
           
        }
        private void crearFormaCanonica() {
            int counterZ = 0;
            int counterR = 0;
            int count = 0;

            double[] numeritos = new double[restrictionsSize * (zSize + 1)];
            foreach (var control in Application.OpenForms["Form1"].Controls["panelSimplex"].Controls)
            {
                var textBox = control as TextBox;
                if (textBox != null && Zstr.Contains(textBox.Name) && counterZ < zSize)
                {
                    Z[counterZ] = Double.Parse(textBox.Text);
                    counterZ++;
                }

                if (textBox != null && restrictionsStr.Contains(textBox.Name) && counterR < restrictionsSize * (zSize + 1))
                {
                    numeritos[counterR] = Double.Parse(textBox.Text);
                    counterR++;
                }
            }
            count = 0;
            for (int i = 0; i < restrictions.GetLength(1); i++)
            {
                for (int j = 0; j < restrictions.GetLength(0); j++)
                {
                    restrictions[j, i] = numeritos[count];
                    count++;
                }
            }

            foreach (var num in Z)
            {
                Console.Write(num);
            }
            Console.Write("\n");
            Console.Write("\n");
            for (int i = 0; i < restrictions.GetLength(1); i++)
            {
                for (int j = 0; j < restrictions.GetLength(0); j++)
                {
                    Console.Write(restrictions[j, i]);
                }
                Console.Write("\n");
            }
        }
        private void crearFormaEstandar() {
            for (int i = 0; i < zEstandar.Length; i++) {
                if (i <Z.Length) {
                    zEstandar[i] = Z[i];
                } else {
                    zEstandar[i] = 0;
                }
            }
            int pivote = 0;
            int count = 0;
            for (int i = 0; i < restrictionsEstandar.GetLength(1); i++)
            {
                for (int j = 0; j < restrictionsEstandar.GetLength(0) - 1; j++)
                {
                    if (i < restrictions.GetLength(1) && j < restrictions.GetLength(0) - 1) {
                        restrictionsEstandar[j,i] = restrictions[j, i];
                    } else {
                        if (count == pivote) {
                            restrictionsEstandar[j, i] = 1;
                            pivote++;
                            count = 100;
                        }
                        else
                        {
                            restrictionsEstandar[j, i] = 0;
                            count++;
                        }
                    }
                }
                count = 0;
            }

            for (int i = 0; i < restrictionsEstandar.GetLength(1); i++) {
                restrictionsEstandar[restrictionsEstandar.GetLength(0) - 1, i] = restrictions[restrictions.GetLength(0) - 1, i];
                b[i] = restrictions[restrictions.GetLength(0) - 1, i];
            }

            Console.Write("\n");
            Console.Write("\n");
            for (int i = 0; i < restrictionsEstandar.GetLength(1); i++)
            {
                for (int j = 0; j < restrictionsEstandar.GetLength(0); j++)
                {
                    Console.Write(restrictionsEstandar[j, i]);
                }
                Console.Write("\n");
            }

            int pointX = 250;
            int pointY = 10;
            RichTextBox richTextBox = new RichTextBox();
            richTextBox.Name = "richTextBox";
            richTextBox.Location = new Point(pointX, pointY);
            richTextBox.Height = 100;
            richTextBox.Width = 200;

            richTextBox.Text = "Max. z = ";

            for (int i = 0; i < zEstandar.Length; i++)
            {
                double num = zEstandar[i];
                if (i != 0) {
                    if (num >= 0)
                    {
                        richTextBox.AppendText(" + ");
                    }
                    else
                    {
                        richTextBox.AppendText(" - ");
                    }
                }
                richTextBox.AppendText(Math.Abs(num).ToString() + "x" + (i + 1).ToString());
            }
            richTextBox.AppendText("\n\n");
            //////////////////////////////////////////////////////////////////////////////////
            for (int i = 0; i < restrictionsEstandar.GetLength(1); i++)
            {
                for (int j = 0; j < restrictionsEstandar.GetLength(0); j++)
                {
                    double num = restrictionsEstandar[j, i];

                    if (j != 0) {
                        if (j != restrictionsEstandar.GetLength(0) - 1)
                        {
                            if (num >= 0)
                            {
                                richTextBox.AppendText(" + ");
                            }
                            else if (num < 0)
                            {
                                richTextBox.AppendText(" - ");
                            }
                        }
                        else
                        {
                            richTextBox.AppendText(" = ");
                        }
                    }

                    richTextBox.AppendText(Math.Abs(num).ToString() + "x" + (j + 1).ToString());
                }
                richTextBox.AppendText("\n");
            }

            float font_size = richTextBox.Font.Size;
            Font small_font = new Font(
                richTextBox.Font.FontFamily,
                font_size * 0.85f);

            List<int> subs = new List<int>();

            for (int i = 0; i < richTextBox.Text.Length; i++)
            {
                if (richTextBox.Text[i] == 'x')
                {
                    subs.Add(i + 1);
                }
            }

            int offset = (int)(font_size * 0.5);
            foreach (int position in subs)
            {
                richTextBox.Select(position, 1);
                richTextBox.SelectionCharOffset = -offset;
                richTextBox.SelectionFont = small_font;
            }

            Application.OpenForms["Form1"].Controls["panelSimplex"].Controls.Add(richTextBox);
            Application.OpenForms["Form1"].Show();
            SImplex();
        }
        private void calculateRows() {
            int xSize = A.GetLength(0);
            int ySize = A.GetLength(1);
            int pivote = 0;
            
            //buscar identidad solo si es primer paso
            if (lowestRow == -1) {
                for (int y = 0; y < ySize; y++)
                {
                    for (int x = 0; x < xSize; x++)
                    {
                        if (A[x, y] == 1)
                        {
                            if (y == pivote)
                            {
                                if (pivote == ySize - 1) {
                                    for (int aa = pivote - 1; aa >= 0; aa--)
                                    {
                                        if (A[x, aa] != 0)
                                        {
                                            break;
                                        }
                                        else if (A[x, aa] == 0 && aa == 0)
                                        {
                                            considerations[pivote] = x;
                                            pivote++;
                                        }
                                    }
                                }
                                for (int a = pivote + 1; a < ySize; a++)
                                {
                                    if (A[x, a] != 0)
                                    {
                                        break;
                                    }
                                    else if (A[x, a] == 0 && a == ySize - 1)
                                    {
                                        if (pivote == 0) {
                                            considerations[pivote] = x;
                                            pivote++;
                                        }
                                        else
                                        {
                                            for (int aa = pivote - 1; aa >= 0; aa--)
                                            {
                                                if (A[x, aa] != 0)
                                                {
                                                    break;
                                                }
                                                else if (A[x, aa] == 0 && aa == 0)
                                                {
                                                    considerations[pivote] = x;
                                                    pivote++;
                                                }
                                            }
                                        }
                                    }
                                }
                               
                            }
                        }
                    }
                }
            }
            int i = 0;
        }
        private void SImplex() {
            int xSize = A.GetLength(0);
            int ySize = A.GetLength(1);

            Cj = zEstandar;

            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    A[x, y] = restrictionsEstandar[x, y];
                }
            }

            for (int i = 0; i < ySize; i++)
            {
                for (int j = 0; j < xSize; j++)
                {
                    Console.Write(A[j, i]);
                }
                Console.Write("\n");
            }
            cB = new double[B.GetLength(0)];
            Paso1();
        }
        private void Paso1() {
            calculateRows();
            
            int xSize = A.GetLength(0);
            int ySize = A.GetLength(1);

            int xX = 0;
            int yY = 0;

            if (lowestRow == -1)
            {
                for (int x = 0; x < xSize; x++)
                {
                    if (considerations.Contains(x))
                    {
                        containedRows++;
                        for (int y = 0; y < ySize; y++)
                        {
                            B[xX, yY] = A[x, y];
                            yY++;
                        }
                        yY = 0;
                        xX++;
                    }
                }
            }
            EDUARDO = zEstandarSize - containedRows;
            Paso2();
        }
        private void Paso2() {
            matrix.createIMatrix(B.GetLength(0));
            double[,] result = (double[,])B.Clone();
            Binverse = matrix.calcularInversa(result);
            Paso3();
        }
        private void Paso3() {
            /////////////////////NOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO
            xB = matrix.matrixMult2(Binverse, b);
            Paso4();
        }
        private void Paso4()
        {
            for (int i = 0; i < considerations.Length; i++)
            {
                for (int nono = 0; nono < zEstandar.Length; nono++)
                {
                    if (considerations[i] == nono)
                    {
                        cB[i] = zEstandar[nono];
                        break;
                    }
                }
            }

            //Maximo es zRara en los puntos del vector xB;
            zRara = matrix.matrixMult(cB, xB);

            //SE CALCULA yJ Y Zj
            Yj = new double[B.GetLength(0), zEstandarSize - containedRows];
            int xDx5 = 0;
            vectorA = new double[restrictionsSize, restrictionsSize];
            int opCounter = 0;
            for (int i = 0; i < Cj.Length; i++)
            {
                if (!considerations.Contains(i))
                {
                    double[] vec = new double[restrictionsSize];
                    for (int j = 0; j < restrictionsSize; j++)
                    {
                        vec[j] = A[i, j];
                        if (opCounter >= 2)
                        {
                            opCounter = 0;
                        }
                        vectorA[j, opCounter] = A[i, j];
                    }
                    opCounter++;
                    double[,] m;
                    m = matrix.matrixMult2(Binverse, vec);

                    for (int h = 0; h < m.GetLength(0); h++)
                    {
                        Yj[h, xDx5] = m[h, 0];
                    }
                    xDx5++;
                }
            }

            Zj = new double[EDUARDO];

            for (int i = 0; i < Zj.Length; i++)
            {
                double[] vec = new double[Yj.GetLength(0)];
                for (int j = 0; j < Yj.GetLength(1); j++)
                {
                    //vec[j] = Yj[j, i];
                    vec[j] = Yj[j, i];
                }
                double[,] mat;
                mat = matrix.matrixMult(cB, vec);
                for (int j = 0; j < Yj.GetLength(1); j++)
                {
                    Zj[i] = mat[0, 0];
                }

            }
            Paso5();
        }
        private void Paso5()
        {

            int counter = 0;
            //Solucion
            solucion = new double[Zj.Length];
            counter = 0;

            for (int i = 0; i < zEstandarSize; i++)
            {
                if (!considerations.Contains(i))
                {
                    solucion[counter] = Zj[counter] - Cj[i];
                    //  solucion[counter] = Zj[counter] - Cj[i];//
                    counter++;
                }
            }

            isMaximun = true;
            lowestValue = 0;
            for (int i = 0; i < solucion.Length; i++)
            {
                if (solucion[i] < 0)
                {
                    isMaximun = false;
                    break;
                }
            }
            if (isMaximun) { donuts = true; }
            Paso6();
        }
        private void Paso6()
        {
            if (!isMaximun)
            {
                lowestValue = solucion.Min();
                for (int x = 0; x < solucion.Length; x++)
                {
                    if (lowestValue == solucion[x])
                    {
                        //lowestValue or x is row index, posicion en Yj, lowest value = -5, x = 1
                        lowestRow = x;
                        double lowesValue2 = 0;
                        double[] lowesValueVec = new double[xB.GetLength(0)];
                        int counter2 = 0;
                        for (int fila = 0; fila < Yj.GetLength(0); fila++)
                        {
                            lowesValueVec[fila] = xB[fila, 0] / Yj[counter2++, x];
                            lowesValue2 = lowesValueVec.Min();
                        }
                        int counterXDf = 0;
                        foreach (double value in lowesValueVec)
                        {
                            if (lowesValue2 == value)
                            {
                                //lowestValue or x is row index, posicion en Matrix B,lowesValue2 = 2, secondLowestRow = 1
                                secondLowestRow = counterXDf;
                                int lol = A.GetLength(0);
                                
                                int verifyYjInA = 0;
                                for (int l = 0; l < A.GetLength(0); l++)
                                {
                                    verifyYjInA = 0;
                                    for (int ll = 0; ll < A.GetLength(1); ll++)
                                    {
                                        if (vectorA[ll, lowestRow] == A[l, ll] && !excludedConsiderations.Contains(l))
                                        {
                                            verifyYjInA++;
                                            if (verifyYjInA == A.GetLength(1))
                                            {
                                                //encontrado
                                                excludedConsiderations.Append(l);
                                                //cambiar valores de B
                                                for (int opa = 0; opa < A.GetLength(1); opa++)
                                                {
                                                    B[secondLowestRow, opa] = A[l, opa];
                                                }
                                                considerations[secondLowestRow] = l;
                                                break;
                                            }
                                        }
                                    }
                                    if (donuts) { break; }
                                }

                                Paso1();
                                break;
                            }
                            counterXDf++;
                        }
                    }
                    if (donuts) { break; }
                }
            }
            else
            {
                PasoFinal();
            }
        }
        private void PasoFinal()
        {
            if (isMaximun )
            {
                for (int i = 0; i < solucion.Length; i++)
                {
                    Console.WriteLine("Como los valores {0} son psotivios ", solucion[i]);
                }
                Console.WriteLine("Por tanto, z es óptima. Así la solución óptima es");
                Console.WriteLine("z = CbXb = " + matrix.drawMatrix(cB) + "" + "*" + " " + matrix.drawMatrix(xB) + " = " + matrix.drawMatrix(zRara));

            }
        }
    }
}
