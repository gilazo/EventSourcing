namespace EventSourcing;

public class SourcedEntity<TEntity>
    where TEntity : notnull
{
    private readonly Dictionary<Type, Func<object, TEntity, TEntity>> _whenHandlers = new();

    public int CurrentVersion { get; private set; }

    public int NextVersion => CurrentVersion + 1;

    public SourcedEntity<TEntity> BuildFrom<TSource>(IEnumerable<TSource> stream, int streamVersion, TEntity entity)
        where TSource : notnull
    {
        foreach (var source in stream)
        {
            DispatchWhen(source.GetType(), source, entity);
        }

        CurrentVersion = streamVersion;

        return this;
    }

    public SourcedEntity<TEntity> RegisterWhenHandler<TSource>(Func<TSource, TEntity, TEntity> whenHandler)
        where TSource : notnull
    {
        _whenHandlers.TryAdd(typeof(TSource), (domainEvent, entity) => whenHandler((TSource)domainEvent, entity));
        return this;
    }

    public SourcedEntity<TEntity> Apply<TSource>(TSource source, TEntity entity)
        where TSource : notnull
    {
        AppliedSources.Add(source);
        DispatchWhen<TSource>(source, entity);
        return this;
    }

    public List<object> AppliedSources { get; } = new();

    private TEntity DispatchWhen<TSource>(TSource source, TEntity entity)
        where TSource : notnull =>
        DispatchWhen(typeof(TSource), source, entity);

    private TEntity DispatchWhen(Type type, object source, TEntity entity) =>
        _whenHandlers[type](source, entity);

    public SourcedEntity()
        : this(0) { }

    private SourcedEntity(int streamVersion) =>
        CurrentVersion = streamVersion;
}
