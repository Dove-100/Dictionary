import { FileBlob, SpreadsheetFile } from "@oai/artifact-tool";

const path = "E:/西班牙语学习词典/outputs/01a0cc1b-5b38-7271-a04e-ec9746728387/汉西英词典-黄金查询集-V0.1.xlsx";
const blob = await FileBlob.load(path);
const workbook = await SpreadsheetFile.importXlsx(blob);
const sheets = await workbook.inspect({ kind: "sheet", include: "id,name", maxChars: 4000 });
const summary = await workbook.inspect({ kind: "table", range: "Summary!A5:J19", include: "values,formulas", tableMaxRows: 20, tableMaxCols: 10, maxChars: 10000 });
const errors = await workbook.inspect({ kind: "match", searchTerm: "#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!|#SPILL!|#CALC!", options: { useRegex: true, maxResults: 100 }, summary: "saved workbook formula error scan", maxChars: 4000 });
console.log("SHEETS\n" + sheets.ndjson);
console.log("SAVED_SUMMARY\n" + summary.ndjson);
console.log("SAVED_ERRORS\n" + errors.ndjson);
