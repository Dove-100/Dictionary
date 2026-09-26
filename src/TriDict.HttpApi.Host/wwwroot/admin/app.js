import { api, ApiError, hasAccessToken, setAccessToken } from './api.js';
import { buildRevisionDiff } from './diff.js';

const STATUS = ['草稿', '待审核', '已退回', '已批准', '已发布', '已停用'];
const IMPORT_STATUS = ['待处理', '校验中', '已完成', '失败'];
const LANGUAGES = [
  { tag: 'zh-Hans', label: '中文', short: '中' },
  { tag: 'es', label: 'Español', short: '西' },
  { tag: 'en', label: 'English', short: '英' },
];
const state = {
  route: 'editor',
  domains: [], sources: [], editorList: [], editorTotal: 0, editorSkip: 0, editorQuery: '',
  concept: null, workingRevision: null, isNew: true, activeLanguage: 'zh-Hans', dirty: false,
  reviewStatus: 1, reviewList: [], reviewTotal: 0, reviewSkip: 0, reviewConcept: null,
  imports: [], importTotal: 0, importSkip: 0, selectedImport: null, uploadFile: null,
  busy: false,
};

const $ = selector => document.querySelector(selector);
const $$ = selector => [...document.querySelectorAll(selector)];
const esc = value => String(value ?? '').replace(/[&<>"']/g, character => ({
  '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;',
}[character]));
const stamp = value => value ? new Date(value).toLocaleString('zh-CN') : '—';
const domainName = id => {
  const domain = state.domains.find(item => item.id === id);
  return domain ? `${domain.code} · ${domain.nameZh}` : id || '请选择领域';
};
const sourceName = id => state.sources.find(item => item.id === id)?.title || id || '未选择';
const statusChip = status => `<span class="status-chip ${['draft','review','rejected','approved','','draft'][Number(status)] || ''}">${esc(STATUS[Number(status)] || '未知')}</span>`;
const importChip = status => `<span class="status-chip ${Number(status) === 3 ? 'failed' : Number(status) === 2 ? '' : 'review'}">${esc(IMPORT_STATUS[Number(status)] || '未知')}</span>`;
const preferred = (revision, language) => revision?.terms?.find(term => term.languageTag === language && term.isPreferred)?.text || '—';

function toast(message, error = false) {
  const node = $('#toast');
  node.textContent = message;
  node.classList.toggle('error', error);
  node.classList.add('visible');
  clearTimeout(toast.timer);
  toast.timer = setTimeout(() => node.classList.remove('visible'), 4200);
}

function showError(error) {
  const message = error instanceof ApiError ? error.message : (error?.message || '操作失败，请稍后重试。');
  toast(message, true);
  if (error?.status === 401) openConnection();
}

function newRevision() {
  return {
    domainId: state.domains[0]?.id || '', reliabilityCode: 0,
    changeSummary: '初次录入', status: 0, version: 1, revisionToken: '',
    terms: LANGUAGES.map(({ tag }) => ({ languageTag: tag, text: '', termType: 0, partOfSpeech: 'noun', isPreferred: true, senseOrder: 1, usageContext: '', region: '' })),
    definitions: [{ languageTag: 'zh-Hans', text: '', scenarioLabel: '', sourceId: null }],
    sources: [],
  };
}

function navigate() {
  const route = location.hash.replace('#', '') || 'editor';
  state.route = ['editor', 'review', 'imports'].includes(route) ? route : 'editor';
  for (const view of ['editor', 'review', 'imports']) {
    $(`#${view}-view`).hidden = view !== state.route;
    $(`[data-route="${view}"]`)?.classList.toggle('active', view === state.route);
  }
  $('#breadcrumb-current').textContent = ({ editor: '术语编辑', review: '审核差异', imports: '导入结果' })[state.route];
  render();
}

function render() {
  if (state.route === 'editor') renderEditor();
  if (state.route === 'review') renderReview();
  if (state.route === 'imports') renderImports();
}

