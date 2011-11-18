﻿#region using
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using System;
using VVVV.Utils.VColor;

#endregion

namespace VVVV.Nodes.EmguCV
{
	public class FrameDelayInstance : IFilterInstance
	{
		CVImage FBuffer = new CVImage();

		protected override void Initialise()
		{
			FBuffer.Initialise(FInput.ImageAttributes);
		}

		public override void Process()
		{
			if (FInput.Allocated)
			{
				if (FBuffer.Allocated)
				{
					FOutput.Image.SetImage(FBuffer);
					FOutput.Send();
				}

				FBuffer.SetImage(FInput.Image);
			}
		}

	}

	#region PluginInfo
	[PluginInfo(Name = "FrameDelay", Category = "EmguCV", Version = "Filter", Help = "Delay output by 1 frame", Author = "", Credits = "", Tags = "")]
	#endregion PluginInfo
	public class FrameDelayNode : IFilterNode<FrameDelayInstance>
	{

		protected override void Update(int SpreadMax)
		{
		}
	}
}