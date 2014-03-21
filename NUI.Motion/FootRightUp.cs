﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;
using NUI.Data;

namespace NUI.Motion
{
    /// <summary>
    /// 右抬脚
    /// </summary>
    partial class FootRightUp : MotionSuper
    {
        const float MinFootHeight = 13f;
        const float MaxFootHeight = 25f;
        protected override bool Recognise(FeatureData data)
        {
            switch (_motionState)
            {
                case MotionState.Initial:
                    float footHeight = data.CalculateDifferY(JointType.AnkleRight, JointType.AnkleLeft);
                    // 判断是否抬高脚data.CompareThresholdY(JointType.AnkleRight, JointType.AnkleLeft, 15) == 1
                    if (footHeight > MinFootHeight &&
                        footHeight < MaxFootHeight) // 用Y判断脚抬高度,脚离地阈值为15
                    {
                        _motionState = MotionState.Invalid;// 标记为无效，等待放下脚恢复初始状态
                        if (data.CompareThresholdZ(JointType.AnkleRight, JointType.HipRight, 30) == 0 && // 用Z排除非前踢腿
                        data.CompareThresholdX(JointType.AnkleRight, JointType.HipRight, 15) == 0) // 用X排除非左右侧踢腿
                        {
                            return true;
                        }
                    }
                    break;
                default:
                    // 放下脚恢复初始状态
                    if (data.CompareThresholdY(JointType.AnkleRight, JointType.AnkleLeft, 5) == 0)
                    {
                        _motionState = MotionState.Initial;
                    }
                    break;
            }
            return false;
        }
        protected override int SendCommand()
        {
            // Console.WriteLine("右抬脚" + _recognizedID.ToString());
            // RecognizeLogFactory.GetLogRecorder().WriteLog("识别为右抬脚:FootRightUp");
            return 2;
        }
    }
}
