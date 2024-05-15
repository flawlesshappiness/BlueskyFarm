using Godot;
using System;
using System.Linq;
using System.Reflection;

public partial class NodeScript : Node
{
    public override void _Ready()
    {
        FindNodesFromAttribute(this, GetType());

        base._Ready();
    }

    public static void FindNodesFromAttribute(Node root, Type type)
    {
        Debug.TraceMethod(root.Name);
        Debug.Indent++;

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);

        foreach (FieldInfo field in fields)
        {
            if (field.CustomAttributes.Count() == 0) continue;

            try
            {
                FindNodeFromAttribute(root, field);
            }
            catch (Exception e)
            {
                Debug.LogError($"{field.Name} field value not found: " + e.Message);
            }
        }

        Debug.Indent--;
    }

    private static Node FindNodeFromAttribute(Node root, FieldInfo field)
    {
        if (Attribute.GetCustomAttribute(field, typeof(NodePathAttribute)) as NodePathAttribute is var nodePathAtt && nodePathAtt != null)
        {
            var node = root.GetNode(nodePathAtt.Value);
            _ = node ?? throw new NullReferenceException("Node was null");

            field.SetValue(root, node);
            return node;
        }
        else if (Attribute.GetCustomAttribute(field, typeof(NodeNameAttribute)) as NodeNameAttribute is var nodeNameAtt && nodeNameAtt != null)
        {
            var node = root.GetNodeInChildren<Node>(nodeNameAtt.Value);
            _ = node ?? throw new NullReferenceException("Node was null");

            field.SetValue(root, node);
            return node;
        }
        else if (Attribute.GetCustomAttribute(field, typeof(NodeTypeAttribute)) as NodeTypeAttribute is var nodeTypeAtt && nodeTypeAtt != null)
        {
            var node = root.GetNodeInChildren(nodeTypeAtt.Type);
            _ = node ?? throw new NullReferenceException("Node was null");

            field.SetValue(root, node);
            return node;
        }

        return null;
        //throw new NullReferenceException($"No valid attribute was found");
    }

    public bool IsVisibleInTree()
    {
        var parent = GetParent();
        while (parent != null)
        {
            if (!Visible(parent))
            {
                return false;
            }

            parent = parent.GetParent();
        }

        return true;

        bool Visible(Node node)
        {
            if (node is Node3D n3)
            {
                return n3.IsVisibleInTree();
            }
            else if (node is Node2D n2)
            {
                return n2.IsVisibleInTree();
            }

            return true;
        }
    }
}
