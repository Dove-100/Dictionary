let accessToken = '';

export function setAccessToken(value) {
  accessToken = value.trim().replace(/^Bearer\s+/i, '');
}

export function hasAccessToken() {
  return accessToken.length > 0;
}

export class ApiError extends Error {
  constructor(message, status, code) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.code = code;
  }
}

async function request(path, options = {}) {
  if (!accessToken) throw new ApiError('请先连接管理 API。', 401, 'MissingToken');
  const headers = new Headers(options.headers || {});
  headers.set('Authorization', `Bearer ${accessToken}`);
  if (options.body && !(options.body instanceof FormData)) headers.set('Content-Type', 'application/json');
  const response = await fetch(path, { ...options, headers, cache: 'no-store' });
  if (!response.ok) {
    let payload = null;
    try { payload = await response.json(); } catch { /* Non-JSON errors retain HTTP status. */ }
    const code = payload?.error?.code || payload?.code || '';
    const detail = payload?.error?.message || payload?.message;
    const fallback = response.status === 401 ? '身份验证失败，请更换访问令牌。'
      : response.status === 403 ? '当前账号没有执行此操作的权限。'
      : response.status === 409 ? '修订已被其他人修改，请重新加载。'
      : `请求失败（HTTP ${response.status}）。`;
    throw new ApiError(detail || fallback, response.status, code);
  }
  if (response.status === 204) return null;
  return response.json();
}

function params(values) {
  const query = new URLSearchParams();
  for (const [key, value] of Object.entries(values)) {
    if (value !== '' && value !== null && value !== undefined) query.set(key, value);
  }
  return query.toString();
}

export const api = {
  listConcepts: (query = '', revisionStatus = null, skipCount = 0) =>
    request(`/api/v1/admin/concepts?${params({ query, revisionStatus, skipCount, maxResultCount: 20 })}`),
  getConcept: id => request(`/api/v1/admin/concepts/${encodeURIComponent(id)}`),
  getDomains: () => request('/api/v1/admin/concepts/domains'),
  createConcept: body => request('/api/v1/admin/concepts', { method: 'POST', body: JSON.stringify(body) }),
  startRevision: (id, changeSummary) => request(`/api/v1/admin/concepts/${encodeURIComponent(id)}/revisions`, {
    method: 'POST', body: JSON.stringify({ changeSummary }),
  }),
  updateRevision: (id, body) => request(`/api/v1/admin/revisions/${encodeURIComponent(id)}`, {
    method: 'PUT', body: JSON.stringify(body),
  }),
  revisionAction: (id, action, revisionToken, comment = null) =>
    request(`/api/v1/admin/revisions/${encodeURIComponent(id)}/${action}`, {
      method: 'POST', body: JSON.stringify({ revisionToken, comment }),
    }),
  listSources: () => request('/api/v1/admin/sources'),
  createSource: body => request('/api/v1/admin/sources', { method: 'POST', body: JSON.stringify(body) }),
  listImports: (skipCount = 0) => request(`/api/v1/admin/import-jobs?${params({ skipCount, maxResultCount: 20 })}`),
  getImport: id => request(`/api/v1/admin/import-jobs/${encodeURIComponent(id)}`),
  uploadCsv: file => {
    const body = new FormData();
    body.append('file', file);
    return request('/api/v1/admin/import-jobs/csv', { method: 'POST', body });
  },
};
