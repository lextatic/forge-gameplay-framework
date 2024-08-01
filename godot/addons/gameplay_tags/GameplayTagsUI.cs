using GameplayTags.Runtime;
using Godot;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class GameplayTagsUI : VBoxContainer
{
	public bool IsPluginInstance = false;

	private readonly Dictionary<TreeItem, GameplayTagNode> _treeItemToNode = new();

	private RegisteredTags _registeredTags;

	private Tree _tree;
	private LineEdit _tagNameTextField;
	private Button _addTagButton;

	private Texture2D _addIcon;
	private Texture2D _removeIcon;

	public override void _Ready()
	{
		base._Ready();

		if (!IsPluginInstance)
		{
			return;
		}

		_registeredTags = ResourceLoader.Load<RegisteredTags>("res://addons/gameplay_tags/RegisteredTags.tres");

		GD.Print("Initialize Tag Tree");
		GameplayTagsManager.Instance.ConstructGameplayTagTreeFromList(_registeredTags.Tags.ToList());

		_addIcon = EditorInterface.Singleton.GetEditorTheme().GetIcon("Add", "EditorIcons");
		_removeIcon = EditorInterface.Singleton.GetEditorTheme().GetIcon("Remove", "EditorIcons");

		_tree = GetNode<Tree>("%Tree");
		_tagNameTextField = GetNode<LineEdit>("%TagNameField");
		_addTagButton = GetNode<Button>("%AddTagButton");

		TreeItem rootTreeNode = _tree.CreateItem();
		_tree.HideRoot = true;
		ConstructTreeNode(_tree, rootTreeNode, GameplayTagsManager.Instance.RootNode);

		_tree.ButtonClicked += TreeButtonClicked;
		_addTagButton.Pressed += AddTagButton_Pressed;
	}

	private void AddTagButton_Pressed()
	{
		//if (!GameplayTag.IsValidGameplayTagString(_tagNameTextField.Text, out string error, out string fixedTag))
		//{
		//	_tagNameTextField.Text = fixedTag;
		//	return;
		//}

		GD.Print($"Add: {_tagNameTextField.Text}");
		_registeredTags.Tags.Add(_tagNameTextField.Text);

		ReconstructTreeNode();
	}

	private void ReconstructTreeNode()
	{
		GameplayTagsManager.Instance.DestroyGameplayTagTree();
		GameplayTagsManager.Instance.ConstructGameplayTagTreeFromList(_registeredTags.Tags.ToList());

		_tree.Clear();
		TreeItem rootTreeNode = _tree.CreateItem();
		_tree.HideRoot = true;
		ConstructTreeNode(_tree, rootTreeNode, GameplayTagsManager.Instance.RootNode);

		//EditorInterface.Singleton.GetResourceFilesystem().Scan();
		//EditorFileSystem.Scan();
	}

	public override void _ExitTree()
	{
		base._ExitTree();

		if (!IsPluginInstance)
		{
			return;
		}

		GD.Print("Destroy Tag Tree");

		GameplayTagsManager.Instance.DestroyGameplayTagTree();
	}

	private void ConstructTreeNode(Tree tree, TreeItem currentTreeItem, GameplayTagNode currentNode)
	{
		foreach (var childTagNode in currentNode.ChildTags)
		{
			TreeItem childTreeNode = tree.CreateItem(currentTreeItem);
			childTreeNode.SetText(0, childTagNode.TagName.ToString());
			childTreeNode.AddButton(0, _addIcon);
			childTreeNode.AddButton(0, _removeIcon);

			_treeItemToNode.Add(childTreeNode, childTagNode);

			ConstructTreeNode(tree, childTreeNode, childTagNode);
		}
	}

	private void TreeButtonClicked(TreeItem item, long column, long id, long mouseButtonIndex)
	{
		if (mouseButtonIndex == 1)
		{
			if (id == 0)
			{
				_tagNameTextField.Text = $"{_treeItemToNode[item].CompleteTagName}.";
				_tagNameTextField.GrabFocus();
				_tagNameTextField.CaretColumn = _tagNameTextField.Text.Length;
			}

			if (id == 1)
			{
				GD.Print($"Remove: {_treeItemToNode[item].CompleteTagName}");

				_registeredTags.Tags.Remove(_treeItemToNode[item].CompleteTagName.ToString());

				ReconstructTreeNode();
			}
		}
	}
}
