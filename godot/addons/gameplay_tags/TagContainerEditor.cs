using GameplayTags.Runtime;
using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class TagContainerEditor : VBoxContainer
{
	public bool IsPluginInstance = false;
	public Tags TagsNode;
	
	private readonly Dictionary<TreeItem, GameplayTagNode> _treeItemToNode = new();

	private Tree _tree;

	private Texture2D _checkedIcon;
	private Texture2D _uncheckedIcon;

	public event Action OnChanged;

	public override void _Ready()
	{
		base._Ready();

		if (!IsPluginInstance)
		{
			return;
		}

		_tree = GetNode<Tree>("%Tree");

		_checkedIcon = EditorInterface.Singleton.GetEditorTheme().GetIcon("GuiChecked", "EditorIcons");
		_uncheckedIcon = EditorInterface.Singleton.GetEditorTheme().GetIcon("GuiUnchecked", "EditorIcons");

		TreeItem rootTreeNode = _tree.CreateItem();
		_tree.HideRoot = true;

		GD.Print($"min height: {rootTreeNode.CustomMinimumHeight}");

		ConstructTreeNode(_tree, rootTreeNode, GameplayTagsManager.Instance.RootNode);

		_tree.ItemCollapsed += TreeItemCollapsed;
		_tree.CustomMinimumSize += new Vector2(0, 12);

		_tree.ButtonClicked += TreeButtonClicked;
	}

	private void TreeButtonClicked(TreeItem item, long column, long id, long mouseButtonIndex)
	{
		var itemName = _treeItemToNode[item].CompleteTagName.ToString();

		if (mouseButtonIndex == 1)
		{
			if (id == 0)
			{
				if (TagsNode.ContainerTags.Contains(itemName))
				{
					item.SetButton((int)column, (int)id, _uncheckedIcon);
					GD.Print($"Remove: {itemName}");
					TagsNode.ContainerTags.Remove(itemName);
				}
				else
				{
					item.SetButton((int)column, (int)id, _checkedIcon);
					GD.Print($"Add: {itemName}");
					TagsNode.ContainerTags.Add(itemName);
				}

				OnChanged?.Invoke();
				NotifyPropertyListChanged();
			}
		}
	}

	private void ConstructTreeNode(Tree tree, TreeItem currentTreeItem, GameplayTagNode currentNode)
	{
		foreach (var childTagNode in currentNode.ChildTags)
		{
			TreeItem childTreeNode = tree.CreateItem(currentTreeItem);
			childTreeNode.SetText(0, childTagNode.TagName.ToString());

			if(TagsNode.ContainerTags.Contains(childTagNode.CompleteTagName.ToString()))
			{
				GD.Print(childTagNode.CompleteTagName.ToString());
				childTreeNode.AddButton(0, _checkedIcon);
			}
			else
			{
				childTreeNode.AddButton(0, _uncheckedIcon);
			}

			_tree.CustomMinimumSize += new Vector2(0, 28);

			_treeItemToNode.Add(childTreeNode, childTagNode);

			ConstructTreeNode(tree, childTreeNode, childTagNode);
		}
	}

	private void TreeItemCollapsed(TreeItem item)
	{
		int childCount = 0;
		TotalChilds(item, ref childCount);

		if (item.Collapsed)
		{
			_tree.CustomMinimumSize -= new Vector2(0, childCount * 28);
		}
		else
		{
			_tree.CustomMinimumSize += new Vector2(0, childCount * 28);
		}
	}

	private void TotalChilds(TreeItem item, ref int childCount)
	{
		foreach (var child in item.GetChildren())
		{
			childCount++;
			TotalChilds(child, ref childCount);
		}
	}
}
