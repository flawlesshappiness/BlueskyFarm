using Godot;

public abstract partial class ResourceController<C, R> : SingletonController
    where C : ResourceCollection<R>
    where R : Resource
{
    private C _collection;
    public C Collection => GetCollection();
    protected C GetCollection() => _collection ?? (_collection = LoadCollection($"{Directory}/Resources/{typeof(C).Name}.tres"));
    private C LoadCollection(string path) => ResourceCollection<R>.Load<C>(path);
}
