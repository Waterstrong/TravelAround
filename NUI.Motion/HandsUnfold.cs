using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUI.Data;
using Microsoft.Kinect;


namespace NUI.Motion
{
    ///// <summary>
    ///// 双手水平展开
    ///// </summary>
    //partial class HandsUnfold : MotionSuper
    //{
    //    protected override bool Recognise(FeatureData data)
    //    {
    //        switch (_motionState)
    //        {
    //            case MotionState.Initial:
    //                // 手超过了脊椎的位置
    //                if (data.CompareThresholdY(JointType.HandRight, JointType.Spine, 0) == 1 &&
    //                    data.CompareThresholdY(JointType.HandLeft, JointType.Spine, 0) == 1) 
    //                {
    //                    // 用肩中心到脊椎的距离替代肩宽，理论上比肩宽大一些
    //                    // 并且两手距离很近
    //                    if (data.Hand2HandHorizontalDistance < data.ShoulderCenter2SpineHeight)
    //                    {
    //                        // 双手明显交叉
    //                        if (data.CalculateDifferX(JointType.HandRight, JointType.HandLeft) < 0/*data.CalculateDifferX(JointType.ShoulderLeft, JointType.ShoulderCenter)*/)
    //                        {
    //                            _motionState = MotionState.Valid;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        _motionState = MotionState.Invalid;
    //                    }
    //                }
    //                break;
    //            case MotionState.Valid:
    //                // 上半身的高度，用于近视计算手的比例
    //                // 当两手距离超过阈值后识别成功
    //                if (data.Hand2HandHorizontalDistance > 2 * data.ShoulderCenter2HipCenterHeight )
    //                {
    //                    _motionState = MotionState.Invalid; // 无论是否成功识别者标记为无效状态
    //                    // 区别于双手向下的动作
    //                    if (data.CompareThresholdY(JointType.HandRight, JointType.ShoulderRight, 5) >= 0 &&
    //                        data.CompareThresholdY(JointType.HandLeft, JointType.ShoulderLeft, 5) >= 0 )
    //                    {
    //                        return true;
    //                    }
    //                }
    //                break;
    //            default:
    //                break;
    //        }
    //        // 如果手抬起太高，则抬起失效
    //        if (_motionState != MotionState.Invalid &&
    //            (data.CompareThresholdY(JointType.HandRight, JointType.ShoulderCenter, 10) == 1 ||
    //            data.CompareThresholdY(JointType.HandLeft, JointType.ShoulderCenter, 10) == 1))
    //        {
    //            _motionState = MotionState.Invalid;
    //        }
    //        // 当手放下时恢复初始
    //        if (_motionState != MotionState.Initial)
    //        {
    //            if (data.CompareThresholdY(JointType.HandRight, JointType.Spine, 0) == -1 ||
    //                data.CompareThresholdY(JointType.HandLeft, JointType.Spine, 0) == -1)
    //            {
    //                _motionState = MotionState.Initial;
    //            }
    //        }
    //        return false;
    //    }
    //    protected override void SendCommand()
    //    {
    //        UDPFactory.GetSender().SendMessage("23");
    //        // Console.WriteLine("双手水平展开" + _recognizedID.ToString());
    //    }
    //}
}
