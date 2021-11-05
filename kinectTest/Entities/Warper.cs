using System;

namespace kinectTest
{
    /// <summary>
    /// This class is responsible for transforming a 2D-coordinate on a source rectangle onto a that of a destination rectangle.
    /// The transformation is linear and will not take into account bent or curved surfaces (the transformations are affine!).
    /// This is based on the work done by Johnny Lee and can be found here: http://johnnylee.net/projects/wii/
    /// </summary>
    internal class Warper
    {
        // 储存原矩阵的x轴坐标
        private double[] srcX = new double[5];
        // 储存原矩阵的y轴坐标
        private double[] srcY = new double[5];
        // 储存目的矩阵的坐标信息
        private double[] dstX = new double[5];
        private double[] dstY = new double[5];

        // 原矩阵
        private double[] srcMat = new double[16];
        // 目的矩阵
        private double[] dstMat = new double[16];
        // 将原矩阵变换到目的矩阵的仿射矩阵
        private double[] warpMat = new double[16];
        private bool dirty;

        /// <summary>
        /// Construct a new warper class.  Initialise with a perfect mapping.
        /// </summary>
        public Warper()
        {
            setIdentity();
        }

        public void setIdentity()
        {
            // Set the source and destination rectangles to be the same.
            setSource(0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 1.0, 1.0);
            setDestination(0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 1.0, 1.0);

            // Compute the matrix which transforms between the two.  Simples.
            computeWarp();
        }

        /// <summary>
        /// Set the source rectangle we are transforming coordinates from.
        /// </summary>
        /// <param name="x0">Top Left X</param>
        /// <param name="y0">Top Left Y</param>
        /// <param name="x1">Top Right X</param>
        /// <param name="y1">Top Right Y</param>
        /// <param name="x2">Bottom Left X</param>
        /// <param name="y2">Bottom Left Y</param>
        /// <param name="x3">Bottom Right X</param>
        /// <param name="y3">Bottom Right Y</param>
        /// <param name="x4">Center X</param>
        /// <param name="y4">Center Y</param>
        public void setSource(double x0, double y0,
                        double x1, double y1,
                        double x2, double y2,
                        double x3, double y3)
        {
            // Set the data.
            srcX[0] = x0;
            srcY[0] = y0;
            srcX[1] = x1;
            srcY[1] = y1;
            srcX[2] = x2;
            srcY[2] = y2;
            srcX[3] = x3;
            srcY[3] = y3;

            // Flag we need to change.
            dirty = true;
        }

        /// <summary>
        /// Set the destination rectangle we are transforming coordinates onto.
        /// </summary>
        /// <param name="x0">Top Left X</param>
        /// <param name="y0">Top Left Y</param>
        /// <param name="x1">Top Right X</param>
        /// <param name="y1">Top Right Y</param>
        /// <param name="x2">Bottom Left X</param>
        /// <param name="y2">Bottom Left Y</param>
        /// <param name="x3">Bottom Right X</param>
        /// <param name="y3">Bottom Right Y</param>
        /// <param name="x4">Center X</param>
        /// <param name="y4">Center Y</param>
        public void setDestination(double x0, double y0,
                        double x1, double y1,
                        double x2, double y2,
                        double x3, double y3)
        {
            // Set data.
            dstX[0] = x0;
            dstY[0] = y0;
            dstX[1] = x1;
            dstY[1] = y1;
            dstX[2] = x2;
            dstY[2] = y2;
            dstX[3] = x3;
            dstY[3] = y3;

            // Flag we need to change.
            dirty = true;
        }

        /// <summary>
        /// 计算任意一个四边形到另一个任意四边形的映射矩阵，以单位正方形为媒介
        /// Compute the new 'warp' matrix based on the source and destination rectangles.
        /// </summary>
        public void computeWarp()
        {
            // 以单位正方形为媒介计算原到目的的变换矩阵
            computeQuadToSquare(srcX[0], srcY[0], srcX[1], srcY[1], srcX[2], srcY[2], srcX[3], srcY[3], srcMat);
            computeSquareToQuad(dstX[0], dstY[0], dstX[1], dstY[1], dstX[2], dstY[2], dstX[3], dstY[3], dstMat);
            // 进行相乘,生成最终的映射矩阵
            multMats(srcMat, dstMat, warpMat);

            // Remove our flag so that we are not in need of update again until something changes.
            dirty = false;
        }

        /// <summary>
        /// A helper function to multiply two matracies.
        /// </summary>
        /// <param name="srcMat">Source matrix as a 16-double flattened 4x4 matrix.</param>
        /// <param name="dstMat">Destination matrix as a 16-double flattened 4x4 matrix.</param>
        /// <param name="resMat">Result matrix as a 16-double flattened 4x4 matrix.</param>
        public void multMats(double[] srcMat, double[] dstMat, double[] resMat)
        {
            // DSTDO/CBB: could be faster, but not called often enough to matter
            for (int r = 0; r < 4; r++)
            {
                int ri = r * 4;
                for (int c = 0; c < 4; c++)
                {
                    resMat[ri + c] = (srcMat[ri] * dstMat[c] +
                              srcMat[ri + 1] * dstMat[c + 4] +
                              srcMat[ri + 2] * dstMat[c + 8] +
                              srcMat[ri + 3] * dstMat[c + 12]);
                }
            }
        }