function renderEditor() {
  const revision = state.workingRevision || newRevision();
  const concept = state.concept;
  const canEdit = state.isNew || [0, 2].includes(Number(revision.status));
  const list = state.editorList.map(item => {
    const active = concept?.id === item.id && !state.isNew;
    const current = item.activeRevision;
    return `<button type="button" class="list-item ${active ? 'active' : ''}" data-open-concept="${esc(item.id)}">
      <span class="list-item-head"><span>${esc(item.conceptCode)}</span>${statusChip(current?.status ?? item.status)}</span>
      <span class="list-item-term">${esc(preferred(current, 'zh-Hans'))} <span style="color:#9aaba4;font-weight:400">/ ${esc(preferred(current, 'es'))}</span></span>
      <span class="list-item-sub"><span>${esc(domainName(item.domainId))}</span><span>V${esc(current?.version || 1)}</span></span>
    </button>`;
  }).join('') || emptyList('暂无术语', '新建概念或调整查询条件。');

  $('#editor-view').innerHTML = `
    <div class="page-heading"><div><p class="eyebrow">TERMINOLOGY / EDITOR</p><h1 id="editor-title">术语编辑</h1><p>以概念为单位维护三语术语、场景定义与可追溯来源。</p></div>
      <div class="heading-actions"><button class="button" type="button" data-refresh-editor>刷新列表</button><button class="button primary" type="button" data-new-concept>＋ 新建概念</button></div></div>
    <div class="metric-grid">
      <div class="metric"><span>概念总数</span><strong>${hasAccessToken() ? esc(state.editorTotal) : '—'}<small>条</small></strong></div>
      <div class="metric orange"><span>正在编辑</span><strong>${concept ? esc(concept.conceptCode) : '新概念'}</strong></div>
      <div class="metric blue"><span>当前版本</span><strong>V${esc(revision.version || 1)}<small>${esc(STATUS[Number(revision.status)] || '草稿')}</small></strong></div>
    </div>
    <div class="work-grid">
      <aside class="card" aria-label="概念列表"><div class="card-head"><h2>概念列表</h2><small>${esc(state.editorTotal)} 条记录</small></div>
        <div class="search-row"><input id="concept-search" type="search" value="${esc(state.editorQuery)}" placeholder="搜索概念编码" aria-label="搜索概念编码"><button class="button small" type="button" data-search-concepts>搜索</button></div>
        <div class="list-scroll">${list}</div>${pager(state.editorTotal, state.editorSkip, 'editor')}
      </aside>
      <article class="card editor-card">
        <div class="editor-top"><div><p class="eyebrow">${state.isNew ? 'NEW CONCEPT' : esc(concept?.conceptCode)}</p><h2>${state.isNew ? '创建三语概念' : canEdit ? '编辑修订草稿' : '查看修订内容'}</h2><p>${state.isNew ? '先保存草稿，再提交审核。' : `版本 ${esc(revision.version)} · ${esc(STATUS[Number(revision.status)] || '草稿')}`}</p></div>
          <div class="editor-toolbar">${!canEdit && Number(revision.status) === 4 ? '<button class="button primary" type="button" data-start-revision>创建新修订</button>' : ''}
            ${canEdit ? '<button class="button" type="button" data-save-editor>保存草稿</button><button class="button primary" type="button" data-submit-editor>提交审核 →</button>' : ''}</div></div>
        <div class="quality-callout" id="quality-callout"><strong>提交前检查</strong><span>三语首选术语、定义和来源均为必需。</span></div>
        <form id="editor-form" class="editor-form" novalidate>
          <fieldset ${canEdit ? '' : 'disabled'} style="border:0;padding:0;margin:0;min-width:0">
            <div class="form-grid">
              <label class="field-label">概念编码 <span class="field-help">每个义项使用独立编码</span><input class="field" name="conceptCode" maxlength="64" value="${esc(concept?.conceptCode || revision.conceptCode || '')}" ${state.isNew ? '' : 'readonly'} placeholder="例如 IT-AI-001"></label>
              <label class="field-label">主领域 <span class="required">*</span><select class="field" name="domainId"><option value="">请选择领域</option>${state.domains.map(item => `<option value="${esc(item.id)}" ${revision.domainId === item.id ? 'selected' : ''}>${esc(item.code)} · ${esc(item.nameZh)}</option>`).join('')}</select></label>
              <label class="field-label">可靠等级<select class="field" name="reliabilityCode">${['未核验','语料支持','专家审核','权威来源'].map((item,index) => `<option value="${index}" ${Number(revision.reliabilityCode) === index ? 'selected' : ''}>${item}</option>`).join('')}</select></label>
              <label class="field-label">修订说明 <span class="required">*</span><input class="field" name="changeSummary" maxlength="2000" value="${esc(revision.changeSummary || '')}" placeholder="说明本次新增或修改的内容"></label>
            </div>
            <div class="form-section"><div class="section-heading"><div><h3>三语术语与定义</h3><p>同形多义词请分别标注使用场景和义项序号。</p></div></div>
              <div class="language-tabs" role="tablist" aria-label="编辑语言">${LANGUAGES.map(item => `<button type="button" role="tab" aria-selected="${state.activeLanguage === item.tag}" data-language="${item.tag}" class="${state.activeLanguage === item.tag ? 'active' : ''}">${item.short} · ${item.label}</button>`).join('')}</div>
              ${LANGUAGES.map(item => languagePanel(item, revision, canEdit)).join('')}
            </div>
            <div class="form-section"><div class="section-heading"><div><h3>来源证据</h3><p>发布前至少关联一项有效来源，并尽量填写页码或章节。</p></div>
              ${canEdit ? '<button class="button small" type="button" data-open-source>＋ 新建来源</button>' : ''}</div>
              <div id="source-rows">${(revision.sources || []).map((item,index) => sourceRow(item,index,canEdit)).join('')}</div>
              ${canEdit ? '<button class="button text small" type="button" data-add-source>＋ 关联已有来源</button>' : ''}
            </div>
          </fieldset>
          <div class="form-bottom"><p>${state.dirty ? '有尚未保存的修改' : '修改后请保存草稿；审核状态下不可直接编辑。'}</p><div>${canEdit ? '<button class="button" type="button" data-save-editor>保存草稿</button><button class="button primary" type="button" data-submit-editor>提交审核 →</button>' : ''}</div></div>
        </form>
      </article>
    </div>`;
  updateQuality();
}

function languagePanel(language, revision, canEdit) {
  const terms = (revision.terms || []).map((item,index) => ({ item,index })).filter(x => x.item.languageTag === language.tag);
  const definitions = (revision.definitions || []).map((item,index) => ({ item,index })).filter(x => x.item.languageTag === language.tag);
  return `<div class="language-panel" data-language-panel="${language.tag}" ${state.activeLanguage === language.tag ? '' : 'hidden'}>
    <div class="section-heading"><div><h3>${language.label}术语</h3><p>首选术语与同义形式分别保存。</p></div>${canEdit ? `<button class="button text small" type="button" data-add-term="${language.tag}">＋ 添加术语</button>` : ''}</div>
    ${terms.map(({item,index}) => termRow(item,index,canEdit)).join('') || '<div class="field-help" style="margin-bottom:12px">尚无术语</div>'}
    <div class="section-heading" style="margin-top:20px"><div><h3>${language.label}定义</h3><p>多义词的不同场景建议分别建概念，并准确填写场景标签。</p></div>${canEdit ? `<button class="button text small" type="button" data-add-definition="${language.tag}">＋ 添加定义</button>` : ''}</div>
    ${definitions.map(({item,index}) => definitionRow(item,index,canEdit)).join('') || '<div class="field-help">尚无定义</div>'}
  </div>`;
}

