﻿#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using System.Threading;
using System.Collections.Generic;

#endregion usings

namespace VVVV.Nodes.EmguCV
{
	class CaptureVideoInstance
	{
		public int FCameraID = -1;
		public string Status;

		Thread FCaptureThread;
		bool FRunCaptureThread = false;

		Capture FCapture;
		public bool IsRunning = false;

		Image<Bgr, byte> FBuffer = null;
		public CVImageLink Image;

		public void Initialise(int id)
		{
			Close();
			try
			{
				FCapture = new Capture(id); //create a camera captue
			}
			catch
			{
				Status = "Camera open failed";
				IsRunning = false;
				return;
			}

			Status = "Camera open success";
			IsRunning = true;

			FCameraID = id;

			FBuffer = new Image<Bgr, byte>(new System.Drawing.Size(FCapture.Width, FCapture.Height));
			Image = new CVImageLink();
			
			FRunCaptureThread = true;
			FCaptureThread = new Thread(fnCapture);
			FCaptureThread.Start();
		}

		private void fnCapture()
		{
			while (FRunCaptureThread)
			{
				FBuffer = FCapture.QueryFrame();

				Image.UpdateImage(FBuffer);

				//allow a gap where we're not locked
				Thread.Sleep(5);
			}
		}

		public void Close()
		{
			if (IsRunning)
			{

				FRunCaptureThread = false;
				FCaptureThread.Join(100);
				FCapture.Dispose();
				FBuffer.Dispose();
				IsRunning = false;
			}
		}

	}
    #region PluginInfo
    [PluginInfo(Name = "VideoIn",
                Category = "EmguCV",
                Version = "",
                Help = "Captures from DShow device to IPLImage",
                Tags = "")]
    #endregion PluginInfo
    public class CaptureVideoNode : IPluginEvaluate, IDisposable
    {
        #region fields & pins

        [Input("Camera ID", DefaultValue = 0, MinValue=0)]
        IDiffSpread<int> FPinInCameraID;

        [Output("Image")]
        ISpread<CVImageLink> FPinOutImage;

        [Output("Status")]
        ISpread<string> FPinOutStatus;

        [Import]
        ILogger FLogger;

        IPluginHost FHost;

		Dictionary<int, CaptureVideoInstance> FCaptures = new Dictionary<int, CaptureVideoInstance>();

        #endregion fields & pins

        // import host and hand it to base constructor
        [ImportingConstructor()]
		public CaptureVideoNode(IPluginHost host)
        {
            FHost = host;
        }

		public void Dispose()
		{
			foreach (KeyValuePair<int, CaptureVideoInstance> capture in FCaptures)
				capture.Value.Close();

			GC.SuppressFinalize(this);
		}

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
			if (SpreadMax == 0)
			{
				Resize(0);
				return;
			}

			Resize(SpreadMax);

			GiveOutputs();
        }

		void GiveOutputs()
		{
			foreach (KeyValuePair<int, CaptureVideoInstance> capture in FCaptures)
			{
				FPinOutImage[capture.Key] = capture.Value.Image;
				FPinOutStatus[capture.Key] = capture.Value.Status;
			}
		}

		void Resize(int SpreadMax)
		{
			FPinOutStatus.SliceCount = SpreadMax;
			FPinOutImage.SliceCount = SpreadMax;

			for (int i = 0; i < SpreadMax; i++)
			{
				if (FPinOutImage[i] == null)
				{
					FPinOutImage[i] = new CVImageLink();
				}
			}

			for (int i = 0; i < SpreadMax; i++)
			{
				if (!FCaptures.ContainsKey(i))
				{
					FCaptures.Add(i, new CaptureVideoInstance());
					FCaptures[i].Initialise(FPinInCameraID[i]);
				}
				if (FCaptures[i].FCameraID != FPinInCameraID[i])
					FCaptures[i].Initialise(FPinInCameraID[i]);
			}

			if (FCaptures.Count > SpreadMax)
			{
				for (int i = SpreadMax; i < FCaptures.Count; i++)
				{
					FCaptures.Remove(i);
				}
			}
		}
    }
}