const concepts = [
  {
    id: "IT-000001",
    zh: "人工智能",
    es: "inteligencia artificial",
    en: "artificial intelligence",
    domain: "IT.01 人工智能",
    definition: "研究和构建能够执行通常需要人类智能的任务之计算系统的领域及相关技术。",
    aliases: ["AI", "IA"]
  },
  {
    id: "IT-000002",
    zh: "机器学习",
    es: "aprendizaje automático",
    en: "machine learning",
    domain: "IT.01 人工智能",
    definition: "使计算系统能够从数据中改进任务表现的方法与研究领域。",
    aliases: ["ML", "aprendizaje de máquina"]
  },
  {
    id: "ECON-000001",
    zh: "供应链",
    es: "cadena de suministro",
    en: "supply chain",
    domain: "ECON.05 物流与供应链",
    definition: "产品、服务、信息和资金从供应方到使用方流动所涉及的组织与活动网络。",
    aliases: ["供应网络"]
  },
  {
    id: "RES-000001",
    zh: "开放获取",
    es: "acceso abierto",
    en: "open access",
    domain: "RES.08 学术出版与科研管理",
    definition: "使研究成果可在线免费获取并按照明确许可条件使用的出版与传播模式。",
    aliases: ["OA"]
  }
];

const panels = [...document.querySelectorAll("[data-view-panel]")];
const navButtons = [...document.querySelectorAll(".topbar [data-view]")];
const resultList = document.querySelector("#result-list");

function normalize(value) {
  return value.trim().toLocaleLowerCase().normalize("NFC");
}

function selectConcepts(query) {
  const q = normalize(query);
  if (!q) return concepts;
  const hits = concepts.filter((item) => [item.zh, item.es, item.en, ...item.aliases]
    .some((term) => normalize(term).includes(q) || q.includes(normalize(term))));
  return hits.length ? hits : concepts.slice(0, 2);
}

function renderResults(query = "人工智能") {
  const hits = selectConcepts(query);
  resultList.innerHTML = hits.map((item, index) => `
    <button class="result-card" type="button" data-concept="${item.id}">
      <span class="meta"><span class="match">${index === 0 ? "精确匹配" : "相关概念"}</span><span>${item.domain}</span><span>${item.id}</span></span>
      <h2>${item.zh}</h2>
      <div class="translations">${item.es} · ${item.en}</div>
      <p class="definition">${item.definition}</p>
    </button>`).join("");
  document.querySelector("#results-title").textContent = `“${query}”的概念匹配`;
  document.querySelector("#result-query").value = query;
  resultList.querySelectorAll("[data-concept]").forEach((button) => button.addEventListener("click", () => showView("detail")));
}

function showView(name) {
  panels.forEach((panel) => panel.classList.toggle("view-active", panel.dataset.viewPanel === name));
  navButtons.forEach((button) => button.classList.toggle("nav-active", button.dataset.view === name));
  window.scrollTo({ top: 0, behavior: "smooth" });
}

document.querySelectorAll("[data-view]").forEach((button) => {
  button.addEventListener("click", () => showView(button.dataset.view));
});

document.querySelectorAll("[data-search-form]").forEach((form) => {
  form.addEventListener("submit", (event) => {
    event.preventDefault();
    const query = new FormData(form).get("query") || "人工智能";
    renderResults(String(query));
    showView("results");
  });
});

document.querySelectorAll("[data-query]").forEach((button) => {
  button.addEventListener("click", () => {
    renderResults(button.dataset.query);
    showView("results");
  });
});

const voiceDialog = document.querySelector("#voice-dialog");
document.querySelectorAll("[data-voice]").forEach((button) => {
  button.addEventListener("click", () => voiceDialog.showModal());
});

document.querySelectorAll("[data-language-tab]").forEach((button) => {
  button.addEventListener("click", () => {
    document.querySelectorAll("[data-language-tab]").forEach((tab) => tab.classList.toggle("active", tab === button));
    document.querySelectorAll("[data-language-panel]").forEach((panel) => panel.classList.toggle("panel-active", panel.dataset.languagePanel === button.dataset.languageTab));
  });
});

renderResults();