function termRow(item, index, canEdit) {
  return `<div class="row-card" data-term-row data-index="${index}" data-language-tag="${esc(item.languageTag)}">
    <div class="row-title"><span>${item.isPreferred ? '首选术语' : '其他术语'} · ${esc(item.languageTag)}</span>${canEdit ? `<button type="button" data-remove-term="${index}">移除</button>` : ''}</div>
    <div class="row-grid"><label class="field-label">术语文本<input class="field" name="text" maxlength="512" value="${esc(item.text)}" placeholder="输入术语"></label>
      <label class="field-label">使用场景<input class="field" name="usageContext" maxlength="256" value="${esc(item.usageContext || '')}" placeholder="例如 科研建模"></label></div>
    <div class="row-grid three"><label class="field-label">词性<input class="field" name="partOfSpeech" maxlength="64" value="${esc(item.partOfSpeech || 'noun')}"></label>
      <label class="field-label">术语类型<select class="field" name="termType">${['首选','许用','弃用','缩略语','符号','变体'].map((label,value) => `<option value="${value}" ${Number(item.termType) === value ? 'selected' : ''}>${label}</option>`).join('')}</select></label>
      <label class="field-label">义项序号<input class="field" name="senseOrder" type="number" min="0" value="${esc(item.senseOrder ?? 1)}"></label></div>
    <div class="row-grid"><label class="field-label">地区标记<input class="field" name="region" maxlength="35" value="${esc(item.region || '')}" placeholder="可选"></label>
      <label class="checkbox-label"><input name="isPreferred" type="checkbox" ${item.isPreferred ? 'checked' : ''}> 设为该语言首选术语</label></div>
  </div>`;
}

function definitionRow(item, index, canEdit) {
  return `<div class="row-card" data-definition-row data-index="${index}" data-language-tag="${esc(item.languageTag)}">
    <div class="row-title"><span>定义 ${index + 1}</span>${canEdit ? `<button type="button" data-remove-definition="${index}">移除</button>` : ''}</div>
    <label class="field-label">定义文本<textarea class="field" name="text" maxlength="4000" rows="3" placeholder="准确解释当前概念及适用场景">${esc(item.text)}</textarea></label>
    <div class="row-grid"><label class="field-label">场景标签<input class="field" name="scenarioLabel" maxlength="256" value="${esc(item.scenarioLabel || '')}" placeholder="例如 金融 / 科研"></label>
      <label class="field-label">定义来源<select class="field" name="sourceId"><option value="">尚未指定</option>${state.sources.map(source => `<option value="${esc(source.id)}" ${item.sourceId === source.id ? 'selected' : ''}>${esc(source.title)}</option>`).join('')}</select></label></div>
  </div>`;
}

function sourceRow(item, index, canEdit) {
  return `<div class="source-row" data-source-row data-index="${index}">
    <label class="field-label">来源<select class="field" name="sourceId"><option value="">选择来源</option>${state.sources.filter(x => x.isActive || x.id === item.sourceId).map(source => `<option value="${esc(source.id)}" ${item.sourceId === source.id ? 'selected' : ''}>${esc(source.title)}</option>`).join('')}</select></label>
    <label class="field-label">证据类型<select class="field" name="evidenceType">${['术语','定义','例句','通用'].map((label,value) => `<option value="${value}" ${Number(item.evidenceType) === value ? 'selected' : ''}>${label}</option>`).join('')}</select></label>
    <label class="field-label">证据位置<input class="field" name="locator" maxlength="256" value="${esc(item.locator || '')}" placeholder="页码 / 章节"></label>
    ${canEdit ? `<button class="icon-button" type="button" data-remove-source="${index}" aria-label="移除来源">×</button>` : '<span></span>'}
  </div>`;
}

function emptyList(title, detail) {
  return `<div class="empty-state"><span class="empty-symbol">◇</span><strong>${esc(title)}</strong><p>${esc(detail)}</p></div>`;
}

function pager(total, skip, scope) {
  if (total <= 20) return `<div class="list-foot">${total ? `显示 ${Math.min(total, 20)} / ${total} 条` : '暂无记录'}</div>`;
  return `<div class="list-foot">${skip + 1}–${Math.min(skip + 20, total)} / ${total}
    <button type="button" data-page="${scope}" data-offset="${Math.max(0, skip - 20)}" ${skip === 0 ? 'disabled' : ''}>上一页</button>
    <button type="button" data-page="${scope}" data-offset="${skip + 20}" ${skip + 20 >= total ? 'disabled' : ''}>下一页</button></div>`;
}

