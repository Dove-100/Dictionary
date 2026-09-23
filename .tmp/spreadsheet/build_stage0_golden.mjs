import fs from "node:fs/promises";
import { SpreadsheetFile, Workbook } from "@oai/artifact-tool";

const outputDir = "E:/西班牙语学习词典/outputs/01a0cc1b-5b38-7271-a04e-ec9746728387";
const previewDir = "E:/西班牙语学习词典/.tmp/spreadsheet/previews";
await fs.mkdir(outputDir, { recursive: true });
await fs.mkdir(previewDir, { recursive: true });

const concepts = [
  ["ECON-000001","ECON.01","关税","arancel","tariff","noun","",""],
  ["ECON-000002","ECON.05","供应链","cadena de suministro","supply chain","noun","供应网络",""],
  ["ECON-000003","ECON.06","电子商务","comercio electrónico","e-commerce","noun","电商","electronic commerce"],
  ["ECON-000004","ECON.08","通货膨胀","inflación","inflation","noun","通胀",""],
  ["ECON-000005","ECON.08","国内生产总值","producto interno bruto","gross domestic product","noun","GDP","PIB"],
  ["ECON-000006","ECON.01","信用证","carta de crédito","letter of credit","noun","L/C","crédito documentario"],
  ["ECON-000007","ECON.01","外国直接投资","inversión extranjera directa","foreign direct investment","noun","FDI","IED"],
  ["ECON-000008","ECON.02","汇率","tipo de cambio","exchange rate","noun","外汇汇率",""],
  ["IT-000001","IT.01","人工智能","inteligencia artificial","artificial intelligence","noun","AI","IA"],
  ["IT-000002","IT.01","机器学习","aprendizaje automático","machine learning","noun","ML","aprendizaje de máquina"],
  ["IT-000003","IT.01","自然语言处理","procesamiento del lenguaje natural","natural language processing","noun","NLP","PLN"],
  ["IT-000004","IT.03","数据库","base de datos","database","noun","DB",""],
  ["IT-000005","IT.02","应用程序编程接口","interfaz de programación de aplicaciones","application programming interface","noun","API",""],
  ["IT-000006","IT.05","网络安全","ciberseguridad","cybersecurity","noun","信息安全","cyber security"],
  ["IT-000007","IT.04","云计算","computación en la nube","cloud computing","noun","云端计算",""],
  ["IT-000008","IT.02","源代码","código fuente","source code","noun","源码",""],
  ["RES-000001","RES.01","假设","hipótesis","hypothesis","noun","研究假设",""],
  ["RES-000002","RES.08","同行评议","revisión por pares","peer review","noun","同行审议",""],
  ["RES-000003","RES.02","样本","muestra","sample","noun","统计样本",""],
  ["RES-000004","RES.02","回归分析","análisis de regresión","regression analysis","noun","",""],
  ["RES-000005","RES.08","开放获取","acceso abierto","open access","noun","OA",""],
  ["RES-000006","RES.08","科研伦理","ética de la investigación","research ethics","noun","研究伦理",""],
  ["RES-000007","RES.08","引文","cita","citation","noun","引用文献",""],
  ["RES-000008","RES.01","可重复性","reproducibilidad","reproducibility","noun","",""],
  ["CUL-000001","CUL.01","翻译","traducción","translation","noun","笔译",""],
  ["CUL-000002","CUL.01","本地化","localización","localization","noun","L10n","localisation"],
  ["CUL-000003","CUL.03","文化遗产","patrimonio cultural","cultural heritage","noun","",""],
  ["CUL-000004","CUL.05","跨文化交际","comunicación intercultural","intercultural communication","noun","跨文化沟通",""],
  ["CUL-000005","CUL.04","字幕","subtítulo","subtitle","noun","字幕翻译","subtitling"],
  ["CUL-000006","CUL.01","语料库","corpus","corpus","noun","语料数据库",""],
  ["CUL-000007","CUL.01","口译","interpretación","interpreting","noun","口头翻译","interpretation"],
  ["CUL-000008","CUL.08","创意产业","industrias creativas","creative industries","noun","文化创意产业","creative economy"]
];

const domainNames = {
  ECON: "经贸", IT: "信息技术", RES: "科研教育", CUL: "文化交流"
};

