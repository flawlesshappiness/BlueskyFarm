public partial class BasementRoomController : ResourceController<BasementRoomCollection, BasementRoomInfo>
{
    public override string Directory => "Basement/Room";
    public static BasementRoomController Instance => Singleton.Get<BasementRoomController>();
}
