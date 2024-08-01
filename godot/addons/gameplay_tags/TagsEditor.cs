#if TOOLS
using Godot;
using Godot.Collections;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

#endif

// RandomIntEditor.cs
#if TOOLS

public partial class TagsEditor : EditorProperty
{
	public event Action ButtonToggled;

	public PackedScene InspectorScene { get; set; }

	public bool Collapsed { get; private set; }

	// The main control for editing the property.
	private Button _propertyControl = new ();
	// An internal value of the property.
	private int _currentValue;
	// A guard against internal changes when the property is updated.
	private bool _updating = false;

	public TagsEditor()
	{
		InspectorScene = ResourceLoader.Load<PackedScene>("res://addons/gameplay_tags/TagContainer.tscn");

		// Add the control as a direct child of EditorProperty node.
		AddChild(_propertyControl);
		// Make sure the control is able to retain the focus.
		AddFocusable(_propertyControl);
		// Setup the initial state and connect to the signal to track changes.
		RefreshControlText();

		_propertyControl.ToggleMode = true;
		_propertyControl.Toggled += OnButtonToggled;
	}

	public void EmitChanged()
	{
		EmitChanged(GetEditedProperty(), GetEditedObject().Get(GetEditedProperty()).AsGodotArray<string>());
	}

	private void OnButtonToggled(bool toggledOn)
	{
		// Ignore the signal if the property is currently being updated.
		if (_updating)
		{
			return;
		}

		Collapsed = toggledOn;

		GD.Print("Toggled");

		ButtonToggled?.Invoke();

		//EmitChanged(GetEditedProperty(), _currentValue);		
	}

	public override void _UpdateProperty()
	{
		GD.Print($"_UpdateProperty: {GetEditedObject().Get(GetEditedProperty()).AsGodotArray<string>()}");

		// Read the current value from the property.
		var newValue = ((Godot.Collections.Array<string>)GetEditedObject().Get(GetEditedProperty())).Count;
		if (newValue == _currentValue)
		{
			return;
		}

		// Update the control with the new value.
		_updating = true;
		_currentValue = newValue;
		RefreshControlText();
		_updating = false;
	}

	private void RefreshControlText()
	{
		_propertyControl.Text = $"Container (size:{_currentValue})";
	}
}
#endif