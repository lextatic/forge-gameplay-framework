#if TOOLS
using Godot;

[Tool]
public partial class GameplayTagsLoader : EditorPlugin
{
	public PackedScene PluginScene { get; set; }

	private GameplayTagsUI _dockedScene;

	private Window _myWindow;

	private TagsInspectorPlugin _tagsInspectorPlugin;

	public override void _EnterTree()
	{
		GD.Print("_EnterTreeBeginning");
		
		PluginScene = ResourceLoader.Load<PackedScene>("res://addons/gameplay_tags/GameplayTags.tscn");

		_dockedScene = (GameplayTagsUI)PluginScene.Instantiate();
		_dockedScene.IsPluginInstance = true;

		AddControlToDock(DockSlot.LeftUr, _dockedScene);

		//// Called when the plugin is added to the editor
		//AddToolMenuItem("Custom Tools/Open My Window", new Callable(this, MethodName.OnOpenMyWindow));

		//// Create the window but don't add it to the editor yet
		//_myWindow = new Window();
		//_myWindow.Size = new Vector2I(400, 300);
		//_myWindow.Title = "My Custom Window";
		//_myWindow.CloseRequested += () =>
		//{
		//	_myWindow.Hide();
		//};

		//// Add a Label to the window
		//var label = new Label();
		//label.Text = "This is a custom window";
		//_myWindow.AddChild(label);

		//// Add a Button to the window
		//var button = new Button();
		//button.Text = "Close";
		//button.Connect("pressed", new Callable(_myWindow, Window.MethodName.Hide));
		//_myWindow.AddChild(button);

		//// Add the window to the scene tree
		//GetTree().Root.AddChild(_myWindow);
		//_myWindow.Hide(); // Initially hide the window

		GD.Print("_EnterTreeEnd");

		_tagsInspectorPlugin = new TagsInspectorPlugin();
		AddInspectorPlugin(_tagsInspectorPlugin);

		AddAutoload();
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveControlFromDocks(_dockedScene);
		_dockedScene.Free();

		// Called when the plugin is removed from the editor
		//RemoveToolMenuItem("Custom Tools/Open My Window");
		//_myWindow.QueueFree();
		GD.Print("_ExitTree");

		RemoveInspectorPlugin(_tagsInspectorPlugin);

		RemoveAutoload();
	}

	private void OnOpenMyWindow()
	{
		GD.Print("Open Window");
		// Show the window
		//_myWindow.PopupCentered();
	}

	private const string AutoloadPath = "res://addons/gameplay_tags/GameplayTagsStartup.cs";
	private const string AutoloadName = "GameplayTagsStartupScript";

	private void AddAutoload()
	{
		var config = ProjectSettings.LoadResourcePack(AutoloadPath);
		if (config == null)
		{
			GD.PrintErr($"Failed to load script at {AutoloadPath}");
			return;
		}

		if (!ProjectSettings.HasSetting($"autoload/{AutoloadName}"))
		{
			ProjectSettings.SetSetting($"autoload/{AutoloadName}", AutoloadPath);
			ProjectSettings.Save();
			GD.Print($"{AutoloadName} added to autoload.");
		}
	}

	private void RemoveAutoload()
	{
		if (ProjectSettings.HasSetting($"autoload/{AutoloadName}"))
		{
			ProjectSettings.Clear($"autoload/{AutoloadName}");
			ProjectSettings.Save();
			GD.Print($"{AutoloadName} removed from autoload.");
		}
	}
}

#endif
