using System.Text;
using Microsoft.AspNetCore.Authorization;
using TriDict.Permissions;
using TriDict.Terminology;
using TriDict.Workflow;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace TriDict.Importing;

public sealed class ImportAppService(
    IRepository<ImportJob, Guid> importJobRepository,
    IRepository<DomainCategory, Guid> domainRepository,
    IRepository<Source, Guid> sourceRepository,
    IConceptSearchRepository conceptRepository,
    IRepository<ConceptRevision, Guid> revisionRepository,
    IGuidGenerator guidGenerator)
    : ApplicationService, IImportAppService
{
    public const string TemplateVersion = "1.0";
    public const int MaxRows = 5000;
    public const int MaxBytes = 2 * 1024 * 1024;

    private static readonly string[] RequiredHeaders =
    [
        "ConceptCode", "DomainCode", "ZhTerm", "EsTerm", "EnTerm", "PartOfSpeech",
        "SenseOrder", "DefinitionZh", "SourceTitle", "SourceLicense"
    ];

    [Authorize(TriDictPermissions.ImportsExecute)]
    public async Task<ImportJobDto> ImportCsvAsync(CreateCsvImportInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var actorId = CurrentUser.Id ?? throw new BusinessException("TriDict:AuthenticatedUserRequired");
        var job = new ImportJob(guidGenerator.Create(), input.FileName, TemplateVersion, actorId);
        job.StartValidation();
        await importJobRepository.InsertAsync(job, autoSave: false, cancellationToken);

        if (Encoding.UTF8.GetByteCount(input.CsvContent) > MaxBytes)
        {
            job.Fail("CSV 文件超过 2 MB 限制。");
            await importJobRepository.UpdateAsync(job, autoSave: true, cancellationToken);
            return Map(job);
        }

        IReadOnlyList<CsvRecord> records;
        try
        {
            records = CsvTableParser.Parse(input.CsvContent.TrimStart('\uFEFF'));
        }
        catch (FormatException exception)
        {
            job.Fail(exception.Message);
            await importJobRepository.UpdateAsync(job, autoSave: true, cancellationToken);
            return Map(job);
        }

        if (records.Count == 0)
        {
            job.Fail("CSV 文件为空。");
            await importJobRepository.UpdateAsync(job, autoSave: true, cancellationToken);
            return Map(job);
        }

        var headers = records[0].Values.Select((name, index) => (name, index))
            .ToDictionary(x => x.name, x => x.index, StringComparer.OrdinalIgnoreCase);
        foreach (var required in RequiredHeaders.Where(x => !headers.ContainsKey(x)))
        {
            job.AddError(guidGenerator.Create(), 1, required, "MissingHeader", $"缺少必填列 {required}。");
        }
        if (job.Errors.Count > 0)
        {
            job.Complete(0, 0);
            await importJobRepository.UpdateAsync(job, autoSave: true, cancellationToken);
            return Map(job);
        }

        var dataRecords = records.Skip(1).ToList();
        if (dataRecords.Count > MaxRows)
        {
            job.Fail($"数据行数超过 {MaxRows} 行限制。");
            await importJobRepository.UpdateAsync(job, autoSave: true, cancellationToken);
            return Map(job);
        }

        var domains = (await domainRepository.GetListAsync(includeDetails: false, cancellationToken))
            .ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);
        var sources = await sourceRepository.GetListAsync(includeDetails: false, cancellationToken);
        var sourcesByKey = sources
            .Where(x => !string.IsNullOrWhiteSpace(x.Url) || !string.IsNullOrWhiteSpace(x.Identifier))
            .GroupBy(SourceKey)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
        var conceptQuery = await conceptRepository.GetQueryableAsync();
        var existingCodes = (await AsyncExecuter.ToListAsync(conceptQuery.Select(x => x.ConceptCode), cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var fileCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var validRows = 0;

        foreach (var record in dataRecords)
        {
            var row = new CsvRow(record, headers);
            var errorCount = job.Errors.Count;
            var conceptCode = row.Required("ConceptCode", job, guidGenerator);
            var domainCode = row.Required("DomainCode", job, guidGenerator);
            var zhTerm = row.Required("ZhTerm", job, guidGenerator);
            var esTerm = row.Required("EsTerm", job, guidGenerator);
            var enTerm = row.Required("EnTerm", job, guidGenerator);
            var partOfSpeech = row.Required("PartOfSpeech", job, guidGenerator);
            var definitionZh = row.Required("DefinitionZh", job, guidGenerator);
            var sourceTitle = row.Required("SourceTitle", job, guidGenerator);
            var sourceLicense = row.Required("SourceLicense", job, guidGenerator);
            var sourceUrl = row.Optional("SourceUrl");
            var sourceIdentifier = row.Optional("SourceIdentifier");

            if (!int.TryParse(row.Required("SenseOrder", job, guidGenerator), out var senseOrder) || senseOrder < 0)
                row.Error(job, guidGenerator, "SenseOrder", "InvalidNumber", "SenseOrder 必须为非负整数。");
            if (!domains.TryGetValue(domainCode, out var domain))
                row.Error(job, guidGenerator, "DomainCode", "UnknownDomain", "领域编码不存在。");
            var normalizedCode = conceptCode.Trim().ToUpperInvariant();
            if (existingCodes.Contains(normalizedCode) || !fileCodes.Add(normalizedCode))
                row.Error(job, guidGenerator, "ConceptCode", "DuplicateConceptCode", "概念编码已存在或在文件中重复。");
            if (string.IsNullOrWhiteSpace(sourceUrl) && string.IsNullOrWhiteSpace(sourceIdentifier))
                row.Error(job, guidGenerator, "SourceUrl", "MissingSourceIdentity", "SourceUrl 与 SourceIdentifier 至少填写一个。");

            if (job.Errors.Count != errorCount) continue;

            var sourceKey = SourceKey(sourceUrl, sourceIdentifier);
            if (!sourcesByKey.TryGetValue(sourceKey, out var source))
            {
                source = new Source(guidGenerator.Create(), sourceTitle, sourceLicense, url: sourceUrl, identifier: sourceIdentifier, accessedAt: DateTime.UtcNow);
                sourcesByKey[sourceKey] = source;
                await sourceRepository.InsertAsync(source, autoSave: false, cancellationToken);
            }

            var concept = new Concept(guidGenerator.Create(), normalizedCode, domain!.Id);
            concept.AddTerm(guidGenerator.Create(), "zh-Hans", zhTerm, TermType.Preferred, partOfSpeech, true, senseOrder, row.Optional("UsageContext"));
            concept.AddTerm(guidGenerator.Create(), "es", esTerm, TermType.Preferred, partOfSpeech, true, senseOrder, row.Optional("UsageContext"));
            concept.AddTerm(guidGenerator.Create(), "en", enTerm, TermType.Preferred, partOfSpeech, true, senseOrder, row.Optional("UsageContext"));
            concept.AddDefinition(guidGenerator.Create(), "zh-Hans", definitionZh, row.Optional("ScenarioLabel"), source.Id);
            AddOptionalDefinition(concept, guidGenerator, "es", row.Optional("DefinitionEs"), row.Optional("ScenarioLabel"), source.Id);
            AddOptionalDefinition(concept, guidGenerator, "en", row.Optional("DefinitionEn"), row.Optional("ScenarioLabel"), source.Id);
            concept.AddSource(guidGenerator.Create(), source.Id, EvidenceType.General, row.Optional("SourceLocator"));
            var revision = new ConceptRevision(guidGenerator.Create(), concept.Id, 1, ConceptRevisionSnapshot.FromConcept(concept), "CSV 批量导入草稿", actorId);
            await conceptRepository.InsertAsync(concept, autoSave: false, cancellationToken);
            await revisionRepository.InsertAsync(revision, autoSave: false, cancellationToken);
            existingCodes.Add(normalizedCode);
            validRows++;
        }

        job.Complete(dataRecords.Count, validRows);
        await importJobRepository.UpdateAsync(job, autoSave: true, cancellationToken);
        return Map(job);
    }

    [Authorize(TriDictPermissions.ImportsView)]
    public async Task<ImportJobDto> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Map(await importJobRepository.GetAsync(id, includeDetails: true, cancellationToken));

    private static void AddOptionalDefinition(Concept concept, IGuidGenerator generator, string language, string? text, string? scenario, Guid sourceId)
    {
        if (!string.IsNullOrWhiteSpace(text)) concept.AddDefinition(generator.Create(), language, text, scenario, sourceId);
    }

    private static string SourceKey(Source source) => SourceKey(source.Url, source.Identifier);
    private static string SourceKey(string? url, string? identifier) =>
        !string.IsNullOrWhiteSpace(identifier) ? "id:" + identifier.Trim() : "url:" + url!.Trim();

    private static ImportJobDto Map(ImportJob job) => new()
    {
        Id = job.Id,
        FileName = job.FileName,
        TemplateVersion = job.TemplateVersion,
        Status = job.Status,
        TotalRows = job.TotalRows,
        ValidRows = job.ValidRows,
        InvalidRows = job.InvalidRows,
        FailureReason = job.FailureReason,
        Errors = job.Errors.OrderBy(x => x.RowNumber).Select(x => new ImportRowErrorDto
        {
            RowNumber = x.RowNumber, Field = x.Field, Code = x.Code, Message = x.Message,
        }).ToList(),
    };

    private sealed class CsvRow(CsvRecord record, IReadOnlyDictionary<string, int> headers)
    {
        public string Required(string field, ImportJob job, IGuidGenerator generator)
        {
            var value = Optional(field) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(value)) Error(job, generator, field, "Required", $"{field} 为必填项。");
            return value;
        }

        public string? Optional(string field)
        {
            if (!headers.TryGetValue(field, out var index) || index >= record.Values.Count) return null;
            return string.IsNullOrWhiteSpace(record.Values[index]) ? null : record.Values[index];
        }

        public void Error(ImportJob job, IGuidGenerator generator, string field, string code, string message) =>
            job.AddError(generator.Create(), record.RowNumber, field, code, message);
    }
}
