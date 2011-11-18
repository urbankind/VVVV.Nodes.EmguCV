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
	public class ErodeInstance : IFilterInstance
	{

		private int FIterations = 1;
		public int Iterations
		{
			set
			{
				if (value < 0)
					value = 0;
				if (value > 64)
					value = 64;

				FIterations = value;
			}
		}

		protected override void Initialise()
		{
			FOutput.Image.Initialise(FInput.Image.ImageAttributes);
		}

		public override void Process()
		{
			CvInvoke.cvErode(FInput.CvMat, FOutput.CvMat, IntPtr.Zero, FIterations);
			FOutput.Send();
		}

	}

	#region PluginInfo
	[PluginInfo(Name = "Erode", Category = "EmguCV", Version = "Filter", Help = "Reduce features in image, i.e. remove noise", Author = "", Credits = "", Tags = "")]
	#endregion PluginInfo
	public class ErodeNode : IFilterNode<ErodeInstance>
	{
		[Input("Iterations", MinValue=0, MaxValue=64, DefaultValue=1)]
		IDiffSpread<int> FIterations;

		protected override void Update(int InstanceCount)
		{
			if (FIterations.IsChanged)
				for (int i = 0; i < InstanceCount; i++)
					FProcessor[i].Iterations = FIterations[i];
		}
	}
}
