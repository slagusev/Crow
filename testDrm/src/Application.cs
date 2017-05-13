﻿//
// DrmKms.cs
//
// Author:
//       Jean-Philippe Bruyère <jp.bruyere@hotmail.com>
//
// Copyright (c) 2013-2017 Jean-Philippe Bruyère
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using OpenTK.Platform.Linux;
using OpenTK;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Crow.Linux
{
	public class Application : IDisposable
	{
		DRMContext drm;
		public Interface CrowInterface;
		public bool mouseIsInInterface = false;

		void interfaceThread()
		{
			while (CrowInterface.ClientRectangle.Size.Width == 0)
				Thread.Sleep (5);

			while (true) {
				CrowInterface.Update ();
				Thread.Sleep (1);
			}
		}


		public Application(){
			drm = new DRMContext();
			CrowInterface = new Interface ();
			drm.CrowInterface = CrowInterface;

			Thread t = new Thread (interfaceThread);
			t.IsBackground = true;
			t.Start ();

			CrowInterface.ProcessResize (new Size (drm.width, drm.height));
			drm.updateCursor (XCursor.Default);

			CrowInterface.MouseCursorChanged += CrowInterface_MouseCursorChanged;
		}

		void CrowInterface_MouseCursorChanged (object sender, MouseCursorChangedEventArgs e)
		{
			drm.updateCursor (e.NewCursor);
		}

		public void Run (){
			drm.Run ();
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			drm.Dispose ();
		}
		#endregion
	}
}