const cases = [];
let sequence = 1;
const addCase = (priority, lang, query, c, matchType, topK, note = "", status = "Draft") => {
  cases.push([
    `GQ-${String(sequence++).padStart(4,"0")}`, priority, lang, query,
    c?.[0] ?? "", c?.[2] ?? "", c?.[3] ?? "", c?.[4] ?? "",
    c?.[1] ?? "", matchType, topK, note, status
  ]);
};

for (const c of concepts) {
  addCase("P0","zh-Hans",c[2],c,"Exact",1,"三语首选术语精确命中");
  addCase("P0","es",c[3],c,"Exact",1,"西班牙语首选术语精确命中");
  addCase("P0","en",c[4],c,"Exact",1,"英语首选术语精确命中");
}

for (let i = 0; i < concepts.length; i++) {
  const c = concepts[i];
  const mod = i % 3;
  if (mod === 0) addCase("P1","zh-Hans",c[2].slice(0, Math.max(1,c[2].length - 1)),c,"Prefix",5,"前缀召回，允许多个候选");
  if (mod === 1) addCase("P1","es",c[3].split(" ")[0],c,"Prefix",5,"首词前缀或短语首词召回");
  if (mod === 2) addCase("P1","en",c[4].split(" ")[0],c,"Prefix",5,"首词前缀或短语首词召回");
  if (c[6]) addCase("P0",/^[A-Za-z0-9/.-]+$/.test(c[6]) ? "und" : "zh-Hans",c[6],c,"Variant",3,"别名或缩略语命中");
  if (c[7]) addCase("P0",/[áéíóúñü]/i.test(c[7]) || c[7].includes(" ") && !/^[A-Z/.-]+$/.test(c[7]) ? "es" : "en",c[7],c,"Variant",3,"地区变体、缩略语或替代表达");
}

const accentCases = [
  ["comercio electronico","ECON-000003"],["inflacion","ECON-000004"],["carta de credito","ECON-000006"],
  ["inversion extranjera directa","ECON-000007"],["aprendizaje automatico","IT-000002"],["codigo fuente","IT-000008"],
  ["hipotesis","RES-000001"],["analisis de regresion","RES-000004"],["etica de la investigacion","RES-000006"],
  ["traduccion","CUL-000001"],["localizacion","CUL-000002"],["comunicacion intercultural","CUL-000004"],
  ["subtitulo","CUL-000005"],["interpretacion","CUL-000007"]
];
for (const [query, code] of accentCases) addCase("P1","es",query,concepts.find(c => c[0] === code),"AccentFold",3,"查询可缺重音，展示必须保留规范拼写");

const typoCases = [
  ["artifical intelligence","IT-000001","en"],["machien learning","IT-000002","en"],
  ["cybersecurty","IT-000006","en"],["suply chain","ECON-000002","en"],
  ["inteligencia artifical","IT-000001","es"],["cadena de suminstro","ECON-000002","es"],
  ["reproduciblidad","RES-000008","es"],["intercultural comunication","CUL-000004","en"]
];
for (const [query, code, lang] of typoCases) addCase("P1",lang,query,concepts.find(c => c[0] === code),"Fuzzy",5,"单处拼写错误容错");

const definitionCases = [
  ["对进口商品征收的税","ECON-000001","zh-Hans"],
  ["network of organizations moving goods to customers","ECON-000002","en"],
  ["sistemas que realizan tareas de inteligencia humana","IT-000001","es"],
  ["computing resources delivered over a network","IT-000007","en"],
  ["evaluación de un trabajo por especialistas del mismo campo","RES-000002","es"],
  ["研究成果在线免费获取","RES-000005","zh-Hans"],
  ["adaptación de un producto a una lengua y mercado","CUL-000002","es"],
  ["preserved expressions and objects inherited from the past","CUL-000003","en"]
];
for (const [query, code, lang] of definitionCases) addCase("P2",lang,query,concepts.find(c => c[0] === code),"Definition",5,"定义全文召回，需专家复核表述");

for (const [query, lang] of [["不存在的术语甲乙丙","zh-Hans"],["termino inexistente xyzq","es"],["nonexistent jargon xyzq","en"],["%%%%","und"]]) {
  addCase("P1",lang,query,null,"NoResult",0,"应返回明确无结果状态和反馈入口");
}

