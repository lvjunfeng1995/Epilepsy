﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Epilepsy
{
    /// <summary>
    /// CustomPoint.xaml 的交互逻辑
    /// </summary>
    public partial class CustomPoint : UserControl,IDisposable
    {
        public CustomPoint(double left,double top)
        {
            InitializeComponent();
            this.Margin = new Thickness(left,top,0,0);
        }

        /// <summary>
        /// 坐标
        /// </summary>
        public Point Point
        {
            get
            {
                return new Point(this.Margin.Left, this.Margin.Top);
            }
            set
            {
                this.Margin = new Thickness(value.X, value.Y, 0, 0);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                   
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // 清理非托管资源             
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        public void Close()
        {
            Dispose();
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~CustomPoint() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);

            //通知垃圾回收机制不再调用终结器（析构器）
            GC.SuppressFinalize(this);
        }

        ~CustomPoint()
        {
            Dispose(false);
        }
        #endregion
    }
}
