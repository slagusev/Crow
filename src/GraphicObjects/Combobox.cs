﻿using System;


using System.Collections.Generic;
using System.Linq;
using System.Text;
//using OpenTK.Graphics.OpenGL;

using Cairo;

using winColors = System.Drawing.Color;
using System.Diagnostics;
using System.Xml.Serialization;
using OpenTK.Input;
using System.ComponentModel;
using System.Xml;
using System.IO;
using System.Collections;

namespace go
{
	[DefaultTemplate("#go.Templates.Combobox.goml")]
	[DefaultOverlayTemplate("#go.Templates.ComboboxOverlay.goml")]
	public class Combobox : TemplatedContainer
    {		
		#region CTOR
		public Combobox() : base(){	}	
		#endregion

		bool _isPopped;
		string text;
		GraphicObject _overlay;
		Group _list;
		IList data;
		int _selectedIndex;
		object _selectedItem;
		string _itemTemplate;
		string _overlayTemplate;

		public event EventHandler<SelectionChangeEventArgs> SelectedItemChanged;

		#region implemented abstract members of TemplatedControl
		protected override void loadTemplate (GraphicObject template)
		{
			base.loadTemplate (template);
			loadOverlayTemplate (null);
		}
		public override GraphicObject Content {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		#endregion

		protected virtual void loadOverlayTemplate(GraphicObject overlayTemplate)
		{
			if (overlayTemplate == null) {
				DefaultOverlayTemplate dt = (DefaultOverlayTemplate)this.GetType ().GetCustomAttributes (typeof(DefaultOverlayTemplate), true).FirstOrDefault ();
				Overlay = Interface.Load (dt.Path, this, !Interface.DontResoveGOML);
			} else
				Overlay = overlayTemplate;
			_list = Overlay.FindByName ("List") as Group;
		}

		[XmlAttributeAttribute][DefaultValue("#go.Templates.ItemTemplate.goml")]
		public string ItemTemplate {
			get { return _itemTemplate; }
			set { 
				//TODO:reload list with new template?
				_itemTemplate = value; 
			}
		}
		[XmlAttributeAttribute][DefaultValue("#go.Templates.ComboboxOverlay.goml")]
		public string OverlayTemplate {
			get { return _overlayTemplate; }
			set { 
				//TODO:reload list with new template?
				_overlayTemplate = value; 

				Overlay = Interface.Load (_overlayTemplate, this, !Interface.DontResoveGOML);
				_list = Overlay.FindByName ("List") as Group;
			}
		}
		[XmlAttributeAttribute][DefaultValue(-1)]
		public int SelectedIndex{
			get { return _selectedIndex; }
			set { _selectedIndex = value; }
		}
		public object SelectedItem{
			set { 				
				_selectedItem = value;

//				NotifyValueChanged ("SelectedItem", _selectedItem);
//
				if (SelectedItem == null)
					Text = "";
				else
					Text = _selectedItem.ToString ();
			}
				
			get { return _selectedItem; }
		}
		[XmlAttributeAttribute][DefaultValue(null)]
		public IList Data {
			get {
				return data;
			}
			set {				
				data = value;
				if (_list == null)
					return;

				foreach (GraphicObject c in _list.Children) {
					c.ClearBinding ();
				}
				_list.Children.Clear ();
				_list.registerForGraphicUpdate ();
				if (data == null)
					return;

				#if DEBUG
				Stopwatch loadingTime = new Stopwatch ();
				loadingTime.Start ();
				#endif

				MemoryStream ms = new MemoryStream ();
				using (Stream stream = Interface.GetStreamFromPath (ItemTemplate))					
					stream.CopyTo (ms);

				Type t = Interface.GetTopContainerOfGOMLStream (ms);

				foreach (var item in data) {
					ms.Seek(0,SeekOrigin.Begin);
					GraphicObject g = Interface.Load (ms, t, item);
					g.MouseClick += itemClick;
					_list.addChild(g);
				}

				ms.Dispose ();

				#if DEBUG
				loadingTime.Stop ();
				Debug.WriteLine("Listbox Loading: {0} ticks \t, {1} ms",
					loadingTime.ElapsedTicks,
					loadingTime.ElapsedMilliseconds);
				#endif
			}
		}

		void itemClick(object sender, OpenTK.Input.MouseButtonEventArgs e){
			object datasource = (sender as GraphicObject).DataSource;
			SelectedItem = datasource;
			SelectedItemChanged.Raise (sender, new SelectionChangeEventArgs(datasource));
			IsPopped = false;

			//Debug.WriteLine ((sender as GraphicObject).DataSource);
		}

		public event EventHandler Pop;
		public event EventHandler Unpop;

		public GraphicObject Overlay {
			get { return _overlay; }
			set { 
				if (_overlay != null) {
					_overlay.LayoutChanged -= _overlay_LayoutChanged;
					_overlay.MouseLeave -= _content_MouseLeave;
				}
				
				_overlay = value; 

				if (_overlay == null)
					return;

				_overlay.Focusable = true;
				_overlay.LayoutChanged += _overlay_LayoutChanged;
				_overlay.MouseLeave += _content_MouseLeave;
			}
		}

		void _content_MouseLeave (object sender, MouseMoveEventArgs e)
		{
			IsPopped = false;
		}

		void _overlay_LayoutChanged (object sender, LayoutChangeEventArgs e)
		{
			ILayoutable tc = Overlay.Parent as ILayoutable;
			if (tc == null)
				return;
			Rectangle r = this.ScreenCoordinates (this.Slot);
			if (e.LayoutType == LayoutingType.Width) {
				if (Overlay.Slot.Width < tc.ClientRectangle.Width) {
					if (r.Left + Overlay.Slot.Width > tc.ClientRectangle.Right)
						Overlay.Left = tc.ClientRectangle.Right - Overlay.Slot.Width;
					else
						Overlay.Left = r.Left;
				}else
					Overlay.Left = 0;
			}else if (e.LayoutType == LayoutingType.Height) {
				if (Overlay.Slot.Height < tc.ClientRectangle.Height) {
					if (r.Bottom + Overlay.Slot.Height > tc.ClientRectangle.Bottom)
						Overlay.Top = r.Top - Overlay.Slot.Height;
					else
						Overlay.Top = r.Bottom;
				}else
					Overlay.Top = 0;
			}
		}
		public override void ClearBinding ()
		{
			//ensure popped window is cleared
			if (Overlay != null) {
				if (Overlay.Parent != null) {
					IGOLibHost tc = Overlay.Parent as IGOLibHost;
					if (tc != null)
						tc.DeleteWidget (Overlay);
				}
			}
			base.ClearBinding ();

		}

		[XmlAttributeAttribute()][DefaultValue(true)]//overiden to get default to true
		public override bool Focusable
		{
			get { return base.Focusable; }
			set { base.Focusable = value; }
		}

		[XmlAttributeAttribute()][DefaultValue("Combobox")]
		public string Text {
			get { return text; } 
			set {
				if (text == value)
					return;
				text = value; 
				NotifyValueChanged ("Text", text);
			}
		}        

		[XmlAttributeAttribute()][DefaultValue(false)]
        public bool IsPopped
        {
			get { return _isPopped; }
            set
            {
				_isPopped = value;

				if (_isPopped) {
					onPop (this, null);
					NotifyValueChanged ("SvgSub", "expanded");
					return;
				}

				onUnpop (this, null);
				NotifyValueChanged ("SvgSub", "collapsed");
            }
        }
			
		public virtual void onPop(object sender, EventArgs e)
		{
			IGOLibHost tc = TopContainer;
			if (tc == null)
				return;
			if (Overlay != null) {
				Overlay.Visible = true;
				if (Overlay.Parent == null)
					tc.AddWidget (Overlay);
				(tc as OpenTKGameWindow).PutOnTop (Overlay);
			}
			Pop.Raise (this, e);
		}
		public virtual void onUnpop(object sender, EventArgs e)
		{
			IGOLibHost tc = TopContainer;
			if (tc == null)
				return;
			Overlay.Visible = false;
			Unpop.Raise (this, e);
		}
			
		public override void onMouseClick (object sender, MouseButtonEventArgs e)
		{
			IsPopped = !IsPopped;
			base.onMouseClick (sender, e);
		}

	}
}