        /// <summary>
        /// 其中设定的参数表明 是单位正方形到任意四边形的映射矩阵计算
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        /// <param name="mat"></param>
        public void computeSquareToQuad(double x0,
                                            double y0,
                                            double x1,
                                            double y1,
                                            double x2,
                                            double y2,
                                            double x3,
                                            double y3,
                                            double[] mat)
        {

            double dx1 = x1 - x2, dy1 = y1 - y2;
            double dx2 = x3 - x2, dy2 = y3 - y2;
            double sx = x0 - x1 + x2 - x3;
            double sy = y0 - y1 + y2 - y3;
            double g = (sx * dy2 - dx2 * sy) / (dx1 * dy2 - dx2 * dy1);
            double h = (dx1 * sy - sx * dy1) / (dx1 * dy2 - dx2 * dy1);
            double a = x1 - x0 + g * x1;
            double b = x3 - x0 + h * x3;
            double c = x0;
            double d = y1 - y0 + g * y1;
            double e = y3 - y0 + h * y3;
            double f = y0;

            mat[0] = a; mat[1] = d; mat[2] = 0; mat[3] = g;
            mat[4] = b; mat[5] = e; mat[6] = 0; mat[7] = h;
            mat[8] = 0; mat[9] = 0; mat[10] = 1; mat[11] = 0;
            mat[12] = c; mat[13] = f; mat[14] = 0; mat[15] = 1;
        }

        public void computeQuadToSquare(double x0,
                                            double y0,
                                            double x1,
                                            double y1,
                                            double x2,
                                            double y2,
                                            double x3,
                                            double y3,
                                            double[] mat)
        {
            // 计算由单位正方形到任意四边形的变换矩阵，再求逆运算!!!!
            computeSquareToQuad(x0, y0, x1, y1, x2, y2, x3, y3, mat);

            // invert through adjoint

            double a = mat[0], d = mat[1],   /* ignore */        g = mat[3];
            double b = mat[4], e = mat[5],   /* 3rd col*/        h = mat[7];
            /* ignore 3rd row */
            double c = mat[12], f = mat[13];

            
            double A = e - f * h;
            double B = c * h - b;
            double C = b * f - c * e;
            double D = f * g - d;
            double E = a - c * g;
            double F = c * d - a * f;
            double G = d * h - e * g;
            double H = b * g - a * h;
            double I = a * e - b * d;

            // Probably unnecessary since 'I' is also scaled by the determinant,
            //   and 'I' scales the homogeneous coordinate, which, in turn,
            //   scales the X,Y coordinates.
            // Determinant  =   a * (e - f * h) + b * (f * g - d) + c * (d * h - e * g);
            double idet = 1.0f / (a * A + b * D + c * G);

            mat[0] = A * idet; mat[1] = D * idet; mat[2] = 0; mat[3] = G * idet;
            mat[4] = B * idet; mat[5] = E * idet; mat[6] = 0; mat[7] = H * idet;
            mat[8] = 0; mat[9] = 0; mat[10] = 1; mat[11] = 0;
            mat[12] = C * idet; mat[13] = F * idet; mat[14] = 0; mat[15] = I * idet;
        }

        /// <summary>
        /// Return a reference to the array which is the warp matrix.
        /// </summary>
        /// <returns>An array reference of a 4x4 matrix represented as a flattened 16-double array.</returns>
        public double[] getWarpMatrix()
        {
            return warpMat;
        }

        /// <summary>
        /// Transform a point from one rectangle onto another using this class's warp matrix.
        /// </summary>
        /// <param name="srcX">Source point, X coordinate.</param>
        /// <param name="srcY">Source point, Y coordinate.</param>
        /// <param name="dstX">Destination point, X coordinate.</param>
        /// <param name="dstY">Destination point, Y coordinate.</param>
        public void warp(double srcX, double srcY, ref double dstX, ref double dstY)
        {
            // If our matrix is out of date, recompute it.
            if (dirty)
                computeWarp();

            // Compute the coordinate transform.
            Warper.warp(warpMat, srcX, srcY, ref dstX, ref dstY);
        }

        /// <summary>
        /// Use a given matrix to transform a point on one rectangle onto another.
        /// </summary>
        /// <param name="mat">A reference to the array which describes the matrix we want to use.</param>
        /// <param name="srcX">Source point, X coordinate.</param>
        /// <param name="srcY">Source point, Y coordinate.</param>
        /// <param name="dstX">Destination point, X coordinate.</param>
        /// <param name="dstY">Destination point, Y coordinate.</param>
        public static void warp(double[] mat, double srcX, double srcY, ref double dstX, ref double dstY)
        {
            double[] result = new double[4];
            double z = 0;
            result[0] = (double)(srcX * mat[0] + srcY * mat[4] + z * mat[8] + 1 * mat[12]);
            result[1] = (double)(srcX * mat[1] + srcY * mat[5] + z * mat[9] + 1 * mat[13]);
            result[2] = (double)(srcX * mat[2] + srcY * mat[6] + z * mat[10] + 1 * mat[14]);
            result[3] = (double)(srcX * mat[3] + srcY * mat[7] + z * mat[11] + 1 * mat[15]);
            dstX = result[0] / result[3];
            dstY = result[1] / result[3];
        }
    }
}
