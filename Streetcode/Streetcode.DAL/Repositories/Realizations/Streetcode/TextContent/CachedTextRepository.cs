using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;

namespace Streetcode.DAL.Repositories.Realizations.Streetcode.TextContent;

public class CachedTextRepository: ITextRepository
{
    private readonly ITextRepository _textRepository;
    private readonly IDistributedCache _distributedCache;
    
    public CachedTextRepository(StreetcodeDbContext streetcodeDbContext, IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
        _textRepository = new TextRepository(streetcodeDbContext);
    }
    
    public IQueryable<Text> FindAll(Expression<Func<Text, bool>>? predicate = default)
    {
        return _textRepository.FindAll(predicate);
    }

    public Text Create(Text entity)
    {
        return _textRepository.Create(entity);
    }

    public Task<Text> CreateAsync(Text entity)
    {
        return _textRepository.CreateAsync(entity);
    }

    public Task CreateRangeAsync(IEnumerable<Text> items)
    {
        return _textRepository.CreateRangeAsync(items);
    }

    public EntityEntry<Text> Update(Text entity)
    {
        return _textRepository.Update(entity);
    }

    public void UpdateRange(IEnumerable<Text> items)
    {
        _textRepository.UpdateRange(items);
    }

    public void Delete(Text entity)
    {
        _textRepository.Delete(entity);
    }

    public void DeleteRange(IEnumerable<Text> items)
    {
        _textRepository.DeleteRange(items);
    }

    public void Attach(Text entity)
    {
        _textRepository.Attach(entity);
    }

    public void Detach(Text entity)
    {
        _textRepository.Detach(entity);
    }

    public EntityEntry<Text> Entry(Text entity)
    {
        return _textRepository.Entry(entity);
    }

    public Task ExecuteSqlRaw(string query)
    {
        return _textRepository.ExecuteSqlRaw(query);
    }

    public IQueryable<Text> Include(params Expression<Func<Text, object>>[] includes)
    {
        return _textRepository.Include(includes);
    }

    public async Task<IEnumerable<Text>> GetAllAsync(
        Expression<Func<Text, bool>>? predicate = default, 
        Func<IQueryable<Text>, IIncludableQueryable<Text, object>>? include = default)
    {
        const string key = "text-all";

        string? cachedText = await _distributedCache.GetStringAsync(key);

        List<Text> allTexts;
        if (string.IsNullOrEmpty(cachedText))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Cache miss");
            
            allTexts = (await _textRepository.GetAllAsync(predicate, include)).ToList();

            if (!allTexts.Any())
            {
                return allTexts;
            }

            string serializedTexts = JsonConvert.SerializeObject(allTexts);
            await _distributedCache.SetStringAsync(key, serializedTexts);

            return allTexts;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Cache hit");

        allTexts = JsonConvert.DeserializeObject<List<Text>>(cachedText) 
                   ?? throw new Exception("Error while deserializing Texts from redis json");
        return allTexts;
    }

    public async Task<IEnumerable<Text>?> GetAllAsync(
        Expression<Func<Text, Text>> selector, 
        Expression<Func<Text, bool>>? predicate = default, 
        Func<IQueryable<Text>, IIncludableQueryable<Text, object>>? include = default)
    {
        return await _textRepository.GetAllAsync(selector, predicate);
    }

    public Task<Text?> GetSingleOrDefaultAsync(
        Expression<Func<Text, bool>>? predicate = default, 
        Func<IQueryable<Text>, IIncludableQueryable<Text, object>>? include = default)
    {
        return _textRepository.GetFirstOrDefaultAsync(predicate, include);
    }

    public Task<Text?> GetFirstOrDefaultAsync(
        Expression<Func<Text, bool>>? predicate = default, 
        Func<IQueryable<Text>, IIncludableQueryable<Text, object>>? include = default)
    {
        return _textRepository.GetFirstOrDefaultAsync(predicate, include);
    }

    public Task<Text?> GetFirstOrDefaultAsync(
        Expression<Func<Text, Text>> selector, 
        Expression<Func<Text, bool>>? predicate = default, 
        Func<IQueryable<Text>, IIncludableQueryable<Text, object>>? include = default)
    {
        return _textRepository.GetFirstOrDefaultAsync(selector, predicate, include);
    }
}