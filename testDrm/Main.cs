﻿//
// Main.cs
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
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using Cairo;
using Crow.Linux;
using Crow;
using System.Reflection;
using System.Linq;

namespace testDrm
{
	
	public class TestApp : Application, IValueChange 
	{
		[STAThread]
		static void Main ()
		{
			System.Threading.Thread.CurrentThread.Name = "Main";
			try {
				using (TestApp crowApp = new TestApp ()) {
					crowApp.Run ();
				}
			} catch (Exception ex) {
				Console.WriteLine (ex.ToString ());
			}
			Console.WriteLine ("terminating");
		}

		#region IValueChange implementation
		public event EventHandler<ValueChangeEventArgs> ValueChanged;
		public virtual void NotifyValueChanged(string MemberName, object _value)
		{
			//Debug.WriteLine ("Value changed: {0}->{1} = {2}", this, MemberName, _value);
			ValueChanged.Raise(this, new ValueChangeEventArgs(MemberName, _value));
		}
		#endregion



		public TestApp () : base () {

		}

		public int frameTime = 0;
		public int frameMin = int.MaxValue;
		public int frameMax = 0;
		#region Test values for Binding
//		public int intValue = 500;
//		DirectoryInfo curDir = new DirectoryInfo (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
//		public FileSystemInfo[] CurDirectory {
//			get { return curDir.GetFileSystemInfos (); }
//		}
//		public int IntValue {
//			get {
//				return intValue;
//			}
//			set {
//				intValue = value;
//				NotifyValueChanged ("IntValue", intValue);
//			}
//		}
//		void onSpinnerValueChange(object sender, ValueChangeEventArgs e){
//			if (e.MemberName != "Value")
//				return;
//			intValue = Convert.ToInt32(e.NewValue);
//		}
//		void change_alignment(object sender, EventArgs e){
//			RadioButton rb = sender as RadioButton;
//			if (rb == null)
//				return;
//			NotifyValueChanged ("alignment", Enum.Parse(typeof(Alignment), rb.Caption));
//		}
//		public IList<String> List2 = new List<string>(new string[]
//			{
//				"string1",
//				"string2",
//				"string3",
//			}
//		);
//		public IList<String> TestList2 {
//			set{
//				List2 = value;
//				NotifyValueChanged ("TestList2", testList);
//			}
//			get { return List2; }
//		}
		IList<Crow.Color> testList = Crow.Color.ColorDic;
		public IList<Crow.Color> TestList {
			set{
				testList = value;
				NotifyValueChanged ("TestList", testList);
			}
			get { return testList; }
		}
//		string curSources = "";
//		public string CurSources {
//			get { return curSources; }
//			set {
//				if (value == curSources)
//					return;
//				curSources = value;
//				NotifyValueChanged ("CurSources", curSources);
//			}
//		}
//		bool boolVal = true;
//		public bool BoolVal {
//			get { return boolVal; }
//			set {
//				if (boolVal == value)
//					return;
//				boolVal = value;
//				NotifyValueChanged ("BoolVal", boolVal);
//			}
//		}

		#endregion

		public override void Run ()
		{			
			Stopwatch frame = new Stopwatch ();
//			Load ("#testDrm.ui.menu.crow").DataSource = this;
//			Load ("#testDrm.ui.0.crow").DataSource = this;
//			Load ("#testDrm.ui.0.crow").DataSource = this;
//			Load ("#testDrm.ui.0.crow").DataSource = this;

			System.Threading.Thread.Sleep (50);
			Load ("#testDrm.ui.menu.crow").DataSource = this;
			int i = 0;
			while(Running){
				try {
					frame.Restart();
					i++;

					base.Run ();

					frame.Stop();
					frameTime = (int)frame.ElapsedTicks;
					NotifyValueChanged("frameTime", frameTime);
					if (frameTime > frameMax){
						frameMax = frameTime;
						NotifyValueChanged("frameMax", frameMax);	
					}
					if (frameTime < frameMin){
						frameMin = frameTime;
						NotifyValueChanged("frameMin", frameMin);	
					}

				} catch (Exception ex) {
					Console.WriteLine (ex.ToString());
				}
			}
		}
		void onQuitClick(object send, Crow.MouseButtonEventArgs e)
		{
			Running = false;
		}
		void onLoadClick(object send, Crow.MouseButtonEventArgs e)
		{
			Console.WriteLine ("********** LOADING ui item ******************");
			GraphicObject go = Load ("#testDrm.ui.2.crow");
			Console.WriteLine ("********** SETTING DATASOURCE ON ITEM ******************");
			go.DataSource = this;
			Console.WriteLine ("********** LOADING FINISHED ******************");
		}
	}
}
