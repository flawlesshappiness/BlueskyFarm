public partial class MaterialProcessor : Node3DScript
{
    /*
    [Export]
    public Color ColorOff;

    [Export]
    public Color ColorOn;

    [NodeName]
    public Area3D InputArea;

    [NodeName]
    public Node3D OutputPosition;

    [NodeName]
    public InteractableLever Lever;

    [NodeName]
    public Node3D Glowsticks;

    public int InputCount => Data.Game.MaterialProcessorInputs.Count;

    private List<MeshInstance3D> _glowsticks = new();

    private int _count_glowsticks_on;
    private bool _machine_running;
    private const int COUNT_FOR_MATERIAL = 2;

    private static bool _registered_debug_actions;

    private class InputItem
    {
        public ItemInfo Info { get; set; }
        public ItemData Data { get; set; }
    }

    public override void _Ready()
    {
        base._Ready();

        InputArea.BodyEntered += BodyEntered;
        Lever.OnStateChanged += _ => StartProcessing();

        InitializeGlowsticks();
        UpdateInputCount();

        RegisterDebugActions();
    }

    private void InitializeGlowsticks()
    {
        _glowsticks = Glowsticks.GetNodesInChildren<MeshInstance3D>().ToList();
        foreach (var gs in _glowsticks)
        {
            SetGlowstickColor(gs, ColorOff);
        }
    }

    private void RegisterDebugActions()
    {
        if (_registered_debug_actions) return;
        _registered_debug_actions = true;

        var category = "Material processor";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Unfix",
            Action = _ => Data.Game.Flag_MaterialProcessorFixed = false
        });
    }

    private void BodyEntered(Node3D body)
    {
        if (!IsInstanceValid(body)) return;

        var item = body.GetNodeInParents<Item>();
        if (item == null) return;

        ItemEntered(item);
    }

    private void ItemEntered(Item item)
    {
        if (!IsInstanceValid(item)) return;

        if (item.Data.CustomId == "MaterialProcessorOil")
        {
            FixMachine(item);
            return;
        }

        if (IsValidInput(item))
        {
            CreateInput(item);
            ItemController.Instance.UntrackItem(item);
            item.QueueFree();
            return;
        }
    }

    private void CreateInput(Item item)
    {
        if (InputCount == 0)
        {
            Data.Game.MaterialProcessorCurrentType = item.Info.MaterialType;
        }

        Data.Game.MaterialProcessorInputs.Add(item.Data);

        UpdateInputCount();
    }

    private bool IsValidInput(Item item)
    {
        return item.Info.CanProcessToMaterial && (InputCount == 0 || item.Info.MaterialType == Data.Game.MaterialProcessorCurrentType);
    }

    private void Clear()
    {
        Data.Game.MaterialProcessorInputs.Clear();
    }

    private void UpdateInputCount()
    {
        var count = InputCount;

        if (count == _count_glowsticks_on)
        {
            return;
        }

        if (count == 0)
        {
            AnimateGlowsticksOff();
            return;
        }

        var t = (float)count / COUNT_FOR_MATERIAL;
        var gs_count = (int)(_glowsticks.Count * t);

        AnimateGlowsticksOn(gs_count);
    }

    private Coroutine AnimateGlowsticksOn(int count_on)
    {
        return StartCoroutine(Cr, "glowsticks");
        IEnumerator Cr()
        {
            count_on = Mathf.Clamp(count_on, 0, _glowsticks.Count);
            var i_start = Mathf.Clamp(_count_glowsticks_on, 0, _glowsticks.Count - 1);
            _count_glowsticks_on = count_on;

            for (int i = i_start; i < count_on; i++)
            {
                var gs = _glowsticks[i];
                SetGlowstickColor(gs, ColorOn);
                PlayGlowstickSFX(i);

                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private Coroutine AnimateGlowsticksOff()
    {
        return StartCoroutine(Cr, "glowsticks");
        IEnumerator Cr()
        {
            var i_start = Mathf.Clamp(_count_glowsticks_on, 0, _glowsticks.Count - 1);
            _count_glowsticks_on = 0;

            for (int i = i_start; i >= 0; i--)
            {
                var gs = _glowsticks[i];
                SetGlowstickColor(gs, ColorOff);
                PlayGlowstickSFX(i);

                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    private void PlayGlowstickSFX(int i_pitch)
    {
        var pitch = 0.9f + (0.2f * i_pitch);
        var asp = SoundController.Instance.Play("sfx_pickup", GlobalPosition);
        asp.PitchScale = pitch;
    }

    private void SetGlowstickColor(MeshInstance3D mesh, Color color)
    {
        var mat = mesh.GetSurfaceOverrideMaterial(0);

        if (mat == null)
        {
            mat = mesh.Mesh.SurfaceGetMaterial(0).Duplicate() as Material;
            mesh.SetSurfaceOverrideMaterial(0, mat);
        }

        mat.Set("albedo_color", color);
        mat.Set("emission", color);
    }

    private void StartProcessing()
    {
        if (_machine_running) return;

        if (!Data.Game.Flag_MaterialProcessorFixed)
        {
            FailProcessing();
            return;
        }

        if (InputCount == 0)
        {
            return;
        }

        _machine_running = true;

        var count_materials = InputCount / COUNT_FOR_MATERIAL;
        var count_leftover = InputCount - count_materials * 2;
        Data.Game.MaterialProcessorInputs.Reverse();
        var leftovers = Data.Game.MaterialProcessorInputs.Take(count_leftover).ToList();
        Clear();
        UpdateInputCount();

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            SoundController.Instance.Play("sfx_material_processor_run", GlobalPosition);

            yield return new WaitForSeconds(3.2f);

            for (int i = 0; i < count_materials; i++)
            {
                SpawnMaterial(Data.Game.MaterialProcessorCurrentType);
                yield return new WaitForSeconds(0.2f);
            }

            foreach (var input in leftovers)
            {
                SpawnInputItem(input);
                yield return new WaitForSeconds(0.2f);
            }

            _machine_running = false;
        }
    }

    private void FailProcessing()
    {
        _machine_running = true;

        var inputs = Data.Game.MaterialProcessorInputs.ToList();
        Clear();
        UpdateInputCount();

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            SoundController.Instance.Play("sfx_material_processor_fail", GlobalPosition);

            yield return new WaitForSeconds(2f);

            foreach (var input in inputs)
            {
                SpawnInputItem(input);
                yield return new WaitForSeconds(0.2f);
            }

            var view = View.Get<GameView>();
            DialogueController.Instance.SetNode("##MAT_PROC_001##");

            _machine_running = false;
        }
    }

    private void SpawnMaterial(CraftingMaterialType type)
    {
        var path = ""; // Removed
        var item = ItemController.Instance.CreateItem(path);
        SpawnOutput(item);
    }

    private void SpawnInputItem(ItemData data)
    {
        var item = ItemController.Instance.CreateItemFromData(data);
        SpawnOutput(item);
    }

    private void SpawnOutput(Item item)
    {
        item.GlobalPosition = OutputPosition.GlobalPosition;
        item.RigidBody.LinearVelocity = OutputPosition.GlobalBasis * Vector3.Forward * 3;

        SoundController.Instance.Play("sfx_pickup", OutputPosition.GlobalPosition);
    }

    private void FixMachine(Item item)
    {
        if (Data.Game.Flag_MaterialProcessorFixed) return;
        Data.Game.Flag_MaterialProcessorFixed = true;

        ItemController.Instance.UntrackItem(item);
        item.QueueFree();

        SoundController.Instance.Play("sfx_material_processor_fix", OutputPosition.GlobalPosition);
    }
    */
}
