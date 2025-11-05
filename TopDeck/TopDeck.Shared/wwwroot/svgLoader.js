window.loadSvg = async function (path) {
    const r = await fetch(path);
    return await r.text();
};