using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kinectTest.Entities;

namespace kinectTest.Entities
{
    /// <summary>
    /// image逻辑坐标集合类
    /// </summary>
    public class ScreenRectangle
    {

        /// <summary>
        /// 左上顶点
        /// </summary>
        private TPoint leftTop;

        /// <summary>
        /// 右上顶点
        /// </summary>
        private TPoint rightTop;

        /// <summary>
        /// 左下顶点
        /// </summary>
        private TPoint leftDown;

        /// <summary>
        /// 右下顶点
        /// </summary>
        private TPoint rightDown;

        public ScreenRectangle() {
            this.leftDown = new TPoint(-1,-1);
            this.leftTop = new TPoint(-1, -1);
            this.rightDown = new TPoint(-1, -1);
            this.rightTop = new TPoint(-1, -1);
            
        } 

        public void setLeftTop(double xtemp, double ytemp) {
            this.leftTop.x = xtemp;
            this.leftTop.y = ytemp;
        }

        public void setRightTop(double xtemp, double ytemp)
        {
            this.rightTop.x = xtemp;
            this.rightTop.y = ytemp;
        }

        public void setRightDown(double xtemp, double ytemp)
        {
            this.rightDown.x = xtemp;
            this.rightDown.y = ytemp;
        }

        public void setLeftDown(double xtemp, double ytemp)
        {
            this.leftDown.x = xtemp;
            this.leftDown.y = ytemp;
        }

        public TPoint getRightTop() {
            return this.rightTop;
        }

        public TPoint getLeftTop()
        {
            return this.leftTop;
        }

        public TPoint getRightDown()
        {
            return this.rightDown;
        }

        public TPoint getLeftDown()
        {
            return this.leftDown;
        }
    }
}
