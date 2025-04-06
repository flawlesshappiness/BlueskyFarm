public partial class DialogueCharacterController : ResourceController<DialogueCharacterCollection, DialogueCharacterInfo>
{
    public override string Directory => $"Dialogue";
    protected override string CustomResourceCollectionDirectory => "DialogueCharacter";
    public static DialogueCharacterController Instance => Singleton.Get<DialogueCharacterController>();
}
