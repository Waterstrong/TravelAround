using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUI.Data;
using Microsoft.Kinect;


namespace NUI.Motion
{
    /// <summary>
    /// 手右向下
    /// </summary>
    partial class HandRightDown : MotionSuper
    {
        private const double TimeMillisecondsThreshold = 850d; // 时间阈值，以毫秒计算
        protected override bool Recognise(FeatureData data)
        {
            // 如果另一只手不在初始位置，则不识别
            if (data.CompareThresholdY(JointType.HandLeft, JointType.Spine, 0) == 1)
            {
                _motionState = MotionState.Invalid;
                return false;
            }
            switch (_motionState)
            {
                case MotionState.Initial:
                    // 如果手超过肩位置，则手抬起开始生效
                    if (data.CompareThresholdY(JointType.HandRight, JointType.ShoulderRight, 0) == 1)
                    {
                        _dtBegin = DateTime.Now; // 获取开始时间
                        _motionState = MotionState.Valid;
                    }
                    break;
                case MotionState.Valid:
                    // 如果生效了，则判断是否回到初始位置，如果是，则识别成功
                    if (data.CompareThresholdY(JointType.HandRight, JointType.Spine, 0) == -1)
                    {
                        _motionState = MotionState.Initial;
                        _dtEnd = DateTime.Now; // 获取截止时间
                        // 如果时间差小于一定的阈值则识别
                        if (_dtEnd.Subtract(_dtBegin).TotalMilliseconds < TimeMillisecondsThreshold)
                        {
                            return true;
                        }
                    }
                    break;
                default:
                    // 其他状态回到了初始位置，则重置状态为初始状态
                    if (data.CompareThresholdY(JointType.HandRight, JointType.Spine, 0) == -1)
                    {
                        _motionState = MotionState.Initial;
                    }
                    break;
            }
            if (_motionState != MotionState.Invalid)
            {
                // 如果手抬起太高，则抬起失效
                if (data.CompareThresholdY(JointType.HandRight, JointType.Head, 8) == 1)
                {
                    _motionState = MotionState.Invalid;
                }
                // 超出身体指定的范围标记为无效，此时刚好为到Y轴的距离
                if (data.HandRight2BodyHorizontalDistance > data.Head2HipCenterHeight)
                {
                    _motionState = MotionState.Invalid;
                }
            }
            return false;
        }
        protected override int SendCommand()
        {
            // Console.WriteLine("手右向下" + _recognizedID.ToString());
            return 37;
        }
    }
}
