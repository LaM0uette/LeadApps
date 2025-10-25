window.TopDeck = window.TopDeck || {};
window.TopDeck.scrollCardIntoView = function (id) {
    const el = document.getElementById(id);
    if (!el) return;
    el.scrollIntoView({ behavior: 'smooth', block: 'center', inline: 'center' });
    el.classList.add('flash');
    setTimeout(() => el.classList.remove('flash'), 600);
};