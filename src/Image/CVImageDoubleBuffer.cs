﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using System.Threading;

namespace VVVV.Nodes.EmguCV
{
	public class CVImageDoubleBuffer
	{
		#region Data
		private CVImage FBackBuffer = new CVImage();
		private CVImage FFrontBuffer = new CVImage();
		private CVImageAttributes FImageAttributes;
		private bool FAllocated = false;
		#endregion

		#region Locking
		private ReaderWriterLock FFrontLock = new ReaderWriterLock();
		private Object FBackLock = new Object();
		private Object FAttributesLock = new Object();
		public static int LockTimeout = 10000;

		public void LockForReading()
		{
			FFrontLock.AcquireReaderLock(CVImageDoubleBuffer.LockTimeout);
		}

		public void ReleaseForReading()
		{
			FFrontLock.ReleaseReaderLock();	
		}
		#endregion

		#region Events
		//these virtual functions are overwriten in child classes that want to trigger events
		public virtual void OnImageUpdate() { }
		public virtual void OnImageAttributesUpdate(CVImageAttributes attributes) { }
		#endregion

		#region Swapping and copying
		/// <summary>
		/// Swap the front buffer and back buffer
		/// </summary>
		public void Swap()
		{
			lock (FBackLock)
			{
				FFrontLock.AcquireWriterLock(LockTimeout);
				try
				{
					CVImage swap = FBackBuffer;
					FBackBuffer = FFrontBuffer;
					FFrontBuffer = swap;
				}
				finally
				{
					FFrontLock.ReleaseWriterLock();
				}
			}
		}
		#endregion

		#region Get/set the image
		public void GetImage(CVImage target)
		{
			LockForReading();
			try
			{
				FFrontBuffer.GetImage(target);
			}
			finally
			{
				ReleaseForReading();
			}
		}

		/// <summary>
		/// Copy the input image into the back buffer
		/// </summary>
		/// <param name="source">Input image</param>
		public void SetImage(CVImage source)
		{
			bool Reinitialise;

			lock (FBackLock)
				Reinitialise = FBackBuffer.SetImage(source);

			FAllocated = true;
			OnImageUpdate();

			if (Reinitialise)
			{
				lock (FAttributesLock)
					FImageAttributes = source.ImageAttributes.Clone() as CVImageAttributes;
				OnImageAttributesUpdate(FImageAttributes);
			}

			Swap();
		}

		/// <summary>
		/// Copy the input image into the back buffer
		/// </summary>
		/// <param name="source">Input image</param>
		public void SetImage(IImage source)
		{
			bool Reinitialise;

			lock (FBackLock)
				Reinitialise = FBackBuffer.SetImage(source);
			
			FAllocated = true;
			OnImageUpdate();

			if (Reinitialise)
			{
				lock (FAttributesLock)
					FImageAttributes = FBackBuffer.ImageAttributes.Clone() as CVImageAttributes;
				OnImageAttributesUpdate(FImageAttributes);
			}

			Swap();
		}
		#endregion

		#region Accessors
		/// <summary>
		/// Get the front buffer. Be sure to lock the front buffer for reading!
		/// </summary>
		public CVImage FrontImage
		{
			get { return FFrontBuffer; }
		}

		public CVImageAttributes ImageAttributes
		{
			get {
				lock (FAttributesLock)
					return FImageAttributes;
			}
		}

		public bool Allocated
		{
			get
			{
				return FFrontBuffer.Allocated;
			}
		}

		public ReaderWriterLock FrontLock
		{
			get
			{
				return FFrontLock;
			}
		}
		#endregion
	}
}
