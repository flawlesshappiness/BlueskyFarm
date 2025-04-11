using Godot;
using System.Collections;

public partial class InventoryUpgradeItem : Item
{
    [Export]
    public int InventoryUpgradeAmount = 1;

    public override void PickUp()
    {
        IsBeingHandled = true;
        SetGrabbable(false);

        OnPickUp?.Invoke();

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var position = GlobalPosition;
            SoundController.Instance.Play("sfx_throw_light", position);

            yield return AnimateDisappearAndQueueFree();

            InventoryController.Instance.IncreaseInventorySize(InventoryUpgradeAmount);
            Particle.PlayOneShot("ps_smoke_puff_small", position);
            SoundController.Instance.Play("sfx_crafting_complete");
        }
    }
}
