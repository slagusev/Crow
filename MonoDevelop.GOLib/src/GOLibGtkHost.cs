﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using go;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using System.Diagnostics;
using OpenTK.Input;
using MonoDevelop.DesignerSupport;

namespace MonoDevelop.GOLib
{
	public class GOLibGtkHost : Gtk.DrawingArea, ILayoutable, IGOLibHost, IPropertyPadProvider
	{
		#region IPropertyPadProvider implementation

		public object GetActiveComponent ()
		{
			return activeWidget;
		}
		public object GetProvider ()
		{
			return activeWidget;
		}
		public void OnEndEditing (object obj)
		{

		}
		public void OnChanged (object obj)
		{

		}
		#endregion
	
		string _path;
		GraphicObject goWidget;

		public GOLibGtkHost ()
		{
			this.ExposeEvent += onExpose;
			this.ButtonPressEvent += GOLibGtkHost_ButtonPressEvent;
			this.ButtonReleaseEvent += GOLibGtkHost_ButtonReleaseEvent;
			this.MotionNotifyEvent += GOLibGtkHost_MotionNotifyEvent;

			this.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask |
				Gdk.EventMask.PointerMotionMask | Gdk.EventMask.PointerMotionHintMask;
		}
		static double[] dashed = {2.0};

		void onExpose(object o, Gtk.ExposeEventArgs args)
		{
			Gtk.DrawingArea area = (Gtk.DrawingArea) o;
			Cairo.Context cr =  Gdk.CairoHelper.Create(area.GdkWindow);
			_redrawClip.AddRectangle (this.ClientRectangle);
			update (cr);

			if (_hoverWidget != null) {
				cr.Rectangle (_hoverWidget.ScreenCoordinates(_hoverWidget.getSlot ()));
				cr.LineWidth = 1;
				cr.SetDash (dashed, 0);
				cr.Color = go.Color.Yellow;
				cr.Stroke ();
			}
			((IDisposable) cr.Target).Dispose();                                      
			((IDisposable) cr).Dispose();
		}
		// TODO: should find a more safer way to link gtk button to otk.
		void GOLibGtkHost_ButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			MouseButtonEventArgs e = new MouseButtonEventArgs 
				((int)args.Event.X, (int)args.Event.Y, 
					gtkButtonIdToOpenTkButton(args.Event.Button), true);
			activeWidget = hoverWidget;
			DesignerSupport.DesignerSupport.Service.SetPadContent (this);
		}
		void GOLibGtkHost_ButtonReleaseEvent (object o, Gtk.ButtonReleaseEventArgs args)
		{
			MouseButtonEventArgs e = new MouseButtonEventArgs 
				((int)args.Event.X, (int)args.Event.Y, 
					gtkButtonIdToOpenTkButton(args.Event.Button), false);			
		}
		int lastX,LastY;
		void GOLibGtkHost_MotionNotifyEvent (object o, Gtk.MotionNotifyEventArgs args)
		{
			
			Mouse_Move(this, new MouseMoveEventArgs (
				(int)args.Event.X,(int)args.Event.Y,
				(int)args.Event.X-lastX, (int)args.Event.Y-LastY
			));
			lastX = (int)args.Event.X;
			LastY = (int)args.Event.Y;

			QueueDraw ();
		}

		MouseButton gtkButtonIdToOpenTkButton(uint gtkMouseButton)
		{
			switch (gtkMouseButton) {
			case 1:
				return MouseButton.Left;
			case 2:
				return MouseButton.Middle;
			case 3:
				return MouseButton.Right;
			case 4:
				return MouseButton.Button4;
			case 5:
				return MouseButton.Button5;
			}
			return MouseButton.LastButton;
		}



		public void Load(string path)
		{
			goWidget = GraphicObject.Load (path);
			this.AddWidget (goWidget);
		}



		public List<GraphicObject> GraphicObjects = new List<GraphicObject>();
		public Color Background = Color.Transparent;

		Rectangles _redrawClip = new Rectangles();//should find another way to access it from child
		List<GraphicObject> _gobjsToRedraw = new List<GraphicObject>();

		public Rectangles redrawClip {
			get {
				return _redrawClip;
			}
			set {
				_redrawClip = value;
			}
		}
		public List<GraphicObject> gobjsToRedraw {
			get {
				return _gobjsToRedraw;
			}
			set {
				_gobjsToRedraw = value;
			}
		}			

