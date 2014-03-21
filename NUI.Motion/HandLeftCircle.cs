﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUI.Data;
using Microsoft.Kinect;


namespace NUI.Motion
{
    /// <summary>
    /// 手左划圆
    /// </summary>
    partial class HandLeftCircle : MotionSuper
    {
        protected override bool Recognise(FeatureData data)
        {
            // 如果另一只手不在初始位置，则不识别
            if (data.CompareThresholdY(JointType.HandRight, JointType.Spine, 0) == 1)
            {
                _motionState = MotionState.Invalid;
                return false;
            }

            switch (_motionState)
            {
                case MotionState.Initial:
                    // 手超过一定位置
                    if (data.CompareThresholdY(JointType.HandLeft, JointType.ShoulderRight, 0) == 1)
                    {
                        // 用肩中心到脊椎的距离大致估计手的比例，确保身高不影响,超出身体指定的范围标记为无效，此时刚好为到Y轴的距离
                        // 并且手在身体附近
                        if (data.CalculateDifferX(JointType.ShoulderCenter, JointType.HandLeft) < 5 ||
                           (data.HandLeft2BodyHorizontalDistance < data.ShoulderCenter2SpineHeight &&
                            data.CalculateSpaceDistance(JointType.HandLeft, JointType.ShoulderRight) < data.ShoulderCenter2SpineHeight))
                        {
                            _motionState = MotionState.Valid;
                        }
                        else
                        {
                            _motionState = MotionState.Invalid;
                        }
                    }
                    break;
                case MotionState.Valid: // 达到第一个有效点后
                    // 判断是否超过了头
                    if (data.CompareThresholdY(JointType.HandLeft, JointType.Head, 0) == 1)
                    {
                        _motionState = MotionState.Valid2;
                    }
                    break;
                case MotionState.Valid2: // 第二个有效点
                    if (data.CompareThresholdY(JointType.HandLeft, JointType.ShoulderLeft, 15) == 0) // 经过了肩的位置，表明正放下
                    {
                        _motionState = MotionState.Invalid; // 无论结果如何都无效
                        
                        // 手是否伸直的,上身的高度，用于近视计算手的比例
                        if (data.HandLeft2BodyHorizontalDistance > data.Head2HipCenterHeight*0.8)
                        {
                            return true;
                        }
                    }
                    break;
                default:
                    break;
            }
            // 当手放下时
            if (_motionState != MotionState.Initial &&
                data.CompareThresholdY(JointType.HandLeft, JointType.Spine, 0) == -1)
            {
                _motionState = MotionState.Initial;
            }
            return false;
        }
        protected override int SendCommand()
        {
            // Console.WriteLine("手左划圆" + _recognizedID.ToString());
            return 30;
        }
    }
}
