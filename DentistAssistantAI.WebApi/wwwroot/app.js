// ─── Tab switching ────────────────────────────────────────────────────────────
document.querySelectorAll('.tab-btn').forEach(btn => {
  btn.addEventListener('click', () => {
    const tab = btn.dataset.tab;
    document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
    document.querySelectorAll('.tab-content').forEach(c => c.classList.remove('active'));
    btn.classList.add('active');
    document.getElementById(`tab-${tab}`).classList.add('active');
  });
});

// ─── Mode toggle (Teacher / Student) ─────────────────────────────────────────
let chatMode = 'teacher';
document.querySelectorAll('.mode-btn').forEach(btn => {
  btn.addEventListener('click', () => {
    chatMode = btn.dataset.mode;
    document.querySelectorAll('.mode-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
  });
});

// ─── Helpers ──────────────────────────────────────────────────────────────────
function showLoading() { document.getElementById('loading').classList.remove('hidden'); }
function hideLoading() { document.getElementById('loading').classList.add('hidden'); }

async function apiFetch(url, body) {
  const res = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({ error: `HTTP ${res.status}` }));
    throw new Error(err.error || `HTTP ${res.status}`);
  }
  return res.json();
}

function renderMarkdown(text) {
  // Minimal markdown: bold, headings, list items
  return text
    .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
    .replace(/^### (.+)$/gm, '<h4>$1</h4>')
    .replace(/^## (.+)$/gm, '<h3>$1</h3>')
    .replace(/^# (.+)$/gm, '<h2>$1</h2>')
    .replace(/^[-•] (.+)$/gm, '<li>$1</li>')
    .replace(/✅/g, '<span style="color:#28a745">✅</span>')
    .replace(/❌/g, '<span style="color:#dc3545">❌</span>')
    .replace(/⚠️/g, '<span style="color:#ffc107">⚠️</span>');
}

// ─── Chat ─────────────────────────────────────────────────────────────────────
const messagesArea = document.getElementById('chat-messages');

function addMessage(text, type) {
  const div = document.createElement('div');
  div.className = `message ${type}`;
  div.innerHTML = type === 'ai' ? renderMarkdown(text) : text;
  messagesArea.appendChild(div);
  messagesArea.scrollTop = messagesArea.scrollHeight;
}

async function sendChat() {
  const input = document.getElementById('chat-input');
  const year = parseInt(document.getElementById('chat-year').value, 10);
  const msg = input.value.trim();
  if (!msg) return;

  addMessage(msg, 'user');
  input.value = '';
  document.getElementById('chat-send').disabled = true;
  showLoading();

  try {
    let data;
    if (chatMode === 'student') {
      data = await apiFetch('/api/student/ask', { question: msg, courseYear: year });
    } else {
      data = await apiFetch('/api/chat', { message: msg });
    }
    addMessage(data.response, 'ai');
  } catch (e) {
    addMessage(`Ошибка: ${e.message}`, 'error');
  } finally {
    document.getElementById('chat-send').disabled = false;
    hideLoading();
  }
}

document.getElementById('chat-send').addEventListener('click', sendChat);
document.getElementById('chat-input').addEventListener('keydown', e => {
  if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); sendChat(); }
});

// ─── Generate ─────────────────────────────────────────────────────────────────
const genOutput = document.getElementById('gen-output');
const genContent = document.getElementById('gen-content');
const genTitle = document.getElementById('gen-output-title');

document.querySelectorAll('.btn-gen').forEach(btn => {
  btn.addEventListener('click', async () => {
    const topic = document.getElementById('gen-topic').value.trim();
    const year = parseInt(document.getElementById('gen-year').value, 10);
    const count = parseInt(document.getElementById('gen-count').value, 10) || 10;
    const action = btn.dataset.action;

    if (!topic) {
      alert('Введите тему');
      return;
    }

    document.querySelectorAll('.btn-gen').forEach(b => b.disabled = true);
    showLoading();

    try {
      let data, title;
      if (action === 'lecture') {
        data = await apiFetch('/api/teacher/lecture', { topic, courseYear: year });
        title = `📖 Лекция — ${topic}`;
      } else if (action === 'test') {
        data = await apiFetch('/api/teacher/test', { topic, courseYear: year, questionCount: count });
        title = `✅ Тест — ${topic}`;
      } else {
        data = await apiFetch('/api/teacher/case', { topic, courseYear: year });
        title = `📋 Кейс — ${topic}`;
      }
      genTitle.textContent = title;
      genContent.innerHTML = renderMarkdown(data.response);
      genOutput.classList.remove('hidden');
      genOutput.scrollIntoView({ behavior: 'smooth', block: 'start' });
    } catch (e) {
      alert(`Ошибка: ${e.message}`);
    } finally {
      document.querySelectorAll('.btn-gen').forEach(b => b.disabled = false);
      hideLoading();
    }
  });
});

document.getElementById('gen-copy').addEventListener('click', () => {
  const text = genContent.innerText;
  navigator.clipboard.writeText(text).then(() => {
    const btn = document.getElementById('gen-copy');
    btn.textContent = '✅ Скопировано';
    setTimeout(() => { btn.textContent = '📋 Копировать'; }, 2000);
  });
});

// ─── Clinical Cases ───────────────────────────────────────────────────────────
let currentCaseText = '';

document.getElementById('case-generate').addEventListener('click', async () => {
  const topic = document.getElementById('case-topic').value.trim();
  const year = parseInt(document.getElementById('case-year').value, 10);

  if (!topic) { alert('Введите тему кейса'); return; }

  document.getElementById('case-generate').disabled = true;
  showLoading();

  try {
    const data = await apiFetch('/api/cases/generate', { topic, courseYear: year });
    currentCaseText = data.response;
    document.getElementById('case-text').innerHTML = renderMarkdown(data.response);
    document.getElementById('case-diagnosis').value = '';
    document.getElementById('case-treatment').value = '';
    document.getElementById('case-feedback').classList.add('hidden');
    document.getElementById('case-display').classList.remove('hidden');
  } catch (e) {
    alert(`Ошибка: ${e.message}`);
  } finally {
    document.getElementById('case-generate').disabled = false;
    hideLoading();
  }
});

document.getElementById('case-submit').addEventListener('click', async () => {
  const diagnosis = document.getElementById('case-diagnosis').value.trim();
  const treatment = document.getElementById('case-treatment').value.trim();

  if (!diagnosis) { alert('Введите диагноз'); return; }
  if (!treatment) { alert('Введите план лечения'); return; }

  document.getElementById('case-submit').disabled = true;
  showLoading();

  try {
    const data = await apiFetch('/api/cases/evaluate', {
      caseText: currentCaseText,
      diagnosis,
      treatment,
    });
    document.getElementById('case-feedback-content').innerHTML = renderMarkdown(data.response);
    const feedback = document.getElementById('case-feedback');
    feedback.classList.remove('hidden');
    feedback.scrollIntoView({ behavior: 'smooth', block: 'start' });
  } catch (e) {
    alert(`Ошибка: ${e.message}`);
  } finally {
    document.getElementById('case-submit').disabled = false;
    hideLoading();
  }
});
