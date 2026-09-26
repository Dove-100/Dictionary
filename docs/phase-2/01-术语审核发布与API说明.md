# 术语审核发布与 API 说明

## 1. 版本规则

公开词条与编辑版本分离：`Concept` 保存当前公开版本，`ConceptRevision` 保存待处理的完整快照。首次录入时概念版本为 0；发布修订 1 后才成为公开数据。已发布词条再次修改时创建版本 2，公开查询在版本 2 发布前仍读取版本 1。

快照包含领域、可靠性、三语术语、定义/场景和来源关联。多义词应使用不同概念编码，或至少在定义中保留不同 `ScenarioLabel`；系统不会在审核或发布时自动合并义项。

## 2. 状态机

```text
Draft --提交--> InReview --批准--> Approved --发布--> Published
                         \\--退回--> Rejected --修改--> Draft
Published --创建修订--> 新的 Draft（旧版本继续公开）
```

约束：

- 提交前必须有 `zh-Hans`、`es`、`en` 三个首选术语、至少一个定义和至少一个有效来源。
- 提交人与审核人不能是同一用户。
- 退回必须说明原因。
- 所有修改和状态动作都携带最新 `revisionToken`；旧令牌触发 `TriDict:RevisionConflict`，HTTP 层应映射为冲突响应。
- 发布会写入审核记录和 `Terminology.ConceptPublished` Outbox 事件。

## 3. 主要 API

| 方法与地址 | 权限 | 作用 |
|---|---|---|
| `GET /api/v1/admin/concepts/{id}` | `TriDict.Concepts` | 查看概念和最新修订 |
| `GET /api/v1/admin/concepts` | `TriDict.Concepts` | 分页查看概念，可按编码和最新修订状态筛选 |
| `GET /api/v1/admin/concepts/domains` | `TriDict.Concepts` | 获取领域选项 |
| `POST /api/v1/admin/concepts` | `TriDict.Concepts.Create` | 创建概念和版本 1 草稿 |
| `POST /api/v1/admin/concepts/{id}/revisions` | `TriDict.Concepts.Edit` | 为已发布概念创建下一版本 |
| `PUT /api/v1/admin/revisions/{id}` | `TriDict.Concepts.Edit` | 修改草稿完整快照 |
| `POST /api/v1/admin/revisions/{id}/submit` | `TriDict.Concepts.Edit` | 提交审核 |
| `POST /api/v1/admin/revisions/{id}/approve` | `TriDict.Concepts.Review` | 批准，禁止自审 |
| `POST /api/v1/admin/revisions/{id}/reject` | `TriDict.Concepts.Review` | 退回并记录原因 |
| `POST /api/v1/admin/revisions/{id}/publish` | `TriDict.Concepts.Publish` | 发布并替换公开投影 |
| `GET/POST/PUT /api/v1/admin/sources` | `TriDict.Sources*` | 查询和维护来源 |
| `PUT /api/v1/admin/sources/{id}/active` | `TriDict.Sources.Manage` | 启用/停用来源 |
| `GET /api/v1/admin/import-jobs` | `TriDict.Imports.View` | 分页查看导入任务 |

## 4. 角色建议

| 角色 | 建议权限 |
|---|---|
| Contributor | Concepts、Create、Edit、Sources |
| Editor | Contributor 权限及 Sources.Manage |
| Reviewer | Concepts、Review、Sources |
| Publisher | Concepts、Publish、Sources |
| DataAdmin | Imports、Execute、View、Concepts、Create |

角色到权限的具体分配应在统一身份中心或 ABP 权限管理模块中配置，不在代码中硬编码用户。
