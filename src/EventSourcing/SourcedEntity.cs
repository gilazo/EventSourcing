namespace EventSourcing;

public class SourcedEntity<TEntity>
    where TEntity : notnull
{
    private readonly Dictionary<Type, Func<DomainEvent, TEntity, TEntity>> _whenHandlers = new();

    public int CurrentVersion { get; private set; }

    public int NextVersion => CurrentVersion + 1;

    public SourcedEntity<TEntity> BuildFrom(IEnumerable<DomainEvent> stream, int streamVersion, TEntity entity)
    {
        foreach (var source in stream)
        {
            DispatchWhen(source.GetType().GetGenericArguments().First(), source, entity);
        }

        CurrentVersion = streamVersion;

        return this;
    }

    public SourcedEntity<TEntity> RegisterWhenHandler<TSource>(Func<DomainEvent<TSource>, TEntity, TEntity> whenHandler)
        where TSource : notnull
    {
        _whenHandlers.TryAdd(typeof(TSource), (domainEvent, entity) => whenHandler((DomainEvent<TSource>)domainEvent, entity));
        return this;
    }

    public SourcedEntity<TEntity> Apply<TSource>(DomainEvent<TSource> source, TEntity entity)
        where TSource : notnull
    {
        AppliedSources.Add(source);
        DispatchWhen<TSource>(source, entity);
        return this;
    }

    public List<DomainEvent> AppliedSources { get; } = new();

    private TEntity DispatchWhen<TSource>(DomainEvent source, TEntity entity)
        where TSource : notnull =>
        DispatchWhen(typeof(TSource), source, entity);

    private TEntity DispatchWhen(Type type, DomainEvent source, TEntity entity) =>
        _whenHandlers[type](source, entity);

    public SourcedEntity()
        : this(0) { }

    private SourcedEntity(int streamVersion) =>
        CurrentVersion = streamVersion;
}
