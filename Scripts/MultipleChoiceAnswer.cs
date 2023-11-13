using Godot;
using System;

namespace CompAdv
{
	public class MultipleChoiceAnswer : PanelContainer
	{
		[Export]
		public int id = -1;
		
		[Signal]
		public delegate void ChoiceSelected( int id );

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			
		}
		
		private void _onButtonPressed()
		{
			EmitSignal( "ChoiceSelected", id );
		}
	}
}
