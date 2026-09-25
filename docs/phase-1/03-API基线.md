# API 基线

所有公开接口使用 `/api/v1` 前缀。错误响应由 ASP.NET Core/ABP 统一处理，客户端不得依赖异常文本。

## 系统接口

### `GET /health`

用于容器、负载均衡和发布冒烟检查。成功返回 HTTP 200。

### `GET /api/v1/system/info`

返回服务名、API 版本和框架基线，不包含秘密或内部网络信息。

## 词典查询

### `GET /api/v1/dictionary/search`

查询参数：

| 参数 | 必填 | 说明 |
|---|---|---|
| `query` | 是 | 查询词或短语，1～512 字符 |
| `sourceLanguage` | 否 | BCP 47 语言标签，如 `zh-Hans`、`es`、`en` |
| `domainCode` | 否 | 领域代码 |
| `page` | 否 | 从 1 开始，默认 1 |
| `pageSize` | 否 | 默认 20，最大 100 |

每个结果对应一个独立概念义项，返回 `ConceptCode`、领域、`SenseOrder`、`UsageContext`、三语首选术语以及当前概念自己的定义。无上下文多义词查询可以返回多个结果，但不得把不同结果的定义合并。

## API 文档

开发环境 OpenAPI JSON 地址为 `/openapi/v1.json`。后续前端 SDK 和契约测试应从该文件生成或校验。
