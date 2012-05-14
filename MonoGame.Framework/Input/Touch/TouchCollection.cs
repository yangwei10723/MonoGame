#region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright � 2009-2010 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
#endregion License

#region Using clause
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
#endregion Using clause

namespace Microsoft.Xna.Framework.Input.Touch
{	
	public class TouchCollection : List<TouchLocation>
	{
		/// <summary>
		/// Attributes 
		/// </summary>
		private bool isConnected;
		
		#region Properties
		public bool IsConnected
		{
			get
			{
				return this.isConnected;
			}
		}
		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}
		#endregion
		
		public TouchCollection()
		{
		}
		
		internal TouchCollection(IEnumerable<TouchLocation> locations)	: base (locations)
		{
			
		}
		
		internal void Update()
		{
			TouchLocation t;
			// First update active touches 
			for (int i = this.Count - 1; i >= 0; --i)
			{
				t = this[i];
				t.PressedStateProcessed = true;
				
				switch (t.State)
				{
					case TouchLocationState.Pressed:
						t.PrevPosition = t.Position;
					break;
					case TouchLocationState.Moved:
						t.PrevState = TouchLocationState.Moved;
					break;
				}
				
				this[i] = t;
			}
			
#if IPHONE
			iOSGameView.UpdateGestures();
#endif
			
			// Remove dead touches
			for (int i = this.Count - 1; i >= 0 && i < this.Count;)
			{
				t = this[i];
				if (t.State == TouchLocationState.Invalid || 
				    t.State == TouchLocationState.Released)
				{
					RemoveAt(i);
					
					if(i != this.Count)
						continue;
				}
				
				i--;
			}
		}

		public bool FindById(int id, out TouchLocation touchLocation)
		{
			int index = this.FindIndex((t) => { return t.Id == id; });
			if (index >= 0)
			{
				touchLocation = this[index];
				return true;
			}
			touchLocation = default(TouchLocation);
			return false;
		}

		internal int FindIndexById(int id, out TouchLocation touchLocation)
		{
			for (int i = 0; i < this.Count; i++)
			{
				TouchLocation location = this[i];
				if (location.Id == id)
				{
					touchLocation = location;
					return i;
				}
			}
			touchLocation = default(TouchLocation);
			return -1;
		}

		internal void Add(int id, Vector2 position) {
			for (int i = 0; i < Count; i++) {
				if (this[i].Id == id) {
                    Debug.WriteLine("Error: Attempted to re-add the same touch as a press.");
					Clear ();
				}
			}
			Add(new TouchLocation(id, TouchLocationState.Pressed, position));
		}

		internal void Update(int id, TouchLocationState state, Vector2 position)
		{
			if (state == TouchLocationState.Pressed)
				throw new ArgumentException("Argument 'state' cannot be TouchLocationState.Pressed.");

			for (int i = 0; i < Count; i++)
			{
				if (this[i].Id == id)
				{				
					var touchLocation = this[i];
					touchLocation.Position = position;
					
					// Some OS's can give us moved/released updates before we have a chance to process pressed
					// Give the app a chance to respond to pressed
					if ( !this[i].PressedStateProcessed )
					{
						if (state == TouchLocationState.Moved)
							return;
						
						if (state == TouchLocationState.Invalid || state == TouchLocationState.Released)
						{
							RemoveAt(i);
							return;
						}
					}
					
					touchLocation.State = state;
					this[i] = touchLocation;
					return;
				}
			}

			Debug.WriteLine("Error: Attempted to mark a non-existent touch {0} as {1}.", id, state);
			Clear ();
		}
	}
}