const wb = Workbook.create();
const summary = wb.worksheets.add("Summary");
const tests = wb.worksheets.add("TestCases");
const conceptSheet = wb.worksheets.add("Concepts");
const method = wb.worksheets.add("Method");

const navy = "#173B57";
const blue = "#16627A";
const lightBlue = "#EAF4F8";
const orange = "#D97841";
const lightOrange = "#FFF3E8";
const line = "#D6E1E8";
const text = "#173247";
const muted = "#607487";
const green = "#287A59";
const font = "Arial";

function baseSheet(sheet) {
  sheet.showGridLines = false;
  sheet.getRange("A1:Z500").format.font = { name: font, size: 10, color: text };
  sheet.getRange("A1:Z500").format.verticalAlignment = "center";
}
[summary, tests, conceptSheet, method].forEach(baseSheet);
summary.tabColor = navy;
tests.tabColor = blue;
conceptSheet.tabColor = "#7398AA";
method.tabColor = "#A87B5D";

summary.getRange("A2:K2").merge();
summary.getRange("A2").values = [["汉西英词典黄金查询集 V0.1"]];
summary.getRange("A2").format = { font: { name: font, size: 16, bold: true, color: navy }, rowHeight: 28 };
summary.getRange("A3:K3").merge();
summary.getRange("A3").values = [["第 0 阶段结构性初稿。术语与期望结果需由领域专家逐条复核后方可作为发布门禁。"]];
summary.getRange("A3").format = { font: { name: font, size: 10, italic: true, color: muted }, rowHeight: 24 };
summary.getRange("A5:B8").values = [["指标","值"],["测试用例总数",null],["覆盖概念数",null],["待专家复核",null]];
summary.getRange("B6").formulas = [["=COUNTA(TestCases!$A$6:$A$505)"]];
summary.getRange("B7").formulas = [["=COUNTA(Concepts!$A$6:$A$105)"]];
summary.getRange("B8").formulas = [["=COUNTIFS(TestCases!$M$6:$M$505,\"Draft\")"]];

summary.getRange("D5:F9").values = [["源语言","用例数","占比"],["zh-Hans",null,null],["es",null,null],["en",null,null],["und",null,null]];
summary.getRange("E6").formulas = [["=COUNTIFS(TestCases!$C$6:$C$505,D6)"]];
summary.getRange("E6:E9").fillDown();
summary.getRange("F6").formulas = [["=E6/$B$6"]];
summary.getRange("F6:F9").fillDown();
summary.getRange("F6:F9").format.numberFormat = "0.0%";

summary.getRange("H5:J9").values = [["一级领域","用例数","占比"],["ECON",null,null],["IT",null,null],["RES",null,null],["CUL",null,null]];
summary.getRange("I6").formulas = [["=COUNTIFS(TestCases!$I$6:$I$505,H6&\".01\")+COUNTIFS(TestCases!$I$6:$I$505,H6&\".02\")+COUNTIFS(TestCases!$I$6:$I$505,H6&\".03\")+COUNTIFS(TestCases!$I$6:$I$505,H6&\".04\")+COUNTIFS(TestCases!$I$6:$I$505,H6&\".05\")+COUNTIFS(TestCases!$I$6:$I$505,H6&\".06\")+COUNTIFS(TestCases!$I$6:$I$505,H6&\".07\")+COUNTIFS(TestCases!$I$6:$I$505,H6&\".08\")"]];
summary.getRange("I6:I9").fillDown();
summary.getRange("J6").formulas = [["=I6/$B$6"]];
summary.getRange("J6:J9").fillDown();
summary.getRange("J6:J9").format.numberFormat = "0.0%";

