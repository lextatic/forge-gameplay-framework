#if TOOLS
using Godot;

public partial class TagsInspectorPlugin : EditorInspectorPlugin
{
	public PackedScene InspectorScene { get; set; }

	public override bool _CanHandle(GodotObject @object)
	{
		return @object is Tags;
	}

	// Entendi, mas não vi muita vantagem nisso
	public override void _ParseBegin(GodotObject @object)
	{
		base._ParseBegin(@object);

		if (@object is Tags tagsNode)
		{
			tagsNode.ParseBegin(this);
		}
	}

	private TagContainerEditor _containerScene;

	public override bool _ParseProperty(GodotObject @object, Variant.Type type,
		string name, PropertyHint hintType, string hintString,
		PropertyUsageFlags usageFlags, bool wide)
	{
		InspectorScene = ResourceLoader.Load<PackedScene>("res://addons/gameplay_tags/TagContainer.tscn");
		
		GD.Print($"obj: {@object}, type: {type}, name: {name}, hintType: {hintType}, hintString: {hintString}, usageFlags: {usageFlags}, wide: {wide}");

		//return false;

		// We handle properties of type integer.
		if (type == Variant.Type.Array)
		{
			// Create an instance of the custom property editor and register
			// it to a specific property path.
			_tagsEditor = new TagsEditor();

			//tagsEditor.Connect("property_changed", new Callable(this, MethodName.OnPropertyChanged));
			_tagsEditor.ButtonToggled += OnPropertyChanged;

			AddPropertyEditor(name, _tagsEditor);
			// Inform the editor to remove the default property editor for
			// this property type.

			_containerScene = (TagContainerEditor)InspectorScene.Instantiate();
			_containerScene.IsPluginInstance = true;
			_containerScene.TagsNode = @object as Tags;
			_containerScene.Visible = _tagsEditor.Collapsed;

			_containerScene.OnChanged += ContainerScene_OnChanged;

			GD.Print($"TagContainerEditor: {_containerScene.GetInstanceId()}");
			AddCustomControl(_containerScene);

			GD.Print(_tagsEditor.Collapsed);

			return true;
		}

		return false;
	}

	private void ContainerScene_OnChanged()
	{
		_tagsEditor.EmitChanged();
	}

	private TagsEditor _tagsEditor;

	public void OnPropertyChanged()
	{
		GD.Print("OnPropertyChanged");

		GD.Print($"TagContainerEditor: {_containerScene.GetInstanceId()}");
		//_containerScene.ToggleOptionButton(_tagsEditor.Collapsed);
		//_containerScene.OptionButton_ItemSelected(5);
		_containerScene.Visible = _tagsEditor.Collapsed;

		//if (_tagsEditor.Collapsed)
		//{
		//	var dropdownButton = new Button();
		//	dropdownButton.Text = "Select Items";
		//	//dropdownButton.Connect("pressed", this, nameof(OnDropdownButtonPressed));
		//	AddCustomControl(dropdownButton);
		//}

		NotifyPropertyListChanged();
	}
}
#endif