function captureEditor(includeEmpty = false) {
  const form = $('#editor-form');
  if (!form) return null;
  const value = (element, name) => element.querySelector(`[name="${name}"]`)?.value?.trim() || '';
  const terms = [...form.querySelectorAll('[data-term-row]')].map(row => ({
    languageTag: row.dataset.languageTag,
    text: value(row, 'text'), termType: Number(value(row, 'termType')),
    partOfSpeech: value(row, 'partOfSpeech') || 'noun',
    isPreferred: row.querySelector('[name="isPreferred"]')?.checked || false,
    senseOrder: Number(value(row, 'senseOrder')),
    usageContext: value(row, 'usageContext') || null, region: value(row, 'region') || null,
  })).filter(item => includeEmpty || item.text);
  const definitions = [...form.querySelectorAll('[data-definition-row]')].map(row => ({
    languageTag: row.dataset.languageTag, text: value(row, 'text'),
    scenarioLabel: value(row, 'scenarioLabel') || null, sourceId: value(row, 'sourceId') || null,
  })).filter(item => includeEmpty || item.text);
  const sources = [...form.querySelectorAll('[data-source-row]')].map(row => ({
    sourceId: value(row, 'sourceId'), evidenceType: Number(value(row, 'evidenceType')),
    locator: value(row, 'locator') || null,
  })).filter(item => includeEmpty || item.sourceId);
  return {
    conceptCode: value(form, 'conceptCode').toUpperCase(),
    domainId: value(form, 'domainId'), reliabilityCode: Number(value(form, 'reliabilityCode')),
    changeSummary: value(form, 'changeSummary'), terms, definitions, sources,
  };
}

function captureForStructuralEdit() {
  const payload = captureEditor(true);
  if (!payload) return;
  state.workingRevision = { ...state.workingRevision, ...payload };
  state.dirty = true;
}

function qualityIssues(payload) {
  const issues = [];
  if (!payload?.conceptCode) issues.push('概念编码');
  if (!payload?.domainId) issues.push('主领域');
  if (!payload?.changeSummary) issues.push('修订说明');
  for (const language of LANGUAGES) {
    const preferredTerms = payload?.terms?.filter(item => item.languageTag === language.tag && item.isPreferred) || [];
    if (!preferredTerms.length) issues.push(`${language.label}首选术语`);
    if (preferredTerms.length > 1) issues.push(`${language.label}首选术语重复`);
  }
  if (!payload?.definitions?.length) issues.push('至少一条定义');
  if (!payload?.sources?.length) issues.push('至少一项来源');
  const sourceIds = new Set((payload?.sources || []).map(item => item.sourceId));
  if (payload?.definitions?.some(item => item.sourceId && !sourceIds.has(item.sourceId))) issues.push('定义来源未关联到概念');
  const sourceKeys = (payload?.sources || []).map(item => `${item.sourceId}/${item.evidenceType}`);
  if (sourceKeys.length !== new Set(sourceKeys).size) issues.push('来源证据重复');
  return issues;
}

function updateQuality() {
  const target = $('#quality-callout');
  if (!target) return;
  const issues = qualityIssues(captureEditor());
  target.classList.toggle('warning', issues.length > 0);
  target.innerHTML = `<strong>${issues.length ? `待补全 ${issues.length} 项` : '可以提交审核'}</strong><span>${issues.length ? esc(issues.join('、')) : '三语首选术语、定义和来源已经齐备。'}</span>`;
}

async function saveEditor(submit = false) {
  if (state.busy) return;
  const payload = captureEditor();
  if (!payload) return;
  if (!payload.conceptCode || !payload.domainId || !payload.changeSummary) {
    toast('请填写概念编码、主领域和修订说明。', true);
    return;
  }
  if (submit) {
    const issues = qualityIssues(payload);
    if (issues.length) { toast(`提交前请补全：${issues.join('、')}`, true); return; }
  }
  state.busy = true;
  try {
    let concept;
    if (state.isNew) {
      concept = await api.createConcept(payload);
    } else {
      const revision = await api.updateRevision(state.workingRevision.id, {
        ...payload, revisionToken: state.workingRevision.revisionToken,
      });
      concept = { ...state.concept, activeRevision: revision };
    }
    // Persist the saved identity/token before the optional submit request. A failed
    // submit must not make the next click create the same concept a second time.
    state.concept = concept;
    state.workingRevision = structuredClone(concept.activeRevision);
    state.isNew = false;
    state.dirty = false;
    if (submit) {
      const submitted = await api.revisionAction(concept.activeRevision.id, 'submit', concept.activeRevision.revisionToken);
      concept.activeRevision = submitted;
      state.workingRevision = structuredClone(submitted);
      toast('草稿已提交审核。');
    } else {
      toast('草稿已保存。');
    }
    state.concept = await api.getConcept(concept.id);
    state.workingRevision = structuredClone(state.concept.activeRevision);
    await loadEditorList();
    renderEditor();
  } catch (error) { if (!state.isNew) renderEditor(); showError(error); }
  finally { state.busy = false; }
}

async function loadEditorList() {
  if (!hasAccessToken()) return;
  const page = await api.listConcepts(state.editorQuery, null, state.editorSkip);
  state.editorList = page.items || [];
  state.editorTotal = page.totalCount || 0;
}

async function openConcept(id) {
  if (state.dirty && !confirm('当前修改尚未保存，确定切换概念吗？')) return;
  try {
    state.concept = await api.getConcept(id);
    state.workingRevision = structuredClone(state.concept.activeRevision);
    state.isNew = false;
    state.dirty = false;
    renderEditor();
  } catch (error) { showError(error); }
}

function createNewConcept() {
  if (state.dirty && !confirm('当前修改尚未保存，确定新建概念吗？')) return;
  state.concept = null;
  state.workingRevision = newRevision();
  state.isNew = true;
  state.dirty = false;
  state.activeLanguage = 'zh-Hans';
  renderEditor();
}

