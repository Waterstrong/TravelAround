using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using Microsoft.Kinect;

namespace NUI.Data
{
    /// <summary>
    /// 支持骨骼信息映射数据的类
    /// </summary>
    public class SkeletonData
    {
        private Skeleton _skeleton = null; // 实际的骨骼关节点信息
        private KinectSensor _sensor = null; // Kinect Sensor
        /// <summary>
        /// 设备骨骼数据相当属性
        /// </summary>
        /// <param name="sensor">Kinect Sensor</param>
        /// <param name="skeleton">骨骼</param>
        public void SetSkeletonData(KinectSensor sensor, Skeleton skeleton)
        {
            _sensor = sensor;
            _skeleton = skeleton;
        }

        ///// <summary>
        ///// Maps a SkeletonPoint to lie within our render space and converts to Point
        ///// </summary>
        ///// <param name="_skeleton">point to map</param>
        ///// <returns>mapped point</returns>
        //public Point SkeletonPointToScreen(JointType jointType)
        //{
        //    if (_sensor == null || _skeleton == null)
        //    {
        //        return new Point(0d, 0d);
        //    }
        //    // Convert point to depth space.  
        //    // We are not using depth directly, but we do want the points in our 320x240 output resolution.
        //    DepthImagePoint depthPoint = _sensor.MapSkeletonPointToDepth(_skeleton.Joints[jointType].Position, DepthImageFormat.Resolution320x240Fps30);
        //    return new Point(depthPoint.X, depthPoint.Y);
        //}
        /// <summary>
        /// 获得Spine映射点
        /// </summary>
        /// <returns></returns>
        //public Point SpinePointToScreen()
        //{
        //    return SkeletonPointToScreen(JointType.Spine);
        //}
    }
}
