using System;

namespace cup {
	public class KeyboardHookTriggeredEventArgs : EventArgs {
		public int ID {
			get;
			private set;
		}

		public KeyboardHookTriggeredEventArgs(int id) {
			ID = id;
		}
	}
}
