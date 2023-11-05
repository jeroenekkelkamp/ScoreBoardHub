using Azure;
using Azure.Data.Tables;
using ScoreBoardHub.Models;

namespace ScoreBoardHub.Data;

public class TableService
{
    private readonly IConfiguration configuration;
    
    public TableService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }
    
    public async Task<Pageable<TableEntity>> GetScoreBoards()
    {
        var tableClient = GetTableClient();
        var scoreboards = tableClient.Query<TableEntity>(filter: "PartitionKey eq 'Scoreboard'");
        return scoreboards;
    }

    public void AddScoreBoard(string scoreboardName)
    {
        var tableClient = GetTableClient();
        var tableEntity = new TableEntity("Scoreboard", scoreboardName);
        tableClient.AddEntity(tableEntity);
    }

    public void AddScoreBoardEntry(ScoreBoardEntry entry, string scoreBoardName)
    {
        var tableClient = GetTableClient();
        var tableEntity = new TableEntity(scoreBoardName, Guid.NewGuid().ToString())
        {
            { "PlayerName", entry.PlayerName },
            { "Score", entry.Score }
        };
        tableClient.AddEntity(tableEntity);
    }
    
    public void UpdateScoreBoardEntry(ScoreBoardEntry updatedEntry, string scoreBoardName)
    {
        var tableClient = GetTableClient();
    
        var entity = new TableEntity(scoreBoardName, updatedEntry.RowKey)
        {
            { "PlayerName", updatedEntry.PlayerName },
            { "Score", updatedEntry.Score }
        };

        tableClient.UpsertEntity(entity);
    }
    
    public async Task<Pageable<TableEntity>> GetScoreBoardEntries(string scoreBoard)
    {
        var tableClient = GetTableClient();
        var scoreboardEntries = tableClient.Query<TableEntity>(filter: $"PartitionKey eq '{scoreBoard}'");
        return scoreboardEntries;
    }
    
    private TableClient GetTableClient()
    {
        var storageUri = configuration.GetSection("AzureStorage:StorageUri").Value;
        var accountName = configuration.GetSection("AzureStorage:AccountName").Value;
        var storageAccountKey = configuration.GetSection("AzureStorage:StorageAccountKey").Value;
        var baseTable = configuration.GetSection("AzureStorage:ScoreboardTable").Value;
        
        var serviceClient = new TableClient(
            new Uri(storageUri),
            baseTable,
            new TableSharedKeyCredential(accountName, storageAccountKey)
        );

        try
        {
            serviceClient.Create();
        }
        catch (RequestFailedException ex)
        {
            if (ex.Status != 409)
            {
                throw;
            }
        }

        return serviceClient;
    }
}