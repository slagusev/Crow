#define MONO_CAIRO_DEBUG_DISPOSE


using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;

using System.Diagnostics;

//using GGL;
using Crow;
using System.Threading;
using System.Collections.Generic;
using System.Linq;


namespace testOTK
{
	class GOLIBTests : OpenTKGameWindow
	{
		public GOLIBTests ()
			: base(800, 600,"test: press spacebar to toogle test files")
		{
			VSync = VSyncMode.Off;
			Interface.CurrentInterface = CrowInterface;
		}

		int frameCpt = 0;
		int idx = 0;
		string[] testFiles = {
			"testColorList.crow",
			"testCheckbox.goml",
			"testWindow.goml",
			"fps.goml",
			"testTabView.crow",
			"testExpandable.goml",
			"0.crow",
			"testImage.crow",
			"testOutOfClipUpdate.crow",
//			"test_Listbox.goml",
//			"testTreeView.crow",
			"1.crow",
			"clip4.crow",
			"clip3.crow",
			"clip2.crow",
			"clip0.crow",
			"clip1.crow",
//			"5.crow",
//			"testCombobox.goml",
//			"testPopper.goml",
			"testTextBox.crow",

			"4.crow",
			"testSpinner.goml",
			"testScrollbar.goml",
			"testGroupBox.goml",
			"testGrid.goml",
			"testButton.crow",
			"testBorder.goml",
//			"testButton2.crow",
			"test2WayBinding.crow",
			"test4.goml",
			"2.crow",
			"test1.goml",
			"testWindow2.goml",

			"testWindow3.goml",
			"testLabel.goml",
			"testAll.goml",
//			"testSpinner.goml",
//			"testRadioButton2.goml",
			"testContainer.goml",
			"testRadioButton.goml",
			"testMsgBox.goml",
//			"testMeter.goml",
		};


		volatile public int intValue = 25;

		public int IntValue {
			get {
				return intValue;
			}
			set {
				intValue = value;
				NotifyValueChanged ("IntValue", intValue);
			}
		}
		void onSpinnerValueChange(object sender, ValueChangeEventArgs e){
			if (e.MemberName != "Value")
				return;
			intValue = Convert.ToInt32(e.NewValue);
		}
		void change_alignment(object sender, EventArgs e){
			RadioButton rb = sender as RadioButton;
			if (rb == null)
				return;
			NotifyValueChanged ("alignment", Enum.Parse(typeof(Alignment), rb.Caption));
		}
		volatile public IList<String> List2 = new List<string>(new string[]
			{
				"string1",
				"string2",
				"string3",
				"string4",
				"string5",
				"string6",
				"string7",
				"string8",
				"string8",
				"string8",
				"string8",
				"string8",
				"string8",
				"string9"
			}
		);
		volatile IList<Color> testList = Color.ColorDic.ToList();
		public IList<Color> TestList {
			set{
				testList = value;
				NotifyValueChanged ("TestList", testList);
			}
			get { return testList; }
		}
		void OnClear (object sender, MouseButtonEventArgs e) => TestList = null;

		void OnLoadList (object sender, MouseButtonEventArgs e) => TestList = Color.ColorDic.ToList();

		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
			//this.AddWidget(new test4());
			KeyboardKeyDown += GOLIBTests_KeyboardKeyDown1;;

			CrowInterface.LoadInterfaceDelegate.Invoke ("Interfaces/" + testFiles [idx]);
			//GraphicObject obj = CrowInterface.LoadInterface("Interfaces/" + testFiles[idx]);
			//obj.DataSource = this;

		}
		void GOLIBTests_KeyboardKeyDown1 (object sender, OpenTK.Input.KeyboardKeyEventArgs e)
		{
			if (e.Key == OpenTK.Input.Key.Escape) {
				Quit (null, null);
				return;
			} else if (e.Key == OpenTK.Input.Key.L) {
				TestList.Add ("new string");
				NotifyValueChanged ("TestList", TestList);
				return;
			} else if (e.Key == OpenTK.Input.Key.W) {
				GraphicObject w = CrowInterface.LoadInterface("Interfaces/testWindow.goml");
				w.DataSource = this;
				return;
			}
			CrowInterface.ClearInterface ();
			idx++;
			if (idx == testFiles.Length)
				idx = 0;
			this.Title = testFiles [idx];
			GraphicObject obj = CrowInterface.LoadInterface("Interfaces/" + testFiles[idx]);
			obj.DataSource = this;
		}

//		protected override void OnUpdateFrame (FrameEventArgs e)
//		{
//			//if (frameCpt % 8 == 0)
//				base.OnUpdateFrame (e);
//
//			fps = (int)RenderFrequency;
//
//
//			if (frameCpt > 50) {
//				resetFps ();
//				frameCpt = 0;
//				GC.Collect();
//				GC.WaitForPendingFinalizers();
//				NotifyValueChanged("memory", GC.GetTotalMemory (false).ToString());
//			}
//			frameCpt++;
//		}
		void onButClick(object send, MouseButtonEventArgs e)
		{
			Console.WriteLine ("button clicked:" + send.ToString());
		}
		void onAddTabButClick(object sender, MouseButtonEventArgs e){

			TabView tv = CrowInterface.FindByName("tabview1") as TabView;
			if (tv == null)
				return;
			tv.AddChild (new TabItem () { Caption = "NewTab" });
		}
		[STAThread]
		static void Main ()
		{
			Console.WriteLine ("starting example");
			GOLIBTests win = new GOLIBTests ();
			win.Run (30);
			//win.KeyPressEvent += win.Win_KeyPressEvent;
		}

		void Win_KeyPressEvent (object o, Gtk.KeyPressEventArgs args)
		{
			CrowInterface.ClearInterface ();
			idx++;
			if (idx == testFiles.Length)
				idx = 0;
			this.Title = testFiles [idx];
			GraphicObject obj = CrowInterface.LoadInterface("Interfaces/" + testFiles[idx]);
			obj.DataSource = this;
		}



	}
}