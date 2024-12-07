using System;
using System.Linq;

namespace ShadowRendering
{
    struct Point2D
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Point2D(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    class Sprite
    {
        int x, y;
        int width, length;
        int[,] sprite;

        public Sprite(Point2D spritePoint, int[,] sprite)
        {
            x = (int)spritePoint.X;
            y = (int)spritePoint.Y;
            this.sprite = sprite;
            width = sprite.GetLength(0);
            length = sprite.GetLength(1);
        }

        public static void PrintSprite(Point2D spritePoint, string[] obj)
        {
            int width = obj.GetLength(0);

            for (int i = 0; i < obj.GetLength(0); i++)
            {
                for (int j = 0; j < obj[i].Length; j++)
                {
                    Console.SetCursorPosition((int)spritePoint.X + j, (int)spritePoint.Y + i);

                    if (obj[i][j] != ' ')
                    {
                        Console.Write(obj[i][j]);
                    }
                }
            }

            Console.SetCursorPosition(0, (int)spritePoint.Y + width + 1);
        }

        public void PrintShadow()
        {
            for (int i = 0; i < width; i++)
            {
                Console.SetCursorPosition(x, y + i);

                for (int j = 0; j < length; j++)
                {
                    if (sprite[i, j] == 1)
                    {
                        Console.Write('░');
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                }
            }

            Console.SetCursorPosition(0, y + width + 1);
        }

        public static int[,] GetMask(string[] obj)
        {
            int objWidth = obj.GetLength(0);
            int objLength = obj.Max(x => x.Length);

            int[,] mask = new int[objWidth, objLength];

            for (int i = 0; i < objWidth; i++)
            {
                for (int j = 0; j < objLength; j++)
                {
                    if (j >= obj[i].Length)
                    {
                        mask[i, j] = 0;
                        continue;
                    }

                    if (obj[i][j] != ' ')
                    {
                        mask[i, j] = 1;
                    }
                    else
                    {
                        mask[i, j] = 0;
                    }
                }
            }

            return mask;
        }

        public void CreateShadow(Point2D lightPoint, int lightHeight)
        {
            Point2D lightHeightPoint = new Point2D(lightPoint.X, lightPoint.Y + lightHeight);
            Point2D borderPoint = GetShadowPoint(lightPoint, new Point2D(x + length - 1, y), lightHeightPoint, new Point2D(x + length - 1, y + width));

            int newWidth = y + width - (int)borderPoint.Y;
            int newLength = (int)borderPoint.X - x + 1;
            int[,] shadowMask = new int[newWidth, newLength];

            for (int i = 0; i < newWidth; i++)
            {
                for (int j = 0; j < newLength; j++)
                {
                    shadowMask[i, j] = 0;
                }
            }

            int[] heights = new int[width + newWidth];
            int[] connections = new int[width + newWidth];
            int[] rowStart = new int[newWidth];
            int[] rowEnd = new int[newWidth];

            GetParams(heights, connections, rowStart, rowEnd, borderPoint, lightPoint, lightHeightPoint, newWidth);

            //Имеем heights (тень), connections (маска), rowStart, rowEnd
            //Идем по heights. Заполняем temp строку. Если следующий height такой же, заполняем 2-ую, объединяем и т.д. Иначе переходим дальше.

            return;
        }

        private void GetParams(int[] heights, int[] connections, int[] rowStart, int[] rowEnd, Point2D borderPoint, Point2D lightPoint, Point2D lightHeightPoint, int newWidth)
        {
            heights[0] = (int)borderPoint.Y - (int)borderPoint.Y;
            connections[0] = 0;
            Point2D leftBottomPoint = new Point2D(x, y + width);

            for (int i = 1; i < width; i++)
            {
                Point2D point = GetShadowPoint(lightPoint, new Point2D(x, y + i), lightHeightPoint, leftBottomPoint);
                heights[i] = (int)point.Y - (int)borderPoint.Y;
                connections[i] = i;
            }

            float k1, k2, b1, b2;
            GetCoefficients(lightHeightPoint, leftBottomPoint, out k1, out b1);
            GetCoefficients(lightHeightPoint, new Point2D(x + length - 1, y + width), out k2, out b2);

            for (int i = 0; i < newWidth; i++)
            {
                int tempY = (int)borderPoint.Y + i;
                rowStart[i] = (int)((tempY - b1) / k1);
                rowEnd[i] = (int)Math.Ceiling((tempY - b2) / k2);

                Point2D tempPoint = new Point2D((tempY - b1) / k1, tempY);
                float kTemp, bTemp;
                GetCoefficients(lightPoint, tempPoint, out kTemp, out bTemp);

                int value = (int)(kTemp * x + bTemp) - y;
                connections[width + i] = value < 0 ? 0 : value;

                int index = 0;
                while (index < heights.Length && heights[index] < i)
                {
                    index++;
                }

                for (int j = heights.Length - 1; j > index; j--)
                {
                    heights[j] = heights[j - 1];
                    connections[j] = connections[j - 1];
                }

                heights[index] = i;
            }
        }

        private Point2D GetShadowPoint(Point2D lightPoint, Point2D topPoint, Point2D lightHeightPoint, Point2D bottomPoint)
        {
            Point2D shadowPoint = new Point2D();
            float k1, k2, b1, b2;

            GetCoefficients(lightPoint, topPoint, out k1, out b1);
            GetCoefficients(lightHeightPoint, bottomPoint, out k2, out b2);

            shadowPoint.X = (b2 - b1) / (k1 - k2);
            shadowPoint.Y = k1 * shadowPoint.X + b1;

            return shadowPoint;
        }

        private void GetCoefficients(Point2D point1, Point2D point2, out float k, out float b)
        {
            k = (point2.Y - point1.Y) / (point2.X - point1.X);
            b = point1.Y - k * point1.X;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Point2D spritePoint = new Point2D(2, 1);
            Point2D lightPoint = new Point2D(1, 0);
            int lightHeight = 20;

            string[] obj = new string[18]
            {
                "▓▓▓▓▓▓▓▓▓▓",
                "▓▓▓    ▓▓▓",
                "▓▓▓    ▓▓▓",
                "▓▓▓    ▓▓▓",
                "▓▓▓    ▓▓▓",
                "▓▓▓    ▓▓▓",
                "▓▓▓    ▓▓▓",
                "▓▓▓    ▓▓▓",
                "▓▓▓    ▓▓▓",
                "▓▓▓    ▓▓▓",
                "▓▓▓    ▓▓▓",
                "▓▓▓    ▓▓▓",
                "▓▓▓    ▓▓▓",
                "▓▓▓    ▓▓▓",
                "▓▓▓    ▓▓▓",
                "▓▓▓    ▓▓▓",
                "▓▓▓    ▓▓▓",
                "▓▓▓▓▓▓▓▓▓▓"
            };

            //Point2D spritePoint = new Point2D(2, 2);
            //Point2D lightPoint = new Point2D(1, 0);
            //int lightHeight = 12;

            //string[] obj = new string[5]
            //{
            //    "▓▓▓▓▓",
            //    "▓▓▓▓▓",
            //    "▓▓▓▓▓",
            //    "▓▓▓▓▓",
            //    "▓▓▓▓▓"
            //};

            //Point2D spritePoint = new Point2D(3, 2);
            //Point2D lightPoint = new Point2D(2, 1);
            //int lightHeight = 22;

            //string[] obj = new string[19]
            //{
            //    "▓▓▓▓▓",
            //    "▓   ▓",
            //    "▓   ▓",
            //    "▓   ▓",
            //    "▓   ▓",
            //    "▓   ▓",
            //    "▓▓▓▓▓",
            //    "▓   ▓",
            //    "▓   ▓",
            //    "▓   ▓",
            //    "▓   ▓",
            //    "▓   ▓",
            //    "▓▓▓▓▓",
            //    "▓   ▓",
            //    "▓   ▓",
            //    "▓   ▓",
            //    "▓   ▓",
            //    "▓   ▓",
            //    "▓▓▓▓▓"
            //};


            Sprite sprite = new Sprite(spritePoint, Sprite.GetMask(obj));

            if (lightPoint.Y + lightHeight - spritePoint.Y - obj.GetLength(0) <= 0 || lightPoint.X >= spritePoint.X)
            {
                Console.WriteLine("Ошибка!");
                return;
            }

            //Sprite shadow = sprite.CreateShadow(lightPoint, lightHeight);
            sprite.CreateShadow(lightPoint, lightHeight);

            //shadow.PrintShadow();
            //Sprite.PrintSprite(spritePoint, obj);

            //Console.SetCursorPosition((int)lightPoint.X, (int)lightPoint.Y);
            //Console.Write('☼');

            //Console.SetCursorPosition((int)lightPoint.X, (int)lightPoint.Y + lightHeight);
            //Console.Write('_');

            Console.SetCursorPosition(0, (int)lightPoint.Y + lightHeight + 5);
        }
    }
}