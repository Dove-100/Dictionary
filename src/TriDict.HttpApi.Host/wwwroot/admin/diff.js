export function buildRevisionDiff(published, draft, sourceNames = new Map(), domainNames = new Map()) {
  const oldData = published || { terms: [], definitions: [], sources: [] };
  const newData = draft || { terms: [], definitions: [], sources: [] };
  const rows = [];
  const displayDomain = id => domainNames.get(id) || id || '—';
  addRow(rows, '主领域', displayDomain(oldData.domainId), displayDomain(newData.domainId));
  addRow(rows, '可靠等级', reliability(oldData.reliabilityCode), reliability(newData.reliabilityCode));

  compareCollection(rows, oldData.terms, newData.terms,
    item => `${item.languageTag} · 义项 ${item.senseOrder} · ${item.isPreferred ? '首选' : item.text}`,
    item => [item.text, item.partOfSpeech, item.usageContext, item.region].filter(Boolean).join(' / '),
    key => `术语 · ${key}`);
  compareCollection(rows, oldData.definitions, newData.definitions,
    (item, index) => `${item.languageTag} · ${item.scenarioLabel || '未标注场景'} · ${index + 1}`,
    item => item.text,
    key => `定义 · ${key}`);
  compareCollection(rows, oldData.sources, newData.sources,
    item => `${item.sourceId} · ${item.evidenceType}`,
    item => `${sourceNames.get(item.sourceId) || item.sourceId}${item.locator ? ` / ${item.locator}` : ''}`,
    () => '来源证据');
  return rows.filter(row => row.change !== 'unchanged');
}

function compareCollection(rows, oldItems = [], newItems = [], keyFn, textFn, labelFn) {
  const oldMap = new Map(oldItems.map((item, index) => [keyFn(item, index), textFn(item)]));
  const newMap = new Map(newItems.map((item, index) => [keyFn(item, index), textFn(item)]));
  for (const key of new Set([...oldMap.keys(), ...newMap.keys()])) {
    addRow(rows, labelFn(key), oldMap.get(key) || '', newMap.get(key) || '');
  }
}

function addRow(rows, label, before, after) {
  const change = before === after ? 'unchanged' : !before ? 'added' : !after ? 'removed' : 'changed';
  rows.push({ label, before, after, change });
}

function reliability(value) {
  return ['未核验', '语料支持', '专家审核', '权威来源'][Number(value)] || '—';
}
