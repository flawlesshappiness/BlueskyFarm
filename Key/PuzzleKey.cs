using Godot;
using System.Collections;

public partial class PuzzleKey : Item
{
    [NodeName]
    public GpuParticles3D PsPoof;

    [NodeName]
    public GpuParticles3D PsGlow;

    [NodeType]
    public MeshInstance3D Mesh;

    private bool _animating;

    public Coroutine AnimateGlowThenDisappear()
    {
        if (_animating) return null;
        _animating = true;

        return Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            ReleaseIfGrabbed();
            SetCollision_None();
            LockPosition_All();
            LockRotation_All();

            var start_em = Colors.Black;
            var end_em = Colors.White;
            var mat = Mesh.Mesh.SurfaceGetMaterial(0).Duplicate() as Material;
            Mesh.SetSurfaceOverrideMaterial(0, mat);
            mat.Set("emission_enabled", true);
            mat.Set("emission", start_em);

            yield return LerpEnumerator.Lerp01(1f, f =>
            {
                mat.Set("emission", start_em.Lerp(end_em, f));
            });

            PsPoof.Emitting = true;
            PsGlow.Emitting = true;
            UnparentAndDestroy(PsPoof, 2);
            UnparentAndDestroy(PsGlow, 2);

            Hide();
            QueueFree();
        }
    }

    private void UnparentAndDestroy(Node3D node, float delay)
    {
        node.SetParent(Scene.Current);

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);
            node.QueueFree();
        }
    }
}
