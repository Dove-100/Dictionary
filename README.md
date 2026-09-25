# TriDict 汉西英三语在线词典

本仓库正在实现概念导向的汉语—西班牙语—英语专业术语词典。后端采用 .NET 10、ABP Framework 10.6、Entity Framework Core 与 PostgreSQL；Web 和 UniApp 客户端将在后续阶段接入同一组版本化 API。

## 当前状态

第 1 阶段技术底座已建立：模块化解决方案、术语核心模型、首个数据库迁移、JWT/OIDC 验证边界、查询 API、健康检查、OpenAPI 文档、Docker 开发环境和 CI。

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
- OpenAPI：`GET http://localhost:8080/openapi/v1.json`

数据库迁移由 `TriDict.DbMigrator` 独立执行，API 不会在启动时自动修改数据库结构。

需要逐项了解新增文件用途时，请阅读[代码与文件职责说明](docs/phase-1/05-代码与文件职责说明.md)。
