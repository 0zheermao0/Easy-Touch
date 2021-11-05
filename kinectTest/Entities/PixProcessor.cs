

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace kinectTest.Entities
{

    class PixProcessor
    {
        private int width = -1;
        private int height = -1;

        private Point leftTopPoint = new Point(-1,-1);
        private Point rightTopPoint = new Point(-1, -1);
        private Point leftBottomPoint = new Point(-1, -1);
        private Point rightBottomPoint = new Point(-1, -1);


        private Point maxYPoint = new Point(0,int.MinValue);
        private Point maxXPoint = new Point(int.MinValue,0);
        private Point minYPoint = new Point(0, int.MaxValue );
        private Point minXPoint = new Point(int.MaxValue,0);


        Pix[,] pixes;

        public void setSource(byte[] colorImgs, int width, int height)
        {

            if (colorImgs.Length != width * height * 4)
            {
                Console.WriteLine("数据规模不匹配，失败！！！");
                return;
            }
            this.width = width;
            this.height = height;
            pixes = new Pix[height, width];

            // 数组长度为 1228800 = 640 * 480 * 4
            // 其中640是宽度，480是高度
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int baseIndex = width * 4 * i + j * 4;
                    //Console.WriteLine("{0},{1} 对应的 baseIndex是 {2}", i, j, baseIndex);
                    pixes[i, j] = new Pix(colorImgs[baseIndex], colorImgs[baseIndex + 1], colorImgs[baseIndex + 2]);
                }
            }
        }
        


        private void findVertices()
        {
            
            int time = 1;

            // 为了具有点击后实时呈现的效果，对min和maxPoint进行初始化
            maxYPoint = new Point(0, int.MinValue);
            maxXPoint = new Point(int.MinValue, 0);
            minYPoint = new Point(0, int.MaxValue);
            minXPoint = new Point(int.MaxValue, 0);



            for (int i = 0;i < height;i++)
            {
                for(int j = 0;j < width; j++)
                {
                    // RGB值差距较大
                    if(j < width - 1 && hasBigDiff(pixes[i,j],pixes[i,j + 1]))
                    {
                        Point tmpPoint = new Point(i, j, time++);
                        // 对边界点进行更新
                        if(tmpPoint.x <= minXPoint.x)
                        {
                                minXPoint = tmpPoint;
                        }
                        if(tmpPoint.x >= maxXPoint.x)
                            maxXPoint = tmpPoint;
                        if(tmpPoint.y >= maxYPoint.y) 
                            maxYPoint = tmpPoint;
                        if (tmpPoint.y <= minYPoint.y)
                            minYPoint = tmpPoint;
                    }
                }
            }


            // 判断是否有两个点极为靠近
            List<Point> points = new List<Point>();
            points.Add(minXPoint); points.Add(minYPoint); points.Add(maxXPoint); points.Add(maxYPoint);
           
            if (isOverLap(points))
            {

                int minX = 1000, minY = 1000, maxX = -1, maxY = -1;
                for (int i= 0;i < 4;i++) {
                    minX = Math.Min(minX, points[i].x);
                    minY = Math.Min(minY, points[i].y);
                    maxX = Math.Max(maxX, points[i].x);
                    maxY = Math.Max(maxY, points[i].y);
                }
                leftBottomPoint.x = maxX;leftBottomPoint.y = minY;
                rightBottomPoint.x = maxX;rightBottomPoint.y = maxY;
                leftTopPoint.x = minX;leftTopPoint.y = minY;
                rightTopPoint.x = minX;rightTopPoint.y = maxY;
            }
            else  
            {
                // 判断是向顺时针倾斜还是向逆时针倾斜
                // 顺时针倾斜 是 先出现maxY 再出现 minY
                if (maxYPoint.time < minYPoint.time) // 顺时针倾斜
                {
                    leftTopPoint = minXPoint;
                    rightTopPoint = maxYPoint;
                    leftBottomPoint = minYPoint;
                    rightBottomPoint = maxXPoint;
                }
                else // 逆时针倾斜
                {
                    leftTopPoint = minYPoint;
                    rightTopPoint = minXPoint;
                    leftBottomPoint = maxXPoint;
                    rightBottomPoint = maxYPoint;
                }
            }
        }

        public bool isOverLap(List<Point> points)
        {
            for(int i = 0;i < 4;i++)
            {
                for(int j = 0;j < 4;j++)
                {
                    if (i == j)
                        continue;
                    else
                    {
                        if (points[i].isNear(points[j]))
                            return true;
                    }
                }
            }
            return false;
        }

        public byte[] process()
        {

            findVertices();
            int[,] tmp;
            tmp = getLeftTopIndex();
            render2Red(tmp[0, 0], tmp[0, 1]);
            tmp = getLeftBottomIndex();
            render2Red(tmp[0, 0], tmp[0, 1]);
            tmp = getRightBottomIndex();
            render2Red(tmp[0, 0], tmp[0, 1]);
            tmp = getRightTopIndex();
            render2Red(tmp[0, 0], tmp[0, 1]);
            byte[] colors = new byte[width * height * 4];
            int cnt = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Pix pix = pixes[i, j];
                    colors[cnt] = (byte)pix.R;
                    colors[cnt + 1] = (byte)pix.G;
                    colors[cnt + 2] = (byte)pix.B;
                    colors[cnt + 3] = 0;
                    cnt += 4;
                }
            }
            return colors;
        }

        public int[,] getLeftTopIndex()
        {
            int[, ] res = new int[1,2];
            res[0, 0] = leftTopPoint.x;
            res[0, 1] = leftTopPoint.y;
            return res;
            //return new int[leftTopPoint.x, leftTopPoint.y];
        }

        public int[,] getRightTopIndex()
        {
            int[,] res = new int[1, 2];
            res[0, 0] = rightTopPoint.x;
            res[0, 1] = rightTopPoint.y;
            return res;
        }
        public int[,] getLeftBottomIndex()
        {
            int[,] res = new int[1, 2];
            res[0, 0] = leftBottomPoint.x;
            res[0, 1] = leftBottomPoint.y;
            return res;
        }
        public int[,] getRightBottomIndex()
        {
            int[,] res = new int[1, 2];
            res[0, 0] = rightBottomPoint.x;
            res[0, 1] = rightBottomPoint.y;
            return res;
        }




        // 使用差值和计算两个像素的差距
        private int getDiff(Pix pix1, Pix pix2)
        {
            return Math.Abs(getRGBVal(pix1) - getRGBVal(pix2));
        }


        public void dispDiffs()
        {
            // 先计算横向坐标
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width - 1; j++)
                {
                    Console.Write(getDiff(pixes[i, j], pixes[i, j + 1]) + " ");
                }
                Console.WriteLine();
            }

        }



        public void dispVals()
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                    Console.Write(getRGBVal(pixes[i, j]) + " ");
                Console.WriteLine();
            }

        }


        private int getRGBVal(Pix pix)
        {
            return pix.R + pix.G + pix.B;
        }



        public void render2Red(int x, int y)
        {
            int stride = 4;
            //pixes[x, y] = new Pix(0, 0, 255);
            // 将[x,y]附近的像素染成红色
            for (int i = x - stride; i <= x + stride; i++)
            {
                for (int j = y - stride; j <= y + stride; j++)
                {
                    if (i >= 0 && i < height && j >= 0 && j < width)
                        pixes[i, j] = new Pix(0, 0, 255);
                }
            }
        }



        private bool hasBigDiff(Pix pix1, Pix pix2)
        {
            return getDiff(pix1, pix2) > 100;
        }



    
    }
    class Pix
    {
        public int R, G, B;
        public Pix(int R, int G, int B)
        {
            this.R = R;
            this.G = G;
            this.B = B;
        }
    }

    class Point
    {
        public int x, y, time;
        static int threshold = 30;
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Point(int x,int y,int time)
        {
            this.x = x;
            this.y = y;
            this.time = time;
        }
        public string info()
        {
            return "x = " + x + ", y = " + y + ", time = " + time + "\n";
        }

        public Boolean isNear(Point p)
        {
             return Math.Abs(x - p.x) + Math.Abs(y - p.y) <= threshold;
        }
    }
}
