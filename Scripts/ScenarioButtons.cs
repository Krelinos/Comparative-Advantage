using Godot;
using System;

namespace CompAdv
{
	public class ScenarioButtons : NinePatchRect
	{
		
		private Godot.Collections.Array ButtonContainers;
		private CompAdv.Workspace Workspace;
		
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			ButtonContainers = GetNode("MarginContainer/VBoxContainer").GetChildren();
			Workspace = GetNode("../../Workspace") as CompAdv.Workspace;
			
			foreach ( Control buttonContainer in ButtonContainers )
			{
				Button scenarioButton = buttonContainer.GetNode("Button") as Button;
			}
		}
		
		private void _OnButtonPressed(String scenName)
		{
			Workspace.LoadScenario( scenName );
			ReleaseFocus();
			GD.Print( scenName );
		}
	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }
	}
}
