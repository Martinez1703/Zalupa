using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Schema;
using static System.Windows.Forms.AxHost;

namespace LabaKg3
{
    public partial class Form1 : Form
    {
        bool f = true;
        int k, l, m; 
        float[,] kvSohran = new float[8, 4];
        float[,] kv = new float[8, 4]; 
       
        private float[,] originalKv = new float[8, 4];

        private float scale = 1.0f;       // Масштаб
        private double rotationAngleY = 0; // Угол поворота вокруг оси Y в радианах
        private double rotationAngleX = 0; // Угол вращения вокруг оси X

        private double rotationAngleZ = 0; // Угол вращения вокруг оси Z
        private float offsetX = 0;
        private float offsetY = 0;
        private float offsetZ = 0;
        public Form1()
        {
            InitializeComponent();
         
            Array.Copy(kv, originalKv, kv.Length); // Сохраняем исходное состояние
        }
        //обновление для осей
        private float[,] CalculateFunctionValues(int stepsX, int stepsY)
        {
            float[,] Values = new float[stepsX, stepsY];
            float xMin = -3f, xMax = 3f;
            float yMin = -3f, yMax = 3f;

            for (int i = 0; i < stepsX; i++)
            {
                float x = xMin + (xMax - xMin) * i / (stepsX - 1);
                for (int j = 0; j < stepsY; j++)
                {
                    float y = yMin + (yMax - yMin) * j / (stepsY - 1);//распределение значенй по области
                    Values[i, j] = (float)Math.Exp(Math.Sin(x) - y*y);
                }
            }
            return Values;
        }
        
        private void DrawStaticAxes()
        {
            // Оси будут рисоваться относительно центра pictureBox
            int centerX = pictureBox1.Width / 2;
            int centerY = pictureBox1.Height / 2;

            Pen axisPen = new Pen(Color.Red, 1);
            Graphics g = Graphics.FromHwnd(pictureBox1.Handle);

            // Рисуем горизонтальную ось X
            g.DrawLine(axisPen, 0, centerY, pictureBox1.Width, centerY);

            // Рисуем вертикальную ось Y
            g.DrawLine(axisPen, centerX, 0, centerX, pictureBox1.Height);

            g.Dispose();
            axisPen.Dispose();
        }

       
        private void ClearDrawing()
        {
            pictureBox1.Image = null;
            pictureBox1.Refresh();
        }
        private void ResetFigure()
        {
            Array.Copy(originalKv, kv, originalKv.Length);
            Draw_Kv(); // Перерисовываем фигуру в исходном положении
        }
        private void Draw_Kv()
        {
            ClearDrawing();

            // Создаем матрицы преобразований
            float[,] scaleMatrix = CreateScaleMatrix(scale, scale, scale);
            float[,] rotationX = CreateRotationXMatrix(rotationAngleX);
            float[,] rotationY = CreateRotationYMatrix(rotationAngleY);
            float[,] rotationZ = CreateRotationZMatrix(rotationAngleZ);
            float[,] translationMatrix = CreateTranslationMatrix(offsetX, offsetY, offsetZ);

       
            float[,] transformMatrix = Multiply_matr(scaleMatrix, rotationX);
            transformMatrix = Multiply_matr(transformMatrix, rotationY);
            transformMatrix = Multiply_matr(transformMatrix, rotationZ);
            transformMatrix = Multiply_matr(transformMatrix, translationMatrix);


            float[,] zValues = CalculateFunctionValues(40, 40);
            int stepsX = zValues.GetLength(0);
            int stepsY = zValues.GetLength(1);

            int centerX = pictureBox1.Width / 2;
            int centerY = pictureBox1.Height / 2;

            using (Graphics g = Graphics.FromHwnd(pictureBox1.Handle))
            using (Pen pen = new Pen(Color.Blue, 1))
            {
                //массив для хранения преобразованных точек
                PointF[,] transformedPoints = new PointF[stepsX, stepsY];

                for (int i = 0; i < stepsX; i++)
                {
                    for (int j = 0; j < stepsY; j++)
                    {
                        //Вычисление исходных 3D координат
                        float x = -3f + 6f * i / (stepsX - 1);
                        float y = -3f + 6f * j / (stepsY - 1);
                        float z = zValues[i, j];

                      
                        float[] point = { x, y, z, 1 };
                        float[] transformedPoint = new float[4];

                        //преобразования
                        for (int k = 0; k < 4; k++)
                        {
                            transformedPoint[k] = 0;
                            for (int l = 0; l < 4; l++)
                            {
                                transformedPoint[k] += point[l] * transformMatrix[l, k];
                            }
                        }

             
                        transformedPoints[i, j] = new PointF(
                            centerX + transformedPoint[0] * 20, 
                            centerY - transformedPoint[1] * 20
                        );
                    }
                }

                // Рисуем линии вдоль X (вертикальные линии)
                for (int j = 0; j < stepsY; j++)
                {
                    PointF[] linePoints = new PointF[stepsX];
                    for (int i = 0; i < stepsX; i++)
                    {
                        linePoints[i] = transformedPoints[i, j];
                    }
                    g.DrawLines(pen, linePoints);
                }

                // Рисуем линии вдоль Y (горизонтальные линии)
                for (int i = 0; i < stepsX; i++)
                {
                    PointF[] linePoints = new PointF[stepsY];
                    for (int j = 0; j < stepsY; j++)
                    {
                        linePoints[j] = transformedPoints[i, j];
                    }
                    g.DrawLines(pen, linePoints);
                }
            }

            
        }
       
