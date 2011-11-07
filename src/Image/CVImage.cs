﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using System.Runtime.InteropServices;
using Emgu.CV.CvEnum;

namespace VVVV.Nodes.EmguCV
{
	class CVImage : ImageBase
	{
		IImage FImage;

		public override IImage GetImage()
		{
			return FImage;
		}

		public void Initialise(CVImageAttributes attributes)
		{
			Initialise(attributes.Size, attributes.ColourFormat);
		}
		public void Initialise(System.Drawing.Size size, TColourFormat format)
		{
			bool changedAttributes = FImageAttributes.CheckChanges(format, size);

			if (changedAttributes)
			{
				Allocate();
				OnImageAttributesUpdate(ImageAttributes);
			}
		}

		public void GetImage(TColourFormat format, CVImage target)
		{
			if (format == this.NativeFormat)
				CVImageUtils.CopyImage(this, target);
			else
				CVImageUtils.CopyImageConverted(this, target);
		}

		public unsafe void SetImage(IImage source)
		{
			TColourFormat sourceFormat = CVImageUtils.GetFormat(source);
			Initialise(source.Size, sourceFormat);

			lock (FLock)
			{
				CvInvoke.cvCopy(source.Ptr, FImage.Ptr, (new Image<Gray, byte>(Width, Height,new Gray(1.0d))).Ptr);
			}

			OnImageUpdate();
		}

		public void SetImage(CVImage source)
		{
			Initialise(source.Size, source.NativeFormat);

			CvInvoke.cvCopy(source.Ptr, FImage.Ptr, (new Image<Gray, byte>(Width, Height, new Gray(1.0d))).Ptr);

			OnImageUpdate();
		}

		override public void Allocate()
		{
			lock (FLock)
				FImage = CVImageUtils.CreateImage(this.Width, this.Height, this.NativeFormat);
		}
	}
}
