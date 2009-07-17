// 
// TooltipWindow.cs
// 
// Author:
//   Michael Hutchinson <mhutchinson@novell.com>
// 
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

using Gtk;
using Gdk;

namespace MonoDevelop.Components
{
	public abstract class TooltipWindow : Gtk.Window
	{
		bool nudgeVertical = false;
		bool nudgeHorizontal = false;
		WindowTransparencyDecorator decorator;
		
		public TooltipWindow () : base(Gtk.WindowType.Popup)
		{
			this.SkipPagerHint = true;
			this.SkipTaskbarHint = true;
			this.Decorated = false;
			this.BorderWidth = 2;
			this.TypeHint = TooltipTypeHint;
			this.AllowShrink = false;
			this.AllowGrow = false;
			this.Title = "tooltip"; // fixes the annoying '** Message: ATK_ROLE_TOOLTIP object found, but doesn't look like a tooltip.** Message: ATK_ROLE_TOOLTIP object found, but doesn't look like a tooltip.'
			
			//fake widget name for stupid theme engines
			if (Gtk.Global.CheckVersion (2, 12, 0) == null)
				this.Name = "gtk-tooltip";
			else
				this.Name = "gtk-tooltips";
		}
		
		public bool NudgeVertical {
			get { return nudgeVertical; }
			set { nudgeVertical = value; }
		}
		
		public bool NudgeHorizontal {
			get { return nudgeHorizontal; }
			set { nudgeHorizontal = value; }
		}
		
		public bool EnableTransparencyControl {
			get { return decorator != null; }
			set {
				if (value && decorator == null)
					decorator = WindowTransparencyDecorator.Attach (this);
				else if (!value && decorator != null)
					decorator.Detach ();
			}
		}
		
		protected override bool OnExposeEvent (Gdk.EventExpose args)
		{
			base.OnExposeEvent (args);
			
			int winWidth, winHeight;
			this.GetSize (out winWidth, out winHeight);
			this.GdkWindow.DrawRectangle (this.Style.ForegroundGC (StateType.Insensitive), false, 0, 0, winWidth-1, winHeight-1);
			return false;
		}
		
		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			if (nudgeHorizontal || nudgeVertical) {
				int x, y;
				this.GetPosition (out x, out y);
				int oldY = y, oldX = x;
				const int edgeGap = 2;
				
//				int w = allocation.Width;
//				
//				if (fitWidthToScreen && (x + w >= screenW - edgeGap)) {
//					int fittedWidth = screenW - x - edgeGap;
//					if (fittedWidth < minFittedWidth) {
//						x -= (minFittedWidth - fittedWidth);
//						fittedWidth = minFittedWidth;
//					}
//					LimitWidth (fittedWidth);
//				}
				
				if (nudgeHorizontal) {
					int screenW = Screen.Width;
					if (allocation.Width <= screenW && x + allocation.Width >= screenW - edgeGap)
						x = (screenW - allocation.Height - edgeGap);
					if (x <= 0)
						x = 0;
				}
				
				if (nudgeVertical) {
					int screenH = Screen.Height;
					if (allocation.Height <= screenH && y + allocation.Height >= screenH - edgeGap)
						y = (screenH - allocation.Height - edgeGap);
					if (y <= 0)
						y = 0;
				}
				
				if (y != oldY || x != oldX)
					Move (x, y);
			}
			
			base.OnSizeAllocated (allocation);
		}
		
//		void LimitWidth (int width)
//		{
//			if (Child is MonoDevelop.Components.FixedWidthWrapLabel) 
//				((MonoDevelop.Components.FixedWidthWrapLabel)Child).MaxWidth = width - 2 * (int)this.BorderWidth;
//			
//			int childWidth = Child.SizeRequest ().Width;
//			if (childWidth < width)
//				WidthRequest = childWidth;
//			else
//				WidthRequest = width;
//		}
		
		//this is GTK+ >= 2.10 only, so reflect it
		static Gdk.WindowTypeHint TooltipTypeHint {
			get {
				if (tooltipTypeHint > -1)
					return (Gdk.WindowTypeHint) tooltipTypeHint;
				
				tooltipTypeHint = (int) Gdk.WindowTypeHint.Dialog;
				
				System.Reflection.FieldInfo fi = typeof (Gdk.WindowTypeHint).GetField ("Tooltip");
				if (fi != null)
					tooltipTypeHint = (int) fi.GetValue (typeof (Gdk.WindowTypeHint));
				
				return (Gdk.WindowTypeHint) tooltipTypeHint;
			}
		}
		
		static int tooltipTypeHint = -1;
	}
}
