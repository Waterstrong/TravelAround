using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;
using NUI.Data;

namespace TravelAround
{
    /// <summary>
    /// 骨架处理后的通知者，观察者模式
    /// </summary>
    public class SkeletonSubject 
    {
        public delegate void DataBindingEventHandler(FeatureData data);
        public event DataBindingEventHandler DataBinding;

        FeatureData _featureData = new FeatureData();

        /// <summary>
        /// 通知观察者的目标+1
        /// </summary>
        /// <param name="sensor">Kinect Sensor</param>
        /// <param name="skeleton">传入对应的某个骨架</param>
        public void Notify(Skeleton skeleton)
        {
            if (DataBinding != null && skeleton != null)
            {
                // 传入骨架信息,转化为相对坐标的数据,返回特征数据
                _featureData.SetRelativeJoints(skeleton);
                DataBinding(_featureData);
            }

        }

        /// <summary>
        /// 通知观察者的目标+2
        /// </summary>
        /// <param name="skeletons">传入骨架数组</param>
        public void Notify(Skeleton[] skeletons)
        {
            if (DataBinding != null)
            {
                foreach (Skeleton skeleton in skeletons)
                {
                    // 传入骨架信息,转化为相对坐标的数据,返回特征数据
                    _featureData.SetRelativeJoints(skeleton);
                    DataBinding(_featureData);
                }
            }

        }
    }
}
