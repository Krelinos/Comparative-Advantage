using Godot;
using System;

namespace CompAdv
{

	public class Workspace : VBoxContainer
	{
		private NinePatchRect DialogBoxContainer;
		private RichTextLabel DialogBox;
		private Control QuestionBox;
		private Control UserControlIcons;
		private CompAdv.Glossary Glossary;
		
		private Godot.Collections.Dictionary dialog;
		private Godot.Collections.Dictionary questions;
		
		private PackedScene MultipleChoice = (PackedScene)GD.Load("res://Scenes/MultipleChoice.tscn");
		private PackedScene MultipleChoiceAnswer = (PackedScene)GD.Load("res://Scenes/MultipleChoiceAnswer.tscn");
		
		private String nextDialogId;
		// User answer to be judged when the Submit button is pressed
		private float userAnswer;
		// The correct Id of a multiple choice selection, or float number.
		private float correctAnswer;
		
		// The dialog will not progress on space/enter until this is false.
		// Intended to be used when the user is presented a problem to be solved.
		private bool holdingForProblemCompletion = false;
		
		// The current question being displayed
		Godot.Collections.Dictionary question;
		
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			DialogBoxContainer = GetNode<NinePatchRect>("NinePatchRect/MarginContainer/VBoxContainer/DialogBoxContainer");
			DialogBox = DialogBoxContainer.GetNode<RichTextLabel>("MarginContainer/VBoxContainer/DialogBox");
			QuestionBox = GetNode<Control>("NinePatchRect/MarginContainer/VBoxContainer/Interactives/HBoxContainer/MarginContainer/PanelContainer/QuestionBox");
			UserControlIcons = DialogBoxContainer.GetNode<Control>("MarginContainer/VBoxContainer/PanelContainer/UserControlIcons");
			Glossary = GetNode<CompAdv.Glossary>("/root/Control/TextureRect/HBoxContainer/Menus/Glossary");
			
			// Pre-4.0 C# Godot sure is jank.
			QuestionBox.GetNode<Button>("Submit").Connect(
				"pressed",
				this,
				"ChoiceSubmitted",
				new Godot.Collections.Array { }
			);
			
			LoadScenario("0_Preface");
		}
		
		public override void _Input(InputEvent inputEvent)
		{
			if ( Input.IsActionJustPressed("continueDialog") )
				ContinueDialog();
		}
		
		private void ContinueDialog()
		{
			if ( nextDialogId.Equals("") || holdingForProblemCompletion )
				return;
			
			Godot.Collections.Dictionary nextDialog = dialog[nextDialogId] as Godot.Collections.Dictionary;
			
			// Clean the question box of the previous question
			foreach ( Node child in QuestionBox.GetNode("MarginContainer").GetChildren() )
				child.QueueFree();
			
			
			// If there is text in this dialog, append to dialog box.
			if ( nextDialog.Contains("text") )
			{
				String dialogText = nextDialog["text"] as String;
				if ( !dialogText.Equals("") )
					if ( DialogBox.Text.Equals("") )
						DialogBox.AppendBbcode( dialogText );
					else
						DialogBox.AppendBbcode( "\n\n" + dialogText );
			}
			
			// Show a dialog's cutscene at the top right.
			if ( nextDialog.Contains("cutscene") )
			{
			}
			
			if ( nextDialog.Contains("concept") )
				Glossary.AddTermToGlossary( nextDialog["concept"] as String );
			
			if ( nextDialog.Contains("question") )
				ShowQuestion( nextDialog["question"] as String );
			else
				QuestionBox.Visible = false;
			
			// If there is another dialog after this one, move on to it.
			if ( nextDialog.Contains("next") )
				nextDialogId = nextDialog["next"] as String;
			else
			{
				nextDialogId = "";
				UserControlIcons.Visible = false;
			}
		}
		
