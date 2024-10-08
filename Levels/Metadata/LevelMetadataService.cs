﻿using Newtonsoft.Json;
using TNRD.Zeepkist.GTR.Backend.Zeeplevel.Resources;
using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Metadata;

public interface ILevelMetadataService
{
    bool Exists(string hash);
    void Create(ZeepLevel zeepLevel, string hash);
}

public class LevelMetadataService : ILevelMetadataService
{
    private static readonly int[] CheckpointIds = [22, 372, 373, 1275, 1276, 1277, 1278, 1279, 1615];
    private static readonly int[] FinishIds = [2, 1273, 1274, 1412, 1616];

    private static readonly int[] AlternateCheckpointIds =
    [
        1607, 1608, 1609, 1610, 1611, 1612, 1613, 1314, 1978, 1979, 1980, 1981, 1982, 1983, 1984, 1985, 1986, 1987,
        1978, 1988, 1989, 1990, 1991, 1992, 1993
    ];

    private readonly ILevelMetadataRepository _repository;
    private readonly ILevelService _levelService;
    private readonly ILogger<LevelMetadataService> _logger;

    public LevelMetadataService(
        ILevelMetadataRepository repository,
        ILevelService levelService,
        ILogger<LevelMetadataService> logger)
    {
        _repository = repository;
        _levelService = levelService;
        _logger = logger;
    }

    public bool Exists(string hash)
    {
        if (!_levelService.TryGetByHash(hash, out Level? level))
        {
            return false;
        }

        return _repository.ExistsForLevel(level.Id);
    }

    public void Create(ZeepLevel zeepLevel, string hash)
    {
        if (!_levelService.TryGetByHash(hash, out Level? level))
        {
            _logger.LogWarning("Unable to create metadata for level because there is no level with hash {Hash}", hash);
            return;
        }

        if (_repository.ExistsForLevel(level.Id))
        {
            return;
        }

        LevelMetadata metadata = new()
        {
            IdLevel = level.Id,
            AmountBlocks = zeepLevel.Blocks.Count,
            TypeGround = zeepLevel.Ground,
            TypeSkybox = zeepLevel.Skybox,
            AmountCheckpoints = CountCheckpoints(zeepLevel),
            AmountFinishes = CountFinishes(zeepLevel),
            Blocks = JsonConvert.SerializeObject(zeepLevel.Blocks),
        };

        _repository.Insert(metadata);
    }

    private static int CountCheckpoints(ZeepLevel zeepLevel)
    {
        return zeepLevel.Blocks.Count(x => CheckpointIds.Contains(x.Id)) +
               zeepLevel.Blocks.Where(x => AlternateCheckpointIds.Contains(x.Id)).Count(x => x.Options[5] >= 0.5f);
    }

    private static int CountFinishes(ZeepLevel zeepLevel)
    {
        return zeepLevel.Blocks.Count(x => FinishIds.Contains(x.Id));
    }
}