		#region focus

		GraphicObject _activeWidget;	//button is pressed on widget 
		GraphicObject _hoverWidget;		//mouse is over
		GraphicObject _focusedWidget;	//has keyboard (or other perif) focus 

		public GraphicObject activeWidget
		{
			get { return _activeWidget; }
			set 
			{
				if (_activeWidget == value)
					return;

				_activeWidget = value;
			}
		}
		public GraphicObject hoverWidget
		{
			get { return _hoverWidget; }
			set { _hoverWidget = value; }
		}
		public GraphicObject FocusedWidget {
			get { return _focusedWidget; }
			set {
				if (_focusedWidget == value)
					return;
				if (_focusedWidget != null)
					_focusedWidget.onUnfocused (this, null);
				_focusedWidget = value;
				_focusedWidget.onFocused (this, null);
			}
		}

		#endregion

		Rectangle dirtyZone = Rectangle.Empty;

		#region Chrono's
		public Stopwatch updateTime = new Stopwatch ();
		public Stopwatch layoutTime = new Stopwatch ();
		public Stopwatch guTime = new Stopwatch ();
		public Stopwatch drawingTime = new Stopwatch ();
		#endregion

		void update (Cairo.Context ctx)
		{
			updateTime.Restart ();
			layoutTime.Reset ();
			guTime.Reset ();
			drawingTime.Reset ();

			GraphicObject[] invGOList = new GraphicObject[GraphicObjects.Count];
			GraphicObjects.CopyTo (invGOList,0);
			invGOList = invGOList.Reverse ().ToArray ();

			foreach (GraphicObject p in invGOList) {
				if (p.Visible) {
					layoutTime.Start ();
					while(!p.LayoutIsValid)
						p.UpdateLayout ();
					layoutTime.Stop ();
				}
			}

			GraphicObject[] gotr = new GraphicObject[_gobjsToRedraw.Count];
			_gobjsToRedraw.CopyTo (gotr);
			_gobjsToRedraw.Clear ();
			foreach (GraphicObject p in gotr) {
				p.registerClipRect ();
			}


			lock (_redrawClip) {
				if (_redrawClip.count > 0) {
					//					#if DEBUG_CLIP_RECTANGLE
					//					redrawClip.stroke (ctx, new Color(1.0,0,0,0.3));
					//					#endif
					_redrawClip.clearAndClip (ctx);//rajouté après, tester si utile
					//Link.draw (ctx);
					foreach (GraphicObject p in invGOList) {
						if (p.Visible) {
							drawingTime.Start ();

							ctx.Save ();
							if (_redrawClip.count > 0) {
								Rectangles clip = _redrawClip.intersectingRects (p.ContextCoordinates(p.Slot.Size));

								if (clip.count > 0)
									p.Paint (ref ctx, clip);
							}
							ctx.Restore ();

							drawingTime.Stop ();
						}
					}
					ctx.ResetClip ();
					dirtyZone = _redrawClip.Bounds;
					//					#if DEBUG_CLIP_RECTANGLE
					//					redrawClip.stroke (ctx, Color.Red.AdjustAlpha(0.1));
					//					#endif
					_redrawClip.Reset ();
				}
			}
			//			if (ToolTip.isVisible) {
			//				ToolTip.panel.processkLayouting();
			//				if (ToolTip.panel.layoutIsValid)
			//					ToolTip.panel.Paint(ref ctx);
			//			}
			//			Debug.WriteLine("INTERFACE: layouting: {0} ticks \t graphical update {1} ticks \t drawing {2} ticks",
			//			    layoutTime.ElapsedTicks,
			//			    guTime.ElapsedTicks,
			//			    drawingTime.ElapsedTicks);
			//			Debug.WriteLine("INTERFACE: layouting: {0} ms \t graphical update {1} ms \t drawing {2} ms",
			//			    layoutTime.ElapsedMilliseconds,
			//			    guTime.ElapsedMilliseconds,
			//			    drawingTime.ElapsedMilliseconds);
			updateTime.Stop ();
			//			Debug.WriteLine("UPDATE: {0} ticks \t, {1} ms",
			//				updateTime.ElapsedTicks,
			//				updateTime.ElapsedMilliseconds);

		}						

		public void AddWidget(GraphicObject g)
		{
			g.Parent = this;
			GraphicObjects.Add (g);
		}
		public void DeleteWidget(GraphicObject g)
		{
			g.Visible = false;//trick to ensure clip is added to refresh zone
			GraphicObjects.Remove (g);
		}

