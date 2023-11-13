using Godot;
using System;

public class Game : Control
{
	private File dialogJsonFile;
	
	private Control workspace;
	private Control dialogBox;
	private RichTextLabel textbox;
	private Control userControlIcons;
	
	private int targetVisibleCharacters;
	private int currentVisibleCharacters;
	
	private String nextDialog;
	private Godot.Collections.Dictionary uwu;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		dialogJsonFile = new File();
		// 28 Sept 2023 - ModeFlag is Case-Sensitive; .READ will error, but .Read is fine
		// Must be a C# thing. This may apply to other Enums as well.
		dialogJsonFile.Open("res://Dialog/1_a_Output.json", File.ModeFlags.Read);
		
		workspace = GetNode<Control>("TextureRect/HBoxContainer/Workspace");
		dialogBox = workspace.GetNode<Control>("NinePatchRect/MarginContainer/VBoxContainer/DialogBox");
		textbox = dialogBox.GetNode<RichTextLabel>("MarginContainer/VBoxContainer/RichTextLabel");
		userControlIcons = dialogBox.GetNode<Control>("MarginContainer/VBoxContainer/PanelContainer/UserControlIcons");
		
		targetVisibleCharacters = 0;
		currentVisibleCharacters = 0;
		
		//userControlIcons.Visible = false;
		//GD.Print( dialogJsonFile.GetAsText() );
		JSONParseResult dialog = JSON.Parse( dialogJsonFile.GetAsText() );
		GD.Print( dialog.Result );
		Godot.Collections.Dictionary d = dialog.Result as Godot.Collections.Dictionary;
		uwu = d["dialog"] as Godot.Collections.Dictionary;
		GD.Print( d["dialog"] is Godot.Collections.Dictionary );
		GD.Print( d is Godot.Collections.Dictionary );
		GD.Print( d["dialog"] is Godot.Collections.Array );
		
		GD.Print( uwu["1a2"] );
		Godot.Collections.Dictionary owo = uwu["1a2"] as Godot.Collections.Dictionary;
		GD.Print( owo["text"] );
		
		textbox.BbcodeText = "";
//		textbox.VisibleCharacters = 0;
		nextDialog = "1a0";
	}
	
	public override void _Input(InputEvent inputEvent)
	{
		if ( Input.IsActionJustPressed("continueDialog") )
		{
			GD.Print( nextDialog );
			Godot.Collections.Dictionary oof = uwu[ nextDialog ] as Godot.Collections.Dictionary;
			GD.Print( oof["text"] );
			GD.Print( oof["text"] is System.String );
			textbox.AppendBbcode( oof["text"] as System.String + "\n\n");
			GD.Print( "owo" + textbox.BbcodeText );
			nextDialog = oof["next"] as System.String;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
//		currentVisibleCharacters = currentVisibleCharacters < targetVisibleCharacters ? currentVisibleCharacters+1 : currentVisibleCharacters;
//		textbox.VisibleCharacters = currentVisibleCharacters;
	}
}