		public void LoadScenario( String scenarioName )
		{
			holdingForProblemCompletion = false;
			scenarioName = scenarioName.ToLower();
			File scenarioJsonFile = new File();
			
			// 28 Sept 2023 - ModeFlag is Case-Sensitive; .READ will error, but .Read is fine
			// Must be a C# thing. This may apply to other Enums as well.
			Error e = scenarioJsonFile.Open("res://Dialog/" + scenarioName + ".json", File.ModeFlags.Read);
			
			if ( e != Error.Ok )
				GD.PushError( String.Format("Tried to open scenario file '{0}', but received error '{1}'.", scenarioName, e.ToString()) );
			
			JSONParseResult scenarioJson = JSON.Parse( scenarioJsonFile.GetAsText() );
			scenarioJsonFile.Close();
			
			if ( scenarioJson.Error != Error.Ok )
				GD.PushError( String.Format("Tried to parse JSON of scenario '{0}', but received error '{1}'.", scenarioName, scenarioJson.Error.ToString()) );
			
			var scenarioJsonAsDictionary = scenarioJson.Result as Godot.Collections.Dictionary;
			
			// Put everything in the JSON file's "dialog: {}" into a Dictionary
			// If there is no dialog in this scenario, just have an empty dialog
			// dictionary in the JSON file.
			dialog = scenarioJsonAsDictionary["dialog"] as Godot.Collections.Dictionary;
			// Same with questions
			questions = scenarioJsonAsDictionary["questions"] as Godot.Collections.Dictionary;
			
			nextDialogId = dialog["start"] as String;
			
			if( nextDialogId.Equals("") )
				GD.PushError( String.Format("Scenario JSON '{0}' is missing a start dialog.", scenarioName) );
			
			DialogBox.BbcodeText = "";
			QuestionBox.Visible = false;
			UserControlIcons.Visible = true;
			ContinueDialog();
		}
		
		// Given a question ID, this spawns a question based on the given type and Dictionary info.
		private void ShowQuestion( String questionID )
		{
			holdingForProblemCompletion = true;
			question = questions[ questionID ] as Godot.Collections.Dictionary;
			
			switch ( question["type"] as String )
			{
				case "multiple_choice":
					var mcNode = MultipleChoice.Instance();
					mcNode.GetNode<Label>("Question").Text = question["label"] as String;
					QuestionBox.GetNode("MarginContainer").AddChild( mcNode );
					QuestionBox.Visible = true;
					
					var bGroup = new ButtonGroup();
					
					int id = 0;
					
					foreach ( String answer in question["content"] as Godot.Collections.Array )
					{
						var choice = MultipleChoiceAnswer.Instance();
						choice.GetNode<Label>("MarginContainer/Label").Text = answer;
						choice.GetNode<CheckBox>("CheckBox").Group = bGroup;
						
						choice.GetNode<Button>("Button").Connect(
							"pressed",
							this,
							"ChoiceSelected",
							new Godot.Collections.Array { id, choice }
						);
						
						mcNode.AddChild( choice );
						id++;
					}
					
					correctAnswer = (float)question["correct"];
					
					break;
			}
			
			UserControlIcons.Visible = false;
		}
		
		private void ChoiceSelected( int id, Node choice )
		{
			userAnswer = id;
			choice.GetNode<CheckBox>("CheckBox").Pressed = true;
		}
		
		// User hits the "Submit" button
		private void ChoiceSubmitted()
		{
			var feedback = QuestionBox.GetNode("MarginContainer").GetNodeOrNull<Label>("Feedback");
			if ( feedback == null )
			{
				feedback = new Label();
				feedback.Name = "Feedback";
				feedback.Autowrap = true;
				feedback.SizeFlagsVertical = 0b1000;
				QuestionBox.GetNode("MarginContainer").AddChild( feedback );
			}
			GD.Print( Mathf.Abs(userAnswer - correctAnswer) );
			if ( Mathf.Abs(userAnswer - correctAnswer) <= 0.001 )
			{
				feedback.Modulate = new Color( 0.9f, 1f, 0.9f, 1f );
				holdingForProblemCompletion = false;
				UserControlIcons.Visible = true;
				QuestionBox.GetNode<Button>("Submit").ReleaseFocus();
			}
			else
				feedback.Modulate = new Color( 1f, 0.9f, 0.9f, 1f );
			
			feedback.Text = (question["feedback"] as Godot.Collections.Array)[ (int)userAnswer ] as String;
		}
	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }
	}

}