		public void LoadInterface<T>(string path, out T result)
		{
			GraphicObject.Load<T> (path, out result, this);
			AddWidget (result as GraphicObject);
		}
		public T LoadInterface<T> (string Path)
		{
			T result;
			GraphicObject.Load<T> (Path, out result, this);
			AddWidget (result as GraphicObject);
			return result;
		}



		public void PutOnTop(GraphicObject g)
		{
			if (GraphicObjects.IndexOf(g) > 0)
			{
				GraphicObjects.Remove(g);
				GraphicObjects.Insert(0, g);
			}
		}

		#region Mouse Handling
		void Mouse_Move(object sender, MouseMoveEventArgs e)
		{
			if (_activeWidget != null) {
				//send move evt even if mouse move outside bounds
				_activeWidget.onMouseMove (this, e);
				return;
			}

			if (_hoverWidget != null) {
				if (_hoverWidget.MouseIsIn (e.Position)) {
					_hoverWidget.onMouseMove (this, e);
					return;
				} else {
					_hoverWidget.onMouseLeave (this, e);
					//seek upward from last focused graph obj's
					while (_hoverWidget.Parent as GraphicObject!=null) {
						_hoverWidget = _hoverWidget.Parent as GraphicObject;
						if (_hoverWidget.MouseIsIn (e.Position)) {
							_hoverWidget.onMouseMove (this, e);
							return;
						} else
							_hoverWidget.onMouseLeave (this, e);
					}
				}
			}

			//top level graphic obj's parsing
			for (int i = 0; i < GraphicObjects.Count; i++) {
				GraphicObject g = GraphicObjects[i];
				if (g.MouseIsIn (e.Position)) {
					g.onMouseMove (this, e);
					PutOnTop (g);
					return;
				}
			}
			_hoverWidget = null;
		}
		void Mouse_ButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_activeWidget == null)
				return;

			_activeWidget.onMouseButtonUp (this, e);
			_activeWidget = null;
		}
		void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_hoverWidget == null) {
				return;
			}

			GraphicObject g = _hoverWidget;
			while (!g.Focusable) {
				g = g.Parent as GraphicObject;
				if (g == null) {					
					return;
				}
			}

			_activeWidget = g;
			_activeWidget.onMouseButtonDown (this, e);
		}
		void Mouse_WheelChanged(object sender, MouseWheelEventArgs e)
		{
			if (_hoverWidget == null) {
				return;
			}
			_hoverWidget.onMouseWheel (this, e);
		}        
		#endregion

		#region keyboard Handling
		void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
		{
			if (_focusedWidget == null)
				return;
			_focusedWidget.onKeyDown (sender, e);
		}
		#endregion
		

		#region ILayoutable implementation

		public Rectangle ContextCoordinates (Rectangle r)
		{
			return r;
		}
		public Rectangle ScreenCoordinates (Rectangle r)
		{
			return r;
		}

		public ILayoutable Parent {
			get {
				return null;
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public bool SizeIsValid {
			get { return true; }
			set { throw new NotImplementedException ();	}
		}
		public bool PositionIsValid {
			get {
				return true;
			}
			set {
				throw new NotImplementedException ();
			}
		}
		public bool LayoutIsValid {
			get {
				return true;//tester tout les enfants a mon avis
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public Rectangle ClientRectangle {
			get {			
				return new go.Size(Allocation.Width,Allocation.Height);
			}
		}

		public IGOLibHost TopContainer {
			get { return this as IGOLibHost; }
		}

		public Rectangle getSlot ()
		{
			return ClientRectangle;
		}
		public Rectangle getBounds ()//redundant but fill ILayoutable implementation
		{
			return ClientRectangle;
		}

		public bool WIsValid {
			get {
				return true;
			}
			set {
				throw new NotImplementedException ();
			}
		}
		public bool HIsValid {
			get {
				return true;
			}
			set {
				throw new NotImplementedException ();
			}
		}
		public bool XIsValid {
			get {
				return true;
			}
			set {
				throw new NotImplementedException ();
			}
		}
		public bool YIsValid {
			get {
				return true;
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public virtual void InvalidateLayout ()
		{
			//			foreach (GraphicObject g in GraphicObjects) {
			//				g.InvalidateLayout ();
			//			}
		}

		#endregion
	}
}

