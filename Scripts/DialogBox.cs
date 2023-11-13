using Godot;
using System;

namespace CompAdv
{

	public class DialogBox : NinePatchRect
	{
		[Signal]
		delegate void DialogStarted();
		[Signal]
		delegate void DialogStopped();
		
		// The actual textbox that houses the dialog.
		private RichTextLabel textbox;
		// The Space and Enter icons
		private Control userControlIcons;
		
		// Integer of shown characters on the dialog box
		private int currentVisibleCharacters;
		// Tracks newlines to keep waitTime delays from being offsetted.
		private int newlineCount;
		// Timer for delays in speech due to special characters like ',' and ';'.
		private double waitTime;
		public bool isDialogProgressing;
		// The dialog will not progress on space/enter until this is false.
		// Intended to be used when the user is presented a problem to be solved.
		private bool holdingForProblemCompletion = false;
		
		private Godot.Collections.Dictionary PAUSE_CHARS;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			textbox = GetNode("MarginContainer/VBoxContainer/RichTextLabel") as RichTextLabel;
			userControlIcons = GetNode("MarginContainer/VBoxContainer/PanelContainer/UserControlIcons") as Control;
			
			currentVisibleCharacters = 0;
			newlineCount = 0;
			waitTime = 0.0;
			isDialogProgressing = false;
			
			PAUSE_CHARS = new Godot.Collections.Dictionary
				{
					{',', 0.4}
//					,{'.', 20}
					,{';', 0.4}
					,{':', 0.4}
				};
			
			textbox.VisibleCharacters = currentVisibleCharacters;
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(float delta)
		{
			if ( textbox.GetTotalCharacterCount() <= currentVisibleCharacters )
			{
//				GD.Print( "Total " + textbox.GetTotalCharacterCount() );
				if ( isDialogProgressing )
				{
					isDialogProgressing = false;
					EmitSignal("DialogStopped");
				}
				
				return;
			}
			
			if ( waitTime <= 0 )
			{
				currentVisibleCharacters += 1;
				textbox.VisibleCharacters = currentVisibleCharacters;
				
				char charBeingRevealed = textbox.Text[currentVisibleCharacters+newlineCount-1];
				
				if ( PAUSE_CHARS.Contains( charBeingRevealed ) )
				{
					GD.Print("% " + PAUSE_CHARS[ charBeingRevealed ]);
					waitTime = Convert.ToDouble(PAUSE_CHARS[ charBeingRevealed ]);
				}
			}
			else
				waitTime -= delta;
		}
		
		
		public void AppendDialog( string text )
		{
			isDialogProgressing = true;
			EmitSignal("DialogStarted");
			
			if ( textbox.Text.Length > 0 )
			{
				textbox.AppendBbcode( "\n\n" + text );
				newlineCount += 2;
			}
			else
				textbox.AppendBbcode( text );
		}
		
		public void ClearDialog()
		{
			textbox.BbcodeText = "";
			currentVisibleCharacters = 0;
			newlineCount = 0;
			waitTime = 0.0;
		}
		
		public void ShowDialogInstantly()
		{
			currentVisibleCharacters = textbox.GetTotalCharacterCount();
			textbox.VisibleCharacters = currentVisibleCharacters;
		}
	}

}