        private float[,] Multiply_matr(float[,] a, float[,] b)
        {
            int n = a.GetLength(0);
            int m = b.GetLength(1);
            int m_a = a.GetLength(1);
            if (m_a != b.GetLength(0)) throw new Exception("Матрицы нельзя перемножить!");
            float[,] r = new float[n, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    r[i, j] = 0;
                    for (int ii = 0; ii < m_a; ii++)
                    {
                        r[i, j] += a[i, ii] * b[ii, j];
                    }
                }
            }
            return r;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //середина pictureBox
            k = pictureBox1.Width / 2;
            l = pictureBox1.Height / 2;
            Draw_Kv();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            k = pictureBox1.Width / 2;
            l = pictureBox1.Height / 2;
            DrawStaticAxes();
        }
       
        private float[,] CreateScaleMatrix(float X, float Y, float Z)
        {
            return new float[4, 4]
            {
        { X, 0,      0,      0 },
        { 0,      Y, 0,      0 },
        { 0,      0,      Z, 0 },
        { 0,      0,      0,      1 }
            };
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ClearDrawing();
            DrawStaticAxes();
            l -= 5; //изменение соответствующего элемента матрицы сдвига
            Draw_Kv(); //вывод квадратика
        }
        //Масштабирование  фигуры на плоскости
        private void button9_Click(object sender, EventArgs e)
        {
            if (float.TryParse(textBox1.Text, out float newScale))
            {
                if (newScale > 0) // Проверяем, чтобы масштаб был положительным
                {
                    scale = newScale;
                    Draw_Kv(); // Перерисовываем поверхность с новым масштабом
                }
                else
                {
                    MessageBox.Show("Масштаб должен быть положительным числом!");
                }
            }
            else
            {
                MessageBox.Show("Введите корректное число для масштаба!");
            }
        }
        //Поворот фигуры
        private float[,] CreateTranslationMatrix(float tx, float ty, float tz)
        {
            return new float[4, 4]
            {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 1, 0 },
        { tx, ty, tz, 1 }
            };
        }

        //Отражение фигуры относительно Х
        private void button11_Click(object sender, EventArgs e)
        {
           
            // Матрица отражения относительно оси X
            float[,] reflectX = new float[4, 4]
            {
                { 1, 0, 0, 0 },
                { 0, -1, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 }
            };

            // Применяем отражение к текущим координатам
            float[,] temp = Multiply_matr(kv, reflectX);

            // Обновляем текущие координаты фигуры
            for (int i = 0; i < 8; i++)
            {
                kv[i, 0] = temp[i, 0];
                kv[i, 1] = temp[i, 1];
                kv[i, 2] = temp[i, 2];
                kv[i, 3] = temp[i, 3];
            }

            // Рисуем отраженную фигуру с учетом текущего сдвига
            Draw_Kv();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            
            // Матрица отражения относительно оси Y
            float[,] reflectY = new float[4, 4]
            {
                { -1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 }
            };

            // Применяем отражение к текущим координатам
            float[,] temp = Multiply_matr(kv, reflectY);

            // Обновляем текущие координаты фигуры
            for (int i = 0; i < 8; i++)
            {
                kv[i, 0] = temp[i, 0];
                kv[i, 1] = temp[i, 1];
                kv[i, 2] = temp[i, 2];
                kv[i, 3] = temp[i, 3];
            }

            // Рисуем отраженную фигуру с учетом текущего сдвига
            Draw_Kv();
        }

        private void button13_Click(object sender, EventArgs e)
        {
          
            // Матрица отражения относительно начала координат
            float[,] reflectOrigin = new float[4, 4]
            {
                { -1, 0, 0, 0 },
                { 0, -1, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 }
            };

            // Применяем отражение к текущим координатам
            float[,] temp = Multiply_matr(kv, reflectOrigin);

            // Обновляем текущие координаты фигуры
            for (int i = 0; i < 8; i++)
            {
                kv[i, 0] = temp[i, 0];
                kv[i, 1] = temp[i, 1];
                kv[i, 2] = temp[i, 2];
                kv[i, 3] = temp[i, 3];
            }

            // Рисуем отраженную фигуру с учетом текущего сдвига
            Draw_Kv();
        }

        //Очистка PictureBox1
        private void button3_Click(object sender, EventArgs e)
        {
            ClearDrawing();
        }

        private void button4_Click(object sender, EventArgs e) // Поворот вокруг X
        {
            rotationAngleX += 0.1;
            Draw_Kv();
        }

        private void button5_Click(object sender, EventArgs e) // Поворот вокруг Z
        {
            rotationAngleZ += 0.1;
            Draw_Kv();
        }

        private void button16_Click(object sender, EventArgs e) // Поворот вокруг Y (влево)
        {
            rotationAngleY -= 0.1;
            Draw_Kv();
        }

        private void button15_Click(object sender, EventArgs e) // Поворот вокруг Y (вправо)
        {
            rotationAngleY += 0.1;
            Draw_Kv();
        }


        private void button14_Click(object sender, EventArgs e)
        {
            k = pictureBox1.Width / 2;
            l = pictureBox1.Height / 2;
            ResetFigure(); // Возвращаем фигуру в исходное положение и центр
        }
        private float[,] CreateRotationXMatrix(double angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            return new float[4, 4]
            {
        { 1, 0,    0,     0 },
        { 0, cos, -sin,   0 },
        { 0, sin,  cos,   0 },
        { 0, 0,    0,     1 }
            };
        }

        private void button6_Click(object sender, EventArgs e)
        {
            offsetX -= 1;
            Draw_Kv();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            offsetX += 1;
            Draw_Kv();
        }

        private void button11_Click_1(object sender, EventArgs e)
        {
            offsetY += 1;
            Draw_Kv();
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            offsetY -= 1;
            Draw_Kv();
        }

        private void button12_Click_1(object sender, EventArgs e)
        {
            offsetZ += 1;
            Draw_Kv();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            offsetZ -= 1;
            Draw_Kv();
        }

        // Матрица поворота вокруг оси Y
        private float[,] CreateRotationYMatrix(double angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            return new float[4, 4]
            {
        { cos,  0, sin, 0 },
        { 0,    1, 0,   0 },
        { -sin, 0, cos, 0 },
        { 0,    0, 0,   1 }
            };
        }

        // Матрица поворота вокруг оси Z
        private float[,] CreateRotationZMatrix(double angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            return new float[4, 4]
            {
        { cos, -sin, 0, 0 },
        { sin,  cos, 0, 0 },
        { 0,    0,   1, 0 },
        { 0,    0,   0, 1 }
            };
        }
    }
}
