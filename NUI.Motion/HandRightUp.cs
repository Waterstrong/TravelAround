using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUI.Data;
using Microsoft.Kinect;


namespace NUI.Motion
{
    /// <summary>
    /// 手右手向上
    /// </summary>
    partial class HandRightUp : MotionSuper
    {
        protected override bool Recognise(FeatureData data)
        {
            // 当另一只手也举起则无效
            if (data.CompareThresholdY(JointType.HandLeft, JointType.ShoulderLeft, 0) == 1)
            {
                _motionState = MotionState.Invalid;
                return false;
            }

            switch (_motionState)
            {
                case MotionState.Initial:
                    // 当超过肩开始生效
                    if (data.CompareThresholdY(JointType.HandRight, JointType.ShoulderLeft, 0) == 1)
                    { 
                        // 太近了则无效，区别于划圆
                        if (data.CalculateDifferX(JointType.HandRight, JointType.ShoulderCenter) < 5 ||
                            (data.HandRight2BodyHorizontalDistance < data.ShoulderCenter2SpineHeight &&
                            data.CalculateSpaceDistance(JointType.HandRight, JointType.ShoulderLeft) < data.ShoulderCenter2SpineHeight))
                        {
                            _motionState = MotionState.Invalid;
                        }
                        else
                        {
                            _motionState = MotionState.Valid;
                        }
                    }
                    break;
                case MotionState.Valid:
                    // 手举起后肘关节与头的相对位置
                    //if (data.CompareThresholdY(JointType.ElbowRight, JointType.Head, 13) == 0 &&
                    //    data.CompareThresholdX(JointType.ElbowRight, JointType.Head, 33) == 0)
                    if (data.CompareThresholdY(JointType.HandRight, JointType.Head, 8) == 1)
                    {
                        _motionState = MotionState.Invalid;
                        return true;
                    }
                    //else if (data.HandRight2BodyHorizontalDistance > 1.1 * data.Head2HipCenterHeight) // 在有效状态下，超出身体指定的范围标记为无效，可能是从侧面举起手的
                        
                    //{
                    //    // 水平距离的阈值,大致估计手的比例，确保身高不影响(可用1.8 * data.ShoulderCenter2SpineHeight)
                    //    _motionState = MotionState.Invalid;
                    //}
                    break;
                default:
                    break;
            }

            // 手放下来后恢复初始状态
            if (_motionState != MotionState.Initial &&
                data.CompareThresholdY(JointType.HandRight, JointType.Spine, 0) == -1)
            {
                _motionState = MotionState.Initial;
            }

            return false;
        }
        protected override int SendCommand()
        {
            // Console.WriteLine("手右手向上" + _recognizedID.ToString());
            return 35;
        }
    }
}
