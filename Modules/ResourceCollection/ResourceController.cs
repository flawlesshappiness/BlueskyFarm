using Godot;

public abstract partial class ResourceController<C, R> : SingletonController
    where C : ResourceCollection<R>
    where R : Resource
{
    private C _collection;
    protected C GetCollection(string path) => _collection ?? (_collection = LoadCollection(path));
    private C LoadCollection(string path) => ResourceCollection<R>.Load<C>(path);
}