async function startRevision() {
  if (!state.concept) return;
  const changeSummary = prompt('请简要说明本次修订内容：');
  if (!changeSummary?.trim()) return;
  try {
    await api.startRevision(state.concept.id, changeSummary.trim());
    await openConcept(state.concept.id);
    toast('新修订草稿已创建。');
  } catch (error) { showError(error); }
}

function renderReview() {
  const list = state.reviewList.map(item => `<button type="button" class="list-item ${state.reviewConcept?.id === item.id ? 'active' : ''}" data-open-review="${esc(item.id)}">
    <span class="list-item-head"><span>${esc(item.conceptCode)}</span>${statusChip(item.activeRevision?.status)}</span>
    <span class="list-item-term">${esc(preferred(item.activeRevision, 'zh-Hans'))} <span style="font-weight:400;color:#9aaba4">/ ${esc(preferred(item.activeRevision, 'es'))}</span></span>
    <span class="list-item-sub"><span>${esc(domainName(item.activeRevision?.domainId))}</span><span>V${esc(item.activeRevision?.version || 1)}</span></span>
  </button>`).join('') || emptyList('队列已清空', '当前状态下没有待处理的修订。');

  $('#review-view').innerHTML = `<div class="page-heading"><div><p class="eyebrow">WORKFLOW / REVIEW</p><h1 id="review-title">审核差异</h1><p>逐项核对待发布快照与当前公开版本，保留每条术语的场景和来源。</p></div>
    <div class="heading-actions"><button class="button" type="button" data-refresh-review>刷新队列</button></div></div>
    <div class="metric-grid"><div class="metric"><span>当前队列</span><strong>${hasAccessToken() ? esc(state.reviewTotal) : '—'}<small>条</small></strong></div>
      <div class="metric orange"><span>筛选状态</span><strong style="font-size:17px;margin-top:15px">${state.reviewStatus === 1 ? '待审核' : '已批准'}</strong></div>
      <div class="metric blue"><span>审核依据</span><strong style="font-size:17px;margin-top:15px">版本差异</strong></div></div>
    <div class="review-layout"><aside class="card" aria-label="审核队列"><div class="card-head"><h2>审核队列</h2><small>${esc(state.reviewTotal)} 条</small></div>
      <div class="tab-strip"><button type="button" class="${state.reviewStatus === 1 ? 'active' : ''}" data-review-status="1">待审核</button><button type="button" class="${state.reviewStatus === 3 ? 'active' : ''}" data-review-status="3">已批准</button></div>
      <div class="list-scroll">${list}</div>${pager(state.reviewTotal, state.reviewSkip, 'review')}</aside>
      <article class="card review-content">${state.reviewConcept ? reviewDetail(state.reviewConcept) : emptyList('选择一条修订', '从左侧队列选择词条，查看修改内容与审核操作。')}</article></div>`;
}

function reviewDetail(concept) {
  const revision = concept.activeRevision;
  if (!revision) return emptyList('修订不存在', '请刷新队列。');
  const sourceNames = new Map(state.sources.map(item => [item.id, item.title]));
  const domainNames = new Map(state.domains.map(item => [item.id, `${item.code} · ${item.nameZh}`]));
  const rows = buildRevisionDiff(concept.publishedRevision, revision, sourceNames, domainNames);
  const table = rows.length ? `<table class="diff-table"><thead><tr><th>字段</th><th>当前已发布</th><th>本次修订</th></tr></thead><tbody>${rows.map(row => `<tr class="${row.change}"><td>${esc(row.label)}</td><td>${row.before ? esc(row.before) : '<span class="empty-value">无</span>'}</td><td>${row.after ? esc(row.after) : '<span class="empty-value">无</span>'}</td></tr>`).join('')}</tbody></table>`
    : `<div class="empty-state" style="min-height:120px"><strong>没有字段差异</strong><p>请核对修订说明和来源；无需变更时可退回。</p></div>`;
  return `<div class="review-summary"><div><p class="eyebrow">${esc(concept.conceptCode)} / REVISION ${esc(revision.version)}</p><h2>${esc(preferred(revision, 'zh-Hans'))}</h2><p>${esc(preferred(revision, 'es'))} · ${esc(preferred(revision, 'en'))} · ${esc(domainName(revision.domainId))}</p></div>
    <div class="review-tags">${statusChip(revision.status)}<span class="status-chip draft">V${esc(revision.version)}</span></div></div>
    <div class="diff-intro"><span>${concept.publishedRevision ? `与已发布版本 V${esc(concept.currentVersion)} 比较` : '首次发布：与空白基线比较'}</span><strong>${rows.length} 处差异</strong></div>
    ${table}
    <div class="review-decision"><h3>${Number(revision.status) === 1 ? '审核意见' : '发布说明'}</h3>
      <textarea id="review-comment" maxlength="2000" placeholder="填写审核意见；退回时必须说明原因。"></textarea>
      <div class="decision-actions"><span>${Number(revision.status) === 1 ? '提交人不可审核自己的修订。' : '发布后本版本将替换公开词条。'}</span>
        <div>${Number(revision.status) === 1 ? '<button class="button danger" type="button" data-review-action="reject">退回修改</button><button class="button primary" type="button" data-review-action="approve">批准修订</button>'
          : '<button class="button primary" type="button" data-review-action="publish">发布词条 →</button>'}</div></div></div>`;
}

