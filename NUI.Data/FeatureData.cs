using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;

namespace NUI.Data
{
    /// <summary>
    /// 特征数据类
    /// </summary>
    public class FeatureData
    {
        #region 基本的属性和方法
        private const float Scale = 100f; // 转化m为cm
        private const int MaxSize = 20; // 最大关节点数目

        private Joint[] _relativeJoints = new Joint[MaxSize]; //存储相对关节坐标点及相关信息
        public Joint[] RelativeJoints
        {
            get { return _relativeJoints; }
        }
        private SkeletonPoint _absoluteOrigin; // 原点的绝对坐标
        public SkeletonPoint AbsoluteOrigin
        {
            get { return _absoluteOrigin; }
        }

        private float _head2HipCenterHeight; // Head到HipCenter的高度
        public float Head2HipCenterHeight
        {
            get { return _head2HipCenterHeight; }
        }

        private float _head2SpineHeight; // Head到Spine的高度
        public float Head2SpineHeight
        {
            get { return _head2SpineHeight; }
        }

        private float _shoulderCenter2HipCenterHeight; // ShoulderCenter到HipCenter的高度
        public float ShoulderCenter2HipCenterHeight
        {
            get { return _shoulderCenter2HipCenterHeight; }
        }

        private float _shoulderCenter2SpineHeight; // ShoulderCenter到Spine的高度
        public float ShoulderCenter2SpineHeight
        {
            get { return _shoulderCenter2SpineHeight; }
        }

        private float _handRight2BodyHorizontalDistance; // 右手到身体的水平距离
        public float HandRight2BodyHorizontalDistance
        {
            get { return _handRight2BodyHorizontalDistance; }
        }
        private float _handLeft2BodyHorizontalDistance; // 左手到身体的水平距离
        public float HandLeft2BodyHorizontalDistance
        {
            get { return _handLeft2BodyHorizontalDistance; }
        }

        private float _hand2HandHorizontalDistance; // 两手之间的水平距离
        public float Hand2HandHorizontalDistance
        {
            get { return _hand2HandHorizontalDistance; }
            set { _hand2HandHorizontalDistance = value; }
        }
        
        /// <summary>
        /// 设置相对坐标
        /// </summary>
        /// <param name="s"></param>
        public void SetRelativeJoints(Skeleton s)
        {
            // 设置绝对原点坐标，比例乘100，转化为CM
            _absoluteOrigin.X = (s.Joints[JointType.HipCenter].Position.X + s.Joints[JointType.Spine].Position.X) / 2 * Scale;
            _absoluteOrigin.Y = (s.Joints[JointType.ShoulderRight].Position.Y + s.Joints[JointType.ShoulderLeft].Position.Y) / 2 * Scale;
            _absoluteOrigin.Z = (s.Joints[JointType.ShoulderRight].Position.Z + s.Joints[JointType.ShoulderLeft].Position.Z) / 2 * Scale;

            SkeletonPoint position = new SkeletonPoint(); // struct陷阱，要单独用中间变量
            for (int i = 0; i < s.Joints.Count; ++i )
            {
                _relativeJoints[i] = s.Joints[(JointType)i]; // 将所有信息都赋值
                // 注意struct陷阱，所以要单独用中间变量
                position.X = _relativeJoints[i].Position.X * Scale - _absoluteOrigin.X;
                position.Y = _relativeJoints[i].Position.Y * Scale - _absoluteOrigin.Y;
                position.Z = _absoluteOrigin.Z - _relativeJoints[i].Position.Z * Scale;

                _relativeJoints[i].Position = position; // 只赋值相对坐标，其他信息不变
            }

            // 计算出常用的特征信息
            _head2HipCenterHeight = CalculateDifferY(JointType.Head, JointType.HipCenter);
            _head2SpineHeight = CalculateDifferY(JointType.Head, JointType.Spine);
            _shoulderCenter2HipCenterHeight = CalculateDifferY(JointType.ShoulderCenter, JointType.HipCenter);
            _shoulderCenter2SpineHeight = CalculateDifferY(JointType.ShoulderCenter, JointType.Spine);

            // 右手到身体(Y轴)的水平距离
            float differX = _relativeJoints[(int)JointType.HandRight].Position.X;
            float differZ = _relativeJoints[(int)JointType.HandRight].Position.Z;
            _handRight2BodyHorizontalDistance = (float)Math.Sqrt(differX * differX + differZ * differZ);
            // 左手到身体(Y轴)的水平距离
            differX = _relativeJoints[(int)JointType.HandLeft].Position.X;
            differZ = _relativeJoints[(int)JointType.HandLeft].Position.Z;
            _handLeft2BodyHorizontalDistance = (float)Math.Sqrt(differX * differX + differZ * differZ);

            // 两手之间的水平距离
            differX = CalculateDifferX(JointType.HandRight, JointType.HandLeft);
            differZ = CalculateDifferZ(JointType.HandRight, JointType.HandLeft);
            _hand2HandHorizontalDistance = (float)Math.Sqrt(differX * differX + differZ * differZ);

        }
        #endregion