summary.getRange("A11:C19").values = [["匹配类型","用例数","验收意图"],["Exact",null,"首选术语精确命中"],["Variant",null,"别名、缩略语和地区变体"],["Prefix",null,"联想与前缀召回"],["AccentFold",null,"西语缺失重音容错"],["Fuzzy",null,"单处拼写错误容错"],["Definition",null,"定义全文召回"],["NoResult",null,"明确无结果与反馈入口"],["SpeechTranscript",null,"后续语音转写测试预留"]];
summary.getRange("B12").formulas = [["=COUNTIFS(TestCases!$J$6:$J$505,A12)"]];
summary.getRange("B12:B19").fillDown();
summary.getRange("E11:K11").merge();
summary.getRange("E11").values = [["使用规则"]];
summary.getRange("E12:K16").merge();
summary.getRange("E12").values = [["1. P0 用例用于每次发布回归，P1 用于完整回归，P2 用于搜索调优。\n2. ExpectedTopK=1 表示必须首位命中；5 表示前五名召回。\n3. Draft 表示尚未由领域专家签字，不能作为最终质量声明。\n4. 扩充时保持稳定 CaseId，并记录变更原因。\n5. 目标是扩展到不少于 500 条，覆盖同形异义、地区变体和真实无结果词。"]];
summary.getRange("E12:K16").format = { fill: lightBlue, font: { name: font, size: 10, color: text }, wrapText: true, verticalAlignment: "top", borders: { preset: "outside", style: "thin", color: line } };

for (const range of ["A5:B5","D5:F5","H5:J5","A11:C11","E11:K11"]) {
  summary.getRange(range).format = { fill: navy, font: { name: font, size: 10, bold: true, color: "#FFFFFF" }, horizontalAlignment: "center", borders: { preset: "inside", style: "thin", color: "#FFFFFF" } };
}
summary.getRange("A6:B8").format.fill = "#FFFFFF";
summary.getRange("B6:B8").format = { font: { name: font, size: 14, bold: true, color: blue }, horizontalAlignment: "right" };
summary.getRange("A5:B8").format.borders = { preset: "outside", style: "thin", color: line };
summary.getRange("D5:F9").format.borders = { preset: "outside", style: "thin", color: line };
summary.getRange("H5:J9").format.borders = { preset: "outside", style: "thin", color: line };
summary.getRange("A11:C19").format.borders = { preset: "outside", style: "thin", color: line };
summary.getRange("A1:K20").format.rowHeight = 22;
summary.getRange("A2:K3").format.rowHeight = 28;
summary.getRange("E12:K16").format.rowHeight = 26;
summary.getRange("A:A").format.columnWidth = 22;
summary.getRange("B:B").format.columnWidth = 13;
summary.getRange("C:C").format.columnWidth = 34;
summary.getRange("D:D").format.columnWidth = 16;
summary.getRange("E:K").format.columnWidth = 13;

const testHeaders = ["CaseId","Priority","SourceLanguage","Query","ExpectedConceptCode","ExpectedZH","ExpectedES","ExpectedEN","DomainCode","MatchType","ExpectedTopK","Notes","Status"];
tests.getRange("A2:M2").merge();
tests.getRange("A2").values = [["黄金查询用例"]];
tests.getRange("A2").format = { font: { name: font, size: 15, bold: true, color: navy }, rowHeight: 28 };
tests.getRange("A3:M3").merge();
tests.getRange("A3").values = [["黄色列为评审人员可维护字段。筛选 Priority、Language、Domain 和 MatchType 可形成不同回归套件。"]];
tests.getRange("A3").format = { font: { name: font, italic: true, color: muted } };
tests.getRange("A5:M5").values = [testHeaders];
tests.getRangeByIndexes(5,0,cases.length,testHeaders.length).values = cases;
tests.getRange(`A5:M${5+cases.length}`).format.borders = { insideHorizontal: { style: "thin", color: "#E8EEF2" }, bottom: { style: "thin", color: line } };
tests.getRange("A5:M5").format = { fill: navy, font: { name: font, size: 9, bold: true, color: "#FFFFFF" }, horizontalAlignment: "center", wrapText: true, rowHeight: 32, borders: { preset: "inside", style: "thin", color: "#FFFFFF" } };
tests.getRange(`B6:D${5+cases.length}`).format.fill = lightOrange;
tests.getRange(`K6:M${5+cases.length}`).format.fill = lightOrange;
tests.getRange(`A6:M${5+cases.length}`).format.rowHeight = 24;
tests.getRange(`D6:H${5+cases.length}`).format.wrapText = true;
tests.getRange(`L6:L${5+cases.length}`).format.wrapText = true;
tests.getRange(`B6:C${5+cases.length}`).format.horizontalAlignment = "center";
tests.getRange(`J6:K${5+cases.length}`).format.horizontalAlignment = "center";
tests.getRange(`M6:M${5+cases.length}`).format.horizontalAlignment = "center";
tests.getRange(`B6:B${5+cases.length}`).dataValidation = { rule: { type: "list", values: ["P0","P1","P2"] } };
tests.getRange(`C6:C${5+cases.length}`).dataValidation = { rule: { type: "list", values: ["zh-Hans","es","en","und"] } };
tests.getRange(`J6:J${5+cases.length}`).dataValidation = { rule: { type: "list", values: ["Exact","Variant","Prefix","AccentFold","Fuzzy","Definition","NoResult","SpeechTranscript"] } };
tests.getRange(`M6:M${5+cases.length}`).dataValidation = { rule: { type: "list", values: ["Draft","Reviewed","Retired"] } };
tests.getRange(`M6:M${5+cases.length}`).conditionalFormats.add("containsText", { text: "Draft", format: { fill: "#FFF1D9", font: { color: "#8A5B20", bold: true } } });
tests.getRange(`M6:M${5+cases.length}`).conditionalFormats.add("containsText", { text: "Reviewed", format: { fill: "#E6F4EC", font: { color: green, bold: true } } });
tests.freezePanes.freezeRows(5);
tests.freezePanes.freezeColumns(4);
const testWidths = [14,10,15,30,22,22,28,28,14,16,13,38,12];
testWidths.forEach((w,i) => tests.getRangeByIndexes(0,i,1,1).format.columnWidth = w);

