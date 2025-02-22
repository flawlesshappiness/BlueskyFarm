using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public partial class DialogueController : SingletonController
{
    public override string Directory => "Dialogue";
    public static DialogueController Instance => Singleton.Get<DialogueController>();
    public DialogueNode CurrentNode { get; private set; }
    public DialogueCharacter CurrentCharacter { get; private set; }

    public event Action<DialogueNode> OnDialogue;
    public event Action OnDialogueStart;
    public event Action OnDialogueEnd;
    public event Action OnDialogueCancel;
    public event Action<string> OnDialogueTrigger;

    private DialogueNodeCollection _collection;

    public override void _Ready()
    {
        base._Ready();
        _collection = DeserializeJson();

        RegisterDebugActions();
    }

    private void RegisterDebugActions()
    {
        var category = "DIALOGUE";

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
                view.ContentSearch.AddItem(node.id, () => { SetNode(node); view.Close(); });
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

    public void SetNode(string name) => SetNode(GetNode(name) ?? new DialogueNode { id = name });

    public void SetNode(DialogueNode node)
    {
        if (node == null) return;
        if (CurrentNode == node) return;

        Debug.TraceMethod(node.id);

        var is_first = CurrentNode == null;
        CurrentNode = node;
        UpdateFlags(node);

        if (is_first)
        {
            OnDialogueStart?.Invoke();
        }

        OnDialogue?.Invoke(CurrentNode);
    }

    public void NextNode()
    {
        if (CurrentNode == null) return;

        UpdateTriggers(CurrentNode);

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

        Debug.TraceMethod();

        StopDialogue();
        OnDialogueEnd?.Invoke();
    }

    public void CancelDialogue()
    {
        if (CurrentNode == null) return;

        Debug.TraceMethod();

        StopDialogue();
        OnDialogueCancel?.Invoke();
    }

    private void StopDialogue()
    {
        CurrentNode = null;
    }

    private void UpdateFlags(DialogueNode node)
    {
        if (node == null) return;

        UpdateFlags_Bool(node.flags_b);
        UpdateFlags_Int(node.flags_i);
    }

    private void UpdateFlags_Bool(IEnumerable<DialogueFlagBool> flags)
    {
        if (flags == null) return;

        foreach (var flag in flags)
        {
            SetFlag(flag.id, flag.value);
        }
    }

    private void UpdateFlags_Int(IEnumerable<DialogueFlagInt> flags)
    {
        if (flags == null) return;

        foreach (var flag in flags)
        {
            SetFlag(flag.id, flag.value);
        }
    }

    public void SetFlag(string id, bool value)
    {
        Debug.TraceMethod($"{id}: {value}");

        var data = Data.Game.DialogFlags_Bool.FirstOrDefault(x => x.id == id);
        if (data == null)
        {
            data = new DialogueFlagBool { id = id };
            Data.Game.DialogFlags_Bool.Add(data);
        }

        data.value = value;
    }

    public void SetFlag(string id, int value)
    {
        Debug.TraceMethod($"{id}: {value}");

        var data = Data.Game.DialogFlags_Int.FirstOrDefault(x => x.id == id);
        if (data == null)
        {
            data = new DialogueFlagInt { id = id };
            Data.Game.DialogFlags_Int.Add(data);
        }

        data.value = value;
    }

    public int GetIntFlag(string id)
    {
        var data = Data.Game.DialogFlags_Int.FirstOrDefault(x => x.id == id);
        return data?.value ?? 0;
    }

    public bool GetBoolFlag(string id)
    {
        var data = Data.Game.DialogFlags_Bool.FirstOrDefault(x => x.id == id);
        return data?.value ?? false;
    }

    private void UpdateTriggers(DialogueNode node)
    {
        if (node.triggers == null) return;

        foreach (var trigger in node.triggers)
        {
            Debug.TraceMethod(trigger);

            OnDialogueTrigger?.Invoke(trigger);
        }
    }

    public void SetCharacter(DialogueCharacter character)
    {
        CurrentCharacter = character;
    }

    public void ClearCharacter()
    {
        CurrentCharacter = null;
    }
}
