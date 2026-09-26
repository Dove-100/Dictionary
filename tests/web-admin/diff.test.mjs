import test from 'node:test';
import assert from 'node:assert/strict';
import { buildRevisionDiff } from '../../src/TriDict.HttpApi.Host/wwwroot/admin/diff.js';

test('审核差异按语言、义项和场景分别展示定义变更', () => {
  const sourceId = 'source-1';
  const oldRevision = {
    domainId: 'domain-a', reliabilityCode: 1,
    terms: [
      { languageTag: 'zh-Hans', text: '模型', isPreferred: true, senseOrder: 1, partOfSpeech: 'noun', usageContext: '科研' },
      { languageTag: 'es', text: 'modelo', isPreferred: true, senseOrder: 1, partOfSpeech: 'noun', usageContext: 'investigación' },
    ],
    definitions: [{ languageTag: 'zh-Hans', text: '旧定义', scenarioLabel: '科研' }],
    sources: [{ sourceId, evidenceType: 1, locator: '第 1 页' }],
  };
  const draft = {
    ...oldRevision,
    terms: [oldRevision.terms[0], { ...oldRevision.terms[1], text: 'modelo teórico' }],
    definitions: [
      { languageTag: 'zh-Hans', text: '新定义', scenarioLabel: '科研' },
      { languageTag: 'zh-Hans', text: '产品规格的型号。', scenarioLabel: '工业产品' },
    ],
    sources: [{ sourceId, evidenceType: 1, locator: '第 2 页' }],
  };

  const rows = buildRevisionDiff(oldRevision, draft, new Map([[sourceId, '术语标准']]));

  assert.equal(rows.length, 4);
  assert.ok(rows.some(row => row.label.includes('es') && row.before.includes('modelo') && row.after.includes('modelo teórico')));
  assert.ok(rows.some(row => row.label.includes('科研') && row.before === '旧定义' && row.after === '新定义'));
  assert.ok(rows.some(row => row.label.includes('工业产品') && row.change === 'added'));
  assert.ok(rows.some(row => row.label === '来源证据' && row.after.includes('第 2 页')));
  assert.ok(rows.every(row => !row.label.includes('zh-Hans · 义项 1')));
});

test('首次发布与空白基线比较', () => {
  const rows = buildRevisionDiff(null, {
    domainId: 'domain-a', reliabilityCode: 0,
    terms: [{ languageTag: 'en', text: 'bank', isPreferred: true, senseOrder: 1, partOfSpeech: 'noun' }],
    definitions: [], sources: [],
  });
  assert.ok(rows.some(row => row.label.includes('en') && row.change === 'added'));
});
