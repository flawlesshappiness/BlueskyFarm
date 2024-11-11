using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class DialogueController : SingletonController
{
    public override string Directory => "Dialogue";
    public static DialogueController Instance => Singleton.Get<DialogueController>();
    public DialogueNode CurrentNode { get; private set; }

    public event Action<DialogueNode> OnDialogue;
    public event Action OnDialogueStart;
    public event Action OnDialogueEnd;

    private DialogueNodeCollection _collection;

    public override void _Ready()
    {
        base._Ready();
        _collection = DeserializeJson();

        RegisterDebugActions();
    }

    private void RegisterDebugActions()
    {
        var category = "Dialogue";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Set dialogue node",
            Action = SearchForDialogueNode
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Next dialogue node",
            Action = v => { NextNode(); v.Close(); }
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Set language",
            Action = SetLanguage
        });

        void SearchForDialogueNode(DebugView view)
        {
            view.SetContent_Search();

            foreach (var node in _collection.Nodes.Values)
            {
                view.ContentSearch.AddItem(node.name, () => { SetNode(node); view.Close(); });
            }

            view.ContentSearch.UpdateButtons();
        }

        void SetLanguage(DebugView view)
        {
            view.SetContent_Search();
            foreach (var locale in TranslationServer.GetLoadedLocales())
            {
                view.ContentSearch.AddItem(locale, () => { TranslationServer.SetLocale(locale); view.Close(); });
            }
            view.ContentSearch.UpdateButtons();
        }
    }

    private DialogueNodeCollection DeserializeJson()
    {
        var path = "res://Dialogue/dialogue.json";
        var content = FileAccess.GetFileAsString(path);
        var nodes = JsonSerializer.Deserialize<IEnumerable<DialogueNode>>(content);
        var collection = new DialogueNodeCollection(nodes);
        return collection;
    }

    public DialogueNode GetNode(string name)
    {
        return _collection.Nodes.TryGetValue(name, out var node) ? node : null;
    }

    public void SetNode(string name) => SetNode(GetNode(name));

    public void SetNode(DialogueNode node)
    {
        if (CurrentNode == node) return;
        if (node == null) return;

        var is_first = CurrentNode == null;
        CurrentNode = node;

        if (is_first)
        {
            OnDialogueStart?.Invoke();
        }

        OnDialogue?.Invoke(CurrentNode);
    }

    public void NextNode()
    {
        if (CurrentNode == null) return;

        if (string.IsNullOrEmpty(CurrentNode.next))
        {
            EndDialogue();
        }
        else
        {
            var node = GetNode(CurrentNode.next);
            if (node == null)
            {
                EndDialogue();
            }
            else
            {
                SetNode(node);
            }
        }
    }

    public void EndDialogue()
    {
        if (CurrentNode == null) return;

        CurrentNode = null;
        OnDialogueEnd?.Invoke();
    }
}
