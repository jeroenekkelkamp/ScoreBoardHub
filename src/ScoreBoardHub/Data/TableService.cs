using Azure;
using Azure.Data.Tables;
using ScoreBoardHub.Models;

namespace ScoreBoardHub.Services;

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
        var tableEntity = new TableEntity(scoreBoardName, entry.PlayerName)
        {
            { "PlayerName", entry.PlayerName },
            { "Score", entry.Score }
        };
        tableClient.AddEntity(tableEntity);
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
            // Attempt to create the table
            serviceClient.Create();
        }
        catch (RequestFailedException ex)
        {
            // Ignore if the table already exists
            if (ex.Status != 409)
            {
                throw;
            }
        }

        return serviceClient;
    }
}