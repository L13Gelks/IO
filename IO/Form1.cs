using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IO
{
    public partial  class  Form1 : Form
    {
        Matrix matrix;
        bool estado = false;
        //string[] strPutero;
        public Form1()
        {
            InitializeComponent();
            var pic = new Bitmap(this.BackgroundImage, this.Width, this.Height);
            this.BackgroundImage = pic;
        }
        private async Task calcularInversa()
        {
            if (!estado)
            {
                matrix = new Matrix(Int32.Parse(filasTextBox.Text), Int32.Parse(columnasTextBox.Text));
                matrix.initMatrix();
                estado = true;
            }
            else
            {
                matrix.llenarMatriz();
                matrix.imprimirMatriz("Original");
                matrix.imprimirMatriz("Identidad");

                await matrix.calcularInveras();

                matrix.deleteAllProgressBars();

                Console.Write("La matrix Original es: \n");
                matrix.imprimirMatriz("Original");
                Console.Write("La matrix Inversa es: \n");
                matrix.imprimirMatriz("Identidad");
                Console.Write("La matrix Identidad es: \n");
                matrix.imprimirMatriz("Copia");
                Console.Write("La cantidad de pasos fue: ");
                Console.Write(matrix.stepsCount());

            }
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            _ = calcularInversa();
        }
    }
}
