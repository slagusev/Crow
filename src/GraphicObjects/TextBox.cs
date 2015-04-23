﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Input;
using Cairo;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace go
{
    public class TextBoxWidget : Label
    {
		#region CTOR
		public TextBoxWidget(string _initialValue, GOEvent _onTextChanged = null)
			: base(_initialValue)
		{

		}

		public TextBoxWidget()
		{ }
		#endregion

		static bool _capitalOn = false;	//????????????????
		[XmlIgnore]public static bool capitalOn
		{
			get
			{
				return _capitalOn;
					//Keyboard[Key.ShiftLeft] || Keyboard[Key.ShiftRight] ?
					//!_capitalOn : _capitalOn;
			}
			set { _capitalOn = value; }
		}


		#region GraphicObject overrides
		[XmlIgnore]public override bool HasFocus   //trigger update when lost focus to errase text beam
        {
            get
            {
                return base.HasFocus;
            }
            set
            {
                base.HasFocus = value;
                registerForGraphicUpdate();
            }
        }
		[XmlAttributeAttribute()][DefaultValue(true)]
		public override bool Focusable
		{
			get { return base.Focusable; }
			set { base.Focusable = value; }
		}
		[XmlAttributeAttribute()][DefaultValue("White")]
		public override Color Background {
			get { return base.Background; }
			set { base.Background = value; }
		}
		[XmlAttributeAttribute()][DefaultValue("Black")]
		public override Color Foreground {
			get { return base.Foreground; }
			set { base.Foreground = value; }
		}

		protected override void onDraw (Context gr)
		{
			base.onDraw (gr);
			//			gr.FontOptions.Antialias = Antialias.Subpixel;
			//			gr.FontOptions.HintMetrics = HintMetrics.On;
//			gr.SelectFontFace (Font.Name, Font.Slant, Font.Wheight);
//			gr.SetFontSize (Font.Size);

			FontExtents fe = gr.FontExtents;



//			gr.MoveTo(rText.X, rText.Y + fe.Ascent);
//			#if _WIN32 || _WIN64
//			gr.ShowText(txt);
//			#elif __linux__
//			gr.ShowText(Text);
//			#endif
//			gr.Fill();

		}
		#endregion

		public event EventHandler<TextChangeEventArgs> TextChanged;

		public virtual void onTextChanged(Object sender, TextChangeEventArgs e)
		{
			TextChanged.Raise (this, e);
		}
			
        #region Keyboard handling
		public override void onKeyDown (object sender, KeyboardKeyEventArgs e)
		{
			base.onKeyDown (sender, e);

			Key key = e.Key;

			switch (key)
			{
			case Key.Back:
				if (!selectionIsEmpty)
				{
//					Text = Text.Remove(selectionStart, selectionEnd - selectionStart);
//					selReleasePos = -1;
//					currentCol = selBeginPos;
				}
				else 
					this.DeleteChar();
				break;
			case Key.Clear:
				break;
			case Key.Delete:
				if (selectionIsEmpty)
					currentCol++;
				this.DeleteChar ();
				break;
			case Key.Enter:
			case Key.KeypadEnter:
				onTextChanged(this,new TextChangeEventArgs(Text));
				break;
			case Key.Escape:
				Text = "";
				currentCol = 0;
				selRelease = -1;
				break;
			case Key.Home:
				//TODO
				if (e.Control)
					currentLine = 0;
				currentCol = 0;
				break;
			case Key.End:
				if (e.Control)
					currentLine = int.MaxValue;
				currentCol = int.MaxValue;
				break;
			case Key.Insert:
				break;
			case Key.Left:				
				currentCol--;
				break;
			case Key.Right:				
				currentCol++;
				break;
			case Key.Up:
				currentLine--;
				break;
			case Key.Down:
				currentLine++;
				break;
			case Key.Menu:
				break;
			case Key.NumLock:
				break;
			case Key.PageDown:
				break;
			case Key.PageUp:
				break;
			case Key.RWin:
				break;
			case Key.Tab:
				this.Insert("\t");
				break;
			case Key.KeypadDecimal:
				this.Insert (".");					
				break;
			case Key.Space:
				this.Insert(" ");
				break;
			case Key.KeypadDivide:
			case Key.Slash:
				this.Insert("/");
				break;
			case Key.KeypadMultiply:
				this.Insert("*");
				break;
			case Key.KeypadMinus:
			case Key.Minus:
				this.Insert("-");
				break;
			case Key.KeypadPlus:
			case Key.Plus:
				this.Insert("+");
				break;
			case Key.ShiftLeft:
			case Key.ShiftRight:
			case Key.AltLeft:
			case Key.AltRight:
				break;
			case Key.Semicolon:
				this.Insert(";");
				break;
			default:
				if (!selectionIsEmpty)
				{
//					Text = Text.Remove(selectionStart, selectionEnd - selectionStart);
//					currentCol = selBeginPos;
				}

				string k = "?";
				if ((char)key >= 67 && (char)key <= 76)
					k = ((int)key - 67).ToString();
				else if ((char)key >= 109 && (char)key <= 118)
					k = ((int)key - 109).ToString();
				else if (e.Shift)
					k = key.ToString();
				else
					k = key.ToString().ToLower();

				this.Insert (k);

				selRelease = -1;
				selBegin.X = currentCol;

				break;
			}
			registerForGraphicUpdate();
		}
        #endregion
	} 
}