async function loadReview() {
  if (!hasAccessToken()) return;
  const page = await api.listConcepts('', state.reviewStatus, state.reviewSkip);
  state.reviewList = page.items || [];
  state.reviewTotal = page.totalCount || 0;
  if (!state.reviewList.some(item => item.id === state.reviewConcept?.id)) state.reviewConcept = null;
  if (!state.reviewConcept && state.reviewList.length) state.reviewConcept = await api.getConcept(state.reviewList[0].id);
}

async function reviewAction(action) {
  const revision = state.reviewConcept?.activeRevision;
  if (!revision || state.busy) return;
  const comment = $('#review-comment')?.value.trim() || '';
  if (action === 'reject' && !comment) { toast('退回时请填写原因。', true); return; }
  if (action === 'publish' && !confirm(`确定发布 ${state.reviewConcept.conceptCode} 的 V${revision.version} 修订吗？`)) return;
  state.busy = true;
  try {
    await api.revisionAction(revision.id, action, revision.revisionToken, comment || null);
    state.reviewConcept = null;
    if (action === 'approve') state.reviewStatus = 3;
    await loadReview();
    await loadEditorList();
    renderReview();
    toast(({ approve: '修订已批准。', reject: '修订已退回。', publish: '词条已发布。' })[action]);
  } catch (error) { showError(error); }
  finally { state.busy = false; }
}

function renderImports() {
  const selected = state.selectedImport;
  const history = state.imports.map(job => `<button type="button" class="list-item ${selected?.id === job.id ? 'active' : ''}" data-open-import="${esc(job.id)}">
    <span class="list-item-head"><span>${esc(job.fileName)}</span>${importChip(job.status)}</span>
    <span class="list-item-sub"><span>${esc(job.templateVersion)} 模板</span><span>${esc(job.validRows)} 成功 · ${esc(job.invalidRows)} 错误</span></span></button>`).join('') || emptyList('暂无导入任务', '上传 CSV 后，结果会显示在这里。');
  $('#imports-view').innerHTML = `<div class="page-heading"><div><p class="eyebrow">DATA / IMPORTS</p><h1 id="imports-title">导入结果</h1><p>批量导入只创建草稿。校验错误精确定位到行号与字段。</p></div>
    <div class="heading-actions"><a class="button" href="./template-v1.csv" download>↓ 下载 CSV 模板</a><button class="button" type="button" data-refresh-imports>刷新任务</button></div></div>
    <div class="metric-grid"><div class="metric"><span>导入任务</span><strong>${hasAccessToken() ? esc(state.importTotal) : '—'}<small>次</small></strong></div>
      <div class="metric orange"><span>最近有效行</span><strong>${selected ? esc(selected.validRows) : '—'}<small>条</small></strong></div>
      <div class="metric blue"><span>最近错误行</span><strong>${selected ? esc(selected.invalidRows) : '—'}<small>条</small></strong></div></div>
    <div class="import-layout"><article class="card upload-card"><h2>上传术语数据</h2><p>选择 UTF-8 编码 CSV 文件。单文件最多 2 MB、5000 行；每个有效行会进入待编辑草稿。</p>
      <label class="upload-zone" id="upload-zone" for="csv-file"><span class="upload-icon" aria-hidden="true">↥</span><strong>${state.uploadFile ? esc(state.uploadFile.name) : '拖放文件到此处，或点击选择'}</strong><span>${state.uploadFile ? `${(state.uploadFile.size / 1024).toFixed(1)} KB · 点击更换文件` : '仅支持 .csv · UTF-8 · 最大 2 MB'}</span><input id="csv-file" type="file" accept=".csv,text/csv"></label>
      <div class="upload-foot"><p>模板版本 1.0 · 导入后需逐条审核发布</p><button class="button primary" type="button" data-upload-csv ${state.uploadFile ? '' : 'disabled'}>开始导入 →</button></div>
    </article><aside class="card history-card"><div class="card-head"><h2>任务历史</h2><small>${esc(state.importTotal)} 次</small></div><div class="history-list">${history}</div>${pager(state.importTotal, state.importSkip, 'imports')}</aside></div>
    <div class="card import-result">${selected ? importResult(selected) : emptyList('选择一项导入任务', '上传文件或从任务历史中查看校验结果。')}</div>`;
}

function importResult(job) {
  const errors = job.errors || [];
  return `<div class="result-head"><div><p class="eyebrow">IMPORT JOB / ${esc(job.id.slice(0, 8).toUpperCase())}</p><h2>${esc(job.fileName)}</h2><p>模板 ${esc(job.templateVersion)} · 导入状态：${esc(IMPORT_STATUS[Number(job.status)] || '未知')}</p></div>
    <div>${importChip(job.status)} ${errors.length ? '<button class="button small" type="button" data-download-errors>↓ 下载错误明细</button>' : ''}</div></div>
    <div class="result-stats"><div class="result-stat"><span>数据总行数</span><strong>${esc(job.totalRows)}</strong></div><div class="result-stat good"><span>已创建草稿</span><strong>${esc(job.validRows)}</strong></div><div class="result-stat bad"><span>错误行</span><strong>${esc(job.invalidRows)}</strong></div></div>
    <div class="error-area">${job.failureReason ? `<div class="quality-callout warning" style="margin:0 0 15px"><strong>导入失败</strong><span>${esc(job.failureReason)}</span></div>` : ''}
      <h3>校验明细 ${errors.length ? `· ${errors.length} 项` : ''}</h3>
      ${errors.length ? `<div style="overflow-x:auto"><table class="error-table"><thead><tr><th>行号</th><th>字段</th><th>错误码</th><th>说明</th></tr></thead><tbody>${errors.map(item => `<tr><td>${esc(item.rowNumber)}</td><td>${esc(item.field)}</td><td>${esc(item.code)}</td><td>${esc(item.message)}</td></tr>`).join('')}</tbody></table></div>`
        : '<div class="success-line">当前任务没有行级校验错误。</div>'}</div>`;
}

