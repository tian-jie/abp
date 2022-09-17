﻿using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;

namespace Volo.Abp.Domain.Entities.Caching;

public abstract class EntityCacheBase<TEntity, TEntityCacheItem, TKey> :
    IEntityCache<TEntityCacheItem, TKey>,
    ILocalEventHandler<EntityChangedEventData<TEntity>>
    where TEntity : Entity<TKey>
    where TEntityCacheItem : class
{
    protected IReadOnlyRepository<TEntity, TKey> Repository { get; }
    protected IDistributedCache<TEntityCacheItem, TKey> Cache { get; }

    protected EntityCacheBase(
        IReadOnlyRepository<TEntity, TKey> repository,
        IDistributedCache<TEntityCacheItem, TKey> cache)
    {
        Repository = repository;
        Cache = cache;
    }
    
    public virtual async Task<TEntityCacheItem> FindAsync(TKey id)
    {
        return await Cache.GetOrAddAsync(
            id,
            async () => MapToCacheItem(await Repository.FindAsync(id))
        );
    }

    public virtual async Task<TEntityCacheItem> GetAsync(TKey id)
    {
        return await Cache.GetOrAddAsync(
            id,
            async () => MapToCacheItem(await Repository.GetAsync(id))
        );
    }

    protected abstract TEntityCacheItem MapToCacheItem(TEntity entity);
    
    public async Task HandleEventAsync(EntityChangedEventData<TEntity> eventData)
    {
        if (eventData is EntityCreatedEventData<TEntity>)
        {
            return;
        }
        
        await Cache.RemoveAsync(eventData.Entity.Id);
    }
}