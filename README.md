# TriDict 汉西英三语在线词典

本仓库正在实现概念导向的汉语—西班牙语—英语专业术语词典。后端采用 .NET 10、ABP Framework 10.6、Entity Framework Core 与 PostgreSQL；Web 和 UniApp 客户端将在后续阶段接入同一组版本化 API。

## 当前状态

第 2 阶段术语管理闭环已建立：术语草稿编辑、版本化审核发布、来源证据管理、CSV 批量导入与逐行错误报告，以及可运行的 Web 管理端页面。

## 本地开发

仓库固定 SDK 为 10.0.401。若系统未安装，可使用项目内 SDK：

```powershell
$dotnetExe = Join-Path $PWD '.tools/dotnet/dotnet.exe'
& $dotnetExe restore TriDict.sln
& $dotnetExe build TriDict.sln --configuration Release
& $dotnetExe test TriDict.sln --configuration Release --no-build
```

启动依赖与 API：

```powershell
Copy-Item .env.example .env
docker compose -f deploy/docker/compose.yml up --build
```

服务入口：

- 健康检查：`GET http://localhost:8080/health`
- 系统信息：`GET http://localhost:8080/api/v1/system/info`
- 词典查询：`GET http://localhost:8080/api/v1/dictionary/search?query=modelo&sourceLanguage=es`
- 术语草稿：`POST http://localhost:8080/api/v1/admin/concepts`
- 修订审核：`POST http://localhost:8080/api/v1/admin/revisions/{id}/approve`
- 来源管理：`GET http://localhost:8080/api/v1/admin/sources`
- CSV 导入：`POST http://localhost:8080/api/v1/admin/import-jobs/csv`
- OpenAPI：`GET http://localhost:8080/openapi/v1.json`
- 管理端：`GET http://localhost:8080/admin/`

数据库迁移由 `TriDict.DbMigrator` 独立执行，API 不会在启动时自动修改数据库结构。

管理端通过当前页面内存中的 Bearer 访问令牌调用 API；令牌由项目配置的 OIDC 身份服务提供。页面操作见[管理端页面使用说明](docs/phase-2/05-管理端页面使用说明.md)，新增文件职责见[代码与文件职责说明](docs/phase-2/03-代码与文件职责说明.md)。