async function loadImports() {
  if (!hasAccessToken()) return;
  const page = await api.listImports(state.importSkip);
  state.imports = page.items || [];
  state.importTotal = page.totalCount || 0;
  if (!state.selectedImport && state.imports.length) state.selectedImport = state.imports[0];
}

async function uploadCsv() {
  const file = state.uploadFile;
  if (!file || state.busy) return;
  if (!file.name.toLowerCase().endsWith('.csv')) { toast('请选择 .csv 文件。', true); return; }
  if (file.size > 2 * 1024 * 1024) { toast('CSV 文件不能超过 2 MB。', true); return; }
  state.busy = true;
  const button = $('[data-upload-csv]');
  button.disabled = true;
  button.textContent = '正在校验…';
  try {
    const job = await api.uploadCsv(file);
    state.selectedImport = job;
    state.uploadFile = null;
    state.importSkip = 0;
    await loadImports();
    renderImports();
    toast(job.failureReason ? `导入失败：${job.failureReason}` : `导入完成：${job.validRows} 行草稿，${job.invalidRows} 行错误。`, !!job.failureReason);
  } catch (error) { showError(error); }
  finally { state.busy = false; if (button.isConnected) { button.disabled = false; button.textContent = '开始导入 →'; } }
}

function downloadErrors() {
  const errors = state.selectedImport?.errors || [];
  if (!errors.length) return;
  const quote = value => `"${String(value ?? '').replace(/"/g, '""')}"`;
  const lines = [['RowNumber','Field','Code','Message'].join(','), ...errors.map(item =>
    [item.rowNumber,item.field,item.code,item.message].map(quote).join(','))];
  const blob = new Blob(['\uFEFF', lines.join('\r\n')], { type: 'text/csv;charset=utf-8' });
  const link = document.createElement('a');
  link.href = URL.createObjectURL(blob);
  link.download = `${state.selectedImport.fileName.replace(/\.csv$/i, '')}-errors.csv`;
  link.click();
  setTimeout(() => URL.revokeObjectURL(link.href), 1000);
}

function openConnection() { $('#connection-dialog').showModal(); $('#access-token').focus(); }

async function connect(event) {
  event.preventDefault();
  const token = $('#access-token').value;
  if (!token.trim()) { toast('请输入访问令牌。', true); return; }
  setAccessToken(token);
  $('#access-token').value = '';
  $('#connection-dialog').close();
  $('#connection-button').classList.add('connected');
  $('#connection-label').textContent = '已连接';
  $('#connection-notice').hidden = true;
  const results = await Promise.allSettled([api.getDomains(), api.listSources(), api.listConcepts(), api.listConcepts('', 1), api.listImports()]);
  if (results[0].status === 'fulfilled') state.domains = results[0].value;
  if (results[1].status === 'fulfilled') state.sources = results[1].value;
  if (results[2].status === 'fulfilled') {
    state.editorList = results[2].value.items || [];
    state.editorTotal = results[2].value.totalCount || 0;
  }
  if (results[3].status === 'fulfilled') {
    state.reviewList = results[3].value.items || [];
    state.reviewTotal = results[3].value.totalCount || 0;
  }
  if (results[4].status === 'fulfilled') {
    state.imports = results[4].value.items || [];
    state.importTotal = results[4].value.totalCount || 0;
    state.selectedImport = state.imports[0] || null;
  }
  if (state.reviewList.length) {
    try { state.reviewConcept = await api.getConcept(state.reviewList[0].id); } catch { /* Queue remains usable. */ }
  }
  if (!state.concept && state.isNew) state.workingRevision = newRevision();
  render();
  const failures = results.filter(result => result.status === 'rejected');
  if (failures.length === results.length) showError(failures[0].reason);
  else if (failures.length) toast('已连接；部分列表受当前账号权限限制。');
  else toast('管理数据已加载。');
}

async function createSource(event) {
  event.preventDefault();
  const form = event.target;
  const input = Object.fromEntries(new FormData(form));
  if (!input.url?.trim() && !input.identifier?.trim()) { toast('URL 与持久标识至少填写一项。', true); return; }
  try {
    const source = await api.createSource(input);
    state.sources.push(source);
    $('#source-dialog').close();
    form.reset();
    captureForStructuralEdit();
    state.workingRevision.sources.push({ sourceId: source.id, evidenceType: 3, locator: '' });
    renderEditor();
    toast('来源已保存并关联到当前草稿。');
  } catch (error) { showError(error); }
}

