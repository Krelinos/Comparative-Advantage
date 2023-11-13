using Godot;
using System;

namespace CompAdv
{
	public class Glossary : NinePatchRect
	{
		private Label hoverWindow;
		private PackedScene GlossaryTermScene;
		
		// Repository of terms read from JSON file
		public Godot.Collections.Dictionary Terms;
		
		// Does user have mouse on a glossary term?
		private bool userIsHovering;
		
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			hoverWindow = GetNode<Label>("/root/Control/TextureRect/DescriptionPopup");
			GlossaryTermScene = (PackedScene)GD.Load("res://Scenes/GlossaryTerm.tscn");
			
			userIsHovering = false;
			
			File termsJsonFile = new File();
				
			// 28 Sept 2023 - ModeFlag is Case-Sensitive; .READ will error, but .Read is fine
			// Must be a C# thing. This may apply to other Enums as well.
			Error e = termsJsonFile.Open("res://Dialog/definitions.json", File.ModeFlags.Read);
			
			if ( e != Error.Ok )
				GD.PushError( String.Format("Tried to open file for glossary terms, but received error '{0}'.", e.ToString()) );
			
			JSONParseResult termsJson = JSON.Parse( termsJsonFile.GetAsText() );
			termsJsonFile.Close();
			
			if ( termsJson.Error != Error.Ok )
				GD.PushError( String.Format("Tried to parse JSON for glossary terms, but received error '{0}'.", termsJson.Error.ToString()) );
			
			Terms = termsJson.Result as Godot.Collections.Dictionary;
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(float delta)
		{
			if ( userIsHovering )
				hoverWindow.RectPosition = GetViewport().GetMousePosition() + (new Vector2( 12, 12 ));
		}
		
		public void AddTermToGlossary( String termName )
		{
			// Prevent duplicate terms
			foreach ( Node child in GetNode("MarginContainer/VBoxContainer").GetChildren() )
				if ( child.Name == termName )
					return;
			
			var term = GlossaryTermScene.Instance() as Button;
			term.Name = termName;
			term.Text = termName.Capitalize();
			
			term.Connect(
				"mouse_entered",
				this,
				"ShowTermDefinition",
				new Godot.Collections.Array { termName }
			);
			
			term.Connect(
				"mouse_exited",
				this,
				"HideTermDefinition"
			);
			
			GetNode("MarginContainer/VBoxContainer").AddChild( term );
		}
		
		private void ShowTermDefinition( String termName )
		{
			userIsHovering = true;
			hoverWindow.RectSize = new Vector2( hoverWindow.RectSize.x, 32 );
			hoverWindow.Text = Terms[ termName ] as String;
			hoverWindow.Visible = true;
		}
		
		private void HideTermDefinition()
		{
			userIsHovering = false;
			hoverWindow.Text = "";
			hoverWindow.RectSize = new Vector2( hoverWindow.RectSize.x, 32 );
			hoverWindow.Visible = false;
		}
	}
}
