using MessageProcessingService.Configuration;
using MessageProcessingService.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MessageProcessingService.Data;
public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    public MongoDbContext(IOptions<MongoDbConfig> options)
    {
        var config = options.Value;
        var client = new MongoClient(config.ConnectionString);

        _database = client.GetDatabase(config.DatabaseName);
    }

    public IMongoCollection<ServerStatistics> ServerStatistics =>
        _database.GetCollection<ServerStatistics>("ServerStatistics");
}