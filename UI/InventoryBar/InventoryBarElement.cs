using Godot;
using System.Collections;

public partial class InventoryBarElement : ControlScript
{
    [Export]
    public Label UseLabel;

    [Export]
    public TextureRect SelectedTexture;

    [Export]
    public WorldObject WorldObject;

    [Export]
    public Control DisabledControl;

    public bool Selected { get; private set; }

    private ItemInfo _current_item;
    private Coroutine _cr_warning;

    public override void _Ready()
    {
        base._Ready();
        DisabledControl.Hide();
    }

    public void Clear()
    {
        _current_item = null;
        WorldObject.Clear();
        StopDisabledWarning();
    }

    public void Select()
    {
        Selected = true;
        SelectedTexture.Show();
        WorldObject.StartAnimateSpin();
    }

    public void Deselect()
    {
        Selected = false;
        SelectedTexture.Hide();
        UseLabel.Hide();
        WorldObject.StopAnimateSpin();
    }

    public void AnimateDisabledWarning()
    {
        _cr_warning = StartCoroutine(Cr, "DisabledWarning");
        IEnumerator Cr()
        {
            for (int i = 0; i < 4; i++)
            {
                DisabledControl.Show();
                yield return new WaitForSeconds(0.25f);
                DisabledControl.Hide();
                yield return new WaitForSeconds(0.25f);
            }
        }
    }

    private void StopDisabledWarning()
    {
        Coroutine.Stop(_cr_warning);
        DisabledControl.Hide();
    }

    public void LoadItem(ItemInfo info)
    {
        if (_current_item == info) return;
        _current_item = info;

        var item = ItemController.Instance.CreateItem(info, new CreateItemSettings
        {
            Parent = WorldObject.Origin,
            Tracked = false,
        });

        item.ProcessMode = ProcessModeEnum.Disabled;
        WorldObject.SetObject(item);
        UpdateCameraFromItemInfo(info);
        UpdateRotationOffsetFromItemInfo(info);
    }

    private void UpdateCameraFromItemInfo(ItemInfo info)
    {
        WorldObject.SetCameraDistance(info.InventoryItemCameraDistance);
    }

    private void UpdateRotationOffsetFromItemInfo(ItemInfo info)
    {
        WorldObject.SetRotationOffset(info.InventoryItemRotationOffset);
    }
}
