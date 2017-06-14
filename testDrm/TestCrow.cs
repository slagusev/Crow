﻿//
// TestCrow.cs
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
using Crow;
using System.Runtime.InteropServices;

namespace testDrm
{
	public class TestCrow
	{
		const string lib = "/home/jp/devel/testsharedlib/bin/Debug/libcrow.so";

		[DllImport(lib)]
		unsafe static extern Rectangle* allocate();

		[DllImport(lib)]
		public static extern void update (int w, int h);


		unsafe static Rectangle* rect;

		unsafe static void Main(){
			rect = allocate();
			update (150, 160);
			Console.WriteLine (rect->Height);
			rect->Height = 200;
			Console.WriteLine (rect->Height);
//			Rectangle bounds = new Rectangle(0,0,1024,768);
//			Interface iface = new Interface();
//			iface.ProcessResize (bounds);
//
//			iface.LoadInterface ("#testDrm.ui.go.crow");
//
//			while (true)
//				iface.Update ();
		}
	}
}
