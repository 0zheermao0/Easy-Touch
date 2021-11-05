using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace kinectTest.Entities
{
    /// <summary>
    /// 二维坐标封装类
    /// </summary>
    public class TPoint
    {
        /// <summary>
        /// 横坐标
        /// </summary>
        public double x;

        /// <summary>
        /// 纵坐标
        /// </summary>
        public double y;

     

        public TPoint(double xTemp, double yTemp) {
            this.x = xTemp;
            this.y = yTemp;
        }

        public double getX() {
            return this.x;
        }

        public double getY() {
            return this.y;
        }

        public void setX(double xTemp) {
            this.x = xTemp;
        }

        public void setY(double yTemp) {
            this.y = yTemp;
        }
        
    }
}
