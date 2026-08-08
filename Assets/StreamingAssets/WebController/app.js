const statusElement = document.getElementById('status');
const tapArea = document.getElementById('tap-area');
const settingsPanel = document.getElementById('settings-panel');
const settingsButton = document.getElementById('settings-button');
const closeButton = document.getElementById('close-button');
const launchButton = document.getElementById('launch-button');
const structureSelect = document.getElementById('structure');
const changeColorRow = document.getElementById('change-color-row');
const textFireworkRow = document.getElementById('text-firework-row');
const textFireworkInput = document.getElementById('text-firework');
let accessToken;
let structuresById = new Map();
let sizesById = new Map();
let colorsById = new Map();

function createRequestId() {
  if (crypto.randomUUID) return crypto.randomUUID();
  return `${Date.now().toString(16)}-${Math.random().toString(16).slice(2)}`;
}

function fillOptions(element, options) {
  element.replaceChildren(...options.map((option) => {
    const item = document.createElement('option');
    item.value = option.id;
    item.textContent = option.displayName;
    return item;
  }));
}

function send(command) {
  fetch(`/api/launch?token=${encodeURIComponent(accessToken)}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json; charset=utf-8' },
    body: JSON.stringify({ ...command, requestId: createRequestId() })
  })
    .then((response) => response.json())
    .then((response) => { if (response.type === 'error') statusElement.textContent = response.message; })
    .catch(() => { statusElement.textContent = '送信できません'; });
}

function showTapFeedback(event, size, color) {
  const visualScale = size ? size.visualScale : 1;
  tapArea.style.setProperty('--ripple-x', `${event.clientX}px`);
  tapArea.style.setProperty('--ripple-y', `${event.clientY}px`);
  tapArea.style.setProperty('--ripple-size', `${120 + (visualScale * 110)}px`);
  tapArea.style.setProperty('--ripple-color', color ? color.colorHex : '#93c5fd');
  tapArea.classList.remove('is-tapped');
  void tapArea.offsetWidth;
  tapArea.classList.add('is-tapped');
}

function updateStructureVisibility() {
  const structure = structuresById.get(structureSelect.value);
  changeColorRow.hidden = !structure || !structure.allowsChangeColor;
  textFireworkRow.hidden = !structure || !structure.isTextFirework;
}

function randomItem(items) {
  return items[Math.floor(Math.random() * items.length)];
}

function createLaunchCommand(size, structure, color, changeColor) {
  const command = { type: 'launch', sizeId: size.id, structure: structure.id, colorId: color.id };
  if (structure.isTextFirework) {
    command.textValue = textFireworkInput.value.trim() || String.fromCharCode(65 + Math.floor(Math.random() * 26));
  } else {
    command.changeColorId = changeColor.id;
  }
  return command;
}

async function initialize() {
  try {
    const config = await fetch('/api/config').then((response) => response.json());
    fillOptions(document.getElementById('size'), config.sizes);
    sizesById = new Map(config.sizes.map((size) => [size.id, size]));
    fillOptions(structureSelect, config.structures);
    structuresById = new Map(config.structures.map((structure) => [structure.id, structure]));
    fillOptions(document.getElementById('color'), config.colors);
    fillOptions(document.getElementById('change-color'), config.colors);
    colorsById = new Map(config.colors.map((color) => [color.id, color]));
    updateStructureVisibility();
    accessToken = config.token;
    statusElement.textContent = '接続済み';
  } catch {
    statusElement.textContent = '設定を取得できません';
  }
}

settingsButton.addEventListener('click', (event) => {
  event.stopPropagation();
  settingsPanel.classList.toggle('is-open');
});
closeButton.addEventListener('click', () => settingsPanel.classList.remove('is-open'));
structureSelect.addEventListener('change', updateStructureVisibility);
launchButton.addEventListener('click', () => {
  const structure = structuresById.get(structureSelect.value);
  const text = textFireworkInput.value.trim();
  if (structure && structure.isTextFirework && (!text || text.length > 6)) {
    statusElement.textContent = '文字列は1〜6文字で入力してください';
    return;
  }
  send(createLaunchCommand(
    sizesById.get(document.getElementById('size').value), structure,
    colorsById.get(document.getElementById('color').value),
    colorsById.get(document.getElementById('change-color').value)));
});
tapArea.addEventListener('click', (event) => {
  if (settingsPanel.classList.contains('is-open') || event.target.closest('button, select, input, label')) return;
  const size = randomItem([...sizesById.values()]);
  const structure = randomItem([...structuresById.values()]);
  const color = randomItem([...colorsById.values()]);
  const changeColor = randomItem([...colorsById.values()]);
  showTapFeedback(event, size, color);
  send(createLaunchCommand(size, structure, color, changeColor));
});

initialize();