const conceptHeaders = ["ConceptCode","DomainCode","中文首选术语","Término preferido","Preferred term","PartOfSpeech","VariantOrAbbr1","VariantOrAbbr2","ReviewStatus"];
conceptSheet.getRange("A2:I2").merge();
conceptSheet.getRange("A2").values = [["概念样例源表"]];
conceptSheet.getRange("A2").format = { font: { name: font, size: 15, bold: true, color: navy }, rowHeight: 28 };
conceptSheet.getRange("A3:I3").merge();
conceptSheet.getRange("A3").values = [["32 个概念用于构造第 0 阶段查询集。译词为结构性样例，须经领域专家和来源证据复核。"]];
conceptSheet.getRange("A3").format = { font: { name: font, italic: true, color: muted } };
conceptSheet.getRange("A5:I5").values = [conceptHeaders];
const conceptRows = concepts.map(c => [...c,"Draft"]);
conceptSheet.getRangeByIndexes(5,0,conceptRows.length,conceptHeaders.length).values = conceptRows;
conceptSheet.getRange("A5:I5").format = { fill: navy, font: { name: font, size: 9, bold: true, color: "#FFFFFF" }, horizontalAlignment: "center", wrapText: true, rowHeight: 32, borders: { preset: "inside", style: "thin", color: "#FFFFFF" } };
conceptSheet.getRange(`A6:I${5+conceptRows.length}`).format.borders = { insideHorizontal: { style: "thin", color: "#E8EEF2" } };
conceptSheet.getRange(`C6:H${5+conceptRows.length}`).format.fill = lightOrange;
conceptSheet.getRange(`C6:H${5+conceptRows.length}`).format.wrapText = true;
conceptSheet.getRange(`I6:I${5+conceptRows.length}`).dataValidation = { rule: { type: "list", values: ["Draft","Reviewed","Retired"] } };
conceptSheet.getRange(`I6:I${5+conceptRows.length}`).conditionalFormats.add("containsText", { text: "Draft", format: { fill: "#FFF1D9", font: { color: "#8A5B20", bold: true } } });
conceptSheet.freezePanes.freezeRows(5);
conceptSheet.freezePanes.freezeColumns(2);
[18,14,22,34,34,15,22,26,14].forEach((w,i) => conceptSheet.getRangeByIndexes(0,i,1,1).format.columnWidth = w);
conceptSheet.getRange(`A6:I${5+conceptRows.length}`).format.rowHeight = 32;