        public bool IsJointTracked(JointType jt)
        {
            if (_relativeJoints[(int)jt].TrackingState == JointTrackingState.Tracked)
            {
                return true;
            }
            return false;
        }

        #region 对空间信息进行获取或者操作
        /// <summary>
        /// 获得多个JointType对应的关节点的中心/质心
        /// </summary>
        /// <param name="jointTypes">参数，可以指定多个</param>
        /// <returns>返回多个关节点的中心/质心</returns>
        public SkeletonPoint CalculateCenterPoint(params JointType[] jointTypes)
        {
            SkeletonPoint centerPoint = new SkeletonPoint();
            if (jointTypes.Length < 1)
            {
                centerPoint.X = centerPoint.Y = centerPoint.Z = 0;
                return centerPoint;
            }
            float sumX = 0f;
            float sumY = 0f;
            float sumZ = 0f;
            foreach (JointType jointType in jointTypes)
            {
                sumX += _relativeJoints[(int)jointType].Position.X;
                sumY += _relativeJoints[(int)jointType].Position.Y;
                sumZ += _relativeJoints[(int)jointType].Position.Z;
            }
            centerPoint.X = sumX / jointTypes.Length;
            centerPoint.Y = sumY / jointTypes.Length;
            centerPoint.Z = sumZ / jointTypes.Length;
            return centerPoint;
        }
        /// <summary>
        /// 计算多个点的空间长度距离，若有超过两个关节点，则分别计算两两距离再相加
        /// </summary>
        /// <param name="jointTypes">传入关节点类型集合</param>
        /// <returns>返回空间叠加长度</returns>
        public float CalculateSpaceDistance(params JointType[] jointTypes)
        {
            float distance = 0f;
            if (jointTypes.Length < 2)
            {
                return distance;
            }
            SkeletonPoint prePos = _relativeJoints[(int)jointTypes[0]].Position;
            SkeletonPoint curPos;
            float differX, differY, differZ;
            for (int i = 1; i < jointTypes.Length; ++i)
            {
                curPos = _relativeJoints[(int)jointTypes[i]].Position;
                differX = prePos.X - curPos.X;
                differY = prePos.Y - curPos.Y;
                differZ = prePos.Z - curPos.Z;
                distance += (float)Math.Sqrt(differX * differX + differY * differY + differZ * differZ);
                prePos = curPos;
            }
            return distance;
        }
        #endregion

