# CSV 批量导入说明

## 1. 接口与处理原则

使用 `multipart/form-data` 调用 `POST /api/v1/admin/import-jobs/csv`，字段名为 `file`。文件最大 2 MB、最多 5000 个数据行，采用 UTF-8；解析支持双引号字段、字段内逗号、换行和 `""` 转义。

导入只创建草稿和版本 1 修订，不会提交、批准或发布。有效行可入库，错误行跳过；任务返回有效/无效数量，并为每个错误记录行号、字段、错误码和中文说明。

## 2. 模板字段

模板见 [术语导入模板-v1.csv](术语导入模板-v1.csv)。

| 字段 | 必填 | 说明 |
|---|---:|---|
| ConceptCode | 是 | 全局唯一概念编码 |
| DomainCode | 是 | 已存在的领域编码 |
| ZhTerm / EsTerm / EnTerm | 是 | 三语首选术语 |
| PartOfSpeech | 是 | 词性，如 `noun` |
| SenseOrder | 是 | 非负整数义项序号 |
| UsageContext | 否 | 使用场景，建议多义词必填 |
| DefinitionZh | 是 | 中文定义 |
| DefinitionEs / DefinitionEn | 否 | 西语、英语定义 |
| ScenarioLabel | 否 | 义项场景标签，建议多义词必填 |
| SourceTitle | 是 | 来源标题 |
| SourceUrl / SourceIdentifier | 至少一项 | 来源持久定位信息 |
| SourceLicense | 是 | 许可证或使用依据 |
| SourceLocator | 否 | 页码、章节、条目号等证据位置 |

## 3. 常见错误码

| 错误码 | 含义 |
|---|---|
| MissingHeader | 缺少模板必填列 |
| Required | 行内必填值为空 |
| InvalidNumber | SenseOrder 不是非负整数 |
| UnknownDomain | 领域编码不存在 |
| DuplicateConceptCode | 编码已存在或在当前文件重复 |
| MissingSourceIdentity | URL 和持久标识均为空 |

对于同形多义词，应按义项建立不同 `ConceptCode`，并分别填写 `SenseOrder`、`UsageContext`、`ScenarioLabel` 和定义，避免把金融、医学、法律等场景压成一个含混词条。