method.getRange("A2:F2").merge();
method.getRange("A2").values = [["方法、状态与参考"]];
method.getRange("A2").format = { font: { name: font, size: 15, bold: true, color: navy }, rowHeight: 28 };
method.getRange("A4:B11").values = [
  ["字段/状态","说明"],
  ["P0","发布阻断级核心查询，每次构建均运行"],
  ["P1","完整回归集，发布候选版本运行"],
  ["P2","搜索调优和分析用例，不单独阻断发布"],
  ["Draft","尚未由术语/领域专家复核"],
  ["Reviewed","查询、概念边界、三语期望和优先级均已签字"],
  ["ExpectedTopK","期望概念必须出现在前 K 个结果内；0 表示无结果"],
  ["SourceLanguage=und","缩略语、符号或无法仅凭形式确定语言"]
];
method.getRange("D4:F10").values = [
  ["参考","用途","链接"],
  ["ISO 704:2022","概念导向术语原则","https://www.iso.org/standard/79077.html"],
  ["ISO 1087:2019","术语学基础词汇","https://www.iso.org/standard/62330.html"],
  ["ISO 30042:2019","TBX 交换模型","https://www.iso.org/standard/62510.html"],
  ["SKOS Reference","概念标签与关系映射","https://www.w3.org/TR/skos-reference/"],
  ["RFC 5646","语言标签","https://www.rfc-editor.org/info/rfc5646/"],
  ["Unicode UAX #15","Unicode NFC 规范化","https://www.unicode.org/reports/tr15/"]
];
for (const range of ["A4:B4","D4:F4"]) method.getRange(range).format = { fill: navy, font: { name: font, bold: true, color: "#FFFFFF" }, horizontalAlignment: "center", borders: { preset: "inside", style: "thin", color: "#FFFFFF" } };
method.getRange("A4:B11").format.borders = { preset: "outside", style: "thin", color: line };
method.getRange("D4:F10").format.borders = { preset: "outside", style: "thin", color: line };
method.getRange("A13:F13").merge();
method.getRange("A13").values = [["复核流程"]];
method.getRange("A13").format = { fill: blue, font: { name: font, bold: true, color: "#FFFFFF" } };
method.getRange("A14:F18").merge();
method.getRange("A14").values = [["1. 术语专家检查概念边界、三语首选词和变体。\n2. 测试负责人检查 Query、ExpectedTopK 和匹配类型是否可执行。\n3. 将通过的行改为 Reviewed，并记录外部评审记录。\n4. 搜索实现后自动回填实际排名、耗时和通过状态到测试报告，不覆盖本基线。\n5. 每次新增领域至少补充精确、变体、容错、全文和无结果用例。"]];
method.getRange("A14:F18").format = { fill: lightBlue, wrapText: true, verticalAlignment: "top", borders: { preset: "outside", style: "thin", color: line } };
method.getRange("A:A").format.columnWidth = 24;
method.getRange("B:B").format.columnWidth = 42;
method.getRange("C:C").format.columnWidth = 3;
method.getRange("D:D").format.columnWidth = 22;
method.getRange("E:E").format.columnWidth = 25;
method.getRange("F:F").format.columnWidth = 52;
method.getRange("A4:F18").format.rowHeight = 26;
method.getRange("A14:F18").format.rowHeight = 28;

wb.recalculate();

const summaryCheck = await wb.inspect({ kind: "table", range: "Summary!A2:K19", include: "values,formulas", tableMaxRows: 25, tableMaxCols: 12, maxChars: 12000 });
console.log("SUMMARY_CHECK\n" + summaryCheck.ndjson);
const casesCheck = await wb.inspect({ kind: "table", range: "TestCases!A5:M15", include: "values,formulas", tableMaxRows: 15, tableMaxCols: 13, maxChars: 12000 });
console.log("CASES_CHECK\n" + casesCheck.ndjson);
const errorCheck = await wb.inspect({ kind: "match", searchTerm: "#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!|#SPILL!|#CALC!", options: { useRegex: true, maxResults: 100 }, summary: "final formula error scan", maxChars: 5000 });
console.log("ERROR_CHECK\n" + errorCheck.ndjson);

for (const [sheetName, range] of [["Summary","A1:K20"],["TestCases","A1:M35"],["Concepts","A1:I37"],["Method","A1:F19"]]) {
  const preview = await wb.render({ sheetName, range, scale: 1.2, format: "png" });
  await fs.writeFile(`${previewDir}/${sheetName}.png`, new Uint8Array(await preview.arrayBuffer()));
}

const output = await SpreadsheetFile.exportXlsx(wb);
const outputPath = `${outputDir}/汉西英词典-黄金查询集-V0.1.xlsx`;
await output.save(outputPath);
console.log(JSON.stringify({ outputPath, previewDir, conceptCount: concepts.length, caseCount: cases.length }));
