﻿//
//  TemplatedControl.cs
//
//  Author:
//       Jean-Philippe Bruyère <jp.bruyere@hotmail.com>
//
//  Copyright (c) 2015 jp
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Xml.Serialization;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Linq;
using System.Diagnostics;

namespace Crow
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TemplateAttribute : Attribute
	{
		public string Path = "";
		public TemplateAttribute(string path)
		{
			Path = path;
		}
	}
	[AttributeUsage(AttributeTargets.Class)]
	public class DefaultStyle : TemplateAttribute
	{
		public DefaultStyle(string path) : base(path){}
	}
	[AttributeUsage(AttributeTargets.Class)]
	public class DefaultTemplate : TemplateAttribute
	{
		public DefaultTemplate(string path) : base(path){}
	}
	[AttributeUsage(AttributeTargets.Class)]
	public class DefaultItemTemplate : TemplateAttribute
	{
		public DefaultItemTemplate(string path) : base(path){}
	}

	public abstract class TemplatedControl : PrivateContainer, IXmlSerializable
	{
		#region CTOR
		public TemplatedControl () : base()
		{
			if (Interface.XmlLoaderCount > 0)
				return;
			loadTemplate ();
		}
		#endregion

		string _template;

		[XmlAttributeAttribute][DefaultValue(null)]
		public string Template {
			get { return _template; }
			set {
				if (Template == value)
					return;
				_template = value;

				if (string.IsNullOrEmpty(_template))
					loadTemplate ();
				else
					loadTemplate (Interface.Load (_template, this));
			}
		}

		#region GraphicObject overrides
		public override GraphicObject FindByName (string nameToFind)
		{
			//prevent name searching in template
			return nameToFind == this.Name ? this : null;
		}
		#endregion

		protected virtual void loadTemplate(GraphicObject template = null)
		{
			if (template == null) {
				DefaultTemplate dt = (DefaultTemplate)this.GetType ().GetCustomAttributes (typeof(DefaultTemplate), true).FirstOrDefault();
				this.SetChild (Interface.Load (dt.Path, this));
			}else
				this.SetChild (template);

			this.ResolveBindings ();
		}

		#region IXmlSerializable
		public override System.Xml.Schema.XmlSchema GetSchema(){ return null; }
		public override void ReadXml(System.Xml.XmlReader reader)
		{
			//Template could be either an attribute containing path or expressed inlined
			//as a Template Element
			using (System.Xml.XmlReader subTree = reader.ReadSubtree())
			{
				subTree.Read ();

				string template = reader.GetAttribute ("Template");
				string tmp = subTree.ReadOuterXml ();

				//Load template from path set as attribute in templated control
				if (string.IsNullOrEmpty (template)) {
					//seek for template tag first
					using (XmlReader xr = new XmlTextReader (tmp, XmlNodeType.Element, null)) {
						//load template first if inlined

						xr.Read (); //skip current node

						while (!xr.EOF) {
							xr.Read (); //read first child
							if (!xr.IsStartElement ())
								continue;
							if (xr.Name == "Template") {
								xr.Read ();

								Type t = Type.GetType ("Crow." + xr.Name);
								GraphicObject go = (GraphicObject)Activator.CreateInstance (t);
								(go as IXmlSerializable).ReadXml (xr);

								loadTemplate (go);

								xr.Read ();//go close tag
								xr.Read ();//Template close tag
								break;
							} else
								xr.ReadInnerXml ();
						}
					}
				} else
					loadTemplate (Interface.Load (template, this));

				//if no template found, load default one
				if (this.child == null)
					loadTemplate ();

				//normal xml read
				using (XmlReader xr = new XmlTextReader (tmp, XmlNodeType.Element, null)) {
					xr.Read ();
					base.ReadXml(xr);
				}
			}
		}
		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			//TODO:
			throw new NotImplementedException();
		}
		#endregion
	}
}