async function handleClick(event) {
  const button = event.target.closest('button');
  if (!button) return;
  if (button.matches('[data-close-dialog]')) { button.closest('dialog')?.close(); return; }
  if (button.id === 'connection-button' || button.id === 'notice-connect') { openConnection(); return; }
  if (button.hasAttribute('data-new-concept')) { createNewConcept(); return; }
  if (button.hasAttribute('data-open-concept')) { await openConcept(button.dataset.openConcept); return; }
  if (button.hasAttribute('data-refresh-editor')) { try { await loadEditorList(); renderEditor(); } catch (error) { showError(error); } return; }
  if (button.hasAttribute('data-search-concepts')) { state.editorQuery = $('#concept-search').value.trim(); state.editorSkip = 0; try { await loadEditorList(); renderEditor(); } catch (error) { showError(error); } return; }
  if (button.hasAttribute('data-save-editor')) { await saveEditor(); return; }
  if (button.hasAttribute('data-submit-editor')) { await saveEditor(true); return; }
  if (button.hasAttribute('data-start-revision')) { await startRevision(); return; }
  if (button.hasAttribute('data-language')) {
    state.activeLanguage = button.dataset.language;
    $$('.language-panel').forEach(panel => { panel.hidden = panel.dataset.languagePanel !== state.activeLanguage; });
    $$('[data-language]').forEach(tab => { tab.classList.toggle('active', tab.dataset.language === state.activeLanguage); tab.setAttribute('aria-selected', String(tab.dataset.language === state.activeLanguage)); });
    return;
  }
  if (button.hasAttribute('data-add-term')) {
    captureForStructuralEdit();
    state.workingRevision.terms.push({ languageTag: button.dataset.addTerm, text: '', termType: 1, partOfSpeech: 'noun', isPreferred: false, senseOrder: 1 });
    renderEditor(); return;
  }
  if (button.hasAttribute('data-add-definition')) {
    captureForStructuralEdit(); state.workingRevision.definitions.push({ languageTag: button.dataset.addDefinition, text: '', scenarioLabel: '', sourceId: null });
    renderEditor(); return;
  }
  if (button.hasAttribute('data-add-source')) {
    captureForStructuralEdit(); state.workingRevision.sources.push({ sourceId: '', evidenceType: 3, locator: '' }); renderEditor(); return;
  }
  if (button.hasAttribute('data-remove-term') || button.hasAttribute('data-remove-definition') || button.hasAttribute('data-remove-source')) {
    const key = button.hasAttribute('data-remove-term') ? 'terms' : button.hasAttribute('data-remove-definition') ? 'definitions' : 'sources';
    const index = Number(button.dataset.removeTerm ?? button.dataset.removeDefinition ?? button.dataset.removeSource);
    captureForStructuralEdit(); state.workingRevision[key].splice(index, 1); renderEditor(); return;
  }
  if (button.hasAttribute('data-open-source')) { $('#source-dialog').showModal(); return; }
  if (button.hasAttribute('data-review-status')) {
    state.reviewStatus = Number(button.dataset.reviewStatus); state.reviewSkip = 0; state.reviewConcept = null;
    try { await loadReview(); renderReview(); } catch (error) { showError(error); } return;
  }
  if (button.hasAttribute('data-open-review')) {
    try { state.reviewConcept = await api.getConcept(button.dataset.openReview); renderReview(); } catch (error) { showError(error); } return;
  }
  if (button.hasAttribute('data-review-action')) { await reviewAction(button.dataset.reviewAction); return; }
  if (button.hasAttribute('data-refresh-review')) { try { await loadReview(); renderReview(); } catch (error) { showError(error); } return; }
  if (button.hasAttribute('data-upload-csv')) { await uploadCsv(); return; }
  if (button.hasAttribute('data-refresh-imports')) { try { await loadImports(); renderImports(); } catch (error) { showError(error); } return; }
  if (button.hasAttribute('data-open-import')) { try { state.selectedImport = await api.getImport(button.dataset.openImport); renderImports(); } catch (error) { showError(error); } return; }
  if (button.hasAttribute('data-download-errors')) { downloadErrors(); return; }
  if (button.hasAttribute('data-page')) {
    const offset = Number(button.dataset.offset);
    try {
      if (button.dataset.page === 'editor') { state.editorSkip = offset; await loadEditorList(); renderEditor(); }
      if (button.dataset.page === 'review') { state.reviewSkip = offset; state.reviewConcept = null; await loadReview(); renderReview(); }
      if (button.dataset.page === 'imports') { state.importSkip = offset; await loadImports(); renderImports(); }
    } catch (error) { showError(error); }
  }
}

document.addEventListener('click', handleClick);
document.addEventListener('input', event => {
  if (event.target.closest('#editor-form')) {
    state.workingRevision = { ...state.workingRevision, ...captureEditor(true) };
    state.dirty = true;
    updateQuality();
  }
});
document.addEventListener('change', event => {
  if (event.target.id === 'csv-file') { state.uploadFile = event.target.files?.[0] || null; renderImports(); }
  if (event.target.closest('#editor-form')) {
    state.workingRevision = { ...state.workingRevision, ...captureEditor(true) };
    state.dirty = true;
    updateQuality();
  }
});
document.addEventListener('keydown', event => {
  if (event.key === 'Enter' && event.target.id === 'concept-search') {
    event.preventDefault(); $('[data-search-concepts]')?.click();
  }
});
document.addEventListener('dragover', event => {
  if (event.target.closest('#upload-zone')) { event.preventDefault(); $('#upload-zone').classList.add('dragging'); }
});
document.addEventListener('dragleave', event => { if (event.target.closest('#upload-zone')) $('#upload-zone')?.classList.remove('dragging'); });
document.addEventListener('drop', event => {
  if (!event.target.closest('#upload-zone')) return;
  event.preventDefault();
  state.uploadFile = event.dataTransfer?.files?.[0] || null;
  renderImports();
});
$('#connection-form').addEventListener('submit', connect);
$('#source-form').addEventListener('submit', createSource);
window.addEventListener('hashchange', navigate);
window.addEventListener('beforeunload', event => { if (state.dirty) { event.preventDefault(); event.returnValue = ''; } });
state.workingRevision = newRevision();
navigate();