        #region 对一维方向上的信息进行操作
        /// <summary>
        /// 计算两关节点在X方向上的差值
        /// </summary>
        /// <param name="jointType1">起始关节点</param>
        /// <param name="jointType2">结束关节点</param>
        /// <returns>返回在X方向上关节点1减去关节点2的值</returns>
        public float CalculateDifferX(JointType jointType1, JointType jointType2)
        {
            return (_relativeJoints[(int)jointType1].Position.X - _relativeJoints[(int)jointType2].Position.X);
        }
        /// <summary>
        /// 计算两关节点在Y方向上的差值
        /// </summary>
        /// <param name="jointType1">起始关节点</param>
        /// <param name="jointType2">结束关节点</param>
        /// <returns>返回在Y方向上关节点1减去关节点2的值</returns>
        public float CalculateDifferY(JointType jointType1, JointType jointType2)
        {
            return (_relativeJoints[(int)jointType1].Position.Y - _relativeJoints[(int)jointType2].Position.Y);
        }
        /// <summary>
        /// 计算两关节点在Z方向上的差值
        /// </summary>
        /// <param name="jointType1">起始关节点</param>
        /// <param name="jointType2">结束关节点</param>
        /// <returns>返回在Z方向上关节点1减去关节点2的值</returns>
        public float CalculateDifferZ(JointType jointType1, JointType jointType2)
        {
            return (_relativeJoints[(int)jointType1].Position.Z - _relativeJoints[(int)jointType2].Position.Z);
        }
        #endregion

        #region 判断关节点在某个方向上一定程序的大小关系
        /// <summary>
        /// 比较关节点在X方向上一定程序的大小
        /// </summary>
        /// <param name="jointType1">关节点1</param>
        /// <param name="jointType2">关节点2</param>
        /// <param name="threshold">判断相等条件的阈值,默认为10cm，在[-threshold,threshold]内都为相等，即自由量为2*threshold</param>
        /// <returns>返回判断结果，1表示前者明显大于后者，-1表示小于，0表示在阈值范围内</returns>
        public int CompareThresholdX(JointType jointType1, JointType jointType2, float threshold = 10f)
        {
            float differ = _relativeJoints[(int)jointType1].Position.X - _relativeJoints[(int)jointType2].Position.X;
            return CompareThreshold(differ, threshold);
        }
        /// <summary>
        /// 比较关节点在Y方向上一定程序的大小
        /// </summary>
        /// <param name="jointType1">关节点1</param>
        /// <param name="jointType2">关节点2</param>
        /// <param name="threshold">判断相等条件的阈值,默认为10cm，在[-threshold,threshold]内都为相等，即自由量为2*threshold</param>
        /// <returns>返回判断结果，1表示前者明显大于后者，-1表示小于，0表示在阈值范围内</returns>
        public int CompareThresholdY(JointType jointType1, JointType jointType2, float threshold = 10f)
        {
            float differ = _relativeJoints[(int)jointType1].Position.Y - _relativeJoints[(int)jointType2].Position.Y;
            return CompareThreshold(differ, threshold);
        }
        /// <summary>
        /// 比较关节点在Z方向上一定程序的大小
        /// </summary>
        /// <param name="jointType1">关节点1</param>
        /// <param name="jointType2">关节点2</param>
        /// <param name="threshold">判断相等条件的阈值,默认为10cm，在[-threshold,threshold]内都为相等，即自由量为2*threshold</param>
        /// <returns>返回判断结果，1表示前者明显大于后者，-1表示小于，0表示在阈值范围内</returns>
        public int CompareThresholdZ(JointType jointType1, JointType jointType2, float threshold = 10f)
        {
            float differ = _relativeJoints[(int)jointType1].Position.Z - _relativeJoints[(int)jointType2].Position.Z;
            return CompareThreshold(differ, threshold);
        }
        /// <summary>
        /// 比较两数差在一定程度上的是否超出某个范围
        /// </summary>
        /// <param name="differ">前者减后者的差值</param>
        /// <param name="threshold">判断的阈值，在[-threshold,threshold]内都为相等，即自由量为2*threshold</param>
        /// <returns>返回判断结果，1表示前者明显大于后者，-1表示小于，0表示在阈值范围内</returns>
        private int CompareThreshold(float differ, float threshold)
        {
            if (differ > threshold) // num1 > num2
            {
                return 1;
            }
            else if (differ < -threshold) // num1 < num2
            {
                return -1;
            }
            else // num1 与 num2 相差范围在 [-threshlod, threshlod]视为相等
            {
                return 0;
            }
        }
        #endregion
        
    }
}
