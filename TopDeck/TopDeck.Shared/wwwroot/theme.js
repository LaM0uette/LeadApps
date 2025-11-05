(function(){
  const STORAGE_KEY = 'theme';
  function apply(theme){
    const root = document.documentElement; // <html>
    if(theme === 'dark'){
      root.setAttribute('data-theme','dark');
    } else {
      root.removeAttribute('data-theme');
    }
  }
  function current(){
    const attr = document.documentElement.getAttribute('data-theme');
    return attr === 'dark' ? 'dark' : 'light';
  }
  function set(theme){
    const t = theme === 'dark' ? 'dark' : 'light';
    localStorage.setItem(STORAGE_KEY, t);
    apply(t);
  }
  function toggle(){
    set(current() === 'dark' ? 'light' : 'dark');
  }
  function init(){
    const saved = localStorage.getItem(STORAGE_KEY);
    if(saved === 'dark' || saved === 'light'){
      apply(saved);
    } else {
      // Default to light theme on first visit (if no preference saved)
      const initial = 'light';
      apply(initial);
      localStorage.setItem(STORAGE_KEY, initial);
    }
  }
  window.TopDeckTheme = { init, toggle, set, current };
})();
