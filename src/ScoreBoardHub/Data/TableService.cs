using Azure;
using Azure.Data.Tables;
using ScoreBoardHub.Models;
using ScoreBoardHub.Pages;

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

    public void AddScoreBoard(string scoreboardName, string subtitle, string imageUrl)
    {
        var tableClient = GetTableClient();
        var tableEntity = new TableEntity("Scoreboard", scoreboardName)
        {
            { "Subtitle", subtitle },
            { "ImageUrl", imageUrl }
        };
        tableClient.AddEntity(tableEntity);
    }

    public void AddScoreBoardEntry(ScoreBoardEntry entry, string scoreBoardName)
    {
        var existingRecord = GetScoreBoardEntryByPlayerName(entry.PlayerName, scoreBoardName);
        if (existingRecord != null)
        {
            entry.RowKey = existingRecord.RowKey;
            UpdateScoreBoardEntry(entry, scoreBoardName);
            return;
        }
        
        var tableClient = GetTableClient();
        var tableEntity = new TableEntity(scoreBoardName, Guid.NewGuid().ToString())
        {
            { "PlayerName", entry.PlayerName },
            { "Score", entry.Score < 0 ? 0 : entry.Score }
        };
        tableClient.AddEntity(tableEntity);
    }
    
    public void UpdateScoreBoardEntry(ScoreBoardEntry updatedEntry, string scoreBoardName)
    {
        var tableClient = GetTableClient();
    
        var entity = new TableEntity(scoreBoardName, updatedEntry.RowKey)
        {
            { "PlayerName", updatedEntry.PlayerName },
            { "Score", updatedEntry.Score < 0 ? 0 : updatedEntry.Score }
        };

        tableClient.UpsertEntity(entity);
    }
    
    public async Task<Pageable<TableEntity>> GetScoreBoardEntries(string scoreBoard)
    {
        var tableClient = GetTableClient();
        var scoreboardEntries = tableClient.Query<TableEntity>(filter: $"PartitionKey eq '{scoreBoard}'");
        return scoreboardEntries;
    }
        
    public async Task<TableEntity> GetScoreBoardByName(string scoreBoard)
    {
        var tableClient = GetTableClient();
        var scoreboard = tableClient.Query<TableEntity>(filter: $"PartitionKey eq 'Scoreboard' and RowKey eq '{scoreBoard}'");
        return scoreboard.First();
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

    public void RemoveScoreBoardEntry(string rowKey, string scoreBoardName)
    {
        var scoreBoardEntry = GetScoreBoardEntryByRowKey(rowKey, scoreBoardName);
        if (scoreBoardEntry is null) return;
        
        var tableClient = GetTableClient();
        tableClient.DeleteEntity(scoreBoardEntry.PartitionKey, scoreBoardEntry.RowKey);
        
    }
    
    private TableEntity? GetScoreBoardEntryByPlayerName(string playerName, string scoreBoardName)
    {
        var tableClient = GetTableClient();
        var scoreboardEntries = tableClient.Query<TableEntity>(filter: $"PartitionKey eq '{scoreBoardName}' and PlayerName eq '{playerName}'");
        return scoreboardEntries.FirstOrDefault();
    }
    
    private TableEntity GetScoreBoardEntryByRowKey(string rowKey, string scoreBoardName)
    {
        var tableClient = GetTableClient();
        var scoreboardEntries = tableClient.Query<TableEntity>(filter: $"PartitionKey eq '{scoreBoardName}' and RowKey eq '{rowKey}'");
        return scoreboardEntries.FirstOrDefault();
    }
}