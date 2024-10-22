using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class AutoGuid : Node3DScript
{
    [Export]
    public string IdPropertyName;

    [Export]
    public string Error;

    private Node Target => GetParent();
    private Node TargetParent => Target.GetParent();
    private bool PropertyExists => !string.IsNullOrEmpty(IdPropertyName) && Target?.Get(IdPropertyName) != null;
    private string CurrentValue => Target.Get(IdPropertyName).AsString();

    private bool _was_just_created;

    protected override void _ReadyEditor()
    {
        base._ReadyEditor();
        _was_just_created = true;
    }

    public override void _ValidateProperty(Dictionary property)
    {
        base._ValidateProperty(property);

        if (property["name"].AsStringName() == nameof(Error))
        {
            var flags = property["usage"].As<PropertyUsageFlags>();
            flags |= PropertyUsageFlags.ReadOnly;
            property["usage"] = (int)flags;
        }
    }

    protected override void _ProcessEditor(double delta)
    {
        base._ProcessEditor(delta);

        Error = string.Empty;

        if (!PropertyExists)
        {
            Error = "Property does not exist!";
            return;
        }

        ValidateDuplicate();
        ValidateValue();

        _was_just_created = false;
    }

    private void ValidateDuplicate()
    {
        if (TargetParent == null)
        {
            Error = "Parent does not exist!";
        }

        if (!_was_just_created) return;

        foreach (var child in TargetParent.GetChildren())
        {
            if (child == Target) continue;

            var value = child.Get(IdPropertyName).AsString();
            if (CurrentValue == value)
            {
                Target.Set(IdPropertyName, string.Empty);
                break;
            }
        }
    }

    private void ValidateValue()
    {
        var value = CurrentValue;
        if (string.IsNullOrEmpty(value))
        {
            value = Guid.NewGuid().ToString();
            Target.Set(IdPropertyName, value);
        }
    }
